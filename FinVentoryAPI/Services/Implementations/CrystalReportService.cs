using FinVentoryAPI.Services.Interfaces;
using LijsDev.CrystalReportsRunner.Core;

namespace FinVentoryAPI.Services.Implementations
{
    public class CrystalReportService : ICrystalReportService, IDisposable
    {
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<CrystalReportService> _logger;
        private CrystalReportsEngine? _engine;
        private readonly SemaphoreSlim _semaphore = new(1, 1);

        public CrystalReportService(IConfiguration configuration, IWebHostEnvironment env, ILogger<CrystalReportService> logger)
        {
            _configuration = configuration;
            _env = env;
            _logger = logger;
        }

        public async Task<byte[]> ExportReportToPdfAsync(string reportName, Dictionary<string, object>? parameters = null)
        {
            var reportsPath = Path.Combine(_env.ContentRootPath, "Reports");
            var rptFile = Path.Combine(reportsPath, $"{reportName}.rpt");

            if (!File.Exists(rptFile))
                throw new FileNotFoundException($"Report file '{reportName}.rpt' not found in Reports folder.", rptFile);

            var connString = _configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

            var props = ParseConnectionString(connString);

            _engine ??= new CrystalReportsEngine
            {
                Timeout = TimeSpan.FromSeconds(120)
            };

            await _semaphore.WaitAsync();
            try
            {
                var report = new Report(rptFile, reportName)
                {
                    Connection = CrystalReportsConnectionFactory.CreateODBCSqlConnection(
                        dsnName: "finventroy",
                        database: props.GetValueOrDefault("Initial Catalog", "FinVentoryDB"),
                        username: props.GetValueOrDefault("User ID", "sa"),
                        password: props.GetValueOrDefault("Password", "sql2025")
                    )
                };

                if (parameters != null)
                {
                    foreach (var kvp in parameters)
                    {
                        report.Parameters[kvp.Key] = kvp.Value;
                    }
                }

                using var stream = await _engine.Export(report, ReportExportFormats.PDF);

                using var ms = new MemoryStream();
                await stream.CopyToAsync(ms);
                return ms.ToArray();
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private static Dictionary<string, string> ParseConnectionString(string connectionString)
        {
            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var part in connectionString.Split(';', StringSplitOptions.RemoveEmptyEntries))
            {
                var idx = part.IndexOf('=');
                if (idx > 0)
                {
                    var key = part[..idx].Trim();
                    var value = part[(idx + 1)..].Trim();
                    result[key] = value;
                }
            }
            return result;
        }

        public void Dispose()
        {
            _engine?.Dispose();
            _engine = null;
        }
    }
}
