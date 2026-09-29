using FinVentoryAPI.Data;
using FinVentoryAPI.Helpers;
using FinVentoryAPI.Middleware;
using FinVentoryAPI.Services.Implementations;
using FinVentoryAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Serilog;
using System.Text;
using System.Text.Json.Serialization;


var builder = WebApplication.CreateBuilder(args);

// ── Serilog ────────────────────────────────────────────────────────
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/finventory-.log", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 30)
    .CreateLogger();

builder.Host.UseSerilog();

// ── Database ───────────────────────────────────────────────────────
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ── Services ───────────────────────────────────────────────────────
builder.Services.AddScoped<ICompanyService, CompanyService>();
builder.Services.AddScoped<IFinancialYearService, FinancialYearService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IUserCompanyService, UserCompanyService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ILocationService, LocationService>();
builder.Services.AddScoped<IModuleService, ModuleService>();
builder.Services.AddScoped<IMenuGroupService, MenuGroupService>();
builder.Services.AddScoped<IMenuItemService, MenuItemService>();
builder.Services.AddScoped<IRoleRightService, RoleRightService>();
builder.Services.AddScoped<IAccountGroupService, AccountGroupService>();
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<ITaxService, TaxService>();
builder.Services.AddScoped<IHsnService, HsnService>();
builder.Services.AddScoped<IItemGroupService, ItemGroupService>();
builder.Services.AddScoped<Common>();
builder.Services.AddScoped<IBrandService, BrandService>();
builder.Services.AddScoped<IWarehouseService, WarehouseService>();
builder.Services.AddScoped<IItemService, ItemService>();
builder.Services.AddScoped<IBusinessPartnerService, BusinessPartnerService>();
builder.Services.AddScoped<IOpeningBalanceService, OpeningBalanceService>();
builder.Services.AddScoped<IFinancialReportService, FinancialReportService>();
builder.Services.AddScoped<IOpeningItemBalanceService, OpeningItemBalanceService>();
builder.Services.AddScoped<ISalesInvoiceService, SalesInvoiceService>();
builder.Services.AddScoped<ISalesPersonService, SalesPersonService>();
builder.Services.AddScoped<IDocumentSeriesService, DocumentSeriesService>();
builder.Services.AddScoped<IStockLedgerService, StockLedgerService>();
builder.Services.AddScoped<IStockService, StockService>();
builder.Services.AddScoped<IStockReportService, StockReportService>();
builder.Services.AddScoped<IAccountLedgerPostingService, AccountLedgerPostingService>();
builder.Services.AddScoped<IAccountLedgerService, AccountLedgerService>();
builder.Services.AddScoped<ISalesReportService, SalesReportService>();
builder.Services.AddScoped<IPurchaseReportService, PurchaseReportService>();
builder.Services.AddScoped<IIncomingPaymentService, IncomingPaymentService>();
builder.Services.AddScoped<IPurchaseInvoiceService, PurchaseInvoiceService>();
builder.Services.AddScoped<IOutgoingPaymentService, OutgoingPaymentService>();
builder.Services.AddScoped<ICashBankEntryService, CashBankEntryService>();
builder.Services.AddScoped<IJournalEntryService, JournalEntryService>();
builder.Services.AddScoped<IGSTReportsService, GSTReportsService>();
builder.Services.AddScoped<IBankMasterService, BankMasterService>();
builder.Services.AddScoped<IBomService, BomService>();
builder.Services.AddScoped<ISalesReturnService, SalesReturnService>();
builder.Services.AddScoped<IPurchaseReturnService, PurchaseReturnService>();
builder.Services.AddScoped<ISalesQuotationService, SalesQuotationService>();
builder.Services.AddSingleton<ICrystalReportService, CrystalReportService>();
builder.Services.AddScoped<ICompanyConfigService, CompanyConfigService>();
builder.Services.AddScoped<ISalesOrderService, SalesOrderService>();
builder.Services.AddScoped<IGoodsDeliveryService, GoodsDeliveryService>();
builder.Services.AddScoped<IPurchaseOrderService, PurchaseOrderService>();
builder.Services.AddScoped<IGRNService, GRNService>();
builder.Services.AddScoped<IAuditLogService, AuditLogService>();
builder.Services.AddScoped<ICompanySeedService, CompanySeedService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IProductionOrderService, ProductionOrderService>();
builder.Services.AddScoped<IProductionReportService, ProductionReportService>();
builder.Services.AddScoped<IOutstandingReportService, OutstandingReportService>();
builder.Services.AddScoped<IDocumentTypeService, DocumentTypeService>();
builder.Services.AddScoped<IStockTransferService, StockTransferService>();
builder.Services.AddScoped<IStockAdjustmentService, StockAdjustmentService>();
builder.Services.AddScoped<IProductionIssueService, ProductionIssueService>();
builder.Services.AddScoped<IProductionReceiptService, ProductionReceiptService>();
builder.Services.AddScoped<IJobWorkIssueService, JobWorkIssueService>();
builder.Services.AddScoped<IJobWorkReceiptService, JobWorkReceiptService>();
builder.Services.AddScoped<IApprovalService, ApprovalService>();
builder.Services.AddScoped<ISalesDocumentFlowService, SalesDocumentFlowService>();
builder.Services.AddScoped<IPurchaseDocumentFlowService, PurchaseDocumentFlowService>();

builder.Services.AddHttpContextAccessor();

// ── CORS (environment-aware) ──────────────────────────────────────
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", policyBuilder =>
    {
        if (builder.Environment.IsDevelopment())
        {
            policyBuilder.AllowAnyOrigin()
                         .AllowAnyMethod()
                         .AllowAnyHeader();
        }
        else
        {
            var origins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
                          ?? new[] { "https://finventroy.runasp.net" };
            policyBuilder.WithOrigins(origins)
                         .AllowAnyMethod()
                         .AllowAnyHeader();
        }
    });
});

// ── Controllers ────────────────────────────────────────────────────
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.Converters.Add(new DateOnlyConverter());
        options.JsonSerializerOptions.Converters.Add(new NullableDateOnlyConverter());
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

// ── Swagger with JWT ──────────────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    // Two DTOs share the short name "OpeningBalanceItemDto" (OpeningBalanceDTOs
    // vs OpeningItemBalanceDTOs). Use full names as schema ids to avoid collision.
    options.CustomSchemaIds(type => type.FullName!);

    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "FinVentory API",
        Version = "v1",
        Description = "Finance & Inventory Management API"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your JWT token"
    });

    options.AddSecurityRequirement(_ => new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecuritySchemeReference("Bearer"),
            new List<string>()
        }
    });
});

// ── Authentication ─────────────────────────────────────────────────
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]
                    ?? throw new InvalidOperationException(
                        "JWT Key is not configured. Set it via 'dotnet user-secrets set \"Jwt:Key\" \"<64+ chars>\"' (Development) or the Jwt__Key environment variable (Production).")))
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// ── Global Exception Handler ──────────────────────────────────────
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

// ── CORS ──────────────────────────────────────────────────────────
app.UseCors("CorsPolicy");

// ── Swagger (all environments for now) ────────────────────────────
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "FinVentory API v1");
    options.RoutePrefix = string.Empty;
});

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<PermissionMiddleware>();
app.MapControllers();

// ── Seed applAdmin role + default admin user ────────────────────
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();

    // Seed applAdmin role if not exists
    if (!db.Roles.Any(r => r.RoleName == "applAdmin"))
    {
        db.Roles.Add(new FinVentoryAPI.Entities.Role { RoleName = "applAdmin", IsActive = true });
        db.SaveChanges();
    }

    // Seed default admin user if no users exist
    if (!db.Users.Any())
    {
        var applAdminRole = db.Roles.First(r => r.RoleName == "applAdmin");
        db.Users.Add(new FinVentoryAPI.Entities.User
        {
            FullName = "Admin",
            Email = "admin@test.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
            Mobile = "9999999999",
            RoleId = applAdminRole.RoleId,
            IsActive = true 
        });
        db.SaveChanges();
    }

    // Seed RoleRights for applAdmin with full rights on all MenuItems
    var applAdmin = db.Roles.First(r => r.RoleName == "applAdmin");
    var allMenuItems = db.MenuItems.Where(m => m.IsActive).ToList();
    var existingRights = db.RoleRights
        .Where(rr => rr.RoleId == applAdmin.RoleId)
        .Select(rr => rr.MenuItemId)
        .ToHashSet();

    var missingRights = allMenuItems
        .Where(mi => !existingRights.Contains(mi.MenuItemId))
        .Select(mi => new FinVentoryAPI.Entities.RoleRight
        {
            RoleId = applAdmin.RoleId,
            ModuleId = mi.ModuleId,
            MenuItemId = mi.MenuItemId,
            CanView = true,
            CanAdd = true,
            CanEdit = true,
            CanDelete = true,
            CanPrint = true,
            CanExport = true,
            CanApprove = true,
            GrantedBy = 0,
            GrantedAt = DateTime.UtcNow
        })
        .ToList();

    if (missingRights.Any())
    {
        db.RoleRights.AddRange(missingRights);
        db.SaveChanges();
    }
}

app.Run();
