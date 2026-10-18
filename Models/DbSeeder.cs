using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace LaptopShop.Models
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            using var context = new AppDbContext(
                serviceProvider.GetRequiredService<DbContextOptions<AppDbContext>>());

            // 1. Đảm bảo Database LaptopStoreDB đã tồn tại trên SQL Server
            try
            {
                var connStr = context.Database.GetConnectionString();
                if (!string.IsNullOrEmpty(connStr))
                {
                    var masterBuilder = new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(connStr)
                    {
                        InitialCatalog = "master",
                        ConnectTimeout = 4
                    };
                    using var masterConn = new Microsoft.Data.SqlClient.SqlConnection(masterBuilder.ConnectionString);
                    await masterConn.OpenAsync();
                    using var cmd = masterConn.CreateCommand();
                    cmd.CommandText = @"
                        IF DB_ID(N'LaptopStoreDB') IS NULL
                        BEGIN
                            CREATE DATABASE [LaptopStoreDB];
                        END;
                    ";
                    await cmd.ExecuteNonQueryAsync();
                }
            }
            catch { }

            try
            {
                // Kiểm tra xem bảng Products đã tồn tại và có dữ liệu hay chưa (dùng raw ADO.NET tránh EF Core throw exception)
                bool hasProducts = false;
                try
                {
                    var conn = context.Database.GetDbConnection();
                    if (conn.State != System.Data.ConnectionState.Open)
                    {
                        await conn.OpenAsync();
                    }
                    using var checkCmd = conn.CreateCommand();
                    checkCmd.CommandText = "SELECT CASE WHEN OBJECT_ID(N'[dbo].[Products]', N'U') IS NOT NULL AND EXISTS(SELECT 1 FROM [dbo].[Products]) THEN 1 ELSE 0 END";
                    var res = await checkCmd.ExecuteScalarAsync();
                    hasProducts = Convert.ToInt32(res) == 1;
                }
                catch { }

                // Nếu chưa có bảng hoặc chưa có dữ liệu sản phẩm (máy mới clone từ Git), tự động nạp toàn bộ từ LaptopStoreDB.sql
                if (!hasProducts)
                {
                    string[] candidatePaths = new[]
                    {
                        System.IO.Path.Combine(AppContext.BaseDirectory, "LaptopStoreDB.sql"),
                        System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "LaptopStoreDB.sql"),
                        System.IO.Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "LaptopStoreDB.sql")
                    };

                    string? sqlFilePath = candidatePaths.FirstOrDefault(System.IO.File.Exists);
                    if (!string.IsNullOrEmpty(sqlFilePath))
                    {
                        string fullSql = await System.IO.File.ReadAllTextAsync(sqlFilePath);
                        var batches = System.Text.RegularExpressions.Regex.Split(
                            fullSql,
                            @"^\s*GO\s*$",
                            System.Text.RegularExpressions.RegexOptions.Multiline | System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                        foreach (var batch in batches)
                        {
                            var trimmed = batch.Trim();
                            if (string.IsNullOrWhiteSpace(trimmed)) continue;
                            if (trimmed.StartsWith("USE [master]", StringComparison.OrdinalIgnoreCase)) continue;
                            if (trimmed.StartsWith("IF DB_ID(N'LaptopStoreDB')", StringComparison.OrdinalIgnoreCase)) continue;
                            if (trimmed.StartsWith("USE [LaptopStoreDB]", StringComparison.OrdinalIgnoreCase)) continue;

                            try
                            {
                                await context.Database.ExecuteSqlRawAsync(trimmed);
                            }
                            catch { }
                        }
                    }
                }

                await context.Database.EnsureCreatedAsync();

                // 1. Tạo hoặc cập nhật cấu trúc bảng Coupons
                await context.Database.ExecuteSqlRawAsync(@"
                    IF OBJECT_ID(N'[dbo].[Coupons]', N'U') IS NULL
                    BEGIN
                        CREATE TABLE [dbo].[Coupons](
                            [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                            [Code] NVARCHAR(50) NOT NULL,
                            [Description] NVARCHAR(255) NULL,
                            [DiscountType] NVARCHAR(20) NOT NULL DEFAULT 'Amount',
                            [DiscountValue] DECIMAL(18,2) NOT NULL,
                            [MinOrderAmount] DECIMAL(18,2) NOT NULL DEFAULT 0,
                            [ExpiryDate] DATETIME2 NOT NULL,
                            [UsageCount] INT NOT NULL DEFAULT 0,
                            [MaxUsage] INT NOT NULL DEFAULT 100,
                            [IsActive] BIT NOT NULL DEFAULT 1,
                            [CreatedAt] DATETIME2 NOT NULL DEFAULT GETDATE()
                        );
                    END
                    ELSE
                    BEGIN
                        IF COL_LENGTH('[dbo].[Coupons]', 'ExpiryDate') IS NULL
                        BEGIN
                            ALTER TABLE [dbo].[Coupons] ADD [ExpiryDate] DATETIME2 NOT NULL CONSTRAINT DF_Coupons_ExpiryDate DEFAULT DATEADD(month, 1, GETDATE());
                            IF COL_LENGTH('[dbo].[Coupons]', 'EndDate') IS NOT NULL
                                EXEC('UPDATE [dbo].[Coupons] SET [ExpiryDate] = [EndDate] WHERE [EndDate] IS NOT NULL');
                        END
                        IF COL_LENGTH('[dbo].[Coupons]', 'MaxUsage') IS NULL
                        BEGIN
                            ALTER TABLE [dbo].[Coupons] ADD [MaxUsage] INT NOT NULL CONSTRAINT DF_Coupons_MaxUsage DEFAULT 100;
                            IF COL_LENGTH('[dbo].[Coupons]', 'UsageLimit') IS NOT NULL
                                EXEC('UPDATE [dbo].[Coupons] SET [MaxUsage] = [UsageLimit] WHERE [UsageLimit] IS NOT NULL');
                        END
                        IF COL_LENGTH('[dbo].[Coupons]', 'UsageCount') IS NULL
                        BEGIN
                            ALTER TABLE [dbo].[Coupons] ADD [UsageCount] INT NOT NULL CONSTRAINT DF_Coupons_UsageCount DEFAULT 0;
                            IF COL_LENGTH('[dbo].[Coupons]', 'TimesUsed') IS NOT NULL
                                EXEC('UPDATE [dbo].[Coupons] SET [UsageCount] = [TimesUsed] WHERE [TimesUsed] IS NOT NULL');
                        END
                        IF COL_LENGTH('[dbo].[Coupons]', 'MinOrderAmount') IS NULL
                            ALTER TABLE [dbo].[Coupons] ADD [MinOrderAmount] DECIMAL(18,2) NOT NULL CONSTRAINT DF_Coupons_MinOrder DEFAULT 0;
                        IF COL_LENGTH('[dbo].[Coupons]', 'CreatedAt') IS NULL
                            ALTER TABLE [dbo].[Coupons] ADD [CreatedAt] DATETIME2 NOT NULL CONSTRAINT DF_Coupons_CreatedAt DEFAULT GETDATE();
                    END
                ");

                // 2. Thêm dữ liệu mẫu cho Coupons nếu trống
                if (!await context.Coupons.AnyAsync())
                {
                    context.Coupons.AddRange(
                        new Coupon
                        {
                            Code = "LAPTOP2026",
                            Description = "Khuyến mãi chào năm mới 2026 cho đơn hàng từ 15 triệu",
                            DiscountType = "Amount",
                            DiscountValue = 500000,
                            MinOrderAmount = 15000000,
                            ExpiryDate = DateTime.Now.AddMonths(3),
                            UsageCount = 14,
                            MaxUsage = 100,
                            IsActive = true,
                            CreatedAt = DateTime.Now.AddDays(-10)
                        },
                        new Coupon
                        {
                            Code = "GAMINGVIP",
                            Description = "Giảm trực tiếp 1.000.000đ cho Laptop Gaming cấu hình khủng",
                            DiscountType = "Amount",
                            DiscountValue = 1000000,
                            MinOrderAmount = 25000000,
                            ExpiryDate = DateTime.Now.AddMonths(2),
                            UsageCount = 28,
                            MaxUsage = 50,
                            IsActive = true,
                            CreatedAt = DateTime.Now.AddDays(-5)
                        },
                        new Coupon
                        {
                            Code = "FREESHIP",
                            Description = "Miễn phí vận chuyển toàn quốc cho đơn hàng từ 10 triệu",
                            DiscountType = "Amount",
                            DiscountValue = 150000,
                            MinOrderAmount = 10000000,
                            ExpiryDate = DateTime.Now.AddMonths(6),
                            UsageCount = 65,
                            MaxUsage = 200,
                            IsActive = true,
                            CreatedAt = DateTime.Now.AddDays(-15)
                        },
                        new Coupon
                        {
                            Code = "WELCOME100K",
                            Description = "Voucher 100K dành tặng khách hàng mua lần đầu",
                            DiscountType = "Amount",
                            DiscountValue = 100000,
                            MinOrderAmount = 5000000,
                            ExpiryDate = DateTime.Now.AddMonths(1),
                            UsageCount = 89,
                            MaxUsage = 500,
                            IsActive = true,
                            CreatedAt = DateTime.Now.AddDays(-20)
                        }
                    );
                    await context.SaveChangesAsync();
                }

                // 3. Thêm dữ liệu mẫu cho Reviews nếu trống
                if (!await context.Reviews.AnyAsync())
                {
                    var firstProduct = await context.Products.FirstOrDefaultAsync();
                    int prodId = firstProduct?.Id ?? 1;

                    context.Reviews.AddRange(
                        new Review
                        {
                            ProductId = prodId,
                            UserId = "1",
                            UserName = "Nguyễn Văn Khách",
                            Rating = 5,
                            Comment = "Máy chạy siêu mượt, tản nhiệt tốt, đóng gói rất cẩn thận và giao hàng siêu tốc!",
                            CreatedAt = DateTime.Now.AddDays(-2)
                        },
                        new Review
                        {
                            ProductId = prodId,
                            UserId = "2",
                            UserName = "Trần Thị Mai",
                            Rating = 5,
                            Comment = "Thiết kế mỏng nhẹ, bàn phím gõ rất êm tay. Nhân viên tư vấn nhiệt tình 10/10.",
                            CreatedAt = DateTime.Now.AddDays(-4)
                        },
                        new Review
                        {
                            ProductId = prodId,
                            UserId = "3",
                            UserName = "Lê Hoàng Nam",
                            Rating = 4,
                            Comment = "Cấu hình khỏe trong tầm giá, màn hình đẹp sắc nét, pin dùng tầm 4-5 tiếng ổn định.",
                            CreatedAt = DateTime.Now.AddDays(-7)
                        },
                        new Review
                        {
                            ProductId = prodId,
                            UserId = "4",
                            UserName = "Phạm Đức Anh",
                            Rating = 5,
                            Comment = "Đã test thử các tựa game nặng, FPS ổn định không giật lag. Rất đáng đồng tiền bát gạo!",
                            CreatedAt = DateTime.Now.AddDays(-10)
                        }
                    );
                    await context.SaveChangesAsync();
                }

                // 4. Tạo cấu trúc bảng CartItems nếu chưa có
                await context.Database.ExecuteSqlRawAsync(@"
                    IF OBJECT_ID(N'[dbo].[CartItems]', N'U') IS NULL
                    BEGIN
                        CREATE TABLE [dbo].[CartItems](
                            [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                            [UserId] INT NOT NULL,
                            [ProductId] INT NOT NULL,
                            [Quantity] INT NOT NULL,
                            [CreatedAt] DATETIME NOT NULL DEFAULT GETDATE(),
                            [UpdatedAt] DATETIME NOT NULL DEFAULT GETDATE()
                        );
                    END
                ");
            }
            catch
            {
                // Bỏ qua lỗi seed nếu database đang bị khóa hoặc đã có sẵn
            }
        }
    }
}