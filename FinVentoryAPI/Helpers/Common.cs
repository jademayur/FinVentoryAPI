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
    }
}
