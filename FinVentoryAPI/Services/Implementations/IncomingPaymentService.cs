// ════════════════════════════════════════════════════════════════════
// Services/Implementations/IncomingPaymentService.cs
// ════════════════════════════════════════════════════════════════════
using FinVentoryAPI.Data;
using FinVentoryAPI.DTOs.AccountLedgerPostingDTOs;
using FinVentoryAPI.DTOs.IncomingPaymentDTOs;
using FinVentoryAPI.DTOs.PagedRequestDto;
using FinVentoryAPI.Entities;
using FinVentoryAPI.Helpers;
using FinVentoryAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace FinVentoryAPI.Services.Implementations
{
    public class IncomingPaymentService : IIncomingPaymentService
    {
        private readonly AppDbContext _context;
        private readonly Common _common;
        private readonly IAccountLedgerPostingService _accountLedger;

        public IncomingPaymentService(
            AppDbContext context,
            Common common,
            IAccountLedgerPostingService accountLedger)
        {
            _context = context;
            _common = common;
            _accountLedger = accountLedger;
        }

        // ════════════════════════════════════════════════════
        // GET PENDING BILLS
        // ════════════════════════════════════════════════════
        public async Task<List<PendingBillDto>> GetPendingBillsAsync(int businessPartnerId)
        {
            var companyId = _common.GetCompanyId();

            // Fetch all non-deleted, non-Draft, non-Cancelled invoices for this customer.
            // Include Draft status too if your workflow allows collecting before posting.
            var invoices = await _context.SalesInvoiceMains
                .Where(x =>
                    x.CompanyId == companyId &&
                    x.BusinessPartnerId == businessPartnerId &&
                    !x.IsDeleted)
                .ToListAsync();

            if (!invoices.Any())
                return new List<PendingBillDto>();

            var invoiceIds = invoices.Select(i => i.InvoiceId).ToList();

            // Sum already-paid amounts per invoice from non-cancelled payments
            var paidMap = await _context.IncomingPaymentAllocations
                .Where(a =>
                    invoiceIds.Contains(a.InvoiceId) &&
                    !a.Payment!.IsDeleted &&
                    a.Payment.Status != "Cancelled")
                .GroupBy(a => a.InvoiceId)
                .Select(g => new { InvoiceId = g.Key, Paid = g.Sum(a => a.AmountApplied) })
                .ToDictionaryAsync(x => x.InvoiceId, x => x.Paid);

            var today = DateTime.UtcNow.Date;

            var result = invoices
                .Select(inv =>
                {
                    var paid = paidMap.TryGetValue(inv.InvoiceId, out var p) ? p : 0m;
                    var pending = inv.NetTotal - paid;

                    var dueDate = inv.DueDate == default(DateTime) ? (DateTime?)null : inv.DueDate;
                    var daysOverdue = dueDate.HasValue && dueDate.Value.Date < today
                        ? (today - dueDate.Value.Date).Days : 0;

                    return new PendingBillDto
                    {
                        InvoiceId = inv.InvoiceId,
                        InvoiceNo = inv.InvoiceNo,
                        InvoiceDate = inv.InvoiceDate,
                        DueDate = dueDate,
                        InvoiceTotal = inv.NetTotal,
                        PaidAmount = paid,
                        PendingAmount = pending,
                        DaysOverdue = daysOverdue
                    };
                })
                .Where(x => x.PendingAmount > 0)          // only invoices with outstanding balance
                .OrderBy(x => x.DueDate ?? x.InvoiceDate) // oldest due date first
                .ToList();

            return result;
        }

        // ════════════════════════════════════════════════════
        // CREATE
        // ════════════════════════════════════════════════════
        public async Task<IncomingPaymentResponseDto> CreateAsync(CreateIncomingPaymentDto dto)
        {
            var companyId = _common.GetCompanyId();
            var userId = _common.GetUserId();
            var finYearId = _common.GetFinancialYearId();

            // ── Validate BEFORE transaction ───────────────────
            await ValidatePaymentAsync(dto.BusinessPartnerId, dto.DepositAccountId, companyId);
            ValidateAmounts(dto.TotalAmount, dto.OnAccountAmount, dto.Allocations);

            // Validate individual invoices
            var invoiceIds = dto.Allocations.Select(a => a.InvoiceId).ToList();
            var pendingBills = await GetPendingBillsAsync(dto.BusinessPartnerId);
            await ValidateAllocationsAsync(dto.Allocations, pendingBills, companyId);

            var paymentNo = await GeneratePaymentNoAsync(companyId, finYearId);

            var main = new IncomingPaymentMain
            {
                CompanyId = companyId,
                FinYearId = finYearId,
                PaymentNo = paymentNo,
                PaymentDate = dto.PaymentDate,
                Status = "Draft",
                BusinessPartnerId = dto.BusinessPartnerId,
                DepositAccountId = dto.DepositAccountId,
                PaymentMode = dto.PaymentMode,
                ChequeNo = dto.ChequeNo,
                ChequeDate = dto.ChequeDate,
                BankName = dto.BankName,
                TransactionRef = dto.TransactionRef,
                Remarks = dto.Remarks,
                TotalAmount = dto.TotalAmount,
                AllocatedAmount = dto.Allocations.Sum(a => a.AmountApplied),
                OnAccountAmount = dto.OnAccountAmount,
                CreatedBy = userId,
                CreatedDate = DateTime.UtcNow,
                Allocations = new List<IncomingPaymentAllocation>()
            };

            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _context.IncomingPaymentMains.Add(main);
                await SaveChangesAsync(); // ← get PaymentId

                foreach (var alloc in dto.Allocations)
                {
                    _context.IncomingPaymentAllocations.Add(new IncomingPaymentAllocation
                    {
                        PaymentId = main.PaymentId,
                        InvoiceId = alloc.InvoiceId,
                        AmountApplied = alloc.AmountApplied
                    });
                }

                await SaveChangesAsync(); // persist allocations

                // ── Re-fetch with navigation for ledger posting ──
                var mainForPosting = await LoadMainAsync(main.PaymentId, companyId)
                    ?? throw new Exception("Payment not found after save.");

                await PostAccountLedgerAsync(mainForPosting, isReversal: false);

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }

            return await GetByIdAsync(main.PaymentId)
                ?? throw new Exception("Failed to retrieve saved payment.");
        }

        // ════════════════════════════════════════════════════
        // UPDATE
        // ════════════════════════════════════════════════════
        public async Task<bool> UpdateAsync(int id, UpdateIncomingPaymentDto dto)
        {
            var companyId = _common.GetCompanyId();
            var userId = _common.GetUserId();

            var main = await LoadMainAsync(id, companyId);
            if (main == null) return false;
            if (main.Status != "Draft")
                throw new Exception("Only Draft payments can be updated.");

            await ValidatePaymentAsync(dto.BusinessPartnerId, dto.DepositAccountId, companyId);
            ValidateAmounts(dto.TotalAmount, dto.OnAccountAmount, dto.Allocations);
            var pendingBills = await GetPendingBillsAsync(dto.BusinessPartnerId);

            // Exclude this payment's own existing allocations from pending calculation
            // so we don't double-count them
            await ValidateAllocationsAsync(
                dto.Allocations, pendingBills, companyId,
                excludePaymentId: id);

            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // ── Update header ─────────────────────────────
                main.PaymentDate = dto.PaymentDate;
                main.BusinessPartnerId = dto.BusinessPartnerId;
                main.DepositAccountId = dto.DepositAccountId;
                main.PaymentMode = dto.PaymentMode;
                main.ChequeNo = dto.ChequeNo;
                main.ChequeDate = dto.ChequeDate;
                main.BankName = dto.BankName;
                main.TransactionRef = dto.TransactionRef;
                main.Remarks = dto.Remarks;
                main.TotalAmount = dto.TotalAmount;
                main.AllocatedAmount = dto.Allocations.Sum(a => a.AmountApplied);
                main.OnAccountAmount = dto.OnAccountAmount;
                main.ModifiedBy = userId;
                main.ModifiedDate = DateTime.UtcNow;

                // ── Replace allocations ───────────────────────
                if (main.Allocations != null && main.Allocations.Any())
                    _context.IncomingPaymentAllocations.RemoveRange(main.Allocations);

                foreach (var alloc in dto.Allocations)
                {
                    _context.IncomingPaymentAllocations.Add(new IncomingPaymentAllocation
                    {
                        PaymentId = main.PaymentId,
                        InvoiceId = alloc.InvoiceId,
                        AmountApplied = alloc.AmountApplied
                    });
                }

                await SaveChangesAsync();

                await UpdateAccountLedgerAsync(main);

                await transaction.CommitAsync();
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        // ════════════════════════════════════════════════════
        // DELETE  (soft)
        // ════════════════════════════════════════════════════
        public async Task<bool> DeleteAsync(int id)
        {
            var companyId = _common.GetCompanyId();
            var userId = _common.GetUserId();

            var main = await LoadMainAsync(id, companyId);
            if (main == null) return false;
            if (main.Status != "Draft")
                throw new Exception("Only Draft payments can be deleted.");

            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                main.IsDeleted = true;
                main.IsActive = false;
                main.ModifiedBy = userId;
                main.ModifiedDate = DateTime.UtcNow;

                await _accountLedger.SoftDeleteByVoucherAsync(
                    companyId, main.FinYearId, main.PaymentNo, userId);

                await SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        // ════════════════════════════════════════════════════
        // GET BY ID
        // ════════════════════════════════════════════════════
        public async Task<IncomingPaymentResponseDto?> GetByIdAsync(int id)
        {
            var companyId = _common.GetCompanyId();

            var main = await _context.IncomingPaymentMains
                .AsNoTracking()
                .Include(x => x.BusinessPartner)
                .Include(x => x.DepositAccount)
                .Include(x => x.Allocations!)
                    .ThenInclude(a => a.Invoice)
                .FirstOrDefaultAsync(x =>
                    x.PaymentId == id &&
                    x.CompanyId == companyId &&
                    !x.IsDeleted);

            return main == null ? null : MapToResponseDto(main);
        }

        // ════════════════════════════════════════════════════
        // GET ALL
        // ════════════════════════════════════════════════════
        public async Task<List<IncomingPaymentResponseDto>> GetAllAsync()
        {
            var companyId = _common.GetCompanyId();

            var list = await _context.IncomingPaymentMains
                .Where(x => x.CompanyId == companyId && !x.IsDeleted)
                .Include(x => x.BusinessPartner)
                .Include(x => x.DepositAccount)
                .Include(x => x.Allocations!)
                    .ThenInclude(a => a.Invoice)
                .OrderByDescending(x => x.PaymentDate)
                .ToListAsync();

            return list.Select(MapToResponseDto).ToList();
        }

        // ════════════════════════════════════════════════════
        // GET PAGED
        // ════════════════════════════════════════════════════
        public async Task<PagedResponseDto<IncomingPaymentResponseDto>> GetPagedAsync(PagedRequestDto request)
        {
            var companyId = _common.GetCompanyId();

            var query = _context.IncomingPaymentMains
                .Where(x => x.CompanyId == companyId && !x.IsDeleted)
                .Include(x => x.BusinessPartner)
                .Include(x => x.DepositAccount)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var s = request.Search.ToLower();
                query = query.Where(x =>
                    x.PaymentNo.ToLower().Contains(s) ||
                    x.BusinessPartner!.BusinessPartnerName.ToLower().Contains(s));
            }

            if (request.Filters != null)
            {
                if (request.Filters.ContainsKey("status"))
                {
                    var status = ((JsonElement)request.Filters["status"]).GetString();
                    query = query.Where(x => x.Status == status);
                }
                if (request.Filters.ContainsKey("businessPartnerId"))
                {
                    var bpId = ((JsonElement)request.Filters["businessPartnerId"]).GetInt32();
                    query = query.Where(x => x.BusinessPartnerId == bpId);
                }
                if (request.Filters.ContainsKey("fromDate"))
                {
                    var from = ((JsonElement)request.Filters["fromDate"]).GetDateTime();
                    query = query.Where(x => x.PaymentDate >= from);
                }
                if (request.Filters.ContainsKey("toDate"))
                {
                    var to = ((JsonElement)request.Filters["toDate"]).GetDateTime();
                    query = query.Where(x => x.PaymentDate <= to);
                }
                if (request.Filters.ContainsKey("paymentMode"))
                {
                    var mode = ((JsonElement)request.Filters["paymentMode"]).GetString();
                    query = query.Where(x => x.PaymentMode == mode);
                }
            }

            if (request.Sorts != null && request.Sorts.Any())
            {
                var sort = request.Sorts.First();
                query = sort.Column.ToLower() switch
                {
                    "paymentno" => sort.Direction == "desc" ? query.OrderByDescending(x => x.PaymentNo) : query.OrderBy(x => x.PaymentNo),
                    "paymentdate" => sort.Direction == "desc" ? query.OrderByDescending(x => x.PaymentDate) : query.OrderBy(x => x.PaymentDate),
                    "businesspartnername" => sort.Direction == "desc" ? query.OrderByDescending(x => x.BusinessPartner!.BusinessPartnerName) : query.OrderBy(x => x.BusinessPartner!.BusinessPartnerName),
                    "totalamount" => sort.Direction == "desc" ? query.OrderByDescending(x => x.TotalAmount) : query.OrderBy(x => x.TotalAmount),
                    "status" => sort.Direction == "desc" ? query.OrderByDescending(x => x.Status) : query.OrderBy(x => x.Status),
                    _ => query.OrderByDescending(x => x.PaymentDate)
                };
            }
            else
            {
                query = query.OrderByDescending(x => x.PaymentDate);
            }

            var totalRecords = await query.CountAsync();
            var data = await query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Include(x => x.Allocations!)
                    .ThenInclude(a => a.Invoice)
                .ToListAsync();

            return new PagedResponseDto<IncomingPaymentResponseDto>
            {
                TotalRecords = totalRecords,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                Data = data.Select(MapToResponseDto).ToList()
            };
        }

        // ════════════════════════════════════════════════════
        // PRIVATE HELPERS — Validation
        // ════════════════════════════════════════════════════

        private async Task ValidatePaymentAsync(int businessPartnerId, int depositAccountId, int companyId)
        {
            var bpExists = await _context.BusinessPartners
                .AnyAsync(x =>
                    x.BusinessPartnerId == businessPartnerId &&
                    x.CompanyId == companyId &&
                    !x.IsDeleted);
            if (!bpExists) throw new Exception("Business Partner not found.");

            var accountExists = await _context.Accounts
                .AnyAsync(x =>
                    x.AccountId == depositAccountId &&
                    x.CompanyId == companyId &&
                    !x.IsDeleted);
            if (!accountExists) throw new Exception("Deposit Account not found.");
        }

        private static void ValidateAmounts(
            decimal totalAmount,
            decimal onAccountAmount,
            List<IncomingPaymentAllocationDto> allocations)
        {
            if (totalAmount <= 0)
                throw new Exception("Total amount must be greater than zero.");

            var allocatedSum = allocations.Sum(a => a.AmountApplied);
            var expected = allocatedSum + onAccountAmount;

            if (Math.Abs(expected - totalAmount) > 0.01m)
                throw new Exception(
                    $"Total amount ({totalAmount:N2}) must equal " +
                    $"allocated ({allocatedSum:N2}) + on-account ({onAccountAmount:N2}).");

            if (onAccountAmount < 0)
                throw new Exception("On-account amount cannot be negative.");

            foreach (var a in allocations)
                if (a.AmountApplied <= 0)
                    throw new Exception($"Allocation for invoice {a.InvoiceNo} must be > 0.");
        }

        private async Task ValidateAllocationsAsync(
            List<IncomingPaymentAllocationDto> allocations,
            List<PendingBillDto> pendingBills,
            int companyId,
            int? excludePaymentId = null)
        {
            if (!allocations.Any()) return;

            // Check duplicates in request
            var dupes = allocations.GroupBy(a => a.InvoiceId).Where(g => g.Count() > 1).ToList();
            if (dupes.Any())
                throw new Exception(
                    $"Duplicate invoice in allocations: {string.Join(", ", dupes.Select(g => g.Key))}");

            foreach (var alloc in allocations)
            {
                var pending = pendingBills.FirstOrDefault(p => p.InvoiceId == alloc.InvoiceId);
                if (pending == null)
                    throw new Exception($"Invoice {alloc.InvoiceNo} is not a pending bill for this customer.");

                // When updating, add back this payment's own allocation to available pending
                decimal availablePending = pending.PendingAmount;
                if (excludePaymentId.HasValue)
                {
                    var ownAlloc = await _context.IncomingPaymentAllocations
                        .Where(a => a.PaymentId == excludePaymentId.Value && a.InvoiceId == alloc.InvoiceId)
                        .SumAsync(a => (decimal?)a.AmountApplied) ?? 0;
                    availablePending += ownAlloc;
                }

                if (alloc.AmountApplied > availablePending + 0.01m)
                    throw new Exception(
                        $"Invoice {alloc.InvoiceNo}: applying {alloc.AmountApplied:N2} " +
                        $"but only {availablePending:N2} is pending.");
            }
        }

        // ════════════════════════════════════════════════════
        // PRIVATE HELPERS — Account Ledger
        // ════════════════════════════════════════════════════

        private async Task PostAccountLedgerAsync(IncomingPaymentMain main, bool isReversal)
        {
            var bp = main.BusinessPartner
                ?? await _context.BusinessPartners
                    .FirstOrDefaultAsync(x => x.BusinessPartnerId == main.BusinessPartnerId);
            if (bp == null) return;

            var lines = BuildLedgerLines(main, bp, isReversal);

            await _accountLedger.AddEntriesAsync(
                companyId: main.CompanyId,
                financialYearId: main.FinYearId,
                date: main.PaymentDate,
                voucherType: "Incoming Payment",
                voucherNo: main.PaymentNo,
                lines: lines,
                createdBy: main.CreatedBy);
        }

        private async Task UpdateAccountLedgerAsync(IncomingPaymentMain main)
        {
            var bp = main.BusinessPartner
                ?? await _context.BusinessPartners
                    .FirstOrDefaultAsync(x => x.BusinessPartnerId == main.BusinessPartnerId);
            if (bp == null) return;

            var lines = BuildLedgerLines(main, bp, isReversal: false);

            await _accountLedger.UpdateEntriesAsync(
                    companyId: main.CompanyId,
                    financialYearId: main.FinYearId,
                    date: main.PaymentDate,
                    voucherType: "Incoming Payment",
                    voucherNo: main.PaymentNo,
                    lines: lines,
                    modifiedBy: main.ModifiedBy);
        }

        /// <summary>
        /// Double-entry for a customer payment:
        ///   Dr  Deposit Account (Bank/Cash)        ← money received
        ///   Cr  Customer Receivable (BP Account)   ← reduces outstanding
        /// </summary>
        private static List<AccountLedgerLineDto> BuildLedgerLines(
            IncomingPaymentMain main, BusinessPartner bp, bool isReversal)
        {
            return new List<AccountLedgerLineDto>
            {
                // Bank / Cash account — Dr on receipt, Cr on reversal
                new()
                {
                    AccountId         = main.DepositAccountId,
                    BusinessPartnerId = main.BusinessPartnerId,
                    Debit             = isReversal ? 0              : main.TotalAmount,
                    Credit            = isReversal ? main.TotalAmount : 0,
                    Remarks           = $"Incoming Payment: {main.PaymentNo}"
                },
                // Customer receivable — Cr on receipt (reduces AR), Dr on reversal
                new()
                {
                    AccountId         = bp.AccountId,
                    BusinessPartnerId = main.BusinessPartnerId,
                    Debit             = isReversal ? main.TotalAmount : 0,
                    Credit            = isReversal ? 0              : main.TotalAmount,
                    Remarks           = $"Incoming Payment: {main.PaymentNo}"
                }
            };
        }

        // ════════════════════════════════════════════════════
        // PRIVATE HELPERS — Misc
        // ════════════════════════════════════════════════════

        private async Task<IncomingPaymentMain?> LoadMainAsync(int paymentId, int companyId) =>
            await _context.IncomingPaymentMains
                .Include(x => x.BusinessPartner)
                .Include(x => x.DepositAccount)
                .Include(x => x.Allocations!)
                    .ThenInclude(a => a.Invoice)
                .FirstOrDefaultAsync(x =>
                    x.PaymentId == paymentId &&
                    x.CompanyId == companyId &&
                    !x.IsDeleted);

        private async Task<string> GeneratePaymentNoAsync(int companyId, int finYearId)
        {
            var financialYear = await _context.FinancialYears
                .FirstOrDefaultAsync(x => x.FinancialYearId == finYearId);

            var yearLabel = financialYear != null
                ? $"{financialYear.StartDate.Year % 100}{financialYear.EndDate.Year % 100}"
                : finYearId.ToString();

            var count = await _context.IncomingPaymentMains
                .CountAsync(x =>
                    x.CompanyId == companyId &&
                    x.FinYearId == finYearId &&
                    !x.IsDeleted);

            return $"RCP-{yearLabel}-{(count + 1):D4}";
        }

        private IncomingPaymentResponseDto MapToResponseDto(IncomingPaymentMain main) =>
            new()
            {
                PaymentId = main.PaymentId,
                PaymentNo = main.PaymentNo,
                FinYearId = main.FinYearId,
                PaymentDate = main.PaymentDate,
                Status = main.Status,
                BusinessPartnerId = main.BusinessPartnerId,
                BusinessPartnerName = main.BusinessPartner?.BusinessPartnerName ?? string.Empty,
                BusinessPartnerCode = main.BusinessPartner?.BusinessPartnerCode ?? string.Empty,
                DepositAccountId = main.DepositAccountId,
                DepositAccountName = main.DepositAccount?.AccountName ?? string.Empty,
                PaymentMode = main.PaymentMode,
                ChequeNo = main.ChequeNo,
                ChequeDate = main.ChequeDate,
                BankName = main.BankName,
                TransactionRef = main.TransactionRef,
                Remarks = main.Remarks,
                TotalAmount = main.TotalAmount,
                AllocatedAmount = main.AllocatedAmount,
                OnAccountAmount = main.OnAccountAmount,
                CreatedBy = main.CreatedBy ?? 0,
                CreatedDate = main.CreatedDate,
                ModifiedBy = main.ModifiedBy,
                ModifiedDate = main.ModifiedDate,
                Allocations = main.Allocations?.Select(a => new IncomingPaymentAllocationResponseDto
                {
                    AllocationId = a.AllocationId,
                    PaymentId = a.PaymentId,
                    InvoiceId = a.InvoiceId,
                    InvoiceNo = a.Invoice?.InvoiceNo ?? string.Empty,
                    InvoiceDate = a.Invoice?.InvoiceDate ?? DateTime.MinValue,
                    InvoiceTotal = a.Invoice?.NetTotal ?? 0,
                    AmountApplied = a.AmountApplied
                }).ToList() ?? new()
            };

        private async Task SaveChangesAsync()
        {
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                var inner = ex.InnerException?.Message ?? ex.Message;
                throw new Exception($"Database error: {inner}");
            }
        }
    }
}