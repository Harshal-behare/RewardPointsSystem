
-- =============================================
-- Table: Departments
-- =============================================
CREATE TABLE Departments (
    DepartmentId INT IDENTITY(1,1) PRIMARY KEY,
    DepartmentName NVARCHAR(100) NOT NULL,
    DepartmentCode NVARCHAR(20) UNIQUE NOT NULL,
    Description NVARCHAR(500),
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    ModifiedDate DATETIME2 NULL
);

-- =============================================
-- Table: Employees
-- =============================================
CREATE TABLE Employees (
    EmployeeId INT IDENTITY(1,1) PRIMARY KEY,
    EmployeeCode NVARCHAR(50) UNIQUE NOT NULL,
    FirstName NVARCHAR(100) NOT NULL,
    LastName NVARCHAR(100) NOT NULL,
    Email NVARCHAR(255) UNIQUE NOT NULL,
    PhoneNumber NVARCHAR(20),
    DepartmentId INT NOT NULL,
    ManagerId INT NULL,
    HireDate DATE NOT NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    ModifiedDate DATETIME2 NULL,
    CONSTRAINT FK_Employees_Department FOREIGN KEY (DepartmentId) 
        REFERENCES Departments(DepartmentId),
    CONSTRAINT FK_Employees_Manager FOREIGN KEY (ManagerId) 
        REFERENCES Employees(EmployeeId)
);

-- =============================================
-- Table: RewardCategories
-- =============================================
CREATE TABLE RewardCategories (
    CategoryId INT IDENTITY(1,1) PRIMARY KEY,
    CategoryName NVARCHAR(100) NOT NULL,
    CategoryCode NVARCHAR(20) UNIQUE NOT NULL,
    Description NVARCHAR(500),
    PointMultiplier DECIMAL(3,2) NOT NULL DEFAULT 1.00,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    ModifiedDate DATETIME2 NULL
);

-- =============================================
-- Table: RewardTypes
-- =============================================
CREATE TABLE RewardTypes (
    RewardTypeId INT IDENTITY(1,1) PRIMARY KEY,
    RewardName NVARCHAR(100) NOT NULL,
    RewardCode NVARCHAR(20) UNIQUE NOT NULL,
    Description NVARCHAR(500),
    BasePoints INT NOT NULL,
    CategoryId INT NOT NULL,
    ValidFrom DATE NOT NULL,
    ValidTo DATE NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    ModifiedDate DATETIME2 NULL,
    CONSTRAINT FK_RewardTypes_Category FOREIGN KEY (CategoryId) 
        REFERENCES RewardCategories(CategoryId),
    CONSTRAINT CK_RewardTypes_ValidDates CHECK (ValidTo IS NULL OR ValidTo > ValidFrom),
    CONSTRAINT CK_RewardTypes_BasePoints CHECK (BasePoints > 0)
);

-- =============================================
-- Table: EmployeeRewards (Main transaction table)
-- =============================================
CREATE TABLE EmployeeRewards (
    RewardId INT IDENTITY(1,1) PRIMARY KEY,
    EmployeeId INT NOT NULL,
    RewardTypeId INT NOT NULL,
    RewardPoints INT NOT NULL,
    RewardDate DATE NOT NULL,
    AwardedBy INT NOT NULL,
    RewardReason NVARCHAR(1000),
    ApprovalStatus NVARCHAR(20) NOT NULL DEFAULT 'Approved',
    ApprovalDate DATETIME2 NULL,
    Notes NVARCHAR(MAX),
    CreatedDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    ModifiedDate DATETIME2 NULL,
    CONSTRAINT FK_EmployeeRewards_Employee FOREIGN KEY (EmployeeId) 
        REFERENCES Employees(EmployeeId),
    CONSTRAINT FK_EmployeeRewards_RewardType FOREIGN KEY (RewardTypeId) 
        REFERENCES RewardTypes(RewardTypeId),
    CONSTRAINT FK_EmployeeRewards_AwardedBy FOREIGN KEY (AwardedBy) 
        REFERENCES Employees(EmployeeId),
    CONSTRAINT CK_EmployeeRewards_Points CHECK (RewardPoints > 0),
    CONSTRAINT CK_EmployeeRewards_Status CHECK (ApprovalStatus IN ('Pending', 'Approved', 'Rejected', 'Cancelled'))
);

-- =============================================
-- Table: RewardPointsBalance (Aggregated balance table)
-- =============================================
CREATE TABLE RewardPointsBalance (
    BalanceId INT IDENTITY(1,1) PRIMARY KEY,
    EmployeeId INT NOT NULL UNIQUE,
    TotalPointsEarned INT NOT NULL DEFAULT 0,
    TotalPointsRedeemed INT NOT NULL DEFAULT 0,
    CurrentBalance AS (TotalPointsEarned - TotalPointsRedeemed) PERSISTED,
    LastUpdatedDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT FK_RewardPointsBalance_Employee FOREIGN KEY (EmployeeId) 
        REFERENCES Employees(EmployeeId),
    CONSTRAINT CK_RewardPointsBalance_Points CHECK (TotalPointsEarned >= 0 AND TotalPointsRedeemed >= 0)
);

-- =============================================
-- Table: RedemptionCatalog
-- =============================================
CREATE TABLE RedemptionCatalog (
    CatalogItemId INT IDENTITY(1,1) PRIMARY KEY,
    ItemName NVARCHAR(200) NOT NULL,
    ItemCode NVARCHAR(50) UNIQUE NOT NULL,
    Description NVARCHAR(1000),
    PointsRequired INT NOT NULL,
    QuantityAvailable INT NOT NULL DEFAULT 0,
    Category NVARCHAR(100),
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    ModifiedDate DATETIME2 NULL,
    CONSTRAINT CK_RedemptionCatalog_Points CHECK (PointsRequired > 0),
    CONSTRAINT CK_RedemptionCatalog_Quantity CHECK (QuantityAvailable >= 0)
);

-- =============================================
-- Table: PointRedemptions
-- =============================================
CREATE TABLE PointRedemptions (
    RedemptionId INT IDENTITY(1,1) PRIMARY KEY,
    EmployeeId INT NOT NULL,
    CatalogItemId INT NOT NULL,
    PointsRedeemed INT NOT NULL,
    RedemptionDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    RedemptionStatus NVARCHAR(20) NOT NULL DEFAULT 'Pending',
    ProcessedDate DATETIME2 NULL,
    ProcessedBy INT NULL,
    ShippingAddress NVARCHAR(500),
    TrackingNumber NVARCHAR(100),
    Notes NVARCHAR(MAX),
    CreatedDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    ModifiedDate DATETIME2 NULL,
    CONSTRAINT FK_PointRedemptions_Employee FOREIGN KEY (EmployeeId) 
        REFERENCES Employees(EmployeeId),
    CONSTRAINT FK_PointRedemptions_CatalogItem FOREIGN KEY (CatalogItemId) 
        REFERENCES RedemptionCatalog(CatalogItemId),
    CONSTRAINT FK_PointRedemptions_ProcessedBy FOREIGN KEY (ProcessedBy) 
        REFERENCES Employees(EmployeeId),
    CONSTRAINT CK_PointRedemptions_Points CHECK (PointsRedeemed > 0),
    CONSTRAINT CK_PointRedemptions_Status CHECK (RedemptionStatus IN ('Pending', 'Processing', 'Shipped', 'Delivered', 'Cancelled'))
);

-- =============================================
-- Create Indexes for Performance
-- =============================================
CREATE NONCLUSTERED INDEX IX_Employees_DepartmentId ON Employees(DepartmentId);
CREATE NONCLUSTERED INDEX IX_Employees_ManagerId ON Employees(ManagerId);
CREATE NONCLUSTERED INDEX IX_Employees_Email ON Employees(Email);

CREATE NONCLUSTERED INDEX IX_EmployeeRewards_EmployeeId ON EmployeeRewards(EmployeeId);
CREATE NONCLUSTERED INDEX IX_EmployeeRewards_RewardTypeId ON EmployeeRewards(RewardTypeId);
CREATE NONCLUSTERED INDEX IX_EmployeeRewards_RewardDate ON EmployeeRewards(RewardDate DESC);
CREATE NONCLUSTERED INDEX IX_EmployeeRewards_ApprovalStatus ON EmployeeRewards(ApprovalStatus);

CREATE NONCLUSTERED INDEX IX_PointRedemptions_EmployeeId ON PointRedemptions(EmployeeId);
CREATE NONCLUSTERED INDEX IX_PointRedemptions_RedemptionDate ON PointRedemptions(RedemptionDate DESC);

-- =============================================
-- Stored Procedure: Get Top 3 Employees with Highest Rewards
-- =============================================
CREATE OR ALTER PROCEDURE sp_GetTop3EmployeesWithHighestRewards
    @StartDate DATE = NULL,
    @EndDate DATE = NULL,
    @DepartmentId INT = NULL,
    @IncludeInactive BIT = 0
AS
BEGIN
    SET NOCOUNT ON;

    -- Set default date range if not provided (last 12 months)
    IF @StartDate IS NULL
        SET @StartDate = DATEADD(MONTH, -12, CAST(GETDATE() AS DATE));
    
    IF @EndDate IS NULL
        SET @EndDate = CAST(GETDATE() AS DATE);

    -- CTE to calculate total rewards per employee
    WITH EmployeeRewardTotals AS (
        SELECT 
            e.EmployeeId,
            e.EmployeeCode,
            e.FirstName,
            e.LastName,
            e.Email,
            d.DepartmentName,
            d.DepartmentCode,
            COUNT(DISTINCT er.RewardId) AS TotalRewardCount,
            SUM(er.RewardPoints) AS TotalRewardPoints,
            AVG(CAST(er.RewardPoints AS DECIMAL(10,2))) AS AverageRewardPoints,
            MAX(er.RewardDate) AS LastRewardDate,
            MIN(er.RewardDate) AS FirstRewardDate
        FROM 
            Employees e
            INNER JOIN Departments d ON e.DepartmentId = d.DepartmentId
            INNER JOIN EmployeeRewards er ON e.EmployeeId = er.EmployeeId
        WHERE 
            er.ApprovalStatus = 'Approved'
            AND er.RewardDate BETWEEN @StartDate AND @EndDate
            AND (@DepartmentId IS NULL OR e.DepartmentId = @DepartmentId)
            AND (@IncludeInactive = 1 OR e.IsActive = 1)
        GROUP BY 
            e.EmployeeId,
            e.EmployeeCode,
            e.FirstName,
            e.LastName,
            e.Email,
            d.DepartmentName,
            d.DepartmentCode
    ),
    -- Add ranking
    RankedEmployees AS (
        SELECT 
            *,
            RANK() OVER (ORDER BY TotalRewardPoints DESC) AS RewardRank,
            DENSE_RANK() OVER (ORDER BY TotalRewardPoints DESC) AS DenseRank
        FROM 
            EmployeeRewardTotals
    )
    -- Select top 3 employees
    SELECT TOP 3
        RewardRank AS [Rank],
        EmployeeId,
        EmployeeCode,
        FirstName + ' ' + LastName AS FullName,
        Email,
        DepartmentName,
        DepartmentCode,
        TotalRewardCount,
        TotalRewardPoints,
        CAST(AverageRewardPoints AS DECIMAL(10,2)) AS AverageRewardPoints,
        FirstRewardDate,
        LastRewardDate,
        DATEDIFF(DAY, FirstRewardDate, LastRewardDate) AS RewardPeriodDays,
        -- Calculate points per month
        CASE 
            WHEN DATEDIFF(MONTH, FirstRewardDate, LastRewardDate) > 0
            THEN CAST(TotalRewardPoints AS DECIMAL(10,2)) / DATEDIFF(MONTH, FirstRewardDate, LastRewardDate)
            ELSE CAST(TotalRewardPoints AS DECIMAL(10,2))
        END AS PointsPerMonth,
        -- Get current balance if available
        ISNULL(rpb.CurrentBalance, 0) AS CurrentPointsBalance
    FROM 
        RankedEmployees re
        LEFT JOIN RewardPointsBalance rpb ON re.EmployeeId = rpb.EmployeeId
    WHERE 
        RewardRank <= 3
    ORDER BY 
        RewardRank;

END;
GO

-- =============================================
-- Additional Stored Procedure: Update Employee Reward Balance
-- =============================================
CREATE OR ALTER PROCEDURE sp_UpdateEmployeeRewardBalance
    @EmployeeId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRANSACTION;
    
    BEGIN TRY
        DECLARE @TotalEarned INT, @TotalRedeemed INT;
        
        -- Calculate total earned points
        SELECT @TotalEarned = ISNULL(SUM(RewardPoints), 0)
        FROM EmployeeRewards
        WHERE EmployeeId = @EmployeeId
        AND ApprovalStatus = 'Approved';
        
        -- Calculate total redeemed points
        SELECT @TotalRedeemed = ISNULL(SUM(PointsRedeemed), 0)
        FROM PointRedemptions
        WHERE EmployeeId = @EmployeeId
        AND RedemptionStatus NOT IN ('Cancelled');
        
        -- Update or Insert balance record
        IF EXISTS (SELECT 1 FROM RewardPointsBalance WHERE EmployeeId = @EmployeeId)
        BEGIN
            UPDATE RewardPointsBalance
            SET TotalPointsEarned = @TotalEarned,
                TotalPointsRedeemed = @TotalRedeemed,
                LastUpdatedDate = GETUTCDATE()
            WHERE EmployeeId = @EmployeeId;
        END
        ELSE
        BEGIN
            INSERT INTO RewardPointsBalance (EmployeeId, TotalPointsEarned, TotalPointsRedeemed)
            VALUES (@EmployeeId, @TotalEarned, @TotalRedeemed);
        END
        
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;
GO

-- =============================================
-- Sample Data Insert Script
-- =============================================
-- Insert sample departments
INSERT INTO Departments (DepartmentName, DepartmentCode, Description)
VALUES 
    ('Engineering', 'ENG', 'Software Engineering Department'),
    ('Sales', 'SAL', 'Sales and Business Development'),
    ('Human Resources', 'HR', 'Human Resources Department'),
    ('Marketing', 'MKT', 'Marketing and Communications');

-- Insert sample reward categories
INSERT INTO RewardCategories (CategoryName, CategoryCode, Description, PointMultiplier)
VALUES 
    ('Performance', 'PERF', 'Performance based rewards', 1.5),
    ('Innovation', 'INNO', 'Innovation and creativity rewards', 2.0),
    ('Teamwork', 'TEAM', 'Team collaboration rewards', 1.0),
    ('Customer Service', 'CUST', 'Customer satisfaction rewards', 1.25);

-- Insert sample reward types
INSERT INTO RewardTypes (RewardName, RewardCode, Description, BasePoints, CategoryId, ValidFrom)
VALUES 
    ('Monthly Star Performer', 'MSP', 'Best performer of the month', 500, 1, '2024-01-01'),
    ('Innovation Award', 'INA', 'Outstanding innovation contribution', 1000, 2, '2024-01-01'),
    ('Team Player', 'TMP', 'Excellent team collaboration', 300, 3, '2024-01-01'),
    ('Customer Champion', 'CCP', 'Exceptional customer service', 400, 4, '2024-01-01');

