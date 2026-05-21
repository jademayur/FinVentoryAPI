using FinVentoryAPI.Data;
using FinVentoryAPI.DTOs.AccountLedgerPostingDTOs;
using FinVentoryAPI.DTOs.JournalEntryDTOs;
using FinVentoryAPI.DTOs.PagedRequestDto;
using FinVentoryAPI.Entities;
using FinVentoryAPI.Helpers;
using FinVentoryAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace FinVentoryAPI.Services.Implementations
{
    public class JournalEntryService : IJournalEntryService
    {
        private readonly AppDbContext _context;
        private readonly Common _common;
        private readonly IAccountLedgerPostingService _accountLedger;

        public JournalEntryService(
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
        public async Task<JournalEntryResponseDto> CreateAsync(CreateJournalEntryDto dto)
        {
            var companyId = _common.GetCompanyId();
            var userId = _common.GetUserId();
            var finYearId = _common.GetFinancialYearId();

            await ValidateHeaderAccountAsync(dto.AccountId, companyId);
            ValidateLines(dto.Lines);

            var entryNo = !string.IsNullOrWhiteSpace(dto.EntryNo)
                ? dto.EntryNo
                : await GenerateEntryNumberAsync(companyId, finYearId);

            var entry = new JournalEntry
            {
                CompanyId = companyId,
                FinYearId = finYearId,
                AccountId = dto.AccountId,
                EntryNo = entryNo,
                EntryDate = dto.EntryDate.ToDateTime(TimeOnly.MinValue),
                TotalDebit = dto.Lines.Sum(l => l.Debit),
                TotalCredit = dto.Lines.Sum(l => l.Credit),
                Status = "Draft",
                CreatedBy = userId,
                CreatedDate = DateTime.UtcNow,
                Lines = dto.Lines.Select(l => new JournalEntryLine
                {
                    AccountId = l.AccountId,
                    AccountCode = null,
                    Debit = l.Debit,
                    Credit = l.Credit,
                    Narration = l.Narration
                }).ToList()
            };

            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _context.JournalEntries.Add(entry);
                await SaveChangesAsync();

                var saved = await _context.JournalEntries
                    .Include(e => e.Lines)
                        .ThenInclude(l => l.Account)
                    .Include(e => e.Account)
                    .AsNoTracking()
                    .FirstAsync(e => e.JournalEntryId == entry.JournalEntryId);

                // Denormalise AccountCode on lines
                foreach (var line in entry.Lines)
                {
                    var account = saved.Lines
                        .FirstOrDefault(l => l.AccountId == line.AccountId)?.Account;
                    line.AccountCode = account?.AccountCode;
                }

                var ledgerLines = BuildAccountLedgerLines(saved);

                await _accountLedger.AddEntriesAsync(
                    companyId: companyId,
                    financialYearId: finYearId,
                    date: saved.EntryDate,
                    voucherType: "Journal",
                    voucherNo: saved.EntryNo!,
                    lines: ledgerLines,
                    createdBy: userId);

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }

            return await GetByIdAsync(entry.JournalEntryId)
                ?? throw new Exception("Failed to retrieve saved journal entry.");
        }

        // ════════════════════════════════════════════════════
        // UPDATE  (Draft only)
        // ════════════════════════════════════════════════════
        public async Task<bool> UpdateAsync(int id, UpdateJournalEntryDto dto)
        {
            var companyId = _common.GetCompanyId();
            var userId = _common.GetUserId();
            var finYearId = _common.GetFinancialYearId();

            var entry = await _context.JournalEntries
                .Include(e => e.Lines)
                .FirstOrDefaultAsync(x =>
                    x.JournalEntryId == id &&
                    x.CompanyId == companyId &&
                    !x.IsDeleted);

            if (entry == null) return false;

            if (entry.Status != "Draft")
                throw new Exception("Only Draft entries can be updated.");

            await ValidateHeaderAccountAsync(dto.AccountId, companyId);
            ValidateLines(dto.Lines);

            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                entry.AccountId = dto.AccountId;
                entry.EntryDate = dto.EntryDate.ToDateTime(TimeOnly.MinValue);
                entry.EntryNo = dto.EntryNo ?? entry.EntryNo;
                entry.TotalDebit = dto.Lines.Sum(l => l.Debit);
                entry.TotalCredit = dto.Lines.Sum(l => l.Credit);
                entry.ModifiedBy = userId;
                entry.ModifiedDate = DateTime.UtcNow;

                var existingLines = entry.Lines.ToList();

                for (int i = 0; i < dto.Lines.Count; i++)
                {
                    var incoming = dto.Lines[i];
                    if (i < existingLines.Count)
                    {
                        var existing = existingLines[i];
                        existing.AccountId = incoming.AccountId;
                        existing.Debit = incoming.Debit;
                        existing.Credit = incoming.Credit;
                        existing.Narration = incoming.Narration;
                    }
                    else
                    {
                        entry.Lines.Add(new JournalEntryLine
                        {
                            JournalEntryId = entry.JournalEntryId,
                            AccountId = incoming.AccountId,
                            Debit = incoming.Debit,
                            Credit = incoming.Credit,
                            Narration = incoming.Narration
                        });
                    }
                }

                if (existingLines.Count > dto.Lines.Count)
                    _context.JournalEntryLines.RemoveRange(existingLines.Skip(dto.Lines.Count));

                await SaveChangesAsync();

                var updated = await _context.JournalEntries
                    .Include(e => e.Lines)
                        .ThenInclude(l => l.Account)
                    .Include(e => e.Account)
                    .AsNoTracking()
                    .FirstAsync(e => e.JournalEntryId == entry.JournalEntryId);

                var ledgerLines = BuildAccountLedgerLines(updated);

                await _accountLedger.UpdateEntriesAsync(
                    companyId: companyId,
                    financialYearId: finYearId,
                    date: updated.EntryDate,
                    voucherType: "Journal",
                    voucherNo: updated.EntryNo!,
                    lines: ledgerLines,
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

            var entry = await _context.JournalEntries
                .FirstOrDefaultAsync(x =>
                    x.JournalEntryId == id &&
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
                    voucherNo: entry.EntryNo!,
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
        public async Task<JournalEntryResponseDto?> GetByIdAsync(int id)
        {
            var companyId = _common.GetCompanyId();

            var entry = await _context.JournalEntries
                .AsNoTracking()
                .AsSplitQuery()
                .Include(e => e.Account)
                .Include(e => e.Lines)
                    .ThenInclude(l => l.Account)
                .FirstOrDefaultAsync(x =>
                    x.JournalEntryId == id &&
                    x.CompanyId == companyId &&
                    !x.IsDeleted);

            return entry == null ? null : MapToResponseDto(entry);
        }

        // ════════════════════════════════════════════════════
        // GET ALL
        // ════════════════════════════════════════════════════
        public async Task<List<JournalEntryResponseDto>> GetAllAsync()
        {
            var companyId = _common.GetCompanyId();

            var entries = await _context.JournalEntries
                .Where(x => x.CompanyId == companyId && !x.IsDeleted)
                .Include(e => e.Account)
                .Include(e => e.Lines)
                    .ThenInclude(l => l.Account)
                .OrderByDescending(x => x.EntryDate)
                .ThenByDescending(x => x.JournalEntryId)
                .ToListAsync();

            return entries.Select(MapToResponseDto).ToList();
        }

        // ════════════════════════════════════════════════════
        // GET PAGED
        // ════════════════════════════════════════════════════
        public async Task<PagedResponseDto<JournalEntryResponseDto>> GetPagedAsync(
            PagedRequestDto request)
        {
            var companyId = _common.GetCompanyId();

            var query = _context.JournalEntries
                .Where(x => x.CompanyId == companyId && !x.IsDeleted)
                .Include(e => e.Account)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var search = request.Search.ToLower();
                query = query.Where(x =>
                    (x.EntryNo ?? "").ToLower().Contains(search) ||
                    x.Account!.AccountName.ToLower().Contains(search));
            }

            if (request.Filters != null)
            {
                if (request.Filters.ContainsKey("finYearId"))
                {
                    var finYearId = ((JsonElement)request.Filters["finYearId"]).GetInt32();
                    query = query.Where(x => x.FinYearId == finYearId);
                }

                if (request.Filters.ContainsKey("accountId"))
                {
                    var accountId = ((JsonElement)request.Filters["accountId"]).GetInt32();
                    query = query.Where(x => x.AccountId == accountId);
                }

                if (request.Filters.ContainsKey("status"))
                {
                    var status = ((JsonElement)request.Filters["status"]).GetString();
                    query = query.Where(x => x.Status == status);
                }

                if (request.Filters.ContainsKey("fromDate"))
                {
                    var fromDate = DateTime.Parse(
                        ((JsonElement)request.Filters["fromDate"]).GetString()!);
                    query = query.Where(x => x.EntryDate >= fromDate);
                }

                if (request.Filters.ContainsKey("toDate"))
                {
                    var toDate = DateTime.Parse(
                        ((JsonElement)request.Filters["toDate"]).GetString()!);
                    query = query.Where(x => x.EntryDate <= toDate);
                }
            }

            if (request.Sorts != null && request.Sorts.Any())
            {
                var sort = request.Sorts.First();
                query = sort.Column.ToLower() switch
                {
                    "entryno" => sort.Direction == "desc" ? query.OrderByDescending(x => x.EntryNo) : query.OrderBy(x => x.EntryNo),
                    "entrydate" => sort.Direction == "desc" ? query.OrderByDescending(x => x.EntryDate) : query.OrderBy(x => x.EntryDate),
                    "totaldebit" => sort.Direction == "desc" ? query.OrderByDescending(x => x.TotalDebit) : query.OrderBy(x => x.TotalDebit),
                    "totalcredit" => sort.Direction == "desc" ? query.OrderByDescending(x => x.TotalCredit) : query.OrderBy(x => x.TotalCredit),
                    "status" => sort.Direction == "desc" ? query.OrderByDescending(x => x.Status) : query.OrderBy(x => x.Status),
                    "accountname" => sort.Direction == "desc" ? query.OrderByDescending(x => x.Account!.AccountName) : query.OrderBy(x => x.Account!.AccountName),
                    _ => query.OrderByDescending(x => x.EntryDate).ThenByDescending(x => x.JournalEntryId)
                };
            }
            else
            {
                query = query
                    .OrderByDescending(x => x.EntryDate)
                    .ThenByDescending(x => x.JournalEntryId);
            }

            var totalRecords = await query.CountAsync();

            var data = await query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Include(e => e.Lines)
                    .ThenInclude(l => l.Account)
                .ToListAsync();

            return new PagedResponseDto<JournalEntryResponseDto>
            {
                TotalRecords = totalRecords,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                Data = data.Select(MapToResponseDto).ToList()
            };
        }

        // ════════════════════════════════════════════════════
        // PRIVATE — Account ledger lines builder
        // ════════════════════════════════════════════════════
        private static List<AccountLedgerLineDto> BuildAccountLedgerLines(JournalEntry entry)
        {
            return entry.Lines.Select(l => new AccountLedgerLineDto
            {
                AccountId = l.AccountId,
                BusinessPartnerId = null,
                Debit = l.Debit,
                Credit = l.Credit,
                Remarks = l.Narration ??  $"Journal: {entry.EntryNo}"
            }).ToList();
        }

        // ════════════════════════════════════════════════════
        // PRIVATE — Validation
        // ════════════════════════════════════════════════════
        private async Task ValidateHeaderAccountAsync(int accountId, int companyId)
        {
            var account = await _context.Accounts
                .FirstOrDefaultAsync(x =>
                    x.AccountId == accountId &&
                    x.CompanyId == companyId &&
                    x.IsActive &&
                    !x.IsDeleted);

            if (account == null)
                throw new Exception("Selected account not found or inactive.");
        }

        private static void ValidateLines(List<CreateJournalEntryLineDto> lines) =>
            ValidateLinesCore(lines.Select(l => (l.AccountId, l.Debit, l.Credit)));

        private static void ValidateLines(List<UpdateJournalEntryLineDto> lines) =>
            ValidateLinesCore(lines.Select(l => (l.AccountId, l.Debit, l.Credit)));

        private static void ValidateLinesCore(
            IEnumerable<(int AccountId, decimal Debit, decimal Credit)> lines)
        {
            var list = lines?.ToList();

            if (list == null || !list.Any())
                throw new Exception("At least one journal line is required.");

            foreach (var line in list)
            {
                if (line.AccountId <= 0)
                    throw new Exception("Account is required for each line.");

                if (line.Debit < 0 || line.Credit < 0)
                    throw new Exception("Debit and Credit must be 0 or greater.");

                if (line.Debit == 0 && line.Credit == 0)
                    throw new Exception("Each line must have either a Debit or Credit amount.");

                if (line.Debit > 0 && line.Credit > 0)
                    throw new Exception("A line cannot have both Debit and Credit amounts.");
            }

            var totalDebit = list.Sum(l => l.Debit);
            var totalCredit = list.Sum(l => l.Credit);

            if (totalDebit != totalCredit)
                throw new Exception(
                    $"Journal entry is not balanced. Total Debit: {totalDebit}, Total Credit: {totalCredit}.");

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
        private async Task<string> GenerateEntryNumberAsync(int companyId, int finYearId)
        {
            var financialYear = await _context.FinancialYears
                .FirstOrDefaultAsync(x => x.FinancialYearId == finYearId);

            var yearLabel = financialYear != null
                ? $"{financialYear.StartDate.Year % 100}{financialYear.EndDate.Year % 100}"
                : finYearId.ToString();

            var count = await _context.JournalEntries
                .CountAsync(x =>
                    x.CompanyId == companyId &&
                    x.FinYearId == finYearId &&
                    !x.IsDeleted);

            return $"JV{yearLabel}{(count + 1):D4}";
        }

        // ════════════════════════════════════════════════════
        // PRIVATE — Map to response DTO
        // ════════════════════════════════════════════════════
        private static JournalEntryResponseDto MapToResponseDto(JournalEntry entry)
        {
            return new JournalEntryResponseDto
            {
                JournalEntryId = entry.JournalEntryId,
                CompanyId = entry.CompanyId,
                FinYearId = entry.FinYearId,
                AccountId = entry.AccountId,
                AccountName = entry.Account?.AccountName ?? string.Empty,
                AccountCode = entry.Account?.AccountCode,
                EntryNo = entry.EntryNo,
                EntryDate = DateOnly.FromDateTime(entry.EntryDate),
                TotalDebit = entry.TotalDebit,
                TotalCredit = entry.TotalCredit,
                Status = entry.Status,
                IsActive = entry.IsActive,
                CreatedBy = entry.CreatedBy,
                CreatedDate = entry.CreatedDate,
                ModifiedBy = entry.ModifiedBy,
                ModifiedDate = entry.ModifiedDate,
                Lines = entry.Lines?
                    .Select(l => new JournalEntryLineResponseDto
                    {
                        LineId = l.LineId,
                        JournalEntryId = l.JournalEntryId,
                        AccountId = l.AccountId,
                        AccountName = l.Account?.AccountName ?? string.Empty,
                        AccountCode = l.Account?.AccountCode ?? l.AccountCode,
                        Debit = l.Debit,
                        Credit = l.Credit,
                        Narration = l.Narration
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
