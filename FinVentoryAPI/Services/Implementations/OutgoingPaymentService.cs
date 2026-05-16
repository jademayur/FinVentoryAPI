using FinVentoryAPI.Data;
using FinVentoryAPI.DTOs.AccountLedgerPostingDTOs;
using FinVentoryAPI.DTOs.OutgoingPaymentDTOs;
using FinVentoryAPI.DTOs.PagedRequestDto;
using FinVentoryAPI.Entities;
using FinVentoryAPI.Helpers;
using FinVentoryAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace FinVentoryAPI.Services.Implementations
{
    public class OutgoingPaymentService : IOutgoingPaymentService
    {
        private readonly AppDbContext _context;
        private readonly Common _common;
        private readonly IAccountLedgerPostingService _accountLedger;

        public OutgoingPaymentService(
            AppDbContext context,
            Common common,
            IAccountLedgerPostingService accountLedger)
        {
            _context = context;
            _common = common;
            _accountLedger = accountLedger;
        }

        // ════════════════════════════════════════════════════
        // GET PENDING SUPPLIER BILLS
        // ════════════════════════════════════════════════════
        public async Task<List<PendingSupplierBillDto>> GetPendingBillsAsync(int businessPartnerId)
        {
            var companyId = _common.GetCompanyId();

            var bills = await _context.PurchaseInvoiceMains
                .Where(x =>
                    x.CompanyId == companyId &&
                    x.BusinessPartnerId == businessPartnerId &&
                    !x.IsDeleted)
                .ToListAsync();

            if (!bills.Any())
                return new List<PendingSupplierBillDto>();

            // ── FIX: int PK = InvoiceId ───────────────────────────────────
            var billIds = bills.Select(b => b.InvoiceId).ToList();   // List<int> ✓

            var paidMap = await _context.OutgoingPaymentAllocations
                .Where(a =>
                    billIds.Contains(a.BillId) &&                    // int Contains int ✓
                    !a.Payment!.IsDeleted &&
                    a.Payment.Status != "Cancelled")
                .GroupBy(a => a.BillId)
                .Select(g => new
                {
                    BillId = g.Key,
                    Paid = g.Sum(a => a.AmountApplied)
                })
                .ToDictionaryAsync(x => x.BillId, x => x.Paid);

            var today = DateTime.UtcNow.Date;

            var result = bills
                .Select(bill =>
                {
                    // ── FIX: lookup by InvoiceId (int) ───────────────────
                    var paid = paidMap.TryGetValue(bill.InvoiceId, out var p) ? p : 0m;
                    var pending = bill.NetTotal - paid;

                    var dueDate = bill.DueDate == default(DateTime)
                        ? (DateTime?)null
                        : bill.DueDate;

                    var daysOverdue = dueDate.HasValue && dueDate.Value.Date < today
                        ? (today - dueDate.Value.Date).Days
                        : 0;

                    return new PendingSupplierBillDto
                    {
                        BillId = bill.InvoiceId,          // int PK ✓
                        BillNo = bill.InvoiceNo,          // internal ref ✓
                        SupplierInvoiceNo = bill.SupplierInvoiceNo,  // supplier's ref ✓
                        BillDate = bill.InvoiceDate,        // use InvoiceDate ✓
                        DueDate = dueDate,
                        BillTotal = bill.NetTotal,
                        PaidAmount = paid,
                        PendingAmount = pending,
                        DaysOverdue = daysOverdue
                    };
                })
                .Where(x => x.PendingAmount > 0)
                .OrderBy(x => x.DueDate ?? x.BillDate)
                .ToList();

            return result;
        }

        // ════════════════════════════════════════════════════
        // CREATE
        // ════════════════════════════════════════════════════
        public async Task<OutgoingPaymentResponseDto> CreateAsync(CreateOutgoingPaymentDto dto)
        {
            var companyId = _common.GetCompanyId();
            var userId = _common.GetUserId();
            var finYearId = _common.GetFinancialYearId();

            // ── Validate BEFORE transaction ───────────────────
            await ValidatePaymentAsync(dto.BusinessPartnerId, dto.PaymentAccountId, companyId);
            ValidateAmounts(dto.TotalAmount, dto.OnAccountAmount, dto.Allocations);

            var pendingBills = await GetPendingBillsAsync(dto.BusinessPartnerId);
            await ValidateAllocationsAsync(dto.Allocations, pendingBills, companyId);

            var paymentNo = await GeneratePaymentNoAsync(companyId, finYearId);

            var main = new OutgoingPaymentMain
            {
                CompanyId = companyId,
                FinYearId = finYearId,
                PaymentNo = paymentNo,
                PaymentDate = dto.PaymentDate,
                Status = "Draft",
                BusinessPartnerId = dto.BusinessPartnerId,
                PaymentAccountId = dto.PaymentAccountId,
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
                Allocations = new List<OutgoingPaymentAllocation>()
            };

            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _context.OutgoingPaymentMains.Add(main);
                await SaveChangesAsync(); // get PaymentId

                foreach (var alloc in dto.Allocations)
                {
                    _context.OutgoingPaymentAllocations.Add(new OutgoingPaymentAllocation
                    {
                        PaymentId = main.PaymentId,
                        BillId = alloc.BillId,
                        AmountApplied = alloc.AmountApplied
                    });
                }

                await SaveChangesAsync(); // persist allocations

                // Re-fetch with navigation for ledger posting
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
        public async Task<bool> UpdateAsync(int id, UpdateOutgoingPaymentDto dto)
        {
            var companyId = _common.GetCompanyId();
            var userId = _common.GetUserId();

            var main = await LoadMainAsync(id, companyId);
            if (main == null) return false;
            if (main.Status != "Draft")
                throw new Exception("Only Draft payments can be updated.");

            await ValidatePaymentAsync(dto.BusinessPartnerId, dto.PaymentAccountId, companyId);
            ValidateAmounts(dto.TotalAmount, dto.OnAccountAmount, dto.Allocations);

            var pendingBills = await GetPendingBillsAsync(dto.BusinessPartnerId);

            // Exclude this payment's own allocations so we don't double-count
            await ValidateAllocationsAsync(
                dto.Allocations, pendingBills, companyId,
                excludePaymentId: id);

            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // ── Update header ─────────────────────────────
                main.PaymentDate = dto.PaymentDate;
                main.BusinessPartnerId = dto.BusinessPartnerId;
                main.PaymentAccountId = dto.PaymentAccountId;
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
                    _context.OutgoingPaymentAllocations.RemoveRange(main.Allocations);

                foreach (var alloc in dto.Allocations)
                {
                    _context.OutgoingPaymentAllocations.Add(new OutgoingPaymentAllocation
                    {
                        PaymentId = main.PaymentId,
                        BillId = alloc.BillId,
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
        // DELETE (soft)
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
        public async Task<OutgoingPaymentResponseDto?> GetByIdAsync(int id)
        {
            var companyId = _common.GetCompanyId();

            var main = await _context.OutgoingPaymentMains
                .AsNoTracking()
                .Include(x => x.BusinessPartner)
                .Include(x => x.PaymentAccount)
                .Include(x => x.Allocations!)
                    .ThenInclude(a => a.Bill)
                .FirstOrDefaultAsync(x =>
                    x.PaymentId == id &&
                    x.CompanyId == companyId &&
                    !x.IsDeleted);

            return main == null ? null : MapToResponseDto(main);
        }

        // ════════════════════════════════════════════════════
        // GET ALL
        // ════════════════════════════════════════════════════
        public async Task<List<OutgoingPaymentResponseDto>> GetAllAsync()
        {
            var companyId = _common.GetCompanyId();

            var list = await _context.OutgoingPaymentMains
                .Where(x => x.CompanyId == companyId && !x.IsDeleted)
                .Include(x => x.BusinessPartner)
                .Include(x => x.PaymentAccount)
                .Include(x => x.Allocations!)
                    .ThenInclude(a => a.Bill)
                .OrderByDescending(x => x.PaymentDate)
                .ToListAsync();

            return list.Select(MapToResponseDto).ToList();
        }

        // ════════════════════════════════════════════════════
        // GET PAGED
        // ════════════════════════════════════════════════════
        public async Task<PagedResponseDto<OutgoingPaymentResponseDto>> GetPagedAsync(
            PagedRequestDto request)
        {
            var companyId = _common.GetCompanyId();

            var query = _context.OutgoingPaymentMains
                .Where(x => x.CompanyId == companyId && !x.IsDeleted)
                .Include(x => x.BusinessPartner)
                .Include(x => x.PaymentAccount)
                .AsQueryable();

            // ── Search ────────────────────────────────────────
            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var s = request.Search.ToLower();
                query = query.Where(x =>
                    x.PaymentNo.ToLower().Contains(s) ||
                    x.BusinessPartner!.BusinessPartnerName.ToLower().Contains(s));
            }

            // ── Filters ───────────────────────────────────────
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

            // ── Sorting ───────────────────────────────────────
            if (request.Sorts != null && request.Sorts.Any())
            {
                var sort = request.Sorts.First();
                query = sort.Column.ToLower() switch
                {
                    "paymentno" => sort.Direction == "desc"
                        ? query.OrderByDescending(x => x.PaymentNo)
                        : query.OrderBy(x => x.PaymentNo),
                    "paymentdate" => sort.Direction == "desc"
                        ? query.OrderByDescending(x => x.PaymentDate)
                        : query.OrderBy(x => x.PaymentDate),
                    "businesspartnername" => sort.Direction == "desc"
                        ? query.OrderByDescending(x => x.BusinessPartner!.BusinessPartnerName)
                        : query.OrderBy(x => x.BusinessPartner!.BusinessPartnerName),
                    "totalamount" => sort.Direction == "desc"
                        ? query.OrderByDescending(x => x.TotalAmount)
                        : query.OrderBy(x => x.TotalAmount),
                    "status" => sort.Direction == "desc"
                        ? query.OrderByDescending(x => x.Status)
                        : query.OrderBy(x => x.Status),
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
                    .ThenInclude(a => a.Bill)
                .ToListAsync();

            return new PagedResponseDto<OutgoingPaymentResponseDto>
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

        private async Task ValidatePaymentAsync(
            int businessPartnerId, int paymentAccountId, int companyId)
        {
            var bpExists = await _context.BusinessPartners
                .AnyAsync(x =>
                    x.BusinessPartnerId == businessPartnerId &&
                    x.CompanyId == companyId &&
                    !x.IsDeleted);
            if (!bpExists)
                throw new Exception("Business Partner not found.");

            var accountExists = await _context.Accounts
                .AnyAsync(x =>
                    x.AccountId == paymentAccountId &&
                    x.CompanyId == companyId &&
                    !x.IsDeleted);
            if (!accountExists)
                throw new Exception("Payment Account not found.");
        }

        private static void ValidateAmounts(
            decimal totalAmount,
            decimal onAccountAmount,
            List<OutgoingPaymentAllocationDto> allocations)
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
                    throw new Exception(
                        $"Allocation for bill {a.BillNo} must be greater than zero.");
        }

        private async Task ValidateAllocationsAsync(
            List<OutgoingPaymentAllocationDto> allocations,
            List<PendingSupplierBillDto> pendingBills,
            int companyId,
            int? excludePaymentId = null)
        {
            if (!allocations.Any()) return;

            // Check for duplicate bills in the same request
            var dupes = allocations
                .GroupBy(a => a.BillId)
                .Where(g => g.Count() > 1)
                .ToList();

            if (dupes.Any())
                throw new Exception(
                    $"Duplicate bill in allocations: " +
                    $"{string.Join(", ", dupes.Select(g => g.Key))}");

            foreach (var alloc in allocations)
            {
                var pending = pendingBills
                    .FirstOrDefault(p => p.BillId == alloc.BillId);

                if (pending == null)
                    throw new Exception(
                        $"Bill {alloc.BillNo} is not a pending bill for this supplier.");

                // When updating, add back this payment's own allocation
                // so we don't reject a legitimate edit
                decimal availablePending = pending.PendingAmount;
                if (excludePaymentId.HasValue)
                {
                    var ownAlloc = await _context.OutgoingPaymentAllocations
                        .Where(a =>
                            a.PaymentId == excludePaymentId.Value &&
                            a.BillId == alloc.BillId)
                        .SumAsync(a => (decimal?)a.AmountApplied) ?? 0;

                    availablePending += ownAlloc;
                }

                if (alloc.AmountApplied > availablePending + 0.01m)
                    throw new Exception(
                        $"Bill {alloc.BillNo}: applying {alloc.AmountApplied:N2} " +
                        $"but only {availablePending:N2} is pending.");
            }
        }

        // ════════════════════════════════════════════════════
        // PRIVATE HELPERS — Account Ledger
        // ════════════════════════════════════════════════════

        private async Task PostAccountLedgerAsync(OutgoingPaymentMain main, bool isReversal)
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
                voucherType: "Outgoing Payment",
                voucherNo: main.PaymentNo,
                lines: lines,
                createdBy: main.CreatedBy);
        }

        private async Task UpdateAccountLedgerAsync(OutgoingPaymentMain main)
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
                voucherType: "Outgoing Payment",
                voucherNo: main.PaymentNo,
                lines: lines,
                modifiedBy: main.ModifiedBy);
        }

        /// <summary>
        /// Double-entry for a supplier payment:
        ///   Dr  Supplier Payable (BP Account)   ← reduces outstanding AP
        ///   Cr  Payment Account (Bank/Cash)      ← money going out
        /// This is the OPPOSITE of incoming payment.
        /// </summary>
        private static List<AccountLedgerLineDto> BuildLedgerLines(
            OutgoingPaymentMain main, BusinessPartner bp, bool isReversal)
        {
            return new List<AccountLedgerLineDto>
            {
                // Supplier payable — Dr on payment (reduces AP), Cr on reversal
                new()
                {
                    AccountId         = bp.AccountId,
                    BusinessPartnerId = main.BusinessPartnerId,
                    Debit             = isReversal ? 0               : main.TotalAmount,
                    Credit            = isReversal ? main.TotalAmount : 0,
                    Remarks           = $"Outgoing Payment: {main.PaymentNo}"
                },
                // Bank / Cash account — Cr on payment (money out), Dr on reversal
                new()
                {
                    AccountId         = main.PaymentAccountId,
                    BusinessPartnerId = main.BusinessPartnerId,
                    Debit             = isReversal ? main.TotalAmount : 0,
                    Credit            = isReversal ? 0               : main.TotalAmount,
                    Remarks           = $"Outgoing Payment: {main.PaymentNo}"
                }
            };
        }

        // ════════════════════════════════════════════════════
        // PRIVATE HELPERS — Misc
        // ════════════════════════════════════════════════════

        private async Task<OutgoingPaymentMain?> LoadMainAsync(int paymentId, int companyId) =>
            await _context.OutgoingPaymentMains
                .Include(x => x.BusinessPartner)
                .Include(x => x.PaymentAccount)
                .Include(x => x.Allocations!)
                    .ThenInclude(a => a.Bill)
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

            var count = await _context.OutgoingPaymentMains
                .CountAsync(x =>
                    x.CompanyId == companyId &&
                    x.FinYearId == finYearId &&
                    !x.IsDeleted);

            // PAY = outgoing payment voucher prefix
            return $"PAY-{yearLabel}-{(count + 1):D4}";
        }

        private OutgoingPaymentResponseDto MapToResponseDto(OutgoingPaymentMain main) =>
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
                PaymentAccountId = main.PaymentAccountId,
                PaymentAccountName = main.PaymentAccount?.AccountName ?? string.Empty,
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

                Allocations = main.Allocations?.Select(a => new OutgoingPaymentAllocationResponseDto
                {
                    AllocationId = a.AllocationId,
                    PaymentId = a.PaymentId,
                    BillId = a.BillId,                              // int ✓
                    BillNo = a.Bill?.InvoiceNo ?? string.Empty,    // InvoiceNo ✓
                    BillDate = a.Bill?.InvoiceDate ?? DateTime.MinValue,
                    BillTotal = a.Bill?.NetTotal ?? 0,
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
