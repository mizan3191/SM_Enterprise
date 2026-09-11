using DEALER.DataAccess;
using DEALER.Web;
using DEALER.Web.Components;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using QuestPDF.Infrastructure;
using Radzen;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "auth_token";
        options.LoginPath = "/login";
        options.Cookie.MaxAge = TimeSpan.FromMinutes(180);
        options.AccessDeniedPath = "/access-denied";
    });

builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();

builder.Services.AddDbContext<DEALERContext>(options =>
options.UseSqlServer(builder.Configuration.GetConnectionString("BoniyadiDb")));


builder.Services.AddScoped<ProtectedSessionStorage>();
builder.Services.AddRadzenComponents();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddControllers();

#region Services

builder.Services.AddScoped<NotificationService>();
builder.Services.AddScoped<IAccount, AccountManager>();
builder.Services.AddScoped<ICustomer, CustomerManager>();
builder.Services.AddScoped<IProduct, ProductManager>();
builder.Services.AddScoped<IOrder, OrderManager>();
builder.Services.AddScoped<IReportServices, ReportServices>();
builder.Services.AddScoped<ICompany, CompanyManager>();
builder.Services.AddScoped<ISupplier, SupplierManager>();
builder.Services.AddScoped<IPurchase, PurchaseManager>();
builder.Services.AddScoped<IChart, ChartManager>();
builder.Services.AddScoped<IProductReturn, ProductReturnManager>();
//builder.Services.AddScoped<IProductCategory, ProductCategoryManager>();
builder.Services.AddScoped<IExpense, ExpenseManager>();
//builder.Services.AddScoped<IPreOrder, PreOrderManager>();
builder.Services.AddScoped<ILookup, LookupManager>();
//builder.Services.AddScoped<IDamageProductReturns, DamageProductReturnsManager>();
//builder.Services.AddScoped<ISRDiscount, SRDiscountManager>();
//builder.Services.AddScoped<ISRPaymentHistory, SRPaymentHistoryManager>();
builder.Services.AddScoped<IDSRShopDue, DSRShopDueManager>();
builder.Services.AddScoped<IDSRShopPaymentHistory, DSRShopPaymentHistoryManager>();
//builder.Services.AddScoped<IDamageProduct, DamageProductManager>();
builder.Services.AddScoped<IDailyExpense, DailyExpenseManager>();
builder.Services.AddScoped<IOrderPaymentHistory, OrderPaymentHistoryManager>();
//builder.Services.AddScoped<IDamageProductHandover, DamageProductHandoverManager>();
//builder.Services.AddScoped<IDamageProductHandoverPaymentHistory, DamageProductHandoverPaymentHistoryManager>();
builder.Services.AddScoped<IExtraReturnProduct, ExtraReturnProductManager>();
builder.Services.AddScoped<ICylinderExchange, CylinderExchangeManager>();



builder.Services.AddScoped<IDatabaseBackupManager, DatabaseBackupManager>();
builder.Services.AddScoped<ILicense, LicenseManager>();
#endregion Services

QuestPDF.Settings.License = LicenseType.Community;

builder.Services.AddServerSideBlazor().AddCircuitOptions(options => {
    options.DetailedErrors = true;
});

var app = builder.Build();


app.MapGet("/api/backup/download", async (IDatabaseBackupManager backupManager) =>
{
    var zipBytes = await backupManager.CreateBackupZipAsync();
    var fileName = $"database_backup_{DateTime.Now:yyyyMMdd_HHmmss}.zip";
    return Results.File(zipBytes, "application/zip", fileName);
});


// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();