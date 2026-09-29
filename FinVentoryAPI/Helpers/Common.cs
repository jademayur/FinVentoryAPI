using FinVentoryAPI.Data;
using FinVentoryAPI.Entities;
using Microsoft.EntityFrameworkCore;

namespace FinVentoryAPI.Helpers
{
    public class Common
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        public Common(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor; 

        }
        public int GetCompanyId()
        {
            var claim = _httpContextAccessor.HttpContext?
                .User?.FindFirst("CompanyId")?.Value;

            if (string.IsNullOrEmpty(claim))
                throw new Exception("CompanyId not found in token.");

            return int.Parse(claim);
        }

        public int GetFinancialYearId()
        {
            var claim = _httpContextAccessor.HttpContext?
                .User?.FindFirst("FinancialYearId")?.Value;

            if (string.IsNullOrEmpty(claim))
                throw new Exception("FinancialYearId not found in token.");

            return int.Parse(claim);
        }

        public int GetUserId()
        {
            var claim = _httpContextAccessor.HttpContext?
                .User?.FindFirst("UserId")?.Value;

            if (string.IsNullOrEmpty(claim))
                throw new Exception("User Id not found in token.");

            return int.Parse(claim);
        }

        public async Task<string> GenerateDocumentNumber(AppDbContext context, string documentType)
        {
            var companyId = GetCompanyId();
            var financialYearId = GetFinancialYearId();

            var series = await context.DocumentSeries
                .FirstOrDefaultAsync(s =>
                    s.CompanyId == companyId &&
                    s.DocumentType == documentType &&
                    s.FinancialYearId == financialYearId &&
                    s.IsActive &&
                    !s.IsDeleted);

            if (series == null)
                throw new Exception($"No active Document Series found for '{documentType}' in the current financial year.");

            var numStr = series.NextNumber.ToString().PadLeft(series.DocumentLength, '0');
            var prefix = series.Prefix ?? "";
            var suffix = series.Suffix ?? "";

            string docNumber;
            if (!string.IsNullOrEmpty(series.Format))
            {
                var fy = await context.FinancialYears.FindAsync(financialYearId);
                var yearStart = fy?.StartDate.Year.ToString() ?? "";
                var yearEnd = fy?.EndDate.Year.ToString() ?? "";

                docNumber = series.Format
                    .Replace("{PREFIX}", prefix)
                    .Replace("{SUFFIX}", suffix)
                    .Replace("{NUMBER}", numStr)
                    .Replace("{FY}", financialYearId.ToString())
                    .Replace("{YEAR}", yearEnd.Length >= 4 ? yearEnd.Substring(2) : yearEnd)
                    .Replace("{YEAR_START}", yearStart)
                    .Replace("{YEAR_END}", yearEnd);
            }
            else
            {
                docNumber = $"{prefix}{numStr}{suffix}";
            }

            series.NextNumber++;
            await context.SaveChangesAsync();

            return docNumber;
        }
    }
}
