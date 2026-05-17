using FinVentoryAPI.Data;
using FinVentoryAPI.DTOs.AccountLedgerPostingDTOs;
using FinVentoryAPI.DTOs.CashBankEntryDTOs;
using FinVentoryAPI.DTOs.PagedRequestDto;
using FinVentoryAPI.Entities;
using FinVentoryAPI.Enums;
using FinVentoryAPI.Helpers;
using FinVentoryAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace FinVentoryAPI.Services.Implementations
{
    public class CashBankEntryService : ICashBankEntryService
    {
        private readonly AppDbContext _context;
        private readonly Common _common;
        private readonly IAccountLedgerPostingService _accountLedger;

        public CashBankEntryService(
            AppDbContext context,
            Common common,
            IAccountLedgerPostingService accountLedger)
        {
            _context = context;
            _common = common;
            _accountLedger = accountLedger;
        }

        // ════════════════════════════════════════════════════
        // CREATE
        // ════════════════════════════════════════════════════
        public async Task<CashBankEntryResponseDto> CreateAsync(CreateCashBankEntryDto dto)
        {
            var companyId = _common.GetCompanyId();
            var userId = _common.GetUserId();
            var finYearId = _common.GetFinancialYearId();

            await ValidateHeaderAsync(dto.HeadAccountId, dto.ReferenceNo, companyId);
            ValidateLines(dto.Lines);

            var entryNumber = await GenerateEntryNumberAsync(dto.BookType, dto.EntryType, companyId, finYearId);

            var entry = new CashBankEntry
            {
                CompanyId = companyId,
                FinYearId = finYearId,
                EntryNumber = entryNumber,
                EntryDate = dto.EntryDate,
                EntryType = dto.EntryType,
                HeadAccountId = dto.HeadAccountId,
                AccountDrCr = dto.EntryType == EntryType.Receipt ? BalanceType.Dr : BalanceType.Cr,
                TotalAmount = dto.Lines.Sum(l => l.Amount),
                PaymentMode = dto.PaymentMode,
                ReferenceNo = dto.ReferenceNo,
                ReferenceDate = dto.ReferenceDate,
                Narration = dto.Narration,
                Status = "Draft",
                CreatedBy = userId,
                CreatedDate = DateTime.UtcNow,
                Lines = dto.Lines.Select((l, i) => new CashBankEntryLine
                {
                    AccountId = l.AccountId,
                    DrCr = dto.EntryType == EntryType.Receipt ? BalanceType.Cr : BalanceType.Dr,
                    Amount = l.Amount,
                    Narration = l.Narration,
                    SortOrder = l.SortOrder == 0 ? i + 1 : l.SortOrder
                }).ToList()
            };

            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _context.CashBankEntries.Add(entry);
                await SaveChangesAsync();

                // ── Re-fetch with navigation property to get BookType ─
                var savedEntry = await _context.CashBankEntries
                    .Include(e => e.Lines)
                    .Include(e => e.CashBankAccount)
                    .AsNoTracking()
                    .FirstAsync(e => e.CashBankEntryId == entry.CashBankEntryId);

                var entryDate = savedEntry.EntryDate.ToDateTime(TimeOnly.MinValue);
                var lines = BuildAccountLedgerLines(savedEntry);

                await _accountLedger.AddEntriesAsync(
                    companyId: companyId,
                    financialYearId: finYearId,
                    date: entryDate,
                    voucherType: GetVoucherType(savedEntry.CashBankAccount!.BookType), // ✅ via navigation
                    voucherNo: savedEntry.EntryNumber,
                    lines: lines,
                    createdBy: userId);

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }

            return await GetByIdAsync(entry.CashBankEntryId)
                ?? throw new Exception("Failed to retrieve saved entry.");
        }

        // ════════════════════════════════════════════════════
        // UPDATE  (Draft only)
        // ════════════════════════════════════════════════════
        public async Task<bool> UpdateAsync(int id, UpdateCashBankEntryDto dto)
        {
            var companyId = _common.GetCompanyId();
            var userId = _common.GetUserId();
            var finYearId = _common.GetFinancialYearId();

            var entry = await _context.CashBankEntries
                .Include(e => e.Lines)
                .FirstOrDefaultAsync(x =>
                    x.CashBankEntryId == id &&
                    x.CompanyId == companyId &&
                    !x.IsDeleted);

            if (entry == null) return false;

            if (entry.Status != "Draft")
                throw new Exception("Only Draft entries can be updated.");

            await ValidateHeaderAsync(dto.HeadAccountId, dto.ReferenceNo, companyId);
            ValidateLines(dto.Lines);

            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                entry.EntryDate = dto.EntryDate;
                entry.EntryType = dto.EntryType;
                entry.HeadAccountId = dto.HeadAccountId;
                entry.AccountDrCr = dto.EntryType == EntryType.Receipt ? BalanceType.Dr : BalanceType.Cr;
                entry.TotalAmount = dto.Lines.Sum(l => l.Amount);
                entry.PaymentMode = dto.PaymentMode;
                entry.ReferenceNo = dto.ReferenceNo;
                entry.ReferenceDate = dto.ReferenceDate;
                entry.Narration = dto.Narration;
                entry.ModifiedBy = userId;
                entry.ModifiedDate = DateTime.UtcNow;

                var existingLines = entry.Lines.ToList();
                var contraDrCr = dto.EntryType == EntryType.Receipt ? BalanceType.Cr : BalanceType.Dr;

                for (int i = 0; i < dto.Lines.Count; i++)
                {
                    var incomingLine = dto.Lines[i];
                    if (i < existingLines.Count)
                    {
                        var existing = existingLines[i];
                        existing.AccountId = incomingLine.AccountId;
                        existing.DrCr = contraDrCr;
                        existing.Amount = incomingLine.Amount;
                        existing.Narration = incomingLine.Narration;
                        existing.SortOrder = incomingLine.SortOrder == 0 ? i + 1 : incomingLine.SortOrder;
                    }
                    else
                    {
                        entry.Lines.Add(new CashBankEntryLine
                        {
                            CashBankEntryId = entry.CashBankEntryId,
                            AccountId = incomingLine.AccountId,
                            DrCr = contraDrCr,
                            Amount = incomingLine.Amount,
                            Narration = incomingLine.Narration,
                            SortOrder = incomingLine.SortOrder == 0 ? i + 1 : incomingLine.SortOrder
                        });
                    }
                }

                if (existingLines.Count > dto.Lines.Count)
                    _context.CashBankEntryLines.RemoveRange(existingLines.Skip(dto.Lines.Count));

                await SaveChangesAsync();

                // ── Re-fetch with CashBankAccount to access BookType ──
                var updatedEntry = await _context.CashBankEntries
                    .Include(e => e.Lines)
                    .Include(e => e.CashBankAccount)
                    .AsNoTracking()
                    .FirstAsync(e => e.CashBankEntryId == entry.CashBankEntryId);

                var entryDate = updatedEntry.EntryDate.ToDateTime(TimeOnly.MinValue);
                var lines = BuildAccountLedgerLines(updatedEntry);

                await _accountLedger.UpdateEntriesAsync(
                    companyId: companyId,
                    financialYearId: finYearId,
                    date: entryDate,
                    voucherType: GetVoucherType(updatedEntry.CashBankAccount!.BookType), // ✅ via navigation
                    voucherNo: updatedEntry.EntryNumber,
                    lines: lines,
                    modifiedBy: userId);

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }

            return true;
        }

        // ════════════════════════════════════════════════════
        // DELETE  (Draft only — soft delete)
        // ════════════════════════════════════════════════════
        public async Task<bool> DeleteAsync(int id)
        {
            var companyId = _common.GetCompanyId();
            var userId = _common.GetUserId();
            var finYearId = _common.GetFinancialYearId();

            var entry = await _context.CashBankEntries
                .FirstOrDefaultAsync(x =>
                    x.CashBankEntryId == id &&
                    x.CompanyId == companyId &&
                    !x.IsDeleted);

            if (entry == null) return false;

            if (entry.Status != "Draft")
                throw new Exception("Only Draft entries can be deleted.");

            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                entry.IsDeleted = true;
                entry.IsActive = false;
                entry.ModifiedBy = userId;
                entry.ModifiedDate = DateTime.UtcNow;

                await _accountLedger.SoftDeleteByVoucherAsync(
                    companyId: companyId,
                    financialYearId: finYearId,
                    voucherNo: entry.EntryNumber,
                    modifiedBy: userId);

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
        public async Task<CashBankEntryResponseDto?> GetByIdAsync(int id)
        {
            var companyId = _common.GetCompanyId();

            var entry = await _context.CashBankEntries
                .AsNoTracking()
                .AsSplitQuery()
                .Include(e => e.CashBankAccount)
                .Include(e => e.Lines)
                    .ThenInclude(l => l.Account)
                .FirstOrDefaultAsync(x =>
                    x.CashBankEntryId == id &&
                    x.CompanyId == companyId &&
                    !x.IsDeleted);

            if (entry == null) return null;
            return MapToResponseDto(entry);
        }

        // ════════════════════════════════════════════════════
        // GET ALL
        // ════════════════════════════════════════════════════
        public async Task<List<CashBankEntryResponseDto>> GetAllAsync()
        {
            var companyId = _common.GetCompanyId();

            var entries = await _context.CashBankEntries
                .Where(x => x.CompanyId == companyId && !x.IsDeleted)
                .Include(e => e.CashBankAccount)
                .Include(e => e.Lines)
                    .ThenInclude(l => l.Account)
                .OrderByDescending(x => x.EntryDate)
                .ThenByDescending(x => x.CashBankEntryId)
                .ToListAsync();

            return entries.Select(MapToResponseDto).ToList();
        }

        // ════════════════════════════════════════════════════
        // GET PAGED
        // ════════════════════════════════════════════════════
        public async Task<PagedResponseDto<CashBankEntryResponseDto>> GetPagedAsync(
            PagedRequestDto request)
        {
            var companyId = _common.GetCompanyId();

            var query = _context.CashBankEntries
                .Where(x => x.CompanyId == companyId && !x.IsDeleted)
                .Include(e => e.CashBankAccount)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var search = request.Search.ToLower();
                query = query.Where(x =>
                    x.EntryNumber.ToLower().Contains(search) ||
                    (x.Narration ?? "").ToLower().Contains(search) ||
                    (x.ReferenceNo ?? "").ToLower().Contains(search) ||
                    x.CashBankAccount.AccountName.ToLower().Contains(search));
            }

            if (request.Filters != null)
            {
                if (request.Filters.ContainsKey("bookType"))
                {
                    var bookType = ((JsonElement)request.Filters["bookType"]).GetInt32();
                    query = query.Where(x => (int)x.CashBankAccount.BookType == bookType);
                }

                if (request.Filters.ContainsKey("entryType"))
                {
                    var entryType = ((JsonElement)request.Filters["entryType"]).GetInt32();
                    query = query.Where(x => (int)x.EntryType == entryType);
                }

                if (request.Filters.ContainsKey("status"))
                {
                    var status = ((JsonElement)request.Filters["status"]).GetString();
                    query = query.Where(x => x.Status == status);
                }

                if (request.Filters.ContainsKey("headAccountId"))
                {
                    var headAccountId = ((JsonElement)request.Filters["headAccountId"]).GetInt32();
                    query = query.Where(x => x.HeadAccountId == headAccountId);
                }

                if (request.Filters.ContainsKey("finYearId"))
                {
                    var finYearId = ((JsonElement)request.Filters["finYearId"]).GetInt32();
                    query = query.Where(x => x.FinYearId == finYearId);
                }

                if (request.Filters.ContainsKey("fromDate"))
                {
                    var fromDate = DateOnly.Parse(
                        ((JsonElement)request.Filters["fromDate"]).GetString()!);
                    query = query.Where(x => x.EntryDate >= fromDate);
                }

                if (request.Filters.ContainsKey("toDate"))
                {
                    var toDate = DateOnly.Parse(
                        ((JsonElement)request.Filters["toDate"]).GetString()!);
                    query = query.Where(x => x.EntryDate <= toDate);
                }
            }

            if (request.Sorts != null && request.Sorts.Any())
            {
                var sort = request.Sorts.First();
                query = sort.Column.ToLower() switch
                {
                    "entrynumber" => sort.Direction == "desc" ? query.OrderByDescending(x => x.EntryNumber) : query.OrderBy(x => x.EntryNumber),
                    "entrydate" => sort.Direction == "desc" ? query.OrderByDescending(x => x.EntryDate) : query.OrderBy(x => x.EntryDate),
                    "entrytype" => sort.Direction == "desc" ? query.OrderByDescending(x => x.EntryType) : query.OrderBy(x => x.EntryType),
                    "totalamount" => sort.Direction == "desc" ? query.OrderByDescending(x => x.TotalAmount) : query.OrderBy(x => x.TotalAmount),
                    "status" => sort.Direction == "desc" ? query.OrderByDescending(x => x.Status) : query.OrderBy(x => x.Status),
                    "accountname" => sort.Direction == "desc" ? query.OrderByDescending(x => x.CashBankAccount.AccountName) : query.OrderBy(x => x.CashBankAccount.AccountName),
                    _ => query.OrderByDescending(x => x.EntryDate)
                };
            }
            else
            {
                query = query
                    .OrderByDescending(x => x.EntryDate)
                    .ThenByDescending(x => x.CashBankEntryId);
            }

            var totalRecords = await query.CountAsync();

            var data = await query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Include(e => e.Lines)
                    .ThenInclude(l => l.Account)
                .ToListAsync();

            return new PagedResponseDto<CashBankEntryResponseDto>
            {
                TotalRecords = totalRecords,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                Data = data.Select(MapToResponseDto).ToList()
            };
        }

        // ════════════════════════════════════════════════════
        // PRIVATE — Account Ledger lines builder
        // ════════════════════════════════════════════════════
        private List<AccountLedgerLineDto> BuildAccountLedgerLines(CashBankEntry entry)
        {
            var lines = new List<AccountLedgerLineDto>();
            var isReceipt = entry.EntryType == EntryType.Receipt;

            lines.Add(new AccountLedgerLineDto
            {
                AccountId = entry.HeadAccountId,
                BusinessPartnerId = null,
                Debit = isReceipt ? entry.TotalAmount : 0,
                Credit = isReceipt ? 0 : entry.TotalAmount,
                Remarks = entry.Narration
                    ?? $"{GetVoucherType(entry.CashBankAccount!.BookType)}: {entry.EntryNumber}" // ✅ via navigation
            });

            foreach (var line in entry.Lines ?? Enumerable.Empty<CashBankEntryLine>())
            {
                lines.Add(new AccountLedgerLineDto
                {
                    AccountId = line.AccountId,
                    BusinessPartnerId = null,
                    Debit = isReceipt ? 0 : line.Amount,
                    Credit = isReceipt ? line.Amount : 0,
                    Remarks = line.Narration
                        ?? entry.Narration
                        ?? $"{GetVoucherType(entry.CashBankAccount!.BookType)}: {entry.EntryNumber}" // ✅ via navigation
                });
            }

            return lines;
        }

        /// <summary>
        /// VoucherType label from BookType enum (non-nullable, used internally).
        /// </summary>
        private static string GetVoucherType(BookType bookType) =>
            bookType == BookType.CASH ? "Cash Entry" : "Bank Entry";

        /// <summary>
        /// VoucherType label from nullable BookType — covers navigation property access.
        /// </summary>
        private static string GetVoucherType(BookType? bookType) =>
            GetVoucherType(bookType ?? BookType.CASH);

        /// <summary>
        /// VoucherType label from int bookType (used in GenerateEntryNumber / CreateDto).
        /// </summary>
        private static string GetVoucherType(int bookType) =>
            bookType == 1 ? "Cash Entry" : "Bank Entry";

        // ════════════════════════════════════════════════════
        // PRIVATE — Validation
        // ════════════════════════════════════════════════════
        private async Task ValidateHeaderAsync(
            int headAccountId,
            string? referenceNo,
            int companyId)
        {
            var account = await _context.Accounts
                .FirstOrDefaultAsync(x =>
                    x.AccountId == headAccountId &&
                    x.CompanyId == companyId &&
                    x.IsActive &&
                    !x.IsDeleted);

            if (account == null)
                throw new Exception("Head account not found or inactive.");

            if (account.BookType != BookType.CASH && account.BookType != BookType.BANK)
                throw new Exception("Selected account is not a Cash or Bank account.");

            if (account.BookType == BookType.BANK && string.IsNullOrWhiteSpace(referenceNo))
                throw new Exception("Reference no (Cheque / UTR) is required for Bank entries.");
        }

        private static void ValidateLines(List<CreateCashBankEntryLineDto> lines) =>
            ValidateLinesCore(lines.Select(l => (l.AccountId, l.Amount)));

        private static void ValidateLines(List<UpdateCashBankEntryLineDto> lines) =>
            ValidateLinesCore(lines.Select(l => (l.AccountId, l.Amount)));

        private static void ValidateLinesCore(IEnumerable<(int AccountId, decimal Amount)> lines)
        {
            var list = lines?.ToList();

            if (list == null || !list.Any())
                throw new Exception("At least one contra account line is required.");

            foreach (var line in list)
            {
                if (line.AccountId <= 0)
                    throw new Exception("Account is required for each line.");
                if (line.Amount <= 0)
                    throw new Exception("Amount must be greater than 0 for each line.");
            }

            var duplicates = list
                .GroupBy(l => l.AccountId)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            if (duplicates.Any())
                throw new Exception(
                    $"Duplicate accounts in lines: AccountId(s) {string.Join(", ", duplicates)}.");
        }

        // ════════════════════════════════════════════════════
        // PRIVATE — Entry number generation
        // ════════════════════════════════════════════════════
        private async Task<string> GenerateEntryNumberAsync(
            int bookType, EntryType entryType, int companyId, int finYearId)
        {
            var prefix = (bookType, entryType) switch
            {
                (1, EntryType.Payment) => "CP",  // Cash Payment
                (1, EntryType.Receipt) => "CR",  // Cash Receipt
                (2, EntryType.Payment) => "BP",  // Bank Payment
                (2, EntryType.Receipt) => "BR",  // Bank Receipt
                _ => "CB"    // fallback
            };

            var financialYear = await _context.FinancialYears
                .FirstOrDefaultAsync(x => x.FinancialYearId == finYearId);

            var yearLabel = financialYear != null
                ? $"{financialYear.StartDate.Year % 100}{financialYear.EndDate.Year % 100}"
                : finYearId.ToString();

            var count = await _context.CashBankEntries
                .CountAsync(x =>
                    x.CompanyId == companyId &&
                    x.FinYearId == finYearId &&
                    !x.IsDeleted);

            return $"{prefix}{yearLabel}{(count + 1):D4}";
        }

        // ════════════════════════════════════════════════════
        // PRIVATE — Map to response DTO
        // ════════════════════════════════════════════════════
        private static CashBankEntryResponseDto MapToResponseDto(CashBankEntry entry)
        {
            return new CashBankEntryResponseDto
            {
                CashBankEntryId = entry.CashBankEntryId,
                CompanyId = entry.CompanyId,
                FinYearId = entry.FinYearId,
                EntryNumber = entry.EntryNumber,

                // ✅ BookType via navigation property; cast int? → int with fallback
                BookTypeId = (int)(entry.CashBankAccount?.BookType ?? BookType.CASH),
                BookType = entry.CashBankAccount?.BookType.ToString(),

                EntryDate = entry.EntryDate,
                EntryTypeId = (int)entry.EntryType,
                EntryType = entry.EntryType.ToString(),

                HeadAccountId = entry.HeadAccountId,
                HeadAccountName = entry.CashBankAccount?.AccountName ?? string.Empty,
                HeadAccountCode = entry.CashBankAccount?.AccountCode,

                AccountDrCr = entry.AccountDrCr.ToString(),
                TotalAmount = entry.TotalAmount,
                PaymentMode = entry.PaymentMode,
                ReferenceNo = entry.ReferenceNo,
                ReferenceDate = entry.ReferenceDate,
                Narration = entry.Narration,
                Status = entry.Status,

                IsActive = entry.IsActive,
                CreatedBy = entry.CreatedBy,
                CreatedDate = entry.CreatedDate,
                ModifiedBy = entry.ModifiedBy,
                ModifiedDate = entry.ModifiedDate,

                Lines = entry.Lines?
                    .OrderBy(l => l.SortOrder)
                    .Select(l => new CashBankEntryLineResponseDto
                    {
                        CashBankEntryLineId = l.CashBankEntryLineId,
                        CashBankEntryId = l.CashBankEntryId,
                        AccountId = l.AccountId,
                        AccountName = l.Account?.AccountName ?? string.Empty,
                        AccountCode = l.Account?.AccountCode,
                        AccountType = l.Account?.AccountType.ToString() ?? string.Empty,
                        DrCr = l.DrCr.ToString(),
                        Amount = l.Amount,
                        Narration = l.Narration,
                        SortOrder = l.SortOrder
                    }).ToList() ?? new()
            };
        }

        // ════════════════════════════════════════════════════
        // PRIVATE — SaveChanges wrapper
        // ════════════════════════════════════════════════════
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