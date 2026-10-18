/*
    =============================================================================
    CƠ SỞ DỮ LIỆU: LaptopStoreDB (FULL UNIFIED DATABASE SCRIPT)
    Hệ thống: LaptopShop (.NET 10 ASP.NET Core MVC)
    Người phụ trách: Tú (Leader - Kiến trúc, Database, Xác thực & Phân quyền)
    
    Bao gồm đầy đủ 7 bảng:
      1. Brands (Thương hiệu)
      2. Categories (Danh mục)
      3. Products (90 Sản phẩm Laptop)
      4. Orders (Đơn hàng)
      5. OrderDetails (Chi tiết đơn hàng)
      6. Reviews (Đánh giá)
      7. Users (Tài khoản người dùng, phân quyền Admin/Customer)
    =============================================================================
*/

USE [master];
GO

IF DB_ID(N'LaptopStoreDB') IS NULL
BEGIN
    CREATE DATABASE [LaptopStoreDB];
END;
GO

USE [LaptopStoreDB];
GO

-- =============================================================================
-- 1. TẠO CÁC BẢNG (TABLES)
-- =============================================================================

-- 1.1. Bảng Thương hiệu (Brands)
IF OBJECT_ID(N'[dbo].[Brands]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Brands](
        [Id]   INT IDENTITY(1,1) NOT NULL,
        [Name] NVARCHAR(100)     NOT NULL,
        CONSTRAINT [PK_Brands] PRIMARY KEY CLUSTERED ([Id] ASC)
    );
END
GO

-- 1.2. Bảng Danh mục (Categories)
IF OBJECT_ID(N'[dbo].[Categories]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Categories](
        [Id]   INT IDENTITY(1,1) NOT NULL,
        [Name] NVARCHAR(100)     NOT NULL,
        CONSTRAINT [PK_Categories] PRIMARY KEY CLUSTERED ([Id] ASC)
    );
END
GO

-- 1.3. Bảng Sản phẩm (Products)
IF OBJECT_ID(N'[dbo].[Products]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Products](
        [Id]            INT IDENTITY(1,1) NOT NULL,
        [Name]          NVARCHAR(255)     NOT NULL,
        [Price]         DECIMAL(18, 2)    NOT NULL,
        [StockQuantity] INT               NOT NULL,
        [CPU]           NVARCHAR(100)     NULL,
        [RAM]           NVARCHAR(50)      NULL,
        [GPU]           NVARCHAR(100)     NULL,
        [BrandId]       INT               NULL,
        [CategoryId]    INT               NULL,
        CONSTRAINT [PK_Products] PRIMARY KEY CLUSTERED ([Id] ASC)
    );
END
GO

-- 1.4. Bảng Đơn hàng (Orders)
IF OBJECT_ID(N'[dbo].[Orders]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Orders](
        [Id]              INT IDENTITY(1,1) NOT NULL,
        [UserId]          NVARCHAR(255)     NULL,
        [CustomerName]    NVARCHAR(255)     NULL,
        [PhoneNumber]     NVARCHAR(20)      NULL,
        [ShippingAddress] NVARCHAR(500)     NULL,
        [OrderDate]       DATETIME          NULL CONSTRAINT [DF_Orders_OrderDate] DEFAULT (GETDATE()),
        [TotalAmount]     DECIMAL(18, 2)    NULL,
        [Status]          NVARCHAR(50)      NULL CONSTRAINT [DF_Orders_Status] DEFAULT ('Pending'),
        CONSTRAINT [PK_Orders] PRIMARY KEY CLUSTERED ([Id] ASC)
    );
END
GO

-- 1.5. Bảng Chi tiết đơn hàng (OrderDetails)
IF OBJECT_ID(N'[dbo].[OrderDetails]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[OrderDetails](
        [Id]        INT IDENTITY(1,1) NOT NULL,
        [OrderId]   INT               NULL,
        [ProductId] INT               NULL,
        [Quantity]  INT               NULL,
        [UnitPrice] DECIMAL(18, 2)    NULL,
        CONSTRAINT [PK_OrderDetails] PRIMARY KEY CLUSTERED ([Id] ASC)
    );
END
GO

-- 1.6. Bảng Đánh giá (Reviews)
IF OBJECT_ID(N'[dbo].[Reviews]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Reviews](
        [Id]        INT IDENTITY(1,1) NOT NULL,
        [ProductId] INT               NULL,
        [UserId]    NVARCHAR(255)     NULL,
        [UserName]  NVARCHAR(255)     NULL,
        [Rating]    INT               NULL,
        [Comment]   NVARCHAR(MAX)     NULL,
        [CreatedAt] DATETIME          NULL CONSTRAINT [DF_Reviews_CreatedAt] DEFAULT (GETDATE()),
        CONSTRAINT [PK_Reviews] PRIMARY KEY CLUSTERED ([Id] ASC)
    );
END
GO

-- 1.7. Bảng Người dùng & Phân quyền (Users)
IF OBJECT_ID(N'[dbo].[Users]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Users](
        [Id]           INT IDENTITY(1,1) NOT NULL,
        [FullName]     NVARCHAR(100)     NOT NULL,
        [Email]        NVARCHAR(200)     NOT NULL,
        [PasswordHash] NVARCHAR(MAX)     NOT NULL,
        [PhoneNumber]  NVARCHAR(20)      NULL,
        [Address]      NVARCHAR(500)     NULL,
        [Role]         NVARCHAR(20)      NOT NULL CONSTRAINT [DF_Users_Role] DEFAULT ('Customer'),
        [IsActive]     BIT               NOT NULL CONSTRAINT [DF_Users_IsActive] DEFAULT (1),
        [CreatedAt]    DATETIME          NOT NULL CONSTRAINT [DF_Users_CreatedAt] DEFAULT (GETDATE()),
        CONSTRAINT [PK_Users] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [UQ_Users_Email] UNIQUE NONCLUSTERED ([Email] ASC)
    );
END
GO

-- 1.8. Bảng Mã giảm giá (Coupons)
IF OBJECT_ID(N'[dbo].[Coupons]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Coupons](
        [Id]             INT IDENTITY(1,1) NOT NULL,
        [Code]           NVARCHAR(50)      NOT NULL,
        [Description]    NVARCHAR(255)     NULL,
        [DiscountType]   NVARCHAR(20)      NOT NULL CONSTRAINT [DF_Coupons_DiscountType] DEFAULT ('Amount'),
        [DiscountValue]  DECIMAL(18, 2)    NOT NULL,
        [MinOrderAmount] DECIMAL(18, 2)    NOT NULL CONSTRAINT [DF_Coupons_MinOrderAmount] DEFAULT (0),
        [ExpiryDate]     DATETIME2         NOT NULL CONSTRAINT [DF_Coupons_ExpiryDate] DEFAULT (DATEADD(month, 1, GETDATE())),
        [UsageCount]     INT               NOT NULL CONSTRAINT [DF_Coupons_UsageCount] DEFAULT (0),
        [MaxUsage]       INT               NOT NULL CONSTRAINT [DF_Coupons_MaxUsage] DEFAULT (100),
        [IsActive]       BIT               NOT NULL CONSTRAINT [DF_Coupons_IsActive] DEFAULT (1),
        [CreatedAt]      DATETIME2         NOT NULL CONSTRAINT [DF_Coupons_CreatedAt] DEFAULT (GETDATE()),
        CONSTRAINT [PK_Coupons] PRIMARY KEY CLUSTERED ([Id] ASC)
    );
END
GO

-- 1.9. Bảng Giỏ hàng Khách hàng (CartItems)
IF OBJECT_ID(N'[dbo].[CartItems]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[CartItems](
        [Id]        INT IDENTITY(1,1) NOT NULL,
        [UserId]    INT               NOT NULL,
        [ProductId] INT               NOT NULL,
        [Quantity]  INT               NOT NULL,
        [CreatedAt] DATETIME          NOT NULL CONSTRAINT [DF_CartItems_CreatedAt] DEFAULT (GETDATE()),
        [UpdatedAt] DATETIME          NOT NULL CONSTRAINT [DF_CartItems_UpdatedAt] DEFAULT (GETDATE()),
        CONSTRAINT [PK_CartItems] PRIMARY KEY CLUSTERED ([Id] ASC)
    );
END
GO

-- =============================================================================
-- 2. TẠO KHÓA NGOẠI (FOREIGN KEYS)
-- =============================================================================

-- OrderDetails -> Orders
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_OrderDetails_Orders')
BEGIN
    ALTER TABLE [dbo].[OrderDetails] WITH CHECK 
    ADD CONSTRAINT [FK_OrderDetails_Orders] FOREIGN KEY([OrderId]) REFERENCES [dbo].[Orders] ([Id]);
END
GO

-- OrderDetails -> Products
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_OrderDetails_Products')
BEGIN
    ALTER TABLE [dbo].[OrderDetails] WITH CHECK 
    ADD CONSTRAINT [FK_OrderDetails_Products] FOREIGN KEY([ProductId]) REFERENCES [dbo].[Products] ([Id]);
END
GO

-- Products -> Brands
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Products_Brands')
BEGIN
    ALTER TABLE [dbo].[Products] WITH CHECK 
    ADD CONSTRAINT [FK_Products_Brands] FOREIGN KEY([BrandId]) REFERENCES [dbo].[Brands] ([Id]);
END
GO

-- Products -> Categories
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Products_Categories')
BEGIN
    ALTER TABLE [dbo].[Products] WITH CHECK 
    ADD CONSTRAINT [FK_Products_Categories] FOREIGN KEY([CategoryId]) REFERENCES [dbo].[Categories] ([Id]);
END
GO

-- Reviews -> Products
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Reviews_Products')
BEGIN
    ALTER TABLE [dbo].[Reviews] WITH CHECK 
    ADD CONSTRAINT [FK_Reviews_Products] FOREIGN KEY([ProductId]) REFERENCES [dbo].[Products] ([Id]);
END
GO

-- CartItems -> Products
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_CartItems_Products')
BEGIN
    ALTER TABLE [dbo].[CartItems] WITH CHECK 
    ADD CONSTRAINT [FK_CartItems_Products] FOREIGN KEY([ProductId]) REFERENCES [dbo].[Products] ([Id]);
END
GO

-- CartItems -> Users
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_CartItems_Users')
BEGIN
    ALTER TABLE [dbo].[CartItems] WITH CHECK 
    ADD CONSTRAINT [FK_CartItems_Users] FOREIGN KEY([UserId]) REFERENCES [dbo].[Users] ([Id]);
END
GO

-- =============================================================================
-- 3. NẠP DỮ LIỆU MẪU (SEED DATA)
-- =============================================================================

-- 3.1. Dữ liệu Thương hiệu (Brands)
IF NOT EXISTS (SELECT 1 FROM [dbo].[Brands])
BEGIN
    SET IDENTITY_INSERT [dbo].[Brands] ON;
    INSERT [dbo].[Brands] ([Id], [Name]) VALUES (1, N'Asus');
    INSERT [dbo].[Brands] ([Id], [Name]) VALUES (2, N'MSI');
    INSERT [dbo].[Brands] ([Id], [Name]) VALUES (3, N'Lenovo');
    INSERT [dbo].[Brands] ([Id], [Name]) VALUES (4, N'Dell');
    INSERT [dbo].[Brands] ([Id], [Name]) VALUES (5, N'HP');
    INSERT [dbo].[Brands] ([Id], [Name]) VALUES (6, N'Apple');
    INSERT [dbo].[Brands] ([Id], [Name]) VALUES (7, N'Acer');
    INSERT [dbo].[Brands] ([Id], [Name]) VALUES (8, N'Gigabyte');
    INSERT [dbo].[Brands] ([Id], [Name]) VALUES (9, N'LG');
    INSERT [dbo].[Brands] ([Id], [Name]) VALUES (10, N'Asus');
    INSERT [dbo].[Brands] ([Id], [Name]) VALUES (11, N'MSI');
    INSERT [dbo].[Brands] ([Id], [Name]) VALUES (12, N'Lenovo');
    INSERT [dbo].[Brands] ([Id], [Name]) VALUES (13, N'Dell');
    INSERT [dbo].[Brands] ([Id], [Name]) VALUES (14, N'HP');
    INSERT [dbo].[Brands] ([Id], [Name]) VALUES (15, N'Apple');
    INSERT [dbo].[Brands] ([Id], [Name]) VALUES (16, N'Acer');
    INSERT [dbo].[Brands] ([Id], [Name]) VALUES (17, N'Gigabyte');
    INSERT [dbo].[Brands] ([Id], [Name]) VALUES (18, N'LG');
    INSERT [dbo].[Brands] ([Id], [Name]) VALUES (19, N'Asus');
    INSERT [dbo].[Brands] ([Id], [Name]) VALUES (20, N'MSI');
    INSERT [dbo].[Brands] ([Id], [Name]) VALUES (21, N'Lenovo');
    INSERT [dbo].[Brands] ([Id], [Name]) VALUES (22, N'Dell');
    INSERT [dbo].[Brands] ([Id], [Name]) VALUES (23, N'HP');
    INSERT [dbo].[Brands] ([Id], [Name]) VALUES (24, N'Apple');
    INSERT [dbo].[Brands] ([Id], [Name]) VALUES (25, N'Acer');
    INSERT [dbo].[Brands] ([Id], [Name]) VALUES (26, N'Gigabyte');
    INSERT [dbo].[Brands] ([Id], [Name]) VALUES (27, N'LG');
    SET IDENTITY_INSERT [dbo].[Brands] OFF;
    PRINT N'==> Đã nạp dữ liệu Brands thành công.';
END
GO

-- 3.2. Dữ liệu Danh mục (Categories)
IF NOT EXISTS (SELECT 1 FROM [dbo].[Categories])
BEGIN
    SET IDENTITY_INSERT [dbo].[Categories] ON;
    INSERT [dbo].[Categories] ([Id], [Name]) VALUES (1, N'Laptop Gaming');
    INSERT [dbo].[Categories] ([Id], [Name]) VALUES (2, N'Laptop Học tập - Văn phòng');
    INSERT [dbo].[Categories] ([Id], [Name]) VALUES (3, N'Laptop Đồ họa - Kỹ thuật');
    INSERT [dbo].[Categories] ([Id], [Name]) VALUES (4, N'Laptop Mỏng nhẹ - Cao cấp');
    INSERT [dbo].[Categories] ([Id], [Name]) VALUES (5, N'Laptop Gaming');
    INSERT [dbo].[Categories] ([Id], [Name]) VALUES (6, N'Laptop Học tập - Văn phòng');
    INSERT [dbo].[Categories] ([Id], [Name]) VALUES (7, N'Laptop Đồ họa - Kỹ thuật');
    INSERT [dbo].[Categories] ([Id], [Name]) VALUES (8, N'Laptop Mỏng nhẹ - Cao cấp');
    INSERT [dbo].[Categories] ([Id], [Name]) VALUES (9, N'Laptop Gaming');
    INSERT [dbo].[Categories] ([Id], [Name]) VALUES (10, N'Laptop Học tập - Văn phòng');
    INSERT [dbo].[Categories] ([Id], [Name]) VALUES (11, N'Laptop Đồ họa - Kỹ thuật');
    INSERT [dbo].[Categories] ([Id], [Name]) VALUES (12, N'Laptop Mỏng nhẹ - Cao cấp');
    SET IDENTITY_INSERT [dbo].[Categories] OFF;
    PRINT N'==> Đã nạp dữ liệu Categories thành công.';
END
GO

-- 3.3. Dữ liệu Sản phẩm (Products - 90 dòng sản phẩm)
IF NOT EXISTS (SELECT 1 FROM [dbo].[Products])
BEGIN
    SET IDENTITY_INSERT [dbo].[Products] ON;
    INSERT [dbo].[Products] ([Id], [Name], [Price], [StockQuantity], [CPU], [RAM], [GPU], [BrandId], [CategoryId]) VALUES (1, N'Asus TUF Gaming F15 FX506', CAST(17990000.00 AS Decimal(18, 2)), 20, N'Intel Core i5-11400H', N'8GB', N'NVIDIA RTX 2050 4GB', 1, 1);
    INSERT [dbo].[Products] ([Id], [Name], [Price], [StockQuantity], [CPU], [RAM], [GPU], [BrandId], [CategoryId]) VALUES (2, N'Asus TUF Gaming A15 FA507', CAST(22490000.00 AS Decimal(18, 2)), 15, N'AMD Ryzen 7 7735HS', N'16GB', N'NVIDIA RTX 4050 6GB', 1, 1);
    INSERT [dbo].[Products] ([Id], [Name], [Price], [StockQuantity], [CPU], [RAM], [GPU], [BrandId], [CategoryId]) VALUES (3, N'Asus ROG Strix G15 G513', CAST(25990000.00 AS Decimal(18, 2)), 10, N'AMD Ryzen 7 6800H', N'16GB', N'NVIDIA RTX 3050 4GB', 1, 1);
    INSERT [dbo].[Products] ([Id], [Name], [Price], [StockQuantity], [CPU], [RAM], [GPU], [BrandId], [CategoryId]) VALUES (4, N'Asus ROG Strix G16 G614', CAST(36990000.00 AS Decimal(18, 2)), 8, N'Intel Core i7-13650HX', N'16GB', N'NVIDIA RTX 4060 8GB', 1, 1);
    INSERT [dbo].[Products] ([Id], [Name], [Price], [StockQuantity], [CPU], [RAM], [GPU], [BrandId], [CategoryId]) VALUES (5, N'Asus ROG Zephyrus G14', CAST(42990000.00 AS Decimal(18, 2)), 5, N'AMD Ryzen 9 7940HS', N'16GB', N'NVIDIA RTX 4060 8GB', 1, 1);
    INSERT [dbo].[Products] ([Id], [Name], [Price], [StockQuantity], [CPU], [RAM], [GPU], [BrandId], [CategoryId]) VALUES (6, N'Asus ROG Scar 18 G834', CAST(89990000.00 AS Decimal(18, 2)), 2, N'Intel Core i9-13980HX', N'32GB', N'NVIDIA RTX 4090 16GB', 1, 1);
    INSERT [dbo].[Products] ([Id], [Name], [Price], [StockQuantity], [CPU], [RAM], [GPU], [BrandId], [CategoryId]) VALUES (7, N'MSI Bravo 15 B7ED', CAST(16490000.00 AS Decimal(18, 2)), 20, N'AMD Ryzen 5 7535HS', N'16GB', N'AMD Radeon RX 6550M 4GB', 2, 1);
    INSERT [dbo].[Products] ([Id], [Name], [Price], [StockQuantity], [CPU], [RAM], [GPU], [BrandId], [CategoryId]) VALUES (8, N'MSI Cyborg 15 A12UCX', CAST(18990000.00 AS Decimal(18, 2)), 25, N'Intel Core i5-12450H', N'16GB', N'NVIDIA RTX 2050 4GB', 2, 1);
    INSERT [dbo].[Products] ([Id], [Name], [Price], [StockQuantity], [CPU], [RAM], [GPU], [BrandId], [CategoryId]) VALUES (9, N'MSI Thin GF63 12VE', CAST(19490000.00 AS Decimal(18, 2)), 15, N'Intel Core i5-12450H', N'8GB', N'NVIDIA RTX 4050 6GB', 2, 1);
    INSERT [dbo].[Products] ([Id], [Name], [Price], [StockQuantity], [CPU], [RAM], [GPU], [BrandId], [CategoryId]) VALUES (10, N'MSI Katana 15 B13VFK', CAST(28990000.00 AS Decimal(18, 2)), 10, N'Intel Core i7-13620H', N'16GB', N'NVIDIA RTX 4060 8GB', 2, 1);
    INSERT [dbo].[Products] ([Id], [Name], [Price], [StockQuantity], [CPU], [RAM], [GPU], [BrandId], [CategoryId]) VALUES (11, N'MSI Pulse 15 B13VGK', CAST(35990000.00 AS Decimal(18, 2)), 5, N'Intel Core i7-13700H', N'16GB', N'NVIDIA RTX 4070 8GB', 2, 1);
    INSERT [dbo].[Products] ([Id], [Name], [Price], [StockQuantity], [CPU], [RAM], [GPU], [BrandId], [CategoryId]) VALUES (12, N'Lenovo IdeaPad 3 15ITL6', CAST(10990000.00 AS Decimal(18, 2)), 35, N'Intel Core i3-1115G4', N'8GB', N'Intel UHD Graphics', 3, 2);
    INSERT [dbo].[Products] ([Id], [Name], [Price], [StockQuantity], [CPU], [RAM], [GPU], [BrandId], [CategoryId]) VALUES (13, N'Lenovo IdeaPad Slim 5', CAST(16990000.00 AS Decimal(18, 2)), 25, N'Intel Core i5-12450H', N'16GB', N'Intel UHD Graphics', 3, 2);
    INSERT [dbo].[Products] ([Id], [Name], [Price], [StockQuantity], [CPU], [RAM], [GPU], [BrandId], [CategoryId]) VALUES (14, N'Lenovo ThinkBook 15 G4', CAST(15990000.00 AS Decimal(18, 2)), 20, N'Intel Core i5-1235U', N'8GB', N'Intel Iris Xe Graphics', 3, 2);
    INSERT [dbo].[Products] ([Id], [Name], [Price], [StockQuantity], [CPU], [RAM], [GPU], [BrandId], [CategoryId]) VALUES (15, N'Lenovo ThinkPad E14 Gen 4', CAST(18990000.00 AS Decimal(18, 2)), 15, N'Intel Core i5-1235U', N'8GB', N'Intel Iris Xe Graphics', 3, 2);
    INSERT [dbo].[Products] ([Id], [Name], [Price], [StockQuantity], [CPU], [RAM], [GPU], [BrandId], [CategoryId]) VALUES (16, N'Lenovo ThinkPad T14 Gen 3', CAST(32990000.00 AS Decimal(18, 2)), 10, N'Intel Core i7-1260P', N'16GB', N'Intel Iris Xe Graphics', 3, 4);
    INSERT [dbo].[Products] ([Id], [Name], [Price], [StockQuantity], [CPU], [RAM], [GPU], [BrandId], [CategoryId]) VALUES (17, N'Dell Vostro 3520', CAST(12490000.00 AS Decimal(18, 2)), 40, N'Intel Core i3-1215U', N'8GB', N'Intel UHD Graphics', 4, 2);
    INSERT [dbo].[Products] ([Id], [Name], [Price], [StockQuantity], [CPU], [RAM], [GPU], [BrandId], [CategoryId]) VALUES (18, N'Dell Inspiron 15 3530', CAST(14990000.00 AS Decimal(18, 2)), 35, N'Intel Core i5-1334U', N'8GB', N'Intel Iris Xe Graphics', 4, 2);
    INSERT [dbo].[Products] ([Id], [Name], [Price], [StockQuantity], [CPU], [RAM], [GPU], [BrandId], [CategoryId]) VALUES (19, N'Dell Alienware m16 R1', CAST(59990000.00 AS Decimal(18, 2)), 4, N'Intel Core i7-13700HX', N'16GB', N'NVIDIA RTX 4070 8GB', 4, 1);
    INSERT [dbo].[Products] ([Id], [Name], [Price], [StockQuantity], [CPU], [RAM], [GPU], [BrandId], [CategoryId]) VALUES (20, N'Dell Alienware x16', CAST(75990000.00 AS Decimal(18, 2)), 2, N'Intel Core i9-13900HK', N'32GB', N'NVIDIA RTX 4080 12GB', 4, 1);
    INSERT [dbo].[Products] ([Id], [Name], [Price], [StockQuantity], [CPU], [RAM], [GPU], [BrandId], [CategoryId]) VALUES (21, N'Dell Precision 3580', CAST(32990000.00 AS Decimal(18, 2)), 6, N'Intel Core i7-1355U', N'16GB', N'NVIDIA RTX A500 4GB', 4, 3);
    INSERT [dbo].[Products] ([Id], [Name], [Price], [StockQuantity], [CPU], [RAM], [GPU], [BrandId], [CategoryId]) VALUES (22, N'HP 15s fq5229TU', CAST(11490000.00 AS Decimal(18, 2)), 45, N'Intel Core i3-1215U', N'8GB', N'Intel UHD Graphics', 5, 2);
    INSERT [dbo].[Products] ([Id], [Name], [Price], [StockQuantity], [CPU], [RAM], [GPU], [BrandId], [CategoryId]) VALUES (23, N'HP Pavilion 15 eg3093TU', CAST(16490000.00 AS Decimal(18, 2)), 30, N'Intel Core i5-1335U', N'8GB', N'Intel Iris Xe Graphics', 5, 2);
    INSERT [dbo].[Products] ([Id], [Name], [Price], [StockQuantity], [CPU], [RAM], [GPU], [BrandId], [CategoryId]) VALUES (24, N'HP ProBook 450 G9', CAST(18990000.00 AS Decimal(18, 2)), 20, N'Intel Core i5-1235U', N'8GB', N'Intel Iris Xe Graphics', 5, 2);
    INSERT [dbo].[Products] ([Id], [Name], [Price], [StockQuantity], [CPU], [RAM], [GPU], [BrandId], [CategoryId]) VALUES (25, N'HP Omen 16 wf0131TX', CAST(39990000.00 AS Decimal(18, 2)), 8, N'Intel Core i7-13700HX', N'16GB', N'NVIDIA RTX 4060 8GB', 5, 1);
    INSERT [dbo].[Products] ([Id], [Name], [Price], [StockQuantity], [CPU], [RAM], [GPU], [BrandId], [CategoryId]) VALUES (26, N'HP Omen 17 ck2000TX', CAST(55990000.00 AS Decimal(18, 2)), 3, N'Intel Core i9-13900HX', N'32GB', N'NVIDIA RTX 4080 12GB', 5, 1);
    INSERT [dbo].[Products] ([Id], [Name], [Price], [StockQuantity], [CPU], [RAM], [GPU], [BrandId], [CategoryId]) VALUES (27, N'HP ZBook Firefly 14 G10', CAST(36990000.00 AS Decimal(18, 2)), 6, N'Intel Core i7-1355U', N'16GB', N'NVIDIA RTX A500 4GB', 5, 3);
    INSERT [dbo].[Products] ([Id], [Name], [Price], [StockQuantity], [CPU], [RAM], [GPU], [BrandId], [CategoryId]) VALUES (28, N'MacBook Air 13 M1 2020', CAST(18490000.00 AS Decimal(18, 2)), 100, N'Apple M1', N'8GB', N'Apple M1 7-core GPU', 6, 4);
    INSERT [dbo].[Products] ([Id], [Name], [Price], [StockQuantity], [CPU], [RAM], [GPU], [BrandId], [CategoryId]) VALUES (29, N'MacBook Air 13 M2 2022', CAST(24990000.00 AS Decimal(18, 2)), 50, N'Apple M2', N'8GB', N'Apple M2 8-core GPU', 6, 4);
    INSERT [dbo].[Products] ([Id], [Name], [Price], [StockQuantity], [CPU], [RAM], [GPU], [BrandId], [CategoryId]) VALUES (30, N'MacBook Air 15 M2 2023', CAST(29990000.00 AS Decimal(18, 2)), 30, N'Apple M2', N'8GB', N'Apple M2 10-core GPU', 6, 4);
    INSERT [dbo].[Products] ([Id], [Name], [Price], [StockQuantity], [CPU], [RAM], [GPU], [BrandId], [CategoryId]) VALUES (31, N'MacBook Pro 14 M3 Pro', CAST(49990000.00 AS Decimal(18, 2)), 10, N'Apple M3 Pro', N'18GB', N'Apple M3 Pro 14-core GPU', 6, 3);
    INSERT [dbo].[Products] ([Id], [Name], [Price], [StockQuantity], [CPU], [RAM], [GPU], [BrandId], [CategoryId]) VALUES (32, N'MacBook Pro 16 M3 Pro', CAST(62990000.00 AS Decimal(18, 2)), 8, N'Apple M3 Pro', N'18GB', N'Apple M3 Pro 18-core GPU', 6, 3);
    INSERT [dbo].[Products] ([Id], [Name], [Price], [StockQuantity], [CPU], [RAM], [GPU], [BrandId], [CategoryId]) VALUES (33, N'MacBook Pro 16 M3 Max', CAST(89990000.00 AS Decimal(18, 2)), 3, N'Apple M3 Max', N'36GB', N'Apple M3 Max 30-core GPU', 6, 3);
    INSERT [dbo].[Products] ([Id], [Name], [Price], [StockQuantity], [CPU], [RAM], [GPU], [BrandId], [CategoryId]) VALUES (34, N'Acer Aspire 3 A315', CAST(9990000.00 AS Decimal(18, 2)), 40, N'Intel Core i3-N305', N'8GB', N'Intel UHD Graphics', 7, 2);
    INSERT [dbo].[Products] ([Id], [Name], [Price], [StockQuantity], [CPU], [RAM], [GPU], [BrandId], [CategoryId]) VALUES (35, N'Acer Aspire 5 A515', CAST(13490000.00 AS Decimal(18, 2)), 30, N'Intel Core i5-1235U', N'8GB', N'Intel Iris Xe Graphics', 7, 2);
    INSERT [dbo].[Products] ([Id], [Name], [Price], [StockQuantity], [CPU], [RAM], [GPU], [BrandId], [CategoryId]) VALUES (36, N'Acer Predator Helios Neo 16', CAST(32990000.00 AS Decimal(18, 2)), 15, N'Intel Core i7-13700HX', N'16GB', N'NVIDIA RTX 4060 8GB', 7, 1);
    INSERT [dbo].[Products] ([Id], [Name], [Price], [StockQuantity], [CPU], [RAM], [GPU], [BrandId], [CategoryId]) VALUES (37, N'Acer Predator Helios 16', CAST(45990000.00 AS Decimal(18, 2)), 5, N'Intel Core i9-13900HX', N'16GB', N'NVIDIA RTX 4070 8GB', 7, 1);
    INSERT [dbo].[Products] ([Id], [Name], [Price], [StockQuantity], [CPU], [RAM], [GPU], [BrandId], [CategoryId]) VALUES (38, N'Gigabyte G5 GE', CAST(16490000.00 AS Decimal(18, 2)), 25, N'Intel Core i5-12500H', N'8GB', N'NVIDIA RTX 3050 4GB', 8, 1);
    INSERT [dbo].[Products] ([Id], [Name], [Price], [StockQuantity], [CPU], [RAM], [GPU], [BrandId], [CategoryId]) VALUES (39, N'Gigabyte G5 MF', CAST(19490000.00 AS Decimal(18, 2)), 20, N'Intel Core i5-12500H', N'8GB', N'NVIDIA RTX 4050 6GB', 8, 1);
    INSERT [dbo].[Products] ([Id], [Name], [Price], [StockQuantity], [CPU], [RAM], [GPU], [BrandId], [CategoryId]) VALUES (40, N'Gigabyte Aorus 15 BSF', CAST(34990000.00 AS Decimal(18, 2)), 10, N'Intel Core i7-13700H', N'16GB', N'NVIDIA RTX 4070 8GB', 8, 1);
    INSERT [dbo].[Products] ([Id], [Name], [Price], [StockQuantity], [CPU], [RAM], [GPU], [BrandId], [CategoryId]) VALUES (41, N'Gigabyte AERO 16 OLED', CAST(49990000.00 AS Decimal(18, 2)), 5, N'Intel Core i7-13700H', N'16GB', N'NVIDIA RTX 4070 8GB', 8, 3);
    INSERT [dbo].[Products] ([Id], [Name], [Price], [StockQuantity], [CPU], [RAM], [GPU], [BrandId], [CategoryId]) VALUES (42, N'LG Gram 14 2023', CAST(25990000.00 AS Decimal(18, 2)), 15, N'Intel Core i5-1340P', N'16GB', N'Intel Iris Xe Graphics', 9, 4);
    INSERT [dbo].[Products] ([Id], [Name], [Price], [StockQuantity], [CPU], [RAM], [GPU], [BrandId], [CategoryId]) VALUES (43, N'LG Gram 16 2023', CAST(31990000.00 AS Decimal(18, 2)), 12, N'Intel Core i7-1360P', N'16GB', N'Intel Iris Xe Graphics', 9, 4);
    INSERT [dbo].[Products] ([Id], [Name], [Price], [StockQuantity], [CPU], [RAM], [GPU], [BrandId], [CategoryId]) VALUES (44, N'LG Gram 17 2023', CAST(34990000.00 AS Decimal(18, 2)), 8, N'Intel Core i7-1360P', N'16GB', N'Intel Iris Xe Graphics', 9, 4);
    INSERT [dbo].[Products] ([Id], [Name], [Price], [StockQuantity], [CPU], [RAM], [GPU], [BrandId], [CategoryId]) VALUES (45, N'LG Gram Style 14', CAST(38990000.00 AS Decimal(18, 2)), 5, N'Intel Core i7-1360P', N'16GB', N'Intel Iris Xe Graphics', 9, 4);
    INSERT [dbo].[Products] ([Id], [Name], [Price], [StockQuantity], [CPU], [RAM], [GPU], [BrandId], [CategoryId]) VALUES (46, N'Asus TUF Gaming F15 FX506', CAST(17990000.00 AS Decimal(18, 2)), 20, N'Intel Core i5-11400H', N'8GB', N'NVIDIA RTX 2050 4GB', 1, 1);
    INSERT [dbo].[Products] ([Id], [Name], [Price], [StockQuantity], [CPU], [RAM], [GPU], [BrandId], [CategoryId]) VALUES (47, N'Asus TUF Gaming A15 FA507', CAST(22490000.00 AS Decimal(18, 2)), 15, N'AMD Ryzen 7 7735HS', N'16GB', N'NVIDIA RTX 4050 6GB', 1, 1);
    INSERT [dbo].[Products] ([Id], [Name], [Price], [StockQuantity], [CPU], [RAM], [GPU], [BrandId], [CategoryId]) VALUES (48, N'Asus ROG Strix G15 G513', CAST(25990000.00 AS Decimal(18, 2)), 10, N'AMD Ryzen 7 6800H', N'16GB', N'NVIDIA RTX 3050 4GB', 1, 1);
    INSERT [dbo].[Products] ([Id], [Name], [Price], [StockQuantity], [CPU], [RAM], [GPU], [BrandId], [CategoryId]) VALUES (49, N'Asus ROG Strix G16 G614', CAST(36990000.00 AS Decimal(18, 2)), 8, N'Intel Core i7-13650HX', N'16GB', N'NVIDIA RTX 4060 8GB', 1, 1);
    INSERT [dbo].[Products] ([Id], [Name], [Price], [StockQuantity], [CPU], [RAM], [GPU], [BrandId], [CategoryId]) VALUES (50, N'Asus ROG Zephyrus G14', CAST(42990000.00 AS Decimal(18, 2)), 5, N'AMD Ryzen 9 7940HS', N'16GB', N'NVIDIA RTX 4060 8GB', 1, 1);
    INSERT [dbo].[Products] ([Id], [Name], [Price], [StockQuantity], [CPU], [RAM], [GPU], [BrandId], [CategoryId]) VALUES (51, N'Asus ROG Scar 18 G834', CAST(89990000.00 AS Decimal(18, 2)), 2, N'Intel Core i9-13980HX', N'32GB', N'NVIDIA RTX 4090 16GB', 1, 1);
    INSERT [dbo].[Products] ([Id], [Name], [Price], [StockQuantity], [CPU], [RAM], [GPU], [BrandId], [CategoryId]) VALUES (52, N'MSI Bravo 15 B7ED', CAST(16490000.00 AS Decimal(18, 2)), 20, N'AMD Ryzen 5 7535HS', N'16GB', N'AMD Radeon RX 6550M 4GB', 2, 1);
    INSERT [dbo].[Products] ([Id], [Name], [Price], [StockQuantity], [CPU], [RAM], [GPU], [BrandId], [CategoryId]) VALUES (53, N'MSI Cyborg 15 A12UCX', CAST(18990000.00 AS Decimal(18, 2)), 25, N'Intel Core i5-12450H', N'16GB', N'NVIDIA RTX 2050 4GB', 2, 1);
    INSERT [dbo].[Products] ([Id], [Name], [Price], [StockQuantity], [CPU], [RAM], [GPU], [BrandId], [CategoryId]) VALUES (54, N'MSI Thin GF63 12VE', CAST(19490000.00 AS Decimal(18, 2)), 15, N'Intel Core i5-12450H', N'8GB', N'NVIDIA RTX 4050 6GB', 2, 1);
    INSERT [dbo].[Products] ([Id], [Name], [Price], [StockQuantity], [CPU], [RAM], [GPU], [BrandId], [CategoryId]) VALUES (55, N'MSI Katana 15 B13VFK', CAST(28990000.00 AS Decimal(18, 2)), 10, N'Intel Core i7-13620H', N'16GB', N'NVIDIA RTX 4060 8GB', 2, 1);
    INSERT [dbo].[Products] ([Id], [Name], [Price], [StockQuantity], [CPU], [RAM], [GPU], [BrandId], [CategoryId]) VALUES (56, N'MSI Pulse 15 B13VGK', CAST(35990000.00 AS Decimal(18, 2)), 5, N'Intel Core i7-13700H', N'16GB', N'NVIDIA RTX 4070 8GB', 2, 1);
    INSERT [dbo].[Products] ([Id], [Name], [Price], [StockQuantity], [CPU], [RAM], [GPU], [BrandId], [CategoryId]) VALUES (57, N'Lenovo IdeaPad 3 15ITL6', CAST(10990000.00 AS Decimal(18, 2)), 35, N'Intel Core i3-1115G4', N'8GB', N'Intel UHD Graphics', 3, 2);
    INSERT [dbo].[Products] ([Id], [Name], [Price], [StockQuantity], [CPU], [RAM], [GPU], [BrandId], [CategoryId]) VALUES (58, N'Lenovo IdeaPad Slim 5', CAST(16990000.00 AS Decimal(18, 2)), 25, N'Intel Core i5-12450H', N'16GB', N'Intel UHD Graphics', 3, 2);
    INSERT [dbo].[Products] ([Id], [Name], [Price], [StockQuantity], [CPU], [RAM], [GPU], [BrandId], [CategoryId]) VALUES (59, N'Lenovo ThinkBook 15 G4', CAST(15990000.00 AS Decimal(18, 2)), 20, N'Intel Core i5-1235U', N'8GB', N'Intel Iris Xe Graphics', 3, 2);
    INSERT [dbo].[Products] ([Id], [Name], [Price], [StockQuantity], [CPU], [RAM], [GPU], [BrandId], [CategoryId]) VALUES (60, N'Lenovo ThinkPad E14 Gen 4', CAST(18990000.00 AS Decimal(18, 2)), 15, N'Intel Core i5-1235U', N'8GB', N'Intel Iris Xe Graphics', 3, 2);
    INSERT [dbo].[Products] ([Id], [Name], [Price], [StockQuantity], [CPU], [RAM], [GPU], [BrandId], [CategoryId]) VALUES (61, N'Lenovo ThinkPad T14 Gen 3', CAST(32990000.00 AS Decimal(18, 2)), 10, N'Intel Core i7-1260P', N'16GB', N'Intel Iris Xe Graphics', 3, 4);
    INSERT [dbo].[Products] ([Id], [Name], [Price], [StockQuantity], [CPU], [RAM], [GPU], [BrandId], [CategoryId]) VALUES (62, N'Dell Vostro 3520', CAST(12490000.00 AS Decimal(18, 2)), 40, N'Intel Core i3-1215U', N'8GB', N'Intel UHD Graphics', 4, 2);
    INSERT [dbo].[Products] ([Id], [Name], [Price], [StockQuantity], [CPU], [RAM], [GPU], [BrandId], [CategoryId]) VALUES (63, N'Dell Inspiron 15 3530', CAST(14990000.00 AS Decimal(18, 2)), 35, N'Intel Core i5-1334U', N'8GB', N'Intel Iris Xe Graphics', 4, 2);
    INSERT [dbo].[Products] ([Id], [Name], [Price], [StockQuantity], [CPU], [RAM], [GPU], [BrandId], [CategoryId]) VALUES (64, N'Dell Alienware m16 R1', CAST(59990000.00 AS Decimal(18, 2)), 4, N'Intel Core i7-13700HX', N'16GB', N'NVIDIA RTX 4070 8GB', 4, 1);
    INSERT [dbo].[Products] ([Id], [Name], [Price], [StockQuantity], [CPU], [RAM], [GPU], [BrandId], [CategoryId]) VALUES (65, N'Dell Alienware x16', CAST(75990000.00 AS Decimal(18, 2)), 2, N'Intel Core i9-13900HK', N'32GB', N'NVIDIA RTX 4080 12GB', 4, 1);
    INSERT [dbo].[Products] ([Id], [Name], [Price], [StockQuantity], [CPU], [RAM], [GPU], [BrandId], [CategoryId]) VALUES (66, N'Dell Precision 3580', CAST(32990000.00 AS Decimal(18, 2)), 6, N'Intel Core i7-1355U', N'16GB', N'NVIDIA RTX A500 4GB', 4, 3);
    INSERT [dbo].[Products] ([Id], [Name], [Price], [StockQuantity], [CPU], [RAM], [GPU], [BrandId], [CategoryId]) VALUES (67, N'HP 15s fq5229TU', CAST(11490000.00 AS Decimal(18, 2)), 45, N'Intel Core i3-1215U', N'8GB', N'Intel UHD Graphics', 5, 2);
    INSERT [dbo].[Products] ([Id], [Name], [Price], [StockQuantity], [CPU], [RAM], [GPU], [BrandId], [CategoryId]) VALUES (68, N'HP Pavilion 15 eg3093TU', CAST(16490000.00 AS Decimal(18, 2)), 30, N'Intel Core i5-1335U', N'8GB', N'Intel Iris Xe Graphics', 5, 2);
    INSERT [dbo].[Products] ([Id], [Name], [Price], [StockQuantity], [CPU], [RAM], [GPU], [BrandId], [CategoryId]) VALUES (69, N'HP ProBook 450 G9', CAST(18990000.00 AS Decimal(18, 2)), 20, N'Intel Core i5-1235U', N'8GB', N'Intel Iris Xe Graphics', 5, 2);
    INSERT [dbo].[Products] ([Id], [Name], [Price], [StockQuantity], [CPU], [RAM], [GPU], [BrandId], [CategoryId]) VALUES (70, N'HP Omen 16 wf0131TX', CAST(39990000.00 AS Decimal(18, 2)), 8, N'Intel Core i7-13700HX', N'16GB', N'NVIDIA RTX 4060 8GB', 5, 1);
    INSERT [dbo].[Products] ([Id], [Name], [Price], [StockQuantity], [CPU], [RAM], [GPU], [BrandId], [CategoryId]) VALUES (71, N'HP Omen 17 ck2000TX', CAST(55990000.00 AS Decimal(18, 2)), 3, N'Intel Core i9-13900HX', N'32GB', N'NVIDIA RTX 4080 12GB', 5, 1);
    INSERT [dbo].[Products] ([Id], [Name], [Price], [StockQuantity], [CPU], [RAM], [GPU], [BrandId], [CategoryId]) VALUES (72, N'HP ZBook Firefly 14 G10', CAST(36990000.00 AS Decimal(18, 2)), 6, N'Intel Core i7-1355U', N'16GB', N'NVIDIA RTX A500 4GB', 5, 3);
    INSERT [dbo].[Products] ([Id], [Name], [Price], [StockQuantity], [CPU], [RAM], [GPU], [BrandId], [CategoryId]) VALUES (73, N'MacBook Air 13 M1 2020', CAST(18490000.00 AS Decimal(18, 2)), 100, N'Apple M1', N'8GB', N'Apple M1 7-core GPU', 6, 4);
    INSERT [dbo].[Products] ([Id], [Name], [Price], [StockQuantity], [CPU], [RAM], [GPU], [BrandId], [CategoryId]) VALUES (74, N'MacBook Air 13 M2 2022', CAST(24990000.00 AS Decimal(18, 2)), 50, N'Apple M2', N'8GB', N'Apple M2 8-core GPU', 6, 4);
    INSERT [dbo].[Products] ([Id], [Name], [Price], [StockQuantity], [CPU], [RAM], [GPU], [BrandId], [CategoryId]) VALUES (75, N'MacBook Air 15 M2 2023', CAST(29990000.00 AS Decimal(18, 2)), 30, N'Apple M2', N'8GB', N'Apple M2 10-core GPU', 6, 4);
    INSERT [dbo].[Products] ([Id], [Name], [Price], [StockQuantity], [CPU], [RAM], [GPU], [BrandId], [CategoryId]) VALUES (76, N'MacBook Pro 14 M3 Pro', CAST(49990000.00 AS Decimal(18, 2)), 10, N'Apple M3 Pro', N'18GB', N'Apple M3 Pro 14-core GPU', 6, 3);
    INSERT [dbo].[Products] ([Id], [Name], [Price], [StockQuantity], [CPU], [RAM], [GPU], [BrandId], [CategoryId]) VALUES (77, N'MacBook Pro 16 M3 Pro', CAST(62990000.00 AS Decimal(18, 2)), 8, N'Apple M3 Pro', N'18GB', N'Apple M3 Pro 18-core GPU', 6, 3);
    INSERT [dbo].[Products] ([Id], [Name], [Price], [StockQuantity], [CPU], [RAM], [GPU], [BrandId], [CategoryId]) VALUES (78, N'MacBook Pro 16 M3 Max', CAST(89990000.00 AS Decimal(18, 2)), 3, N'Apple M3 Max', N'36GB', N'Apple M3 Max 30-core GPU', 6, 3);
    INSERT [dbo].[Products] ([Id], [Name], [Price], [StockQuantity], [CPU], [RAM], [GPU], [BrandId], [CategoryId]) VALUES (79, N'Acer Aspire 3 A315', CAST(9990000.00 AS Decimal(18, 2)), 40, N'Intel Core i3-N305', N'8GB', N'Intel UHD Graphics', 7, 2);
    INSERT [dbo].[Products] ([Id], [Name], [Price], [StockQuantity], [CPU], [RAM], [GPU], [BrandId], [CategoryId]) VALUES (80, N'Acer Aspire 5 A515', CAST(13490000.00 AS Decimal(18, 2)), 30, N'Intel Core i5-1235U', N'8GB', N'Intel Iris Xe Graphics', 7, 2);
    INSERT [dbo].[Products] ([Id], [Name], [Price], [StockQuantity], [CPU], [RAM], [GPU], [BrandId], [CategoryId]) VALUES (81, N'Acer Predator Helios Neo 16', CAST(32990000.00 AS Decimal(18, 2)), 15, N'Intel Core i7-13700HX', N'16GB', N'NVIDIA RTX 4060 8GB', 7, 1);
    INSERT [dbo].[Products] ([Id], [Name], [Price], [StockQuantity], [CPU], [RAM], [GPU], [BrandId], [CategoryId]) VALUES (82, N'Acer Predator Helios 16', CAST(45990000.00 AS Decimal(18, 2)), 5, N'Intel Core i9-13900HX', N'16GB', N'NVIDIA RTX 4070 8GB', 7, 1);
    INSERT [dbo].[Products] ([Id], [Name], [Price], [StockQuantity], [CPU], [RAM], [GPU], [BrandId], [CategoryId]) VALUES (83, N'Gigabyte G5 GE', CAST(16490000.00 AS Decimal(18, 2)), 25, N'Intel Core i5-12500H', N'8GB', N'NVIDIA RTX 3050 4GB', 8, 1);
    INSERT [dbo].[Products] ([Id], [Name], [Price], [StockQuantity], [CPU], [RAM], [GPU], [BrandId], [CategoryId]) VALUES (84, N'Gigabyte G5 MF', CAST(19490000.00 AS Decimal(18, 2)), 20, N'Intel Core i5-12500H', N'8GB', N'NVIDIA RTX 4050 6GB', 8, 1);
    INSERT [dbo].[Products] ([Id], [Name], [Price], [StockQuantity], [CPU], [RAM], [GPU], [BrandId], [CategoryId]) VALUES (85, N'Gigabyte Aorus 15 BSF', CAST(34990000.00 AS Decimal(18, 2)), 10, N'Intel Core i7-13700H', N'16GB', N'NVIDIA RTX 4070 8GB', 8, 1);
    INSERT [dbo].[Products] ([Id], [Name], [Price], [StockQuantity], [CPU], [RAM], [GPU], [BrandId], [CategoryId]) VALUES (86, N'Gigabyte AERO 16 OLED', CAST(49990000.00 AS Decimal(18, 2)), 5, N'Intel Core i7-13700H', N'16GB', N'NVIDIA RTX 4070 8GB', 8, 3);
    INSERT [dbo].[Products] ([Id], [Name], [Price], [StockQuantity], [CPU], [RAM], [GPU], [BrandId], [CategoryId]) VALUES (87, N'LG Gram 14 2023', CAST(25990000.00 AS Decimal(18, 2)), 15, N'Intel Core i5-1340P', N'16GB', N'Intel Iris Xe Graphics', 9, 4);
    INSERT [dbo].[Products] ([Id], [Name], [Price], [StockQuantity], [CPU], [RAM], [GPU], [BrandId], [CategoryId]) VALUES (88, N'LG Gram 16 2023', CAST(31990000.00 AS Decimal(18, 2)), 12, N'Intel Core i7-1360P', N'16GB', N'Intel Iris Xe Graphics', 9, 4);
    INSERT [dbo].[Products] ([Id], [Name], [Price], [StockQuantity], [CPU], [RAM], [GPU], [BrandId], [CategoryId]) VALUES (89, N'LG Gram 17 2023', CAST(34990000.00 AS Decimal(18, 2)), 8, N'Intel Core i7-1360P', N'16GB', N'Intel Iris Xe Graphics', 9, 4);
    INSERT [dbo].[Products] ([Id], [Name], [Price], [StockQuantity], [CPU], [RAM], [GPU], [BrandId], [CategoryId]) VALUES (90, N'LG Gram Style 14', CAST(38990000.00 AS Decimal(18, 2)), 5, N'Intel Core i7-1360P', N'16GB', N'Intel Iris Xe Graphics', 9, 4);
    SET IDENTITY_INSERT [dbo].[Products] OFF;
    PRINT N'==> Đã nạp dữ liệu Products (90 máy) thành công.';
END
GO

-- 3.4. Dữ liệu Đơn hàng (Orders)
IF NOT EXISTS (SELECT 1 FROM [dbo].[Orders])
BEGIN
    SET IDENTITY_INSERT [dbo].[Orders] ON;
    INSERT [dbo].[Orders] ([Id], [UserId], [CustomerName], [PhoneNumber], [ShippingAddress], [OrderDate], [TotalAmount], [Status]) VALUES (1, N'user_test_1', NULL, NULL, NULL, CAST(N'2026-10-12T17:36:16.373' AS DateTime), CAST(15000000.00 AS Decimal(18, 2)), N'Completed');
    INSERT [dbo].[Orders] ([Id], [UserId], [CustomerName], [PhoneNumber], [ShippingAddress], [OrderDate], [TotalAmount], [Status]) VALUES (2, N'user_test_1', NULL, NULL, NULL, CAST(N'2026-10-12T17:37:32.707' AS DateTime), CAST(15000000.00 AS Decimal(18, 2)), N'Completed');
    INSERT [dbo].[Orders] ([Id], [UserId], [CustomerName], [PhoneNumber], [ShippingAddress], [OrderDate], [TotalAmount], [Status]) VALUES (3, N'user_test_2', NULL, NULL, NULL, CAST(N'2026-10-12T17:37:32.710' AS DateTime), CAST(25000000.00 AS Decimal(18, 2)), N'Pending');
    SET IDENTITY_INSERT [dbo].[Orders] OFF;
    PRINT N'==> Đã nạp dữ liệu Orders thành công.';
END
GO

-- 3.5. Dữ liệu Người dùng & Phân quyền (Users: Admin & Customer)
-- Tài khoản Admin:
--   Email:    admin@laptopshop.com
--   Mật khẩu: Admin@123
--   Role:     Admin
IF NOT EXISTS (SELECT 1 FROM [dbo].[Users] WHERE [Email] = 'admin@laptopshop.com')
BEGIN
    INSERT INTO [dbo].[Users] ([FullName], [Email], [PasswordHash], [PhoneNumber], [Address], [Role], [IsActive], [CreatedAt])
    VALUES (
        N'Quản trị viên Hệ thống',
        'admin@laptopshop.com',
        '$2a$11$LGHe39D/6P0zdKGz6H3EaelYcTa/Eeskl0fLtgXJSAaSAMuo7F9HS', -- Hash BCrypt của Admin@123
        '0987654321',
        N'Trụ sở chính LaptopShop, Hà Nội',
        'Admin',
        1,
        GETDATE()
    );
    PRINT N'==> Đã tạo tài khoản Admin mặc định (admin@laptopshop.com / Admin@123).';
END
GO

-- Tài khoản Khách hàng (Customer):
--   Email:    customer@laptopshop.com
--   Mật khẩu: Customer@123
--   Role:     Customer
IF NOT EXISTS (SELECT 1 FROM [dbo].[Users] WHERE [Email] = 'customer@laptopshop.com')
BEGIN
    INSERT INTO [dbo].[Users] ([FullName], [Email], [PasswordHash], [PhoneNumber], [Address], [Role], [IsActive], [CreatedAt])
    VALUES (
        N'Nguyễn Văn Khách',
        'customer@laptopshop.com',
        '$2a$11$kG1SvqeuKjQnV1v7wzwjZeveehNcxXkbEvkfQsHT95E.8gzKbM/UW', -- Hash BCrypt của Customer@123
        '0912345678',
        N'123 Cầu Giấy, Hà Nội',
        'Customer',
        1,
        GETDATE()
    );
    PRINT N'==> Đã tạo tài khoản Customer mặc định (customer@laptopshop.com / Customer@123).';
END
GO

-- 3.5. Dữ liệu Mã giảm giá (Coupons)
IF NOT EXISTS (SELECT 1 FROM [dbo].[Coupons])
BEGIN
    INSERT INTO [dbo].[Coupons] ([Code], [Description], [DiscountType], [DiscountValue], [MinOrderAmount], [ExpiryDate], [UsageCount], [MaxUsage], [IsActive], [CreatedAt])
    VALUES 
    (N'LAPTOP2026', N'Khuyến mãi chào năm mới 2026 cho đơn hàng từ 15 triệu', N'Amount', 500000, 15000000, DATEADD(month, 3, GETDATE()), 14, 100, 1, GETDATE()),
    (N'GAMINGVIP', N'Giảm trực tiếp 1.000.000đ cho Laptop Gaming cấu hình khủng', N'Amount', 1000000, 25000000, DATEADD(month, 2, GETDATE()), 28, 50, 1, GETDATE()),
    (N'FREESHIP', N'Miễn phí vận chuyển toàn quốc cho đơn hàng từ 10 triệu', N'Amount', 150000, 10000000, DATEADD(month, 6, GETDATE()), 65, 200, 1, GETDATE()),
    (N'WELCOME100K', N'Voucher 100K dành tặng khách hàng mua lần đầu', N'Amount', 100000, 5000000, DATEADD(month, 1, GETDATE()), 89, 500, 1, GETDATE()),
    (N'VIP2026', N'Khuyến mãi VIP 2026 - Giảm 15% tối đa 3.000.000đ', N'Percent', 15, 5000000, DATEADD(month, 12, GETDATE()), 0, 50, 1, GETDATE()),
    (N'GIAM10', N'Giảm 10% tổng đơn hàng', N'Percent', 10, 1000000, DATEADD(month, 3, GETDATE()), 0, 100, 1, GETDATE()),
    (N'WELCOME50K', N'Giảm trực tiếp 50.000đ cho đơn từ 200.000đ', N'Amount', 50000, 200000, DATEADD(month, 6, GETDATE()), 0, 500, 1, GETDATE());
    PRINT N'==> Đã thêm dữ liệu mẫu cho bảng Coupons.';
END
GO

-- =============================================================================
-- 4. KIỂM TRA TỔNG QUAN DỮ LIỆU ĐÃ TẠO
-- =============================================================================
PRINT N'=====================================================';
PRINT N'THỐNG KÊ DỮ LIỆU LAPTOPSTOREDB:';
SELECT 'Brands' AS [Table], COUNT(*) AS [TotalRows] FROM [dbo].[Brands]
UNION ALL
SELECT 'Categories', COUNT(*) FROM [dbo].[Categories]
UNION ALL
SELECT 'Products', COUNT(*) FROM [dbo].[Products]
UNION ALL
SELECT 'Orders', COUNT(*) FROM [dbo].[Orders]
UNION ALL
SELECT 'OrderDetails', COUNT(*) FROM [dbo].[OrderDetails]
UNION ALL
SELECT 'Reviews', COUNT(*) FROM [dbo].[Reviews]
UNION ALL
SELECT 'Users', COUNT(*) FROM [dbo].[Users]
UNION ALL
SELECT 'Coupons', COUNT(*) FROM [dbo].[Coupons]
UNION ALL
SELECT 'CartItems', COUNT(*) FROM [dbo].[CartItems];
PRINT N'=====================================================';
GO
