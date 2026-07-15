-- 1. TẠO DATABASE MỚI (Chạy dòng này trước nếu chưa có DB)
CREATE DATABASE RestaurantReservationDB;
GO

USE RestaurantReservationDB;
GO

-- ==========================================
-- PHẦN 1: TẠO BẢNG (TABLES)
-- ==========================================
CREATE TABLE [Role] (
    RoleId INT PRIMARY KEY IDENTITY(1,1),
    RoleName VARCHAR(50) NOT NULL
);

CREATE TABLE [User] (
    UserId INT PRIMARY KEY IDENTITY(1,1),
    Username VARCHAR(50) NOT NULL UNIQUE,
    PasswordHash VARCHAR(255) NOT NULL,
    Email VARCHAR(100) UNIQUE,
    Phone VARCHAR(20)
);

CREATE TABLE UserRole (
    UserId INT,
    RoleId INT,
    PRIMARY KEY (UserId, RoleId),
    FOREIGN KEY (UserId) REFERENCES [User](UserId),
    FOREIGN KEY (RoleId) REFERENCES [Role](RoleId)
);

CREATE TABLE Restaurant (
    RestaurantId INT PRIMARY KEY IDENTITY(1,1),
    Name NVARCHAR(100) NOT NULL,
    Address NVARCHAR(255),
    Phone VARCHAR(20),
    OpenTime TIME,
    CloseTime TIME
);

CREATE TABLE DiningTable (
    TableId INT PRIMARY KEY IDENTITY(1,1),
    RestaurantId INT,
    TableNumber VARCHAR(20) NOT NULL,
    Capacity INT NOT NULL,
    Status VARCHAR(20) DEFAULT 'Available',
    FOREIGN KEY (RestaurantId) REFERENCES Restaurant(RestaurantId)
);

CREATE TABLE Category (
    CategoryId INT PRIMARY KEY IDENTITY(1,1),
    Name NVARCHAR(100) NOT NULL
);

CREATE TABLE MenuItem (
    MenuItemId INT PRIMARY KEY IDENTITY(1,1),
    RestaurantId INT,
    CategoryId INT,
    Name NVARCHAR(100) NOT NULL,
    Price DECIMAL(18,2) NOT NULL,
    Available BIT DEFAULT 1,
    FOREIGN KEY (RestaurantId) REFERENCES Restaurant(RestaurantId),
    FOREIGN KEY (CategoryId) REFERENCES Category(CategoryId)
);

CREATE TABLE Reservation (
    ReservationId INT PRIMARY KEY IDENTITY(1,1),
    UserId INT,
    TableId INT,
    [Date] DATE NOT NULL,
    StartTime TIME NOT NULL,
    EndTime TIME NOT NULL,
    GuestCount INT NOT NULL,
    Status VARCHAR(50) DEFAULT 'Pending',
    FOREIGN KEY (UserId) REFERENCES [User](UserId),
    FOREIGN KEY (TableId) REFERENCES DiningTable(TableId)
);

CREATE TABLE [Order] (
    OrderId INT PRIMARY KEY IDENTITY(1,1),
    ReservationId INT UNIQUE,
    TotalAmount DECIMAL(18,2) DEFAULT 0,
    PaymentStatus VARCHAR(50) DEFAULT 'Unpaid',
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (ReservationId) REFERENCES Reservation(ReservationId)
);

CREATE TABLE OrderItem (
    OrderItemId INT PRIMARY KEY IDENTITY(1,1),
    OrderId INT,
    MenuItemId INT,
    Quantity INT NOT NULL,
    PriceAtPurchase DECIMAL(18,2) NOT NULL,
    FOREIGN KEY (OrderId) REFERENCES [Order](OrderId),
    FOREIGN KEY (MenuItemId) REFERENCES MenuItem(MenuItemId)
);

CREATE TABLE Notification (
    NotificationId INT PRIMARY KEY IDENTITY(1,1),
    UserId INT,
    Title NVARCHAR(100) NOT NULL,
    Message NVARCHAR(500) NOT NULL,
    IsRead BIT DEFAULT 0,
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (UserId) REFERENCES [User](UserId)
);
GO

-- ==========================================
-- PHẦN 2: THÊM DỮ LIỆU MẪU (INSERT)
-- ==========================================

-- Thêm Roles
INSERT INTO [Role] (RoleName) VALUES ('Admin'), ('Manager'), ('Customer');

-- Thêm Users
INSERT INTO [User] (Username, PasswordHash, Email, Phone) 
VALUES 
('nguyenvana', 'hashed_pw_1', 'vana@gmail.com', '0901234567'),
('tranvib', 'hashed_pw_2', 'vib@gmail.com', '0912345678');

-- Phân quyền
INSERT INTO UserRole (UserId, RoleId) VALUES (1, 3), (2, 1);

-- Thêm Nhà hàng
INSERT INTO Restaurant (Name, Address, Phone, OpenTime, CloseTime)
VALUES (N'Nhà hàng Hoa Sen', N'123 Nguyễn Huệ, Quận 1, TP.HCM', '0283456789', '08:00', '22:00');

-- Thêm Bàn ăn (Cho nhà hàng 1)
INSERT INTO DiningTable (RestaurantId, TableNumber, Capacity, Status)
VALUES 
(1, 'T01', 2, 'Available'),
(1, 'T02', 4, 'Available'),
(1, 'V01', 10, 'Available'); -- Bàn VIP

-- Thêm Danh mục món ăn
INSERT INTO Category (Name) VALUES (N'Món Khai Vị'), (N'Món Chính'), (N'Nước Uống');

-- Thêm Món ăn
INSERT INTO MenuItem (RestaurantId, CategoryId, Name, Price, Available)
VALUES 
(1, 1, N'Gỏi ngó sen tôm thịt', 85000, 1),
(1, 2, N'Bò lúc lắc khoai tây', 150000, 1),
(1, 3, N'Trà đá', 10000, 1);

-- Khách hàng "nguyenvana" đặt bàn
INSERT INTO Reservation (UserId, TableId, [Date], StartTime, EndTime, GuestCount, Status)
VALUES (1, 2, '2026-07-20', '19:00', '21:00', 4, 'Confirmed');

-- Cập nhật trạng thái bàn
UPDATE DiningTable SET Status = 'Occupied' WHERE TableId = 2;

-- Tạo Order cho lịch đặt bàn vừa rồi
INSERT INTO [Order] (ReservationId, TotalAmount, PaymentStatus)
VALUES (1, 395000, 'Unpaid');

-- Khách gọi món (2 Gỏi, 1 Bò, 4 Trà đá)
INSERT INTO OrderItem (OrderId, MenuItemId, Quantity, PriceAtPurchase)
VALUES 
(1, 1, 2, 85000),  -- 2 * 85,000 = 170,000
(1, 2, 1, 150000), -- 1 * 150,000 = 150,000
(1, 3, 4, 10000);  -- 4 * 10,000 = 40,000
-- Total: 360,000 (Giả sử TotalAmount ở trên có tính thêm 35k phí dịch vụ)

-- Gửi thông báo cho khách
INSERT INTO Notification (UserId, Title, Message)
VALUES (1, N'Đặt bàn thành công', N'Bàn T02 của bạn đã được xác nhận vào lúc 19:00 ngày 20/07/2026.');
GO