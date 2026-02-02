-- ============================================
-- REWARD POINTS SYSTEM - DEMO DATA SCRIPT
-- AGDATA Software Company - Internal Events
-- Run this script to prepare database for demo
-- Enhanced: 8 Users, 8 Events, 20 Products
-- ============================================

USE [RewardPointsDB]
GO

-- ========================================
-- STEP 1: CLEAN EXISTING DATA
-- Delete in correct order (child tables first)
-- ========================================
PRINT 'Cleaning existing data...'

DELETE FROM UserPointsTransactions
DELETE FROM Redemptions
DELETE FROM EventParticipants
DELETE FROM Events
DELETE FROM InventoryItems
DELETE FROM ProductPricings
DELETE FROM Products
DELETE FROM ProductCategories
DELETE FROM UserPointsAccounts
DELETE FROM AdminMonthlyBudgets
DELETE FROM RefreshTokens
DELETE FROM UserRoles
DELETE FROM Users
DELETE FROM Roles

PRINT 'Existing data cleaned.'
GO

-- ========================================
-- STEP 2: DECLARE VARIABLES FOR IDs
-- ========================================
-- 8 Users (2 Admins + 6 Employees)
DECLARE @AdminId UNIQUEIDENTIFIER = NEWID()       -- Harshal (Primary Admin + Active User)
DECLARE @Admin2Id UNIQUEIDENTIFIER = NEWID()      -- Priya (Secondary Admin)
DECLARE @Employee1Id UNIQUEIDENTIFIER = NEWID()   -- Sankalp (Primary Demo Employee)
DECLARE @Employee2Id UNIQUEIDENTIFIER = NEWID()   -- Amit
DECLARE @Employee3Id UNIQUEIDENTIFIER = NEWID()   -- Neha
DECLARE @Employee4Id UNIQUEIDENTIFIER = NEWID()   -- Rahul
DECLARE @Employee5Id UNIQUEIDENTIFIER = NEWID()   -- Ananya
DECLARE @Employee6Id UNIQUEIDENTIFIER = NEWID()   -- Vikram

DECLARE @AdminRoleId UNIQUEIDENTIFIER = NEWID()
DECLARE @EmployeeRoleId UNIQUEIDENTIFIER = NEWID()

-- ========================================
-- STEP 3: CREATE ROLES
-- ========================================
PRINT 'Creating roles...'

INSERT INTO Roles (Id, Name, Description, IsActive, CreatedAt)
VALUES
(@AdminRoleId, 'Admin', 'System Administrator with full access to manage users, events, products, and redemptions', 1, GETUTCDATE()),
(@EmployeeRoleId, 'Employee', 'Regular AGDATA employee with access to view events, earn points, and redeem rewards', 1, GETUTCDATE())

-- ========================================
-- STEP 4: CREATE USERS (8 Total)
-- ========================================
PRINT 'Creating users...'

-- Password: Harshal@123 (PBKDF2 hash)
DECLARE @AdminPasswordHash NVARCHAR(500) = 'eme7PpRkVXegUVxCkpC/voy+0h+ZMceO5bjx+xy00Bml6MS6JCO/ChrMylN6dzE1'
-- Password: Sankalp@123 (PBKDF2 hash)
DECLARE @EmployeePasswordHash NVARCHAR(500) = 'GGDu7+iw3SxKJe68Xw/C2G16FKoorhJx0QUam63rDsx1pJUGHkrH3uoIdV24tTwW'
-- Password: Demo@123! (PBKDF2 hash)
DECLARE @DefaultPasswordHash NVARCHAR(500) = 'SEg9Wuq8TyWy5Q7WxvZOeNmHoXKzVg+BCM4EgIlMVJrntz3ypb1UvsZt0YWcqvN4'

-- Primary Admin (Harshal Behare) - Also participates as employee
INSERT INTO Users (Id, Email, FirstName, LastName, IsActive, CreatedAt, PasswordHash)
VALUES (@AdminId, 'Harshal.Behare@agdata.com', 'Harshal', 'Behare', 1, DATEADD(MONTH, -8, GETUTCDATE()), @AdminPasswordHash)

-- Secondary Admin (Priya Sharma)
INSERT INTO Users (Id, Email, FirstName, LastName, IsActive, CreatedAt, PasswordHash)
VALUES (@Admin2Id, 'Priya.Sharma@agdata.com', 'Priya', 'Sharma', 1, DATEADD(MONTH, -7, GETUTCDATE()), @DefaultPasswordHash)

-- Employee 1 (Sankalp Chakre - Primary Demo Employee)
INSERT INTO Users (Id, Email, FirstName, LastName, IsActive, CreatedAt, PasswordHash)
VALUES (@Employee1Id, 'Sankalp.Chakre@agdata.com', 'Sankalp', 'Chakre', 1, DATEADD(MONTH, -6, GETUTCDATE()), @EmployeePasswordHash)

-- Other Employees
INSERT INTO Users (Id, Email, FirstName, LastName, IsActive, CreatedAt, PasswordHash)
VALUES
(@Employee2Id, 'Amit.Patel@agdata.com', 'Amit', 'Patel', 1, DATEADD(MONTH, -6, GETUTCDATE()), @DefaultPasswordHash),
(@Employee3Id, 'Neha.Gupta@agdata.com', 'Neha', 'Gupta', 1, DATEADD(MONTH, -5, GETUTCDATE()), @DefaultPasswordHash),
(@Employee4Id, 'Rahul.Verma@agdata.com', 'Rahul', 'Verma', 1, DATEADD(MONTH, -4, GETUTCDATE()), @DefaultPasswordHash),
(@Employee5Id, 'Ananya.Singh@agdata.com', 'Ananya', 'Singh', 1, DATEADD(MONTH, -3, GETUTCDATE()), @DefaultPasswordHash),
(@Employee6Id, 'Vikram.Reddy@agdata.com', 'Vikram', 'Reddy', 1, DATEADD(MONTH, -2, GETUTCDATE()), @DefaultPasswordHash)

-- ========================================
-- STEP 5: ASSIGN ROLES
-- ========================================
PRINT 'Assigning roles...'

INSERT INTO UserRoles (UserId, RoleId, AssignedAt, AssignedBy, IsActive)
VALUES
(@AdminId, @AdminRoleId, DATEADD(MONTH, -8, GETUTCDATE()), @AdminId, 1),
(@Admin2Id, @AdminRoleId, DATEADD(MONTH, -7, GETUTCDATE()), @AdminId, 1),
(@Employee1Id, @EmployeeRoleId, DATEADD(MONTH, -6, GETUTCDATE()), @AdminId, 1),
(@Employee2Id, @EmployeeRoleId, DATEADD(MONTH, -6, GETUTCDATE()), @AdminId, 1),
(@Employee3Id, @EmployeeRoleId, DATEADD(MONTH, -5, GETUTCDATE()), @AdminId, 1),
(@Employee4Id, @EmployeeRoleId, DATEADD(MONTH, -4, GETUTCDATE()), @AdminId, 1),
(@Employee5Id, @EmployeeRoleId, DATEADD(MONTH, -3, GETUTCDATE()), @AdminId, 1),
(@Employee6Id, @EmployeeRoleId, DATEADD(MONTH, -2, GETUTCDATE()), @AdminId, 1)

-- ========================================
-- STEP 6: CREATE PRODUCT CATEGORIES (5 Categories)
-- ========================================
PRINT 'Creating product categories...'

DECLARE @Cat1Id UNIQUEIDENTIFIER = NEWID()  -- Gift Cards
DECLARE @Cat2Id UNIQUEIDENTIFIER = NEWID()  -- Tech & Gadgets
DECLARE @Cat3Id UNIQUEIDENTIFIER = NEWID()  -- AGDATA Merchandise
DECLARE @Cat4Id UNIQUEIDENTIFIER = NEWID()  -- Work Perks & Benefits
DECLARE @Cat5Id UNIQUEIDENTIFIER = NEWID()  -- Experience & Lifestyle

INSERT INTO ProductCategories (Id, Name, Description, DisplayOrder, IsActive)
VALUES
(@Cat1Id, 'Gift Cards', 'Digital gift cards for popular stores and platforms', 1, 1),
(@Cat2Id, 'Tech & Gadgets', 'Electronic accessories and tech gadgets for work and personal use', 2, 1),
(@Cat3Id, 'AGDATA Merchandise', 'Exclusive AGDATA branded items and company swag', 3, 1),
(@Cat4Id, 'Work Perks & Benefits', 'Special workplace benefits and experiences', 4, 1),
(@Cat5Id, 'Experience & Lifestyle', 'Lifestyle products and experiential rewards', 5, 1)

-- ========================================
-- STEP 7: CREATE PRODUCTS (20 Total)
-- ========================================
PRINT 'Creating products...'

-- Gift Cards (5)
DECLARE @Prod1Id UNIQUEIDENTIFIER = NEWID()   -- Amazon Rs.500
DECLARE @Prod2Id UNIQUEIDENTIFIER = NEWID()   -- Amazon Rs.1000
DECLARE @Prod3Id UNIQUEIDENTIFIER = NEWID()   -- Swiggy Voucher
DECLARE @Prod4Id UNIQUEIDENTIFIER = NEWID()   -- Flipkart Rs.500
DECLARE @Prod5Id UNIQUEIDENTIFIER = NEWID()   -- BookMyShow Rs.300

-- Tech & Gadgets (5)
DECLARE @Prod6Id UNIQUEIDENTIFIER = NEWID()   -- Wireless Mouse
DECLARE @Prod7Id UNIQUEIDENTIFIER = NEWID()   -- Desk Organizer
DECLARE @Prod8Id UNIQUEIDENTIFIER = NEWID()   -- Bluetooth Earbuds
DECLARE @Prod9Id UNIQUEIDENTIFIER = NEWID()   -- USB-C Hub
DECLARE @Prod10Id UNIQUEIDENTIFIER = NEWID()  -- Laptop Stand

-- AGDATA Merchandise (5)
DECLARE @Prod11Id UNIQUEIDENTIFIER = NEWID()  -- AGDATA T-Shirt
DECLARE @Prod12Id UNIQUEIDENTIFIER = NEWID()  -- AGDATA Hoodie
DECLARE @Prod13Id UNIQUEIDENTIFIER = NEWID()  -- Coffee Mug
DECLARE @Prod14Id UNIQUEIDENTIFIER = NEWID()  -- AGDATA Backpack
DECLARE @Prod15Id UNIQUEIDENTIFIER = NEWID()  -- Notebook & Pen Set

-- Work Perks (3)
DECLARE @Prod16Id UNIQUEIDENTIFIER = NEWID()  -- Work From Home Day
DECLARE @Prod17Id UNIQUEIDENTIFIER = NEWID()  -- Udemy Course Voucher
DECLARE @Prod18Id UNIQUEIDENTIFIER = NEWID()  -- Extended Lunch Break

-- Experience & Lifestyle (2)
DECLARE @Prod19Id UNIQUEIDENTIFIER = NEWID()  -- Spa Voucher
DECLARE @Prod20Id UNIQUEIDENTIFIER = NEWID()  -- Movie Tickets (2)

INSERT INTO Products (Id, Name, Description, ImageUrl, IsActive, CreatedAt, CreatedBy, CategoryId)
VALUES
-- Gift Cards
(@Prod1Id, 'Amazon Gift Card Rs.500', 'Digital Amazon India gift card worth Rs.500', 'https://images.unsplash.com/photo-1556742049-0cfed4f6a45d?w=300', 1, GETUTCDATE(), @AdminId, @Cat1Id),
(@Prod2Id, 'Amazon Gift Card Rs.1000', 'Digital Amazon India gift card worth Rs.1000', 'https://images.unsplash.com/photo-1556742049-0cfed4f6a45d?w=300', 1, GETUTCDATE(), @AdminId, @Cat1Id),
(@Prod3Id, 'Swiggy Voucher Rs.300', 'Swiggy food delivery voucher worth Rs.300', 'https://images.unsplash.com/photo-1565299624946-b28f40a0ae38?w=300', 1, GETUTCDATE(), @AdminId, @Cat1Id),
(@Prod4Id, 'Flipkart Gift Card Rs.500', 'Digital Flipkart gift card worth Rs.500', 'https://images.unsplash.com/photo-1472851294608-062f824d29cc?w=300', 1, GETUTCDATE(), @AdminId, @Cat1Id),
(@Prod5Id, 'BookMyShow Voucher Rs.300', 'BookMyShow voucher for movies and events', 'https://images.unsplash.com/photo-1489599849927-2ee91cede3ba?w=300', 1, GETUTCDATE(), @AdminId, @Cat1Id),
-- Tech & Gadgets
(@Prod6Id, 'Wireless Ergonomic Mouse', 'Logitech wireless ergonomic mouse for comfortable work', 'https://images.unsplash.com/photo-1527864550417-7fd91fc51a46?w=300', 1, GETUTCDATE(), @AdminId, @Cat2Id),
(@Prod7Id, 'Desk Organizer Set', 'Premium wooden desk organizer with pen holder and phone stand', 'https://images.unsplash.com/photo-1544816155-12df9643f363?w=300', 1, GETUTCDATE(), @AdminId, @Cat2Id),
(@Prod8Id, 'Bluetooth Earbuds', 'True wireless Bluetooth earbuds with noise cancellation', 'https://images.unsplash.com/photo-1590658268037-6bf12165a8df?w=300', 1, GETUTCDATE(), @AdminId, @Cat2Id),
(@Prod9Id, 'USB-C Hub 7-in-1', 'Multi-port USB-C hub with HDMI, USB 3.0, SD card reader', 'https://images.unsplash.com/photo-1625723044792-44de16ccb4e9?w=300', 1, GETUTCDATE(), @AdminId, @Cat2Id),
(@Prod10Id, 'Adjustable Laptop Stand', 'Ergonomic aluminum laptop stand with height adjustment', 'https://images.unsplash.com/photo-1527443224154-c4a3942d3acf?w=300', 1, GETUTCDATE(), @AdminId, @Cat2Id),
-- AGDATA Merchandise
(@Prod11Id, 'AGDATA Premium T-Shirt', 'High quality cotton t-shirt with embroidered AGDATA logo', 'https://images.unsplash.com/photo-1521572163474-6864f9cf17ab?w=300', 1, GETUTCDATE(), @AdminId, @Cat3Id),
(@Prod12Id, 'AGDATA Hoodie', 'Comfortable hoodie with printed AGDATA logo - perfect for casual Fridays', 'https://images.unsplash.com/photo-1556821840-3a63f95609a7?w=300', 1, GETUTCDATE(), @AdminId, @Cat3Id),
(@Prod13Id, 'AGDATA Coffee Mug', 'Premium ceramic mug with AGDATA branding', 'https://images.unsplash.com/photo-1514228742587-6b1558fcca3d?w=300', 1, GETUTCDATE(), @AdminId, @Cat3Id),
(@Prod14Id, 'AGDATA Backpack', 'Durable laptop backpack with AGDATA logo and multiple compartments', 'https://images.unsplash.com/photo-1553062407-98eeb64c6a62?w=300', 1, GETUTCDATE(), @AdminId, @Cat3Id),
(@Prod15Id, 'Notebook & Pen Set', 'Premium leather notebook with AGDATA branded pen', 'https://images.unsplash.com/photo-1531346878377-a5be20888e57?w=300', 1, GETUTCDATE(), @AdminId, @Cat3Id),
-- Work Perks
(@Prod16Id, 'Extra Work From Home Day', 'One additional work from home day for the month', 'https://images.unsplash.com/photo-1584438784894-089d6a62b8fa?w=300', 1, GETUTCDATE(), @AdminId, @Cat4Id),
(@Prod17Id, 'Udemy Course Voucher', 'Access to any Udemy course of your choice for skill development', 'https://images.unsplash.com/photo-1516321318423-f06f85e504b3?w=300', 1, GETUTCDATE(), @AdminId, @Cat4Id),
(@Prod18Id, 'Extended Lunch Break Pass', 'One 2-hour lunch break pass for a relaxed afternoon', 'https://images.unsplash.com/photo-1504674900247-0877df9cc836?w=300', 1, GETUTCDATE(), @AdminId, @Cat4Id),
-- Experience & Lifestyle
(@Prod19Id, 'Spa & Wellness Voucher', 'Relaxing spa session voucher at partner wellness center', 'https://images.unsplash.com/photo-1544161515-4ab6ce6db874?w=300', 1, GETUTCDATE(), @AdminId, @Cat5Id),
(@Prod20Id, 'Movie Tickets (Pair)', 'Two premium movie tickets at PVR/INOX cinemas', 'https://images.unsplash.com/photo-1536440136628-849c177e76a1?w=300', 1, GETUTCDATE(), @AdminId, @Cat5Id)

-- ========================================
-- STEP 8: SET PRODUCT PRICING
-- ========================================
PRINT 'Setting product pricing...'

INSERT INTO ProductPricings (Id, ProductId, PointsCost, EffectiveFrom, EffectiveTo, IsActive)
VALUES
-- Gift Cards
(NEWID(), @Prod1Id, 200, DATEADD(MONTH, -6, GETUTCDATE()), NULL, 1),
(NEWID(), @Prod2Id, 400, DATEADD(MONTH, -6, GETUTCDATE()), NULL, 1),
(NEWID(), @Prod3Id, 150, DATEADD(MONTH, -6, GETUTCDATE()), NULL, 1),
(NEWID(), @Prod4Id, 200, DATEADD(MONTH, -6, GETUTCDATE()), NULL, 1),
(NEWID(), @Prod5Id, 150, DATEADD(MONTH, -6, GETUTCDATE()), NULL, 1),
-- Tech & Gadgets
(NEWID(), @Prod6Id, 800, DATEADD(MONTH, -6, GETUTCDATE()), NULL, 1),
(NEWID(), @Prod7Id, 500, DATEADD(MONTH, -6, GETUTCDATE()), NULL, 1),
(NEWID(), @Prod8Id, 1200, DATEADD(MONTH, -6, GETUTCDATE()), NULL, 1),
(NEWID(), @Prod9Id, 900, DATEADD(MONTH, -6, GETUTCDATE()), NULL, 1),
(NEWID(), @Prod10Id, 700, DATEADD(MONTH, -6, GETUTCDATE()), NULL, 1),
-- AGDATA Merchandise
(NEWID(), @Prod11Id, 250, DATEADD(MONTH, -6, GETUTCDATE()), NULL, 1),
(NEWID(), @Prod12Id, 450, DATEADD(MONTH, -6, GETUTCDATE()), NULL, 1),
(NEWID(), @Prod13Id, 100, DATEADD(MONTH, -6, GETUTCDATE()), NULL, 1),
(NEWID(), @Prod14Id, 600, DATEADD(MONTH, -6, GETUTCDATE()), NULL, 1),
(NEWID(), @Prod15Id, 200, DATEADD(MONTH, -6, GETUTCDATE()), NULL, 1),
-- Work Perks
(NEWID(), @Prod16Id, 1500, DATEADD(MONTH, -6, GETUTCDATE()), NULL, 1),
(NEWID(), @Prod17Id, 600, DATEADD(MONTH, -6, GETUTCDATE()), NULL, 1),
(NEWID(), @Prod18Id, 300, DATEADD(MONTH, -6, GETUTCDATE()), NULL, 1),
-- Experience & Lifestyle
(NEWID(), @Prod19Id, 1000, DATEADD(MONTH, -6, GETUTCDATE()), NULL, 1),
(NEWID(), @Prod20Id, 350, DATEADD(MONTH, -6, GETUTCDATE()), NULL, 1)

-- ========================================
-- STEP 9: SET INVENTORY
-- ========================================
PRINT 'Setting inventory...'

INSERT INTO InventoryItems (Id, ProductId, QuantityAvailable, QuantityReserved, ReorderLevel, LastRestocked, LastUpdated, UpdatedBy)
VALUES
(NEWID(), @Prod1Id, 50, 0, 10, GETUTCDATE(), GETUTCDATE(), @AdminId),
(NEWID(), @Prod2Id, 30, 0, 10, GETUTCDATE(), GETUTCDATE(), @AdminId),
(NEWID(), @Prod3Id, 100, 0, 20, GETUTCDATE(), GETUTCDATE(), @AdminId),
(NEWID(), @Prod4Id, 40, 0, 10, GETUTCDATE(), GETUTCDATE(), @AdminId),
(NEWID(), @Prod5Id, 60, 0, 15, GETUTCDATE(), GETUTCDATE(), @AdminId),
(NEWID(), @Prod6Id, 15, 0, 5, GETUTCDATE(), GETUTCDATE(), @AdminId),
(NEWID(), @Prod7Id, 20, 0, 5, GETUTCDATE(), GETUTCDATE(), @AdminId),
(NEWID(), @Prod8Id, 10, 0, 3, GETUTCDATE(), GETUTCDATE(), @AdminId),
(NEWID(), @Prod9Id, 12, 0, 4, GETUTCDATE(), GETUTCDATE(), @AdminId),
(NEWID(), @Prod10Id, 8, 0, 3, GETUTCDATE(), GETUTCDATE(), @AdminId),
(NEWID(), @Prod11Id, 50, 0, 15, GETUTCDATE(), GETUTCDATE(), @AdminId),
(NEWID(), @Prod12Id, 30, 0, 10, GETUTCDATE(), GETUTCDATE(), @AdminId),
(NEWID(), @Prod13Id, 60, 0, 20, GETUTCDATE(), GETUTCDATE(), @AdminId),
(NEWID(), @Prod14Id, 15, 0, 5, GETUTCDATE(), GETUTCDATE(), @AdminId),
(NEWID(), @Prod15Id, 40, 0, 10, GETUTCDATE(), GETUTCDATE(), @AdminId),
(NEWID(), @Prod16Id, 5, 0, 2, GETUTCDATE(), GETUTCDATE(), @AdminId),   -- Low stock!
(NEWID(), @Prod17Id, 25, 0, 8, GETUTCDATE(), GETUTCDATE(), @AdminId),
(NEWID(), @Prod18Id, 10, 0, 3, GETUTCDATE(), GETUTCDATE(), @AdminId),
(NEWID(), @Prod19Id, 8, 0, 3, GETUTCDATE(), GETUTCDATE(), @AdminId),
(NEWID(), @Prod20Id, 20, 0, 5, GETUTCDATE(), GETUTCDATE(), @AdminId)

-- ========================================
-- STEP 10: CREATE EVENTS (8 Total)
-- ========================================
PRINT 'Creating events...'

DECLARE @Event1Id UNIQUEIDENTIFIER = NEWID()  -- Completed - Code Sprint (Harshal won!)
DECLARE @Event2Id UNIQUEIDENTIFIER = NEWID()  -- Completed - Tech Quiz
DECLARE @Event3Id UNIQUEIDENTIFIER = NEWID()  -- Completed - Hackathon
DECLARE @Event4Id UNIQUEIDENTIFIER = NEWID()  -- Active - Wellness Challenge
DECLARE @Event5Id UNIQUEIDENTIFIER = NEWID()  -- Active - Bug Bounty
DECLARE @Event6Id UNIQUEIDENTIFIER = NEWID()  -- Upcoming - Innovation Summit
DECLARE @Event7Id UNIQUEIDENTIFIER = NEWID()  -- Upcoming - Team Building
DECLARE @Event8Id UNIQUEIDENTIFIER = NEWID()  -- Draft - Annual Awards

-- EVENT 1: Completed - Code Sprint Q4 2025 (Harshal won 1st place!)
INSERT INTO Events (Id, Name, Description, EventDate, EventEndDate, Status, TotalPointsPool,
    FirstPlacePoints, SecondPlacePoints, ThirdPlacePoints, CreatedBy, CreatedAt, CompletedAt,
    Location, MaxParticipants, RegistrationStartDate, RegistrationEndDate, BannerImageUrl)
VALUES
(@Event1Id, 'AGDATA Code Sprint Q4 2025',
    'Quarterly internal hackathon where AGDATA developers compete to build innovative features. Harshal Behare demonstrated exceptional skills and won first place with an automated testing framework!',
    DATEADD(DAY, -60, GETUTCDATE()), DATEADD(DAY, -58, GETUTCDATE()),
    'Completed', 2000, 1000, 600, 400, @AdminId, DATEADD(DAY, -90, GETUTCDATE()), DATEADD(DAY, -58, GETUTCDATE()),
    'AGDATA Office - Innovation Lab', 50, DATEADD(DAY, -85, GETUTCDATE()), DATEADD(DAY, -62, GETUTCDATE()),
    'https://images.unsplash.com/photo-1504384308090-c894fdcc538d?w=800')

-- EVENT 2: Completed - Tech Quiz Championship
INSERT INTO Events (Id, Name, Description, EventDate, EventEndDate, Status, TotalPointsPool,
    FirstPlacePoints, SecondPlacePoints, ThirdPlacePoints, CreatedBy, CreatedAt, CompletedAt,
    Location, MaxParticipants, RegistrationStartDate, RegistrationEndDate, BannerImageUrl)
VALUES
(@Event2Id, 'AGDATA Tech Quiz Championship',
    'Test your technical knowledge! Quiz covering programming, databases, cloud computing, and AGDATA products. Individual competition with rapid-fire rounds.',
    DATEADD(DAY, -45, GETUTCDATE()), DATEADD(DAY, -45, GETUTCDATE()),
    'Completed', 1200, 600, 400, 200, @AdminId, DATEADD(DAY, -60, GETUTCDATE()), DATEADD(DAY, -45, GETUTCDATE()),
    'AGDATA Office - Training Room', 30, DATEADD(DAY, -55, GETUTCDATE()), DATEADD(DAY, -46, GETUTCDATE()),
    'https://images.unsplash.com/photo-1606326608606-aa0b62935f2b?w=800')

-- EVENT 3: Completed - Weekend Hackathon
INSERT INTO Events (Id, Name, Description, EventDate, EventEndDate, Status, TotalPointsPool,
    FirstPlacePoints, SecondPlacePoints, ThirdPlacePoints, CreatedBy, CreatedAt, CompletedAt,
    Location, MaxParticipants, RegistrationStartDate, RegistrationEndDate, BannerImageUrl)
VALUES
(@Event3Id, 'AGDATA Weekend Hackathon 2026',
    '48-hour hackathon challenge! Build something amazing over the weekend. Teams of 3-4 compete to create innovative solutions. Pizza and energy drinks provided!',
    DATEADD(DAY, -20, GETUTCDATE()), DATEADD(DAY, -18, GETUTCDATE()),
    'Completed', 1800, 900, 550, 350, @Admin2Id, DATEADD(DAY, -40, GETUTCDATE()), DATEADD(DAY, -18, GETUTCDATE()),
    'AGDATA Office - All Floors', 40, DATEADD(DAY, -35, GETUTCDATE()), DATEADD(DAY, -22, GETUTCDATE()),
    'https://images.unsplash.com/photo-1519389950473-47ba0277781c?w=800')

-- EVENT 4: Active - Wellness Challenge
INSERT INTO Events (Id, Name, Description, EventDate, EventEndDate, Status, TotalPointsPool,
    FirstPlacePoints, SecondPlacePoints, ThirdPlacePoints, CreatedBy, CreatedAt, CompletedAt,
    Location, MaxParticipants, RegistrationStartDate, RegistrationEndDate, BannerImageUrl, VirtualLink)
VALUES
(@Event4Id, 'AGDATA Wellness Challenge February',
    'Month-long wellness initiative! Track your daily steps, meditation minutes, and healthy meals. Weekly leaderboard updates. Stay healthy, earn points!',
    DATEADD(DAY, -3, GETUTCDATE()), DATEADD(DAY, 25, GETUTCDATE()),
    'Active', 1500, 800, 450, 250, @AdminId, DATEADD(DAY, -15, GETUTCDATE()), NULL,
    'Virtual Event', 100, DATEADD(DAY, -12, GETUTCDATE()), DATEADD(DAY, -4, GETUTCDATE()),
    'https://images.unsplash.com/photo-1571019614242-c5c5dee9f50b?w=800', 'https://teams.microsoft.com/agdata-wellness')

-- EVENT 5: Active - Bug Bounty Hunt
INSERT INTO Events (Id, Name, Description, EventDate, EventEndDate, Status, TotalPointsPool,
    FirstPlacePoints, SecondPlacePoints, ThirdPlacePoints, CreatedBy, CreatedAt, CompletedAt,
    Location, MaxParticipants, RegistrationStartDate, RegistrationEndDate, BannerImageUrl)
VALUES
(@Event5Id, 'AGDATA Bug Bounty Hunt Q1',
    'Find bugs, earn points! Help improve our products by identifying and reporting bugs. Critical bugs earn bonus points. Document your findings properly.',
    DATEADD(DAY, -10, GETUTCDATE()), DATEADD(DAY, 20, GETUTCDATE()),
    'Active', 2500, 1200, 800, 500, @AdminId, DATEADD(DAY, -20, GETUTCDATE()), NULL,
    'Remote - All Teams', 60, DATEADD(DAY, -18, GETUTCDATE()), DATEADD(DAY, -11, GETUTCDATE()),
    'https://images.unsplash.com/photo-1555949963-aa79dcee981c?w=800')

-- EVENT 6: Upcoming - Innovation Summit
INSERT INTO Events (Id, Name, Description, EventDate, EventEndDate, Status, TotalPointsPool,
    FirstPlacePoints, SecondPlacePoints, ThirdPlacePoints, CreatedBy, CreatedAt, CompletedAt,
    Location, MaxParticipants, RegistrationStartDate, RegistrationEndDate, BannerImageUrl, VirtualLink)
VALUES
(@Event6Id, 'AGDATA Innovation Summit 2026',
    'Annual company-wide innovation event! Present your ideas to improve products and services. Best ideas will be implemented. All departments welcome!',
    DATEADD(DAY, 30, GETUTCDATE()), DATEADD(DAY, 30, GETUTCDATE()),
    'Upcoming', 2500, 1200, 700, 400, @AdminId, DATEADD(DAY, -5, GETUTCDATE()), NULL,
    'AGDATA Headquarters - Main Auditorium', 80, DATEADD(DAY, -3, GETUTCDATE()), DATEADD(DAY, 25, GETUTCDATE()),
    'https://images.unsplash.com/photo-1540575467063-178a50c2df87?w=800', 'https://teams.microsoft.com/innovation-summit')

-- EVENT 7: Upcoming - Team Building Day
INSERT INTO Events (Id, Name, Description, EventDate, EventEndDate, Status, TotalPointsPool,
    FirstPlacePoints, SecondPlacePoints, ThirdPlacePoints, CreatedBy, CreatedAt, CompletedAt,
    Location, MaxParticipants, RegistrationStartDate, RegistrationEndDate, BannerImageUrl)
VALUES
(@Event7Id, 'AGDATA Team Building Day',
    'Fun outdoor activities! Treasure hunt, relay races, team challenges, followed by BBQ lunch and awards. Great opportunity to bond with colleagues!',
    DATEADD(DAY, 45, GETUTCDATE()), DATEADD(DAY, 45, GETUTCDATE()),
    'Upcoming', 1500, 750, 500, 250, @Admin2Id, DATEADD(DAY, -2, GETUTCDATE()), NULL,
    'Adventure Park Resort', 100, DATEADD(DAY, 5, GETUTCDATE()), DATEADD(DAY, 40, GETUTCDATE()),
    'https://images.unsplash.com/photo-1528605248644-14dd04022da1?w=800')

-- EVENT 8: Draft - Annual Awards Ceremony
INSERT INTO Events (Id, Name, Description, EventDate, EventEndDate, Status, TotalPointsPool,
    FirstPlacePoints, SecondPlacePoints, ThirdPlacePoints, CreatedBy, CreatedAt, CompletedAt,
    Location, MaxParticipants, RegistrationStartDate, RegistrationEndDate)
VALUES
(@Event8Id, 'AGDATA Annual Awards 2026',
    'Celebrating excellence! Annual awards ceremony recognizing outstanding contributions. Categories: Best Performer, Innovation Leader, Team Player, Rising Star.',
    DATEADD(MONTH, 3, GETUTCDATE()), DATEADD(MONTH, 3, GETUTCDATE()),
    'Draft', 3000, 1500, 1000, 500, @AdminId, GETUTCDATE(), NULL,
    'Grand Ballroom - Hotel Marriott', 200, NULL, NULL)

-- ========================================
-- STEP 11: ADD EVENT PARTICIPANTS
-- ========================================
PRINT 'Adding event participants...'

-- EVENT 1: Code Sprint Q4 2025 - Harshal won 1st!
INSERT INTO EventParticipants (Id, EventId, UserId, PointsAwarded, EventRank, RegisteredAt, AwardedAt, AwardedBy, AttendanceStatus)
VALUES
(NEWID(), @Event1Id, @AdminId, 1000, 1, DATEADD(DAY, -80, GETUTCDATE()), DATEADD(DAY, -58, GETUTCDATE()), @Admin2Id, 'Attended'),  -- Harshal WON!
(NEWID(), @Event1Id, @Employee1Id, 600, 2, DATEADD(DAY, -78, GETUTCDATE()), DATEADD(DAY, -58, GETUTCDATE()), @Admin2Id, 'Attended'),
(NEWID(), @Event1Id, @Employee2Id, 400, 3, DATEADD(DAY, -75, GETUTCDATE()), DATEADD(DAY, -58, GETUTCDATE()), @Admin2Id, 'Attended'),
(NEWID(), @Event1Id, @Employee3Id, NULL, NULL, DATEADD(DAY, -74, GETUTCDATE()), NULL, NULL, 'Attended'),
(NEWID(), @Event1Id, @Employee4Id, NULL, NULL, DATEADD(DAY, -72, GETUTCDATE()), NULL, NULL, 'Attended')

-- EVENT 2: Tech Quiz - Sankalp won!
INSERT INTO EventParticipants (Id, EventId, UserId, PointsAwarded, EventRank, RegisteredAt, AwardedAt, AwardedBy, AttendanceStatus)
VALUES
(NEWID(), @Event2Id, @Employee1Id, 600, 1, DATEADD(DAY, -52, GETUTCDATE()), DATEADD(DAY, -45, GETUTCDATE()), @AdminId, 'Attended'),  -- Sankalp WON!
(NEWID(), @Event2Id, @AdminId, 400, 2, DATEADD(DAY, -50, GETUTCDATE()), DATEADD(DAY, -45, GETUTCDATE()), @Admin2Id, 'Attended'),     -- Harshal 2nd
(NEWID(), @Event2Id, @Employee3Id, 200, 3, DATEADD(DAY, -49, GETUTCDATE()), DATEADD(DAY, -45, GETUTCDATE()), @AdminId, 'Attended'),
(NEWID(), @Event2Id, @Employee2Id, NULL, NULL, DATEADD(DAY, -48, GETUTCDATE()), NULL, NULL, 'Attended'),
(NEWID(), @Event2Id, @Employee5Id, NULL, NULL, DATEADD(DAY, -47, GETUTCDATE()), NULL, NULL, 'Attended')

-- EVENT 3: Weekend Hackathon - Amit won!
INSERT INTO EventParticipants (Id, EventId, UserId, PointsAwarded, EventRank, RegisteredAt, AwardedAt, AwardedBy, AttendanceStatus)
VALUES
(NEWID(), @Event3Id, @Employee2Id, 900, 1, DATEADD(DAY, -32, GETUTCDATE()), DATEADD(DAY, -18, GETUTCDATE()), @AdminId, 'Attended'),
(NEWID(), @Event3Id, @Employee1Id, 550, 2, DATEADD(DAY, -30, GETUTCDATE()), DATEADD(DAY, -18, GETUTCDATE()), @AdminId, 'Attended'),
(NEWID(), @Event3Id, @Employee4Id, 350, 3, DATEADD(DAY, -28, GETUTCDATE()), DATEADD(DAY, -18, GETUTCDATE()), @AdminId, 'Attended'),
(NEWID(), @Event3Id, @AdminId, NULL, NULL, DATEADD(DAY, -27, GETUTCDATE()), NULL, NULL, 'Attended'),  -- Harshal participated
(NEWID(), @Event3Id, @Employee6Id, NULL, NULL, DATEADD(DAY, -26, GETUTCDATE()), NULL, NULL, 'Attended')

-- EVENT 4: Wellness Challenge (Active) - All registered
INSERT INTO EventParticipants (Id, EventId, UserId, PointsAwarded, EventRank, RegisteredAt, AttendanceStatus)
VALUES
(NEWID(), @Event4Id, @AdminId, NULL, NULL, DATEADD(DAY, -10, GETUTCDATE()), 'Registered'),
(NEWID(), @Event4Id, @Employee1Id, NULL, NULL, DATEADD(DAY, -9, GETUTCDATE()), 'Registered'),
(NEWID(), @Event4Id, @Employee2Id, NULL, NULL, DATEADD(DAY, -8, GETUTCDATE()), 'Registered'),
(NEWID(), @Event4Id, @Employee3Id, NULL, NULL, DATEADD(DAY, -7, GETUTCDATE()), 'Registered'),
(NEWID(), @Event4Id, @Employee5Id, NULL, NULL, DATEADD(DAY, -6, GETUTCDATE()), 'Registered'),
(NEWID(), @Event4Id, @Employee6Id, NULL, NULL, DATEADD(DAY, -5, GETUTCDATE()), 'Registered')

-- EVENT 5: Bug Bounty (Active)
INSERT INTO EventParticipants (Id, EventId, UserId, PointsAwarded, EventRank, RegisteredAt, AttendanceStatus)
VALUES
(NEWID(), @Event5Id, @Employee1Id, NULL, NULL, DATEADD(DAY, -15, GETUTCDATE()), 'Registered'),
(NEWID(), @Event5Id, @Employee2Id, NULL, NULL, DATEADD(DAY, -14, GETUTCDATE()), 'Registered'),
(NEWID(), @Event5Id, @Employee4Id, NULL, NULL, DATEADD(DAY, -13, GETUTCDATE()), 'Registered'),
(NEWID(), @Event5Id, @Employee6Id, NULL, NULL, DATEADD(DAY, -12, GETUTCDATE()), 'Registered')

-- EVENT 6: Innovation Summit (Upcoming) - Early registrations
INSERT INTO EventParticipants (Id, EventId, UserId, PointsAwarded, EventRank, RegisteredAt, AttendanceStatus)
VALUES
(NEWID(), @Event6Id, @AdminId, NULL, NULL, DATEADD(DAY, -2, GETUTCDATE()), 'Registered'),
(NEWID(), @Event6Id, @Employee1Id, NULL, NULL, DATEADD(DAY, -1, GETUTCDATE()), 'Registered'),
(NEWID(), @Event6Id, @Employee2Id, NULL, NULL, GETUTCDATE(), 'Registered')

-- ========================================
-- STEP 12: CREATE POINTS ACCOUNTS (All 8 Users)
-- ========================================
PRINT 'Creating points accounts...'

INSERT INTO UserPointsAccounts (Id, UserId, CurrentBalance, TotalEarned, TotalRedeemed, PendingPoints, CreatedAt, LastUpdatedAt)
VALUES
-- Harshal (Admin) - Active participant with good balance
(NEWID(), @AdminId, 2150, 3500, 1150, 200, DATEADD(MONTH, -8, GETUTCDATE()), GETUTCDATE()),
-- Priya (Admin 2) - Some activity
(NEWID(), @Admin2Id, 800, 1000, 200, 0, DATEADD(MONTH, -7, GETUTCDATE()), GETUTCDATE()),
-- Sankalp (Top Employee)
(NEWID(), @Employee1Id, 1850, 3100, 1050, 200, DATEADD(MONTH, -6, GETUTCDATE()), GETUTCDATE()),
-- Amit
(NEWID(), @Employee2Id, 1650, 2400, 750, 0, DATEADD(MONTH, -6, GETUTCDATE()), GETUTCDATE()),
-- Neha
(NEWID(), @Employee3Id, 750, 1200, 450, 0, DATEADD(MONTH, -5, GETUTCDATE()), GETUTCDATE()),
-- Rahul
(NEWID(), @Employee4Id, 600, 900, 300, 0, DATEADD(MONTH, -4, GETUTCDATE()), GETUTCDATE()),
-- Ananya
(NEWID(), @Employee5Id, 350, 500, 150, 0, DATEADD(MONTH, -3, GETUTCDATE()), GETUTCDATE()),
-- Vikram (Newest)
(NEWID(), @Employee6Id, 200, 300, 100, 0, DATEADD(MONTH, -2, GETUTCDATE()), GETUTCDATE())

-- ========================================
-- STEP 13: CREATE POINTS TRANSACTIONS
-- ========================================
PRINT 'Creating points transactions...'

-- =====================
-- HARSHAL (Admin) - Active Participant History
-- =====================
INSERT INTO UserPointsTransactions (Id, UserId, UserPoints, TransactionType, TransactionSource, SourceId, Description, Timestamp, BalanceAfter)
VALUES
-- Event wins
(NEWID(), @AdminId, 1000, 'Credit', 'Event', @Event1Id, '1st Place - AGDATA Code Sprint Q4 2025', DATEADD(DAY, -58, GETUTCDATE()), 1000),
(NEWID(), @AdminId, 400, 'Credit', 'Event', @Event2Id, '2nd Place - Tech Quiz Championship', DATEADD(DAY, -45, GETUTCDATE()), 1400),
-- Admin awards from Priya
(NEWID(), @AdminId, 500, 'Credit', 'Admin', @Admin2Id, 'Leadership Excellence Award - Q4 2025', DATEADD(DAY, -40, GETUTCDATE()), 1900),
(NEWID(), @AdminId, 300, 'Credit', 'Admin', @Admin2Id, 'Employee of the Month - December 2025', DATEADD(DAY, -35, GETUTCDATE()), 2200),
(NEWID(), @AdminId, 400, 'Credit', 'Admin', @Admin2Id, 'System Architecture Excellence Award', DATEADD(DAY, -25, GETUTCDATE()), 2600),
(NEWID(), @AdminId, 300, 'Credit', 'Admin', @Admin2Id, 'Mentorship Recognition - Helped 3 new joiners', DATEADD(DAY, -15, GETUTCDATE()), 2900),
(NEWID(), @AdminId, 400, 'Credit', 'Admin', @Admin2Id, 'Quarterly Performance Bonus - Q1 2026', DATEADD(DAY, -5, GETUTCDATE()), 3300),
(NEWID(), @AdminId, 200, 'Credit', 'Admin', @Admin2Id, 'Process Improvement - CI/CD Pipeline Enhancement', DATEADD(DAY, -2, GETUTCDATE()), 3500),
-- Redemptions
(NEWID(), @AdminId, -400, 'Debit', 'Redemption', NEWID(), 'Redeemed: Amazon Gift Card Rs.1000', DATEADD(DAY, -30, GETUTCDATE()), 2200),
(NEWID(), @AdminId, -450, 'Debit', 'Redemption', NEWID(), 'Redeemed: AGDATA Hoodie', DATEADD(DAY, -20, GETUTCDATE()), 2550),
(NEWID(), @AdminId, -100, 'Debit', 'Redemption', NEWID(), 'Redeemed: AGDATA Coffee Mug', DATEADD(DAY, -10, GETUTCDATE()), 2850),
(NEWID(), @AdminId, -200, 'Debit', 'Redemption', NEWID(), 'Pending: Flipkart Gift Card Rs.500', DATEADD(DAY, -1, GETUTCDATE()), 2350)

-- =====================
-- SANKALP (Employee 1) - Top Performer
-- =====================
INSERT INTO UserPointsTransactions (Id, UserId, UserPoints, TransactionType, TransactionSource, SourceId, Description, Timestamp, BalanceAfter)
VALUES
(NEWID(), @Employee1Id, 600, 'Credit', 'Event', @Event1Id, '2nd Place - AGDATA Code Sprint Q4 2025', DATEADD(DAY, -58, GETUTCDATE()), 600),
(NEWID(), @Employee1Id, 600, 'Credit', 'Event', @Event2Id, '1st Place - Tech Quiz Championship', DATEADD(DAY, -45, GETUTCDATE()), 1200),
(NEWID(), @Employee1Id, 550, 'Credit', 'Event', @Event3Id, '2nd Place - Weekend Hackathon', DATEADD(DAY, -18, GETUTCDATE()), 1750),
(NEWID(), @Employee1Id, 500, 'Credit', 'Admin', @AdminId, 'Exceptional Project Delivery - Nexus Module', DATEADD(DAY, -35, GETUTCDATE()), 1300),
(NEWID(), @Employee1Id, 400, 'Credit', 'Admin', @AdminId, 'Employee of the Month - January 2026', DATEADD(DAY, -20, GETUTCDATE()), 2150),
(NEWID(), @Employee1Id, 300, 'Credit', 'Admin', @AdminId, 'Client Appreciation Award', DATEADD(DAY, -10, GETUTCDATE()), 2800),
(NEWID(), @Employee1Id, 300, 'Credit', 'Admin', @AdminId, 'Code Review Excellence', DATEADD(DAY, -3, GETUTCDATE()), 3100),
(NEWID(), @Employee1Id, -200, 'Debit', 'Redemption', NEWID(), 'Redeemed: Amazon Gift Card Rs.500', DATEADD(DAY, -28, GETUTCDATE()), 1100),
(NEWID(), @Employee1Id, -600, 'Debit', 'Redemption', NEWID(), 'Redeemed: AGDATA Backpack', DATEADD(DAY, -15, GETUTCDATE()), 1850),
(NEWID(), @Employee1Id, -250, 'Debit', 'Redemption', NEWID(), 'Redeemed: AGDATA T-Shirt', DATEADD(DAY, -8, GETUTCDATE()), 2600),
(NEWID(), @Employee1Id, -200, 'Debit', 'Redemption', NEWID(), 'Pending: Amazon Gift Card Rs.500', DATEADD(DAY, -1, GETUTCDATE()), 2650)

-- =====================
-- AMIT (Employee 2) - Strong Performer
-- =====================
INSERT INTO UserPointsTransactions (Id, UserId, UserPoints, TransactionType, TransactionSource, SourceId, Description, Timestamp, BalanceAfter)
VALUES
(NEWID(), @Employee2Id, 400, 'Credit', 'Event', @Event1Id, '3rd Place - AGDATA Code Sprint Q4 2025', DATEADD(DAY, -58, GETUTCDATE()), 400),
(NEWID(), @Employee2Id, 900, 'Credit', 'Event', @Event3Id, '1st Place - Weekend Hackathon', DATEADD(DAY, -18, GETUTCDATE()), 1300),
(NEWID(), @Employee2Id, 400, 'Credit', 'Admin', @AdminId, 'Team Leadership Recognition', DATEADD(DAY, -40, GETUTCDATE()), 800),
(NEWID(), @Employee2Id, 350, 'Credit', 'Admin', @AdminId, 'Knowledge Sharing - Tech Talk Series', DATEADD(DAY, -25, GETUTCDATE()), 1650),
(NEWID(), @Employee2Id, 350, 'Credit', 'Admin', @AdminId, 'Bug Bounty - Critical Security Fix', DATEADD(DAY, -12, GETUTCDATE()), 2200),
(NEWID(), @Employee2Id, -400, 'Debit', 'Redemption', NEWID(), 'Redeemed: Amazon Gift Card Rs.1000', DATEADD(DAY, -30, GETUTCDATE()), 1350),
(NEWID(), @Employee2Id, -150, 'Debit', 'Redemption', NEWID(), 'Redeemed: Swiggy Voucher', DATEADD(DAY, -20, GETUTCDATE()), 1500),
(NEWID(), @Employee2Id, -200, 'Debit', 'Redemption', NEWID(), 'Redeemed: Notebook & Pen Set', DATEADD(DAY, -5, GETUTCDATE()), 2050)

-- =====================
-- NEHA (Employee 3)
-- =====================
INSERT INTO UserPointsTransactions (Id, UserId, UserPoints, TransactionType, TransactionSource, SourceId, Description, Timestamp, BalanceAfter)
VALUES
(NEWID(), @Employee3Id, 200, 'Credit', 'Event', @Event2Id, '3rd Place - Tech Quiz Championship', DATEADD(DAY, -45, GETUTCDATE()), 200),
(NEWID(), @Employee3Id, 350, 'Credit', 'Admin', @AdminId, 'Outstanding Documentation Award', DATEADD(DAY, -38, GETUTCDATE()), 550),
(NEWID(), @Employee3Id, 300, 'Credit', 'Admin', @AdminId, 'Peer Recognition - Helpful Colleague', DATEADD(DAY, -28, GETUTCDATE()), 850),
(NEWID(), @Employee3Id, 350, 'Credit', 'Admin', @AdminId, 'Process Improvement Implementation', DATEADD(DAY, -15, GETUTCDATE()), 1200),
(NEWID(), @Employee3Id, -250, 'Debit', 'Redemption', NEWID(), 'Redeemed: AGDATA T-Shirt', DATEADD(DAY, -25, GETUTCDATE()), 600),
(NEWID(), @Employee3Id, -100, 'Debit', 'Redemption', NEWID(), 'Redeemed: AGDATA Coffee Mug', DATEADD(DAY, -10, GETUTCDATE()), 850),
(NEWID(), @Employee3Id, -100, 'Debit', 'Redemption', NEWID(), 'Redeemed: AGDATA Coffee Mug', DATEADD(DAY, -5, GETUTCDATE()), 750)

-- =====================
-- RAHUL (Employee 4)
-- =====================
INSERT INTO UserPointsTransactions (Id, UserId, UserPoints, TransactionType, TransactionSource, SourceId, Description, Timestamp, BalanceAfter)
VALUES
(NEWID(), @Employee4Id, 350, 'Credit', 'Event', @Event3Id, '3rd Place - Weekend Hackathon', DATEADD(DAY, -18, GETUTCDATE()), 350),
(NEWID(), @Employee4Id, 200, 'Credit', 'Admin', @AdminId, 'Welcome Bonus - New Employee', DATEADD(MONTH, -4, GETUTCDATE()), 200),
(NEWID(), @Employee4Id, 200, 'Credit', 'Admin', @AdminId, 'Training Completion - AGDATA Certification', DATEADD(DAY, -50, GETUTCDATE()), 400),
(NEWID(), @Employee4Id, 150, 'Credit', 'Admin', @AdminId, 'First Successful Deployment', DATEADD(DAY, -30, GETUTCDATE()), 900),
(NEWID(), @Employee4Id, -150, 'Debit', 'Redemption', NEWID(), 'Redeemed: Swiggy Voucher', DATEADD(DAY, -25, GETUTCDATE()), 450),
(NEWID(), @Employee4Id, -150, 'Debit', 'Redemption', NEWID(), 'Redeemed: BookMyShow Voucher', DATEADD(DAY, -10, GETUTCDATE()), 600)

-- =====================
-- ANANYA (Employee 5)
-- =====================
INSERT INTO UserPointsTransactions (Id, UserId, UserPoints, TransactionType, TransactionSource, SourceId, Description, Timestamp, BalanceAfter)
VALUES
(NEWID(), @Employee5Id, 150, 'Credit', 'Admin', @AdminId, 'Welcome Bonus - New Employee', DATEADD(MONTH, -3, GETUTCDATE()), 150),
(NEWID(), @Employee5Id, 150, 'Credit', 'Admin', @AdminId, 'Quick Learner Appreciation', DATEADD(DAY, -45, GETUTCDATE()), 300),
(NEWID(), @Employee5Id, 100, 'Credit', 'Admin', @AdminId, 'First Week Completion Bonus', DATEADD(DAY, -60, GETUTCDATE()), 250),
(NEWID(), @Employee5Id, 100, 'Credit', 'Admin', @AdminId, 'Positive Client Feedback', DATEADD(DAY, -15, GETUTCDATE()), 500),
(NEWID(), @Employee5Id, -100, 'Debit', 'Redemption', NEWID(), 'Redeemed: AGDATA Coffee Mug', DATEADD(DAY, -30, GETUTCDATE()), 200),
(NEWID(), @Employee5Id, -50, 'Debit', 'Redemption', NEWID(), 'Partial Redemption', DATEADD(DAY, -10, GETUTCDATE()), 350)

-- =====================
-- VIKRAM (Employee 6) - Newest
-- =====================
INSERT INTO UserPointsTransactions (Id, UserId, UserPoints, TransactionType, TransactionSource, SourceId, Description, Timestamp, BalanceAfter)
VALUES
(NEWID(), @Employee6Id, 150, 'Credit', 'Admin', @AdminId, 'Welcome Bonus - New Employee', DATEADD(MONTH, -2, GETUTCDATE()), 150),
(NEWID(), @Employee6Id, 100, 'Credit', 'Admin', @AdminId, 'Onboarding Completion', DATEADD(DAY, -40, GETUTCDATE()), 250),
(NEWID(), @Employee6Id, 50, 'Credit', 'Admin', @AdminId, 'First Contribution Recognition', DATEADD(DAY, -20, GETUTCDATE()), 300),
(NEWID(), @Employee6Id, -100, 'Debit', 'Redemption', NEWID(), 'Redeemed: AGDATA Coffee Mug', DATEADD(DAY, -15, GETUTCDATE()), 200)

-- =====================
-- PRIYA (Admin 2) - Some activity
-- =====================
INSERT INTO UserPointsTransactions (Id, UserId, UserPoints, TransactionType, TransactionSource, SourceId, Description, Timestamp, BalanceAfter)
VALUES
(NEWID(), @Admin2Id, 500, 'Credit', 'Admin', @AdminId, 'HR Excellence Award - Q4 2025', DATEADD(DAY, -50, GETUTCDATE()), 500),
(NEWID(), @Admin2Id, 300, 'Credit', 'Admin', @AdminId, 'Event Organization Recognition', DATEADD(DAY, -30, GETUTCDATE()), 800),
(NEWID(), @Admin2Id, 200, 'Credit', 'Admin', @AdminId, 'Training Program Development', DATEADD(DAY, -10, GETUTCDATE()), 1000),
(NEWID(), @Admin2Id, -200, 'Debit', 'Redemption', NEWID(), 'Redeemed: Amazon Gift Card Rs.500', DATEADD(DAY, -20, GETUTCDATE()), 600)

-- ========================================
-- STEP 14: CREATE REDEMPTIONS
-- ========================================
PRINT 'Creating redemptions...'

-- HARSHAL's Redemptions (showing full workflow)
INSERT INTO Redemptions (Id, UserId, ProductId, PointsSpent, Status, RequestedAt, ApprovedAt, ApprovedBy, ProcessedAt, ProcessedBy, Quantity)
VALUES (NEWID(), @AdminId, @Prod2Id, 400, 'Delivered', DATEADD(DAY, -31, GETUTCDATE()), DATEADD(DAY, -30, GETUTCDATE()), @Admin2Id, DATEADD(DAY, -28, GETUTCDATE()), @Admin2Id, 1)

INSERT INTO Redemptions (Id, UserId, ProductId, PointsSpent, Status, RequestedAt, ApprovedAt, ApprovedBy, ProcessedAt, ProcessedBy, Quantity)
VALUES (NEWID(), @AdminId, @Prod12Id, 450, 'Delivered', DATEADD(DAY, -21, GETUTCDATE()), DATEADD(DAY, -20, GETUTCDATE()), @Admin2Id, DATEADD(DAY, -18, GETUTCDATE()), @Admin2Id, 1)

INSERT INTO Redemptions (Id, UserId, ProductId, PointsSpent, Status, RequestedAt, ApprovedAt, ApprovedBy, ProcessedAt, ProcessedBy, Quantity)
VALUES (NEWID(), @AdminId, @Prod13Id, 100, 'Delivered', DATEADD(DAY, -11, GETUTCDATE()), DATEADD(DAY, -10, GETUTCDATE()), @Admin2Id, DATEADD(DAY, -9, GETUTCDATE()), @Admin2Id, 1)

-- Harshal's Pending Redemption
INSERT INTO Redemptions (Id, UserId, ProductId, PointsSpent, Status, RequestedAt, Quantity)
VALUES (NEWID(), @AdminId, @Prod4Id, 200, 'Pending', DATEADD(DAY, -1, GETUTCDATE()), 1)

-- SANKALP's Redemptions
INSERT INTO Redemptions (Id, UserId, ProductId, PointsSpent, Status, RequestedAt, ApprovedAt, ApprovedBy, ProcessedAt, ProcessedBy, Quantity)
VALUES (NEWID(), @Employee1Id, @Prod1Id, 200, 'Delivered', DATEADD(DAY, -29, GETUTCDATE()), DATEADD(DAY, -28, GETUTCDATE()), @AdminId, DATEADD(DAY, -26, GETUTCDATE()), @AdminId, 1)

INSERT INTO Redemptions (Id, UserId, ProductId, PointsSpent, Status, RequestedAt, ApprovedAt, ApprovedBy, ProcessedAt, ProcessedBy, Quantity)
VALUES (NEWID(), @Employee1Id, @Prod14Id, 600, 'Delivered', DATEADD(DAY, -16, GETUTCDATE()), DATEADD(DAY, -15, GETUTCDATE()), @AdminId, DATEADD(DAY, -13, GETUTCDATE()), @AdminId, 1)

INSERT INTO Redemptions (Id, UserId, ProductId, PointsSpent, Status, RequestedAt, ApprovedAt, ApprovedBy, Quantity)
VALUES (NEWID(), @Employee1Id, @Prod11Id, 250, 'Approved', DATEADD(DAY, -9, GETUTCDATE()), DATEADD(DAY, -8, GETUTCDATE()), @AdminId, 1)

-- Sankalp's Pending Redemption
INSERT INTO Redemptions (Id, UserId, ProductId, PointsSpent, Status, RequestedAt, Quantity)
VALUES (NEWID(), @Employee1Id, @Prod1Id, 200, 'Pending', DATEADD(DAY, -1, GETUTCDATE()), 1)

-- AMIT's Redemptions
INSERT INTO Redemptions (Id, UserId, ProductId, PointsSpent, Status, RequestedAt, ApprovedAt, ApprovedBy, ProcessedAt, ProcessedBy, Quantity)
VALUES (NEWID(), @Employee2Id, @Prod2Id, 400, 'Delivered', DATEADD(DAY, -31, GETUTCDATE()), DATEADD(DAY, -30, GETUTCDATE()), @AdminId, DATEADD(DAY, -28, GETUTCDATE()), @AdminId, 1)

INSERT INTO Redemptions (Id, UserId, ProductId, PointsSpent, Status, RequestedAt, ApprovedAt, ApprovedBy, ProcessedAt, ProcessedBy, Quantity)
VALUES (NEWID(), @Employee2Id, @Prod3Id, 150, 'Delivered', DATEADD(DAY, -21, GETUTCDATE()), DATEADD(DAY, -20, GETUTCDATE()), @AdminId, DATEADD(DAY, -19, GETUTCDATE()), @AdminId, 1)

INSERT INTO Redemptions (Id, UserId, ProductId, PointsSpent, Status, RequestedAt, ApprovedAt, ApprovedBy, Quantity)
VALUES (NEWID(), @Employee2Id, @Prod15Id, 200, 'Approved', DATEADD(DAY, -6, GETUTCDATE()), DATEADD(DAY, -5, GETUTCDATE()), @AdminId, 1)

-- NEHA's Redemptions
INSERT INTO Redemptions (Id, UserId, ProductId, PointsSpent, Status, RequestedAt, ApprovedAt, ApprovedBy, ProcessedAt, ProcessedBy, Quantity)
VALUES (NEWID(), @Employee3Id, @Prod11Id, 250, 'Delivered', DATEADD(DAY, -26, GETUTCDATE()), DATEADD(DAY, -25, GETUTCDATE()), @AdminId, DATEADD(DAY, -23, GETUTCDATE()), @AdminId, 1)

INSERT INTO Redemptions (Id, UserId, ProductId, PointsSpent, Status, RequestedAt, ApprovedAt, ApprovedBy, ProcessedAt, ProcessedBy, Quantity)
VALUES (NEWID(), @Employee3Id, @Prod13Id, 100, 'Delivered', DATEADD(DAY, -11, GETUTCDATE()), DATEADD(DAY, -10, GETUTCDATE()), @AdminId, DATEADD(DAY, -9, GETUTCDATE()), @AdminId, 1)

INSERT INTO Redemptions (Id, UserId, ProductId, PointsSpent, Status, RequestedAt, ApprovedAt, ApprovedBy, ProcessedAt, ProcessedBy, Quantity)
VALUES (NEWID(), @Employee3Id, @Prod13Id, 100, 'Delivered', DATEADD(DAY, -6, GETUTCDATE()), DATEADD(DAY, -5, GETUTCDATE()), @AdminId, DATEADD(DAY, -4, GETUTCDATE()), @AdminId, 1)

-- RAHUL's Redemptions
INSERT INTO Redemptions (Id, UserId, ProductId, PointsSpent, Status, RequestedAt, ApprovedAt, ApprovedBy, ProcessedAt, ProcessedBy, Quantity)
VALUES (NEWID(), @Employee4Id, @Prod3Id, 150, 'Delivered', DATEADD(DAY, -26, GETUTCDATE()), DATEADD(DAY, -25, GETUTCDATE()), @AdminId, DATEADD(DAY, -23, GETUTCDATE()), @AdminId, 1)

INSERT INTO Redemptions (Id, UserId, ProductId, PointsSpent, Status, RequestedAt, ApprovedAt, ApprovedBy, ProcessedAt, ProcessedBy, Quantity)
VALUES (NEWID(), @Employee4Id, @Prod5Id, 150, 'Delivered', DATEADD(DAY, -11, GETUTCDATE()), DATEADD(DAY, -10, GETUTCDATE()), @AdminId, DATEADD(DAY, -9, GETUTCDATE()), @AdminId, 1)

-- Rejected Redemption (Rahul - WFH during busy period)
INSERT INTO Redemptions (Id, UserId, ProductId, PointsSpent, Status, RequestedAt, ApprovedAt, ApprovedBy, RejectionReason, Quantity)
VALUES (NEWID(), @Employee4Id, @Prod16Id, 1500, 'Rejected', DATEADD(DAY, -8, GETUTCDATE()), DATEADD(DAY, -7, GETUTCDATE()), @AdminId,
    'Work from home day not available during sprint deadline. Please resubmit after March 15th project milestone.', 1)

-- ANANYA's Redemptions
INSERT INTO Redemptions (Id, UserId, ProductId, PointsSpent, Status, RequestedAt, ApprovedAt, ApprovedBy, ProcessedAt, ProcessedBy, Quantity)
VALUES (NEWID(), @Employee5Id, @Prod13Id, 100, 'Delivered', DATEADD(DAY, -31, GETUTCDATE()), DATEADD(DAY, -30, GETUTCDATE()), @AdminId, DATEADD(DAY, -28, GETUTCDATE()), @AdminId, 1)

-- VIKRAM's Redemption
INSERT INTO Redemptions (Id, UserId, ProductId, PointsSpent, Status, RequestedAt, ApprovedAt, ApprovedBy, ProcessedAt, ProcessedBy, Quantity)
VALUES (NEWID(), @Employee6Id, @Prod13Id, 100, 'Delivered', DATEADD(DAY, -16, GETUTCDATE()), DATEADD(DAY, -15, GETUTCDATE()), @AdminId, DATEADD(DAY, -14, GETUTCDATE()), @AdminId, 1)

-- PRIYA's Redemption
INSERT INTO Redemptions (Id, UserId, ProductId, PointsSpent, Status, RequestedAt, ApprovedAt, ApprovedBy, ProcessedAt, ProcessedBy, Quantity)
VALUES (NEWID(), @Admin2Id, @Prod1Id, 200, 'Delivered', DATEADD(DAY, -21, GETUTCDATE()), DATEADD(DAY, -20, GETUTCDATE()), @AdminId, DATEADD(DAY, -18, GETUTCDATE()), @AdminId, 1)

-- ========================================
-- STEP 15: CREATE ADMIN BUDGETS
-- ========================================
PRINT 'Creating admin budgets...'

-- HARSHAL's Budget History
-- Current month
INSERT INTO AdminMonthlyBudgets (Id, AdminUserId, MonthYear, BudgetLimit, PointsAwarded, IsHardLimit, WarningThreshold, CreatedAt, UpdatedAt)
VALUES
(NEWID(), @AdminId, CONVERT(INT, FORMAT(GETUTCDATE(), 'yyyyMM')), 15000, 4500, 0, 80, GETUTCDATE(), GETUTCDATE())

-- Previous month
INSERT INTO AdminMonthlyBudgets (Id, AdminUserId, MonthYear, BudgetLimit, PointsAwarded, IsHardLimit, WarningThreshold, CreatedAt, UpdatedAt)
VALUES
(NEWID(), @AdminId, CONVERT(INT, FORMAT(DATEADD(MONTH, -1, GETUTCDATE()), 'yyyyMM')), 12000, 9800, 0, 80, DATEADD(MONTH, -1, GETUTCDATE()), DATEADD(MONTH, -1, GETUTCDATE()))

-- Two months ago
INSERT INTO AdminMonthlyBudgets (Id, AdminUserId, MonthYear, BudgetLimit, PointsAwarded, IsHardLimit, WarningThreshold, CreatedAt, UpdatedAt)
VALUES
(NEWID(), @AdminId, CONVERT(INT, FORMAT(DATEADD(MONTH, -2, GETUTCDATE()), 'yyyyMM')), 10000, 8500, 0, 80, DATEADD(MONTH, -2, GETUTCDATE()), DATEADD(MONTH, -2, GETUTCDATE()))

-- Three months ago
INSERT INTO AdminMonthlyBudgets (Id, AdminUserId, MonthYear, BudgetLimit, PointsAwarded, IsHardLimit, WarningThreshold, CreatedAt, UpdatedAt)
VALUES
(NEWID(), @AdminId, CONVERT(INT, FORMAT(DATEADD(MONTH, -3, GETUTCDATE()), 'yyyyMM')), 10000, 7200, 0, 80, DATEADD(MONTH, -3, GETUTCDATE()), DATEADD(MONTH, -3, GETUTCDATE()))

-- PRIYA's Budget
INSERT INTO AdminMonthlyBudgets (Id, AdminUserId, MonthYear, BudgetLimit, PointsAwarded, IsHardLimit, WarningThreshold, CreatedAt, UpdatedAt)
VALUES
(NEWID(), @Admin2Id, CONVERT(INT, FORMAT(GETUTCDATE(), 'yyyyMM')), 8000, 2100, 0, 80, GETUTCDATE(), GETUTCDATE())

-- ========================================
-- VERIFICATION
-- ========================================
PRINT ''
PRINT '============================================='
PRINT 'AGDATA DEMO DATA INSERTED SUCCESSFULLY!'
PRINT '============================================='
PRINT ''
PRINT 'LOGIN CREDENTIALS:'
PRINT '  Admin:    Harshal.Behare@agdata.com / Harshal@123'
PRINT '  Admin 2:  Priya.Sharma@agdata.com / Demo@123!'
PRINT '  Employee: Sankalp.Chakre@agdata.com / Sankalp@123'
PRINT ''
PRINT 'Other employees use password: Demo@123!'
PRINT ''
PRINT 'DATA SUMMARY:'

SELECT 'Users' AS Entity, COUNT(*) AS Count FROM Users
UNION ALL SELECT 'Roles', COUNT(*) FROM Roles
UNION ALL SELECT 'Products', COUNT(*) FROM Products
UNION ALL SELECT 'Categories', COUNT(*) FROM ProductCategories
UNION ALL SELECT 'Events', COUNT(*) FROM Events
UNION ALL SELECT 'Event Participants', COUNT(*) FROM EventParticipants
UNION ALL SELECT 'Redemptions', COUNT(*) FROM Redemptions
UNION ALL SELECT 'Points Accounts', COUNT(*) FROM UserPointsAccounts
UNION ALL SELECT 'Transactions', COUNT(*) FROM UserPointsTransactions
UNION ALL SELECT 'Admin Budgets', COUNT(*) FROM AdminMonthlyBudgets

PRINT ''
PRINT 'TOP EMPLOYEES BY POINTS:'
SELECT TOP 5 u.FirstName + ' ' + u.LastName AS Name, upa.CurrentBalance, upa.TotalEarned
FROM Users u
JOIN UserPointsAccounts upa ON u.Id = upa.UserId
ORDER BY upa.TotalEarned DESC

GO
