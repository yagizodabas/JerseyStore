-- Run against a NEW, empty JerseyStoreDb database. This script never drops existing data.
SET XACT_ABORT ON;
BEGIN TRANSACTION;
CREATE TABLE dbo.Products (
    Id int IDENTITY PRIMARY KEY,
    TeamName nvarchar(120) NOT NULL,
    Price decimal(18,2) NOT NULL CHECK (Price >= 0),
    Stock int NOT NULL CHECK (Stock >= 0)
);
CREATE TABLE dbo.Orders (
    Id int IDENTITY PRIMARY KEY,
    OrderNumber varchar(40) NOT NULL UNIQUE,
    CustomerName nvarchar(120) NOT NULL,
    Address nvarchar(500) NOT NULL,
    Phone nvarchar(30) NOT NULL,
    OrderDate datetime2 NOT NULL,
    TotalAmount decimal(18,2) NOT NULL CHECK (TotalAmount >= 0)
);
CREATE TABLE dbo.OrderItems (
    Id int IDENTITY PRIMARY KEY,
    OrderId int NOT NULL REFERENCES dbo.Orders(Id),
    ProductId int NOT NULL REFERENCES dbo.Products(Id),
    Quantity int NOT NULL CHECK (Quantity BETWEEN 1 AND 100),
    UnitPrice decimal(18,2) NOT NULL CHECK (UnitPrice >= 0),
    JerseyNumber int NULL CHECK (JerseyNumber BETWEEN 0 AND 99),
    PrintName nvarchar(40) NULL
);
CREATE INDEX IX_OrderItems_OrderId ON dbo.OrderItems(OrderId);
COMMIT;
