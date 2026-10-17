# Thư mục Dữ liệu & Cấu hình Cơ sở dữ liệu (Data & Database)
**Phụ trách:** Tú (Leader, Kiến trúc, Xác thực & Phân quyền)

---

### 1. Kiến trúc Cơ sở dữ liệu
- **DbContext:** `LaptopShop.Models.AppDbContext` (kế thừa `Microsoft.EntityFrameworkCore.DbContext`)
- **Hệ quản trị CSDL:** Microsoft SQL Server
- **Cơ chế khởi tạo & Seeding:**
  - Script SQL cấu trúc & dữ liệu khởi tạo: [LaptopStoreDB.sql](file:///d:/CODE/WEB/LapTrinhWeb-main/LaptopStoreDB.sql)
  - Dữ liệu mẫu đơn hàng & thành viên: [DBLAPTOP.sql](file:///d:/CODE/WEB/LapTrinhWeb-main/DBLAPTOP.sql)
  - Class tự động Seed dữ liệu khi khởi động ứng dụng: [DbSeeder.cs](file:///d:/CODE/WEB/LapTrinhWeb-main/Models/DbSeeder.cs)

### 2. Các bảng trong hệ thống
1. `Users`: Tài khoản người dùng (Admin, Customer), mật khẩu mã hóa BCrypt.
2. `Products`: Danh sách Laptop, thông số kỹ thuật (CPU, RAM, GPU), số lượng tồn kho.
3. `Categories`: Danh mục sản phẩm (Gaming, Văn phòng, Đồ họa,...).
4. `Brands`: Các hãng sản xuất (Dell, ASUS, Apple, Lenovo, HP, Acer,...).
5. `Orders`: Đơn hàng, địa chỉ, số điện thoại, phương thức thanh toán, trạng thái.
6. `OrderDetails`: Chi tiết từng sản phẩm trong đơn hàng.
7. `Reviews`: Đánh giá 1-5 sao và nhận xét của khách hàng.
8. `Coupons`: Mã giảm giá, hạn sử dụng, số lần dùng, phần trăm/số tiền giảm.

### 3. Tài khoản mặc định hệ thống
- **Quản trị viên (Admin):**
  - Email: `admin@laptopshop.vn`
  - Mật khẩu: `Admin@123`
- **Khách hàng mẫu (Customer):**
  - Email: `customer@laptopshop.vn`
  - Mật khẩu: `Customer@123`
