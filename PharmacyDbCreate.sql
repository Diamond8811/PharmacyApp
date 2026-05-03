-- создаём БД
CREATE DATABASE PharmacyDB;
GO
USE PharmacyDB;
GO

-- 1. Роли сотрудников
CREATE TABLE Roles (
    RoleID INT IDENTITY(1,1) PRIMARY KEY,
    RoleName NVARCHAR(50) NOT NULL UNIQUE
);

-- 2. Сотрудники
CREATE TABLE Employees (
    EmployeeID INT IDENTITY(1,1) PRIMARY KEY,
    FullName NVARCHAR(100) NOT NULL,
    RoleID INT NOT NULL,
    Login NVARCHAR(50) UNIQUE NOT NULL,
    PasswordHash NVARCHAR(255) NOT NULL,
    IsActive BIT DEFAULT 1,
    CONSTRAINT FK_Employees_Role FOREIGN KEY (RoleID) REFERENCES Roles(RoleID)
);

-- 3. Категории препаратов
CREATE TABLE Categories (
    CategoryID INT IDENTITY(1,1) PRIMARY KEY,
    CategoryName NVARCHAR(100) NOT NULL
);

-- 4. Лекарственные формы
CREATE TABLE DrugForms (
    FormID INT IDENTITY(1,1) PRIMARY KEY,
    FormName NVARCHAR(50) NOT NULL UNIQUE
);

-- 5. Лекарственные средства (основной справочник)
CREATE TABLE Drugs (
    DrugID INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(200) NOT NULL,
    CategoryID INT NOT NULL,
    Manufacturer NVARCHAR(150),
    FormID INT NOT NULL,
    Dosage NVARCHAR(50),
    Unit NVARCHAR(20),
    Price MONEY NOT NULL CHECK(Price > 0),
    CONSTRAINT FK_Drugs_Category FOREIGN KEY (CategoryID) REFERENCES Categories(CategoryID),
    CONSTRAINT FK_Drugs_Form FOREIGN KEY (FormID) REFERENCES DrugForms(FormID)
);

-- 6. Поставщики
CREATE TABLE Suppliers (
    SupplierID INT IDENTITY(1,1) PRIMARY KEY,
    SupplierName NVARCHAR(150) NOT NULL,
    ContactPerson NVARCHAR(100),
    Phone NVARCHAR(20),
    Address NVARCHAR(200)
);

-- 7. Партии товара (отслеживание по срокам годности)
CREATE TABLE Batches (
    BatchID INT IDENTITY(1,1) PRIMARY KEY,
    DrugID INT NOT NULL,
    BatchNumber NVARCHAR(50) NOT NULL,
    ExpiryDate DATE NOT NULL,
    Quantity INT NOT NULL CHECK(Quantity >= 0),
    CONSTRAINT FK_Batches_Drug FOREIGN KEY (DrugID) REFERENCES Drugs(DrugID)
);

-- 8. Поставки (шапка)
CREATE TABLE Deliveries (
    DeliveryID INT IDENTITY(1,1) PRIMARY KEY,
    SupplierID INT NOT NULL,
    DeliveryDate DATETIME2(0) NOT NULL DEFAULT GETDATE(),
    EmployeeID INT NOT NULL,
    CONSTRAINT FK_Deliveries_Supplier FOREIGN KEY (SupplierID) REFERENCES Suppliers(SupplierID),
    CONSTRAINT FK_Deliveries_Employee FOREIGN KEY (EmployeeID) REFERENCES Employees(EmployeeID)
);

-- 9. Строки поставки
CREATE TABLE Delivery_Items (
    DeliveryItemID INT IDENTITY(1,1) PRIMARY KEY,
    DeliveryID INT NOT NULL,
    DrugID INT NOT NULL,
    Quantity INT NOT NULL CHECK(Quantity > 0),
    CostPrice MONEY NOT NULL,
    BatchNumber NVARCHAR(50) NOT NULL,
    ExpiryDate DATE NOT NULL,
    CONSTRAINT FK_DelItem_Delivery FOREIGN KEY (DeliveryID) REFERENCES Deliveries(DeliveryID),
    CONSTRAINT FK_DelItem_Drug FOREIGN KEY (DrugID) REFERENCES Drugs(DrugID)
);

-- 10. Продажи (шапка чека)
CREATE TABLE Sales (
    SaleID INT IDENTITY(1,1) PRIMARY KEY,
    SaleDate DATETIME2(0) NOT NULL DEFAULT GETDATE(),
    EmployeeID INT NOT NULL,
    CONSTRAINT FK_Sales_Employee FOREIGN KEY (EmployeeID) REFERENCES Employees(EmployeeID)
);

-- 11. Строки чека
CREATE TABLE Sale_Items (
    SaleItemID INT IDENTITY(1,1) PRIMARY KEY,
    SaleID INT NOT NULL,
    DrugID INT NOT NULL,
    Quantity INT NOT NULL CHECK(Quantity > 0),
    Price MONEY NOT NULL,          -- цена на момент продажи
    CONSTRAINT FK_SaleItem_Sale FOREIGN KEY (SaleID) REFERENCES Sales(SaleID),
    CONSTRAINT FK_SaleItem_Drug FOREIGN KEY (DrugID) REFERENCES Drugs(DrugID)
);

-- Индексы для быстрой выборки FIFO
CREATE INDEX IX_Batches_DrugID ON Batches(DrugID);
CREATE INDEX IX_Batches_ExpiryDate ON Batches(ExpiryDate);
GO

-- ===================== ТРИГГЕРЫ =====================

-- Триггер 1. Пополнение партий при поступлении
CREATE TRIGGER trg_DeliveryItems_Insert
ON Delivery_Items
AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO Batches (DrugID, BatchNumber, ExpiryDate, Quantity)
    SELECT DrugID, BatchNumber, ExpiryDate, Quantity
    FROM inserted;
END;
GO

-- Триггер 2. Списание со склада при продаже (FIFO)
CREATE TRIGGER trg_SaleItems_Insert
ON Sale_Items
AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @SaleItemID INT, @DrugID INT, @QtyToDeduct INT;

    DECLARE sale_cursor CURSOR FOR
        SELECT SaleItemID, DrugID, Quantity FROM inserted;

    OPEN sale_cursor;
    FETCH NEXT FROM sale_cursor INTO @SaleItemID, @DrugID, @QtyToDeduct;

    WHILE @@FETCH_STATUS = 0
    BEGIN
        IF (SELECT ISNULL(SUM(Quantity),0) FROM Batches WHERE DrugID = @DrugID) < @QtyToDeduct
        BEGIN
            RAISERROR('Недостаточно товара на складе для DrugID = %d', 16, 1, @DrugID);
            ROLLBACK TRANSACTION;
            RETURN;
        END;

        DECLARE @BatchID INT, @BatchQty INT, @Remaining INT = @QtyToDeduct;

        DECLARE batch_cursor CURSOR FOR
            SELECT BatchID, Quantity FROM Batches
            WHERE DrugID = @DrugID AND Quantity > 0
            ORDER BY ExpiryDate ASC;

        OPEN batch_cursor;
        FETCH NEXT FROM batch_cursor INTO @BatchID, @BatchQty;

        WHILE @@FETCH_STATUS = 0 AND @Remaining > 0
        BEGIN
            IF @BatchQty <= @Remaining
            BEGIN
                UPDATE Batches SET Quantity = 0 WHERE BatchID = @BatchID;
                SET @Remaining = @Remaining - @BatchQty;
            END
            ELSE
            BEGIN
                UPDATE Batches SET Quantity = Quantity - @Remaining WHERE BatchID = @BatchID;
                SET @Remaining = 0;
            END;
            FETCH NEXT FROM batch_cursor INTO @BatchID, @BatchQty;
        END;

        CLOSE batch_cursor;
        DEALLOCATE batch_cursor;

        FETCH NEXT FROM sale_cursor INTO @SaleItemID, @DrugID, @QtyToDeduct;
    END;

    CLOSE sale_cursor;
    DEALLOCATE sale_cursor;
END;
GO

-- ================== ТЕСТОВЫЕ ДАННЫЕ ==================

-- Роли
INSERT INTO Roles (RoleName) VALUES (N'Заведующий'), (N'Фармацевт');

-- Сотрудники
INSERT INTO Employees (FullName, RoleID, Login, PasswordHash) 
VALUES 
(N'Иванов Иван Иванович', 1, 'admin', 'hash_admin'),
(N'Петрова Анна Сергеевна', 2, 'pharm1', 'hash_pharm1');

-- Категории
INSERT INTO Categories (CategoryName) 
VALUES (N'Антибиотики'), (N'Обезболивающие'), (N'Витамины');

-- Лекарственные формы
INSERT INTO DrugForms (FormName) 
VALUES (N'капсулы'), (N'таблетки'), (N'драже');

-- Препараты
INSERT INTO Drugs (Name, CategoryID, Manufacturer, FormID, Dosage, Unit, Price)
VALUES
(N'Амоксициллин', 1, N'Фармстандарт', 1, N'500 мг', N'упаковка', 120.00),
(N'Парацетамол', 2, N'Биосинтез', 2, N'500 мг', N'упаковка', 50.00),
(N'Аскорбиновая кислота', 3, N'Марбиофарм', 3, N'50 мг', N'упаковка', 30.00);

-- Поставщик
INSERT INTO Suppliers (SupplierName, ContactPerson, Phone, Address)
VALUES (N'ООО "ФармСнаб"', N'Сидоров П.В.', '+7-900-123-45-67', N'г. Москва, ул. Тверская, 1');

-- Начальные остатки (партии)
INSERT INTO Batches (DrugID, BatchNumber, ExpiryDate, Quantity)
VALUES
(1, 'BATCH0101', '2027-12-31', 100),
(2, 'BATCH0201', '2026-06-30', 200),
(3, 'BATCH0301', '2028-01-15', 150);

-- Поставка
INSERT INTO Deliveries (SupplierID, DeliveryDate, EmployeeID) 
VALUES (1, '2026-04-25 10:00', 1);

INSERT INTO Delivery_Items (DeliveryID, DrugID, Quantity, CostPrice, BatchNumber, ExpiryDate)
VALUES (1, 1, 50, 90.00, 'BATCH0102', '2028-06-01');

-- Продажа
INSERT INTO Sales (SaleDate, EmployeeID) 
VALUES ('2026-05-01 14:30', 2);

INSERT INTO Sale_Items (SaleID, DrugID, Quantity, Price)
VALUES (1, 2, 3, (SELECT Price FROM Drugs WHERE DrugID = 2));

-- Проверка остатков
SELECT 
    d.Name,
    b.BatchNumber,
    b.ExpiryDate,
    b.Quantity
FROM Batches b
JOIN Drugs d ON d.DrugID = b.DrugID
ORDER BY d.Name, b.ExpiryDate;