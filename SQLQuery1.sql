
IF OBJECT_ID('dbo.Reports', 'U') IS NOT NULL
    DROP TABLE dbo.Reports;

IF OBJECT_ID('dbo.InventoryTransactions', 'U') IS NOT NULL
    DROP TABLE dbo.InventoryTransactions;

IF OBJECT_ID('dbo.OrderDetails', 'U') IS NOT NULL
    DROP TABLE dbo.OrderDetails;

IF OBJECT_ID('dbo.Orders', 'U') IS NOT NULL
    DROP TABLE dbo.Orders;

IF OBJECT_ID('dbo.ProductImages', 'U') IS NOT NULL
    DROP TABLE dbo.ProductImages;

IF OBJECT_ID('dbo.Products', 'U') IS NOT NULL
    DROP TABLE dbo.Products;

IF OBJECT_ID('dbo.Categories', 'U') IS NOT NULL
    DROP TABLE dbo.Categories;

IF OBJECT_ID('dbo.Suppliers', 'U') IS NOT NULL
    DROP TABLE dbo.Suppliers;

IF OBJECT_ID('dbo.Users', 'U') IS NOT NULL
    DROP TABLE dbo.Users;




CREATE TABLE Users (
    UserID INT PRIMARY KEY IDENTITY,
    Username VARCHAR(50) NOT NULL UNIQUE,
    Password VARCHAR(255) NOT NULL,
    Email VARCHAR(100) NOT NULL UNIQUE,
    FirstName VARCHAR(50) NOT NULL,
    LastName VARCHAR(50) NOT NULL,
    Role VARCHAR(20) NOT NULL CHECK (Role IN ('Admin', 'Manager', 'Employee', 'Customer')),
    RegistrationDate DATETIME DEFAULT GETDATE(),
    LastLogin DATETIME NULL
);


CREATE TABLE Suppliers (
    SupplierID INT PRIMARY KEY IDENTITY,
    CompanyName VARCHAR(100) NOT NULL,
    ContactName VARCHAR(100) NULL,
    ContactEmail VARCHAR(100) NULL,
    ContactPhone VARCHAR(20) NULL,
    Address NVARCHAR(MAX) NULL
);


CREATE TABLE Categories (
    CategoryID INT PRIMARY KEY IDENTITY,
    CategoryName VARCHAR(50) NOT NULL,
    Description NVARCHAR(MAX) NULL,
    ParentCategoryID INT NULL
);


ALTER TABLE Categories
ADD CONSTRAINT FK_Categories_ParentCategory 
FOREIGN KEY (ParentCategoryID) REFERENCES Categories(CategoryID);

CREATE TABLE Products (
    ProductID INT PRIMARY KEY IDENTITY,
    ProductName VARCHAR(100) NOT NULL,
    CategoryID INT NOT NULL,
    Description NVARCHAR(MAX) NULL,
    Price DECIMAL(10, 2) NOT NULL,
    QuantityInStock INT NOT NULL DEFAULT 0,
    CreatedDate DATETIME DEFAULT GETDATE(),
    ModifiedDate DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (CategoryID) REFERENCES Categories(CategoryID)
);


CREATE TABLE ProductImages (
    ImageID INT PRIMARY KEY IDENTITY,
    ProductID INT NOT NULL,
    ImagePath VARCHAR(255) NOT NULL,
    IsPrimary BIT DEFAULT 0,
    UploadDate DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (ProductID) REFERENCES Products(ProductID) ON DELETE CASCADE
);


CREATE TABLE Orders (
    OrderID INT PRIMARY KEY IDENTITY,
    UserID INT NOT NULL,
    OrderDate DATETIME DEFAULT GETDATE(),
    TotalAmount DECIMAL(10, 2) NOT NULL,
    Status VARCHAR(20) DEFAULT 'Pending' CHECK (Status IN ('Pending', 'Processing', 'Shipped', 'Delivered', 'Cancelled')),
    PaymentMethod VARCHAR(50) NULL,
    ShippingAddress NVARCHAR(MAX) NOT NULL,
    DeliveryDate DATETIME NULL,
    FOREIGN KEY (UserID) REFERENCES Users(UserID)
);


CREATE TABLE OrderDetails (
    OrderDetailID INT PRIMARY KEY IDENTITY,
    OrderID INT NOT NULL,
    ProductID INT NOT NULL,
    Quantity INT NOT NULL,
    UnitPrice DECIMAL(10, 2) NOT NULL,
    Discount DECIMAL(5, 2) DEFAULT 0.00,
    FOREIGN KEY (OrderID) REFERENCES Orders(OrderID) ON DELETE CASCADE,
    FOREIGN KEY (ProductID) REFERENCES Products(ProductID)
);


CREATE TABLE InventoryTransactions (
    TransactionID INT PRIMARY KEY IDENTITY,
    ProductID INT NOT NULL,
    SupplierID INT NULL,
    TransactionType VARCHAR(20) NOT NULL CHECK (TransactionType IN ('Purchase', 'Sale', 'Adjustment', 'Return')),
    Quantity INT NOT NULL,
    TransactionDate DATETIME DEFAULT GETDATE(),
    Notes NVARCHAR(MAX) NULL,
    FOREIGN KEY (ProductID) REFERENCES Products(ProductID),
    FOREIGN KEY (SupplierID) REFERENCES Suppliers(SupplierID)
);


CREATE TABLE Reports (
    ReportID INT PRIMARY KEY IDENTITY,
    UserID INT NOT NULL,
    ReportType VARCHAR(50) NOT NULL,
    GeneratedDate DATETIME DEFAULT GETDATE(),
    Parameters NVARCHAR(MAX) NULL,
    FilePath VARCHAR(255) NULL,
    FOREIGN KEY (UserID) REFERENCES Users(UserID)
);