-- 1. Xóa database cũ nếu đã tạo nhầm trước đó để làm lại sạch sẽ từ đầu
USE master;
GO
IF EXISTS (SELECT name FROM sys.databases WHERE name = N'MusicalStore_25DH110239')
BEGIN
    ALTER DATABASE MusicalStore_25DH110239 SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE MusicalStore_25DH110239;
END
GO

-- 2. Tạo mới Database cho đề tài 18-Musical Store
CREATE DATABASE MusicalStore_25DH110239;
GO
USE MusicalStore_25DH110239;
GO

-- 3. Bảng User: Lưu tài khoản đăng nhập cho cả Admin và Khách hàng
CREATE TABLE [User] (
    Username NVARCHAR(50) PRIMARY KEY,
    Password NVARCHAR(255) NOT NULL,
    UserRole NVARCHAR(20) NOT NULL DEFAULT 'Customer', -- Phân quyền: 'Admin' hoặc 'Customer'
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME DEFAULT GETDATE(),
    FullName NVARCHAR(100),
    Email NVARCHAR(100),
    Phone NVARCHAR(15)
);

-- 4. Bảng Customer: Lưu hồ sơ thông tin nhận hàng của khách
CREATE TABLE Customer (
    CustomerID INT IDENTITY(1,1) PRIMARY KEY,
    CustomerName NVARCHAR(100) NOT NULL,
    CustomerPhone NVARCHAR(15) NOT NULL,
    CustomerEmail NVARCHAR(100),
    CustomerAddress NVARCHAR(255) NOT NULL,
    Username NVARCHAR(50) UNIQUE FOREIGN KEY REFERENCES [User](Username)
);

-- 5. Bảng Category: Lưu danh sách các thương hiệu nhạc cụ
CREATE TABLE Category (
    CategoryID INT IDENTITY(1,1) PRIMARY KEY,
    CategoryName NVARCHAR(100) NOT NULL,
    Description NVARCHAR(255)
);

-- 6. Bảng Product: Lưu thông tin chi tiết nhạc cụ, giá, tồn kho, % giảm giá và link ảnh
CREATE TABLE Product (
    ProductID INT IDENTITY(1,1) PRIMARY KEY,
    CategoryID INT FOREIGN KEY REFERENCES Category(CategoryID),
    ProductName NVARCHAR(150) NOT NULL,
    ProductDescription NVARCHAR(MAX),
    ProductPrice DECIMAL(18, 0) NOT NULL,
    ProductImage NVARCHAR(500), -- Lưu URL ảnh trực tiếp hoặc tên file ảnh
    ProductStock INT NOT NULL DEFAULT 0,
    DiscountPercent INT NOT NULL DEFAULT 0
);

-- 7. Bảng Voucher: Lưu mã giảm giá
CREATE TABLE Voucher (
    VoucherID INT IDENTITY(1,1) PRIMARY KEY,
    Code NVARCHAR(50) UNIQUE NOT NULL,
    DiscountPercent INT NOT NULL,
    Description NVARCHAR(255),
    MinOrder DECIMAL(18, 0) NOT NULL DEFAULT 0,
    ExpiryDate DATETIME NOT NULL,
    IsActive BIT NOT NULL DEFAULT 1
);

-- 8. Bảng Order: Lưu thông tin tổng quan đơn đặt hàng
CREATE TABLE [Order] (
    OrderID INT IDENTITY(1,1) PRIMARY KEY,
    CustomerID INT FOREIGN KEY REFERENCES Customer(CustomerID),
    OrderDate DATETIME DEFAULT GETDATE(),
    TotalAmount DECIMAL(18, 0) NOT NULL,
    PaymentStatus NVARCHAR(50) DEFAULT N'Chưa thanh toán',
    AddressDelivery NVARCHAR(255) NOT NULL,
    ReceiverName NVARCHAR(100) NOT NULL,
    ReceiverPhone NVARCHAR(15) NOT NULL,
    ReceiverEmail NVARCHAR(100),
    PaymentMethod NVARCHAR(50) NOT NULL,
    ShippingFee DECIMAL(18, 0) DEFAULT 0,
    OrderStatus NVARCHAR(50) DEFAULT N'Chờ xác nhận',
    VoucherCode NVARCHAR(50),
    DiscountAmount DECIMAL(18, 0) DEFAULT 0
);

-- 9. Bảng OrderDetail: Lưu từng sản phẩm trong một đơn hàng
CREATE TABLE OrderDetail (
    ID INT IDENTITY(1,1) PRIMARY KEY,
    ProductID INT FOREIGN KEY REFERENCES Product(ProductID),
    OrderID INT FOREIGN KEY REFERENCES [Order](OrderID),
    Quantity INT NOT NULL,
    UnitPrice DECIMAL(18, 0) NOT NULL,
    ProductName NVARCHAR(150)
);

-- 10. Thêm dữ liệu mẫu Tài khoản (Admin & Khách hàng)
INSERT INTO [User] (Username, Password, UserRole, IsActive, FullName, Email, Phone)
VALUES 
('admin', '123456', 'Admin', 1, N'Quản Trị Viên', 'admin@musicalstore.vn', '0901234567'),
('trung25dh', '123456', 'Customer', 1, N'Hứa Vũ Trung', 'trung@gmail.com', '0909888777');

INSERT INTO Customer (CustomerName, CustomerPhone, CustomerEmail, CustomerAddress, Username)
VALUES (N'Hứa Vũ Trung', '0909888777', 'trung@gmail.com', N'Quận 5, TP. Hồ Chí Minh', 'trung25dh');

-- 11. Thêm dữ liệu mẫu 5 Hãng nhạc cụ
INSERT INTO Category (CategoryName, Description)
VALUES 
(N'Yamaha', N'Đàn Piano, Guitar và Nhạc cụ Yamaha chính hãng'),
(N'Fender', N'Guitar điện, Guitar Bass và Amplifier Fender'),
(N'Roland', N'Piano điện và Trống điện tử Roland'),
(N'Casio', N'Đàn Organ và Piano điện phổ thông Casio'),
(N'Gibson', N'Guitar Acoustic và Electric cao cấp Gibson');

-- 12. Thêm dữ liệu mẫu 8 Nhạc cụ kèm link hình ảnh thực tế sắc nét
INSERT INTO Product (CategoryID, ProductName, ProductDescription, ProductPrice, ProductImage, ProductStock, DiscountPercent)
VALUES 
(1, N'Đàn Guitar Acoustic Yamaha F310', N'Dáng Dreadnought truyền thống, mặt gỗ vân sam, âm thanh vang sáng phù hợp cho người mới tập.', 3490000, 'https://images.unsplash.com/photo-1510915361894-db8b60106cb1?auto=format&fit=crop&w=600&q=80', 20, 10),
(1, N'Đàn Grand Piano Yamaha C3X', N'88 phím nặng chuẩn biểu diễn hòa nhạc, âm sắc trầm ấm sang trọng.', 45000000, 'https://images.unsplash.com/photo-1520523839897-bd0b52f945a0?auto=format&fit=crop&w=600&q=80', 5, 5),
(2, N'Đàn Guitar Điện Fender Player Stratocaster', N'Thân gỗ Alder, 3 pickup Single-Coil huyền thoại của Fender.', 19800000, 'https://images.unsplash.com/photo-1564186763535-ebb21ef5277f?auto=format&fit=crop&w=600&q=80', 8, 0),
(2, N'Đàn Guitar Bass Fender Jazz Bass', N'Cần đàn mảnh dễ bấm, âm trầm chắc khỏe cho ban nhạc.', 21500000, 'https://images.unsplash.com/photo-1550291652-6ea9114a47b1?auto=format&fit=crop&w=600&q=80', 6, 10),
(3, N'Bộ Trống Điện Tử Roland TD-17KVX', N'Mặt lưới Mesh Head cảm giác đánh chân thực, tích hợp Bluetooth.', 32500000, 'https://images.unsplash.com/photo-1519892300165-cb5542fb47c7?auto=format&fit=crop&w=600&q=80', 7, 15),
(4, N'Đàn Organ Synthesizer Casio CT-X700', N'Nguồn âm thanh AiX, 600 âm sắc, 195 điệu nhạc đa dạng.', 4850000, 'https://images.unsplash.com/photo-1552422535-c45813c61732?auto=format&fit=crop&w=600&q=80', 25, 0),
(5, N'Đàn Guitar Gibson Les Paul Standard 50s', N'Mặt gỗ Maple chạm khắc, âm thanh ấm dày đặc trưng dòng Rock/Blues.', 58000000, 'https://images.unsplash.com/photo-1516924962500-2b4b3b99ea02?auto=format&fit=crop&w=600&q=80', 4, 5),
(5, N'Đàn Guitar Acoustic Gibson Hummingbird', N'Thiết kế cổ điển huyền thoại, thùng đàn gỗ Mahogany nguyên khối.', 64000000, 'https://images.unsplash.com/photo-1525201548942-d8732f6617a0?auto=format&fit=crop&w=600&q=80', 3, 0);

-- 13. Thêm dữ liệu mẫu Mã giảm giá
INSERT INTO Voucher (Code, DiscountPercent, Description, MinOrder, ExpiryDate, IsActive)
VALUES 
('MUSIC10', 10, N'Giảm 10% cho đơn hàng từ 2.000.000đ', 2000000, '2026-12-31', 1),
('WELCOME20', 20, N'Giảm 20% mừng khai trương cho đơn từ 5.000.000đ', 5000000, '2026-12-31', 1);
GO