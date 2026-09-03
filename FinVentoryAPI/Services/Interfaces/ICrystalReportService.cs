namespace FinVentoryAPI.Services.Interfaces
{
    public interface ICrystalReportService
    {
        Task<byte[]> ExportReportToPdfAsync(string reportName, Dictionary<string, object>? parameters = null);
    }
}
