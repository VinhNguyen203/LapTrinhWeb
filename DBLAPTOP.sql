USE LaptopStoreDB;
GO

-- =========================================================
-- 1. LÀM SẠCH DỮ LIỆU CŨ VÀ ĐẶT LẠI BỘ ĐẾM ID
-- =========================================================
DELETE FROM OrderDetails;
DELETE FROM Reviews;
DELETE FROM Orders;
GO

DBCC CHECKIDENT ('Orders', RESEED, 0);
DBCC CHECKIDENT ('OrderDetails', RESEED, 0);
DBCC CHECKIDENT ('Reviews', RESEED, 0);
GO

-- =========================================================
-- 2. THÊM DANH SÁCH ĐƠN HÀNG (ORDERS) VỚI TÊN CÁC THÀNH VIÊN
-- =========================================================
INSERT INTO Orders (UserId, CustomerName, PhoneNumber, ShippingAddress, OrderDate, TotalAmount, Status)
VALUES 
('USER_01', N'Phùng Nguyễn Trọng An', '0901000001', N'12 Cầu Giấy, Hà Nội', DATEADD(day, -30, GETDATE()), 35990000, N'Đã giao'),
('USER_02', N'Lương Thế Anh', '0901000002', N'45 Nguyễn Trãi, Hà Nội', DATEADD(day, -29, GETDATE()), 45000000, N'Đã giao'),
('USER_03', N'Nguyễn Duy Thị Nguyệt Ánh', '0901000003', N'88 Chùa Bộc, Hà Nội', DATEADD(day, -28, GETDATE()), 25000000, N'Đang giao'),
('USER_04', N'Nguyễn Phúc Anh', '0901000004', N'15 Hồ Tùng Mậu, Hà Nội', DATEADD(day, -27, GETDATE()), 52000000, N'Đã giao'),
('USER_05', N'Phạm Thành Biên', '0901000005', N'10 Đại Cồ Việt, Hà Nội', DATEADD(day, -26, GETDATE()), 18500000, N'Đang xử lý'),
('USER_06', N'Mai Thiên Chiếu', '0901000006', N'25 Lê Văn Lương, Hà Nội', DATEADD(day, -25, GETDATE()), 31000000, N'Đã giao'),
('USER_07', N'Bùi Đức Dũng', '0901000007', N'50 Giải Phóng, Hà Nội', DATEADD(day, -24, GETDATE()), 27500000, N'Đã hủy'),
('USER_08', N'Phạm Tiến Dũng', '0901000008', N'68 Tây Sơn, Hà Nội', DATEADD(day, -23, GETDATE()), 42000000, N'Đã giao'),
('USER_09', N'Hoàng Hải Dương', '0901000009', N'99 Hoàng Quốc Việt, Hà Nội', DATEADD(day, -22, GETDATE()), 15000000, N'Đang giao'),
('USER_10', N'Nguyễn Quý Dương', '0901000010', N'50 Phố Huế, Hà Nội', DATEADD(day, -21, GETDATE()), 38900000, N'Đã giao'),
('USER_11', N'Đặng Xuân Duy', '0901000011', N'120 Trương Định, Hà Nội', DATEADD(day, -20, GETDATE()), 29900000, N'Đang xử lý'),
('USER_12', N'Vũ Đức Duy', '0901000012', N'15 Nguyễn Chí Thanh, Hà Nội', DATEADD(day, -19, GETDATE()), 35990000, N'Đã giao'),
('USER_13', N'Bùi Hải Đăng', '0901000013', N'33 Xuân Thủy, Hà Nội', DATEADD(day, -18, GETDATE()), 45000000, N'Đã giao'),
('USER_14', N'Đinh Thành Đạt', '0901000014', N'42 Phạm Văn Đồng, Hà Nội', DATEADD(day, -17, GETDATE()), 25000000, N'Đang giao'),
('USER_15', N'Nguyễn Tiến Đạt', '0901000015', N'78 Thái Hà, Hà Nội', DATEADD(day, -16, GETDATE()), 52000000, N'Đã giao'),
('USER_16', N'Trần Đình Đạt', '0901000016', N'90 Láng Hạ, Hà Nội', DATEADD(day, -15, GETDATE()), 18500000, N'Đang xử lý'),
('USER_17', N'Nguyễn Hoàng Điệp', '0901000017', N'12 Tôn Đức Thắng, Hà Nội', DATEADD(day, -14, GETDATE()), 31000000, N'Đã giao'),
('USER_18', N'Phùng Đỗ Minh Đức', '0901000018', N'45 Kim Mã, Hà Nội', DATEADD(day, -13, GETDATE()), 27500000, N'Đã giao'),
('USER_19', N'Nguyễn Hữu Trường Giang', '0901000019', N'88 Đội Cấn, Hà Nội', DATEADD(day, -12, GETDATE()), 42000000, N'Đang giao'),
('USER_20', N'Nguyễn Vũ Trường Giang', '0901000020', N'19 Hoàng Cầu, Hà Nội', DATEADD(day, -11, GETDATE()), 15000000, N'Đã giao'),
('USER_21', N'Hoàng Minh Hào', '0901000021', N'22 Nguyễn Khang, Hà Nội', DATEADD(day, -10, GETDATE()), 38900000, N'Đã giao'),
('USER_22', N'Đào Quang Hiến', '0901000022', N'56 Trần Duy Hưng, Hà Nội', DATEADD(day, -9, GETDATE()), 29900000, N'Đang xử lý'),
('USER_23', N'Nguyễn Vũ Công Huân', '0901000023', N'77 Trung Kính, Hà Nội', DATEADD(day, -8, GETDATE()), 35990000, N'Đã giao'),
('USER_24', N'Lê Thành Hưng', '0901000024', N'123 Cầu Giấy, Hà Nội', DATEADD(day, -7, GETDATE()), 45000000, N'Đã giao'),
('USER_25', N'Phạm Quang Hưng', '0901000025', N'34 Yên Hòa, Hà Nội', DATEADD(day, -6, GETDATE()), 25000000, N'Đã giao'),
('USER_26', N'Vũ Đoàn Gia Huy', '0901000026', N'67 Quan Hoa, Hà Nội', DATEADD(day, -5, GETDATE()), 52000000, N'Đang giao'),
('USER_27', N'Vũ Quang Huy', '0901000027', N'89 Mỹ Đình, Hà Nội', DATEADD(day, -4, GETDATE()), 18500000, N'Đã giao'),
('USER_28', N'Đàm Nhật Khánh', '0901000028', N'12 Hồ Tùng Mậu, Hà Nội', DATEADD(day, -3, GETDATE()), 31000000, N'Đang xử lý'),
('USER_29', N'Nguyễn Viết Khanh', '0901000029', N'44 Cầu Diễn, Hà Nội', DATEADD(day, -2, GETDATE()), 27500000, N'Đã giao'),
('USER_30', N'Trần Ngọc Khánh', '0901000030', N'55 Lê Đức Thọ, Hà Nội', DATEADD(day, -1, GETDATE()), 42000000, N'Đã giao');
GO

-- =========================================================
-- 3. THÊM CHI TIẾT ĐƠN HÀNG (ORDERDETAILS)
-- =========================================================
INSERT INTO OrderDetails (OrderId, ProductId, Quantity, UnitPrice)
SELECT Id, 1, 1, TotalAmount FROM Orders;
GO

-- =========================================================
-- 4. THÊM ĐÁNH GIÁ (REVIEWS) VỚI CÁC THÀNH VIÊN CÒN LẠI
-- =========================================================
INSERT INTO Reviews (ProductId, UserId, UserName, Rating, Comment, CreatedAt)
VALUES 
(1, 'USER_31', N'Lê Đức Khoa', 5, N'Sản phẩm tuyệt vời, dùng rất mượt mà.', DATEADD(day, -15, GETDATE())),
(2, 'USER_32', N'Lê Đình Long', 4, N'Thiết kế đẹp, pin ổn định.', DATEADD(day, -14, GETDATE())),
(3, 'USER_33', N'Nguyễn Đức Mạnh', 5, N'Cấu hình mạnh mẽ, chiến game cực đỉnh.', DATEADD(day, -13, GETDATE())),
(4, 'USER_34', N'Nguyễn Minh', 3, N'Tạm ổn trong tầm giá.', DATEADD(day, -12, GETDATE())),
(5, 'USER_35', N'Phùng Quang Minh', 5, N'Giao hàng nhanh, nhân viên nhiệt tình.', DATEADD(day, -11, GETDATE())),
(1, 'USER_36', N'Phan Thanh Nam', 4, N'Rất hài lòng với chất lượng máy.', DATEADD(day, -10, GETDATE())),
(2, 'USER_37', N'Hoàng Tiến Quân', 2, N'Cần cải thiện thêm về phần tản nhiệt.', DATEADD(day, -9, GETDATE())),
(3, 'USER_38', N'Lê Đức Tài', 5, N'Tuyệt đỉnh công nghệ!', DATEADD(day, -8, GETDATE())),
(4, 'USER_39', N'Trần Mạnh Tài', 4, N'Màn hình nét, bàn phím gõ êm.', DATEADD(day, -7, GETDATE())),
(5, 'USER_40', N'Phạm Văn Thành', 5, N'Đáng tiền từng xu.', DATEADD(day, -6, GETDATE())),
(1, 'USER_41', N'Phan Nhật Phương Thảo', 5, N'Máy đẹp, nhẹ nhàng thích hợp cho học tập.', DATEADD(day, -5, GETDATE())),
(2, 'USER_42', N'Nguyễn Huy Tiến', 3, N'Bình thường, không có gì quá nổi bật.', DATEADD(day, -4, GETDATE())),
(3, 'USER_43', N'Đàm Đức Trung', 5, N'Chạy mượt các ứng dụng nặng.', DATEADD(day, -3, GETDATE())),
(4, 'USER_44', N'Đỗ Hoàng Trung', 4, N'Sài ổn, giao hàng nhanh.', DATEADD(day, -2, GETDATE())),
(5, 'USER_45', N'Nguyễn Tất Tú', 5, N'Leader duyệt đơn cực kỳ uy tín!', DATEADD(day, -1, GETDATE())),
(1, 'USER_46', N'Hoàng Bảo Uyên', 5, N'Màu sắc máy sang trọng, rất ưng ý.', GETDATE()),
(2, 'USER_47', N'Hoàng Thị Hải Vân', 4, N'Dùng tốt, màn hình sáng rõ.', GETDATE()),
(3, 'USER_48', N'Nguyễn Thành Vinh', 5, N'Hỗ trợ kỹ thuật rất nhiệt tình.', GETDATE()),
(4, 'USER_49', N'Phan Huy Vĩnh', 5, N'Hoàn hảo không điểm chê!', GETDATE());
GO