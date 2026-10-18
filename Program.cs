using LaptopShop.Models;
using LaptopShop.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ================================================
// 1. Đăng ký các dịch vụ (Dependency Injection)
// ================================================

// Tự động nhận diện SQL Server instance phù hợp trên mọi máy (hỗ trợ .\SQLEXPRESS, localhost, ., LocalDB, v.v.)
string? configuredConnStr = builder.Configuration.GetConnectionString("DefaultConnection");
string activeConnStr = configuredConnStr ?? "Server=.\\SQLEXPRESS;Database=LaptopStoreDB;Trusted_Connection=True;TrustServerCertificate=True;";

if (activeConnStr.Contains(".\\\\SQLEXPRESS"))
{
    activeConnStr = activeConnStr.Replace(".\\\\SQLEXPRESS", ".\\SQLEXPRESS");
}

bool canConnect = false;
try
{
    var testBuilder = new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(activeConnStr)
    {
        InitialCatalog = "master",
        ConnectTimeout = 3
    };
    using var testConn = new Microsoft.Data.SqlClient.SqlConnection(testBuilder.ConnectionString);
    testConn.Open();
    canConnect = true;
}
catch { }

if (!canConnect)
{
    var candidateInstances = new[]
    {
        "Server=.\\SQLEXPRESS;Database=LaptopStoreDB;Trusted_Connection=True;TrustServerCertificate=True;",
        "Server=localhost;Database=LaptopStoreDB;Trusted_Connection=True;TrustServerCertificate=True;",
        "Server=.;Database=LaptopStoreDB;Trusted_Connection=True;TrustServerCertificate=True;",
        "Server=(localdb)\\MSSQLLocalDB;Database=LaptopStoreDB;Trusted_Connection=True;TrustServerCertificate=True;",
        "Server=127.0.0.1;Database=LaptopStoreDB;Trusted_Connection=True;TrustServerCertificate=True;",
        "Server=localhost\\SQLEXPRESS;Database=LaptopStoreDB;Trusted_Connection=True;TrustServerCertificate=True;",
        "Server=DESKTOP-0C6109V;Database=LaptopStoreDB;Trusted_Connection=True;TrustServerCertificate=True;"
    };

    foreach (var fb in candidateInstances)
    {
        try
        {
            var fbBuilder = new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(fb)
            {
                InitialCatalog = "master",
                ConnectTimeout = 3
            };
            using var fbConn = new Microsoft.Data.SqlClient.SqlConnection(fbBuilder.ConnectionString);
            fbConn.Open();
            activeConnStr = fb;
            canConnect = true;
            break;
        }
        catch { }
    }
}

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        activeConnStr,
        sqlOptions => sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(3),
            errorNumbersToAdd: null
        )
    )
);

// Đăng ký các dịch vụ nghiệp vụ (Services)
builder.Services.AddScoped<IStockService, StockService>();
builder.Services.AddScoped<CartService>();
builder.Services.AddScoped<CheckoutService>();

// Đăng ký Session với thời gian timeout 30 phút
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;       // Bảo mật: không cho JavaScript đọc cookie
    options.Cookie.IsEssential = true;    // Luôn lưu session
});

// Đăng ký HTTP Context Accessor (truy cập Session từ View/Service)
builder.Services.AddHttpContextAccessor();

// Đăng ký MVC với Controllers và Views
builder.Services.AddControllersWithViews();

var app = builder.Build();

// ================================================
// 2. Tự động seed tài khoản Admin & Khách hàng mẫu
// ================================================
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        await DbSeeder.SeedAsync(services);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}

// ================================================
// 3. Cấu hình middleware pipeline
// ================================================
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Kích hoạt Session TRƯỚC Authentication/Authorization
app.UseSession();

app.UseAuthorization();

// ================================================
// 4. Cấu hình Route
// ================================================
// 1. Route cho khu vực Admin
app.MapControllerRoute(
    name: "admin",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");

// 2. Route mặc định cho người dùng
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();