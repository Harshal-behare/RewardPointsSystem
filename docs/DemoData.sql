-- ============================================
-- REWARD POINTS SYSTEM - DEMO DATA SCRIPT
-- AGDATA Software Company - Internal Events
-- Run this script to prepare database for demo
-- Enhanced: 8 Users, 8 Events, 20 Products
-- CORRECTED: Proper chronological order, enum values, balances
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
-- TIMELINE (relative to today Feb 3, 2026):
-- Event 1: Code Sprint - Dec 5-7, 2025 (Day -60 to -58, completed)
-- Event 2: Tech Quiz - Dec 20, 2025 (Day -45, completed)
-- Event 3: Hackathon - Jan 14-16, 2026 (Day -20 to -18, completed)
-- Event 4: Wellness - Jan 31 to Feb 28 (Day -3 to +25, active)
-- Event 5: Bug Bounty - Jan 24 to Feb 23 (Day -10 to +20, active)
-- Event 6: Innovation Summit - Mar 5 (Day +30, upcoming)
-- Event 7: Team Building - Mar 20 (Day +45, upcoming)
-- Event 8: Annual Awards - May 3 (Month +3, draft)
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

-- EVENT 1: Completed - Code Sprint Q4 2025 (Dec 5-7, 2025) - Harshal won 1st place!
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

-- EVENT 2: Completed - Tech Quiz Championship (Dec 20, 2025)
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

-- EVENT 3: Completed - Weekend Hackathon (Jan 14-16, 2026)
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

-- EVENT 4: Active - Wellness Challenge (Jan 31 - Feb 28, 2026)
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

-- EVENT 5: Active - Bug Bounty Hunt (Jan 24 - Feb 23, 2026)
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

-- EVENT 6: Upcoming - Innovation Summit (Mar 5, 2026)
INSERT INTO Events (Id, Name, Description, EventDate, EventEndDate, Status, TotalPointsPool,
    FirstPlacePoints, SecondPlacePoints, ThirdPlacePoints, CreatedBy, CreatedAt, CompletedAt,
    Location, MaxParticipants, RegistrationStartDate, RegistrationEndDate, BannerImageUrl, VirtualLink)
VALUES
(@Event6Id, 'AGDATA Innovation Summit 2026',
    'Annual company-wide innovation event! Present your ideas to improve products and services. Best ideas will be implemented. All departments welcome!',
    DATEADD(DAY, 30, GETUTCDATE()), DATEADD(DAY, 30, GETUTCDATE()),
    'Upcoming', 2300, 1200, 700, 400, @AdminId, DATEADD(DAY, -5, GETUTCDATE()), NULL,
    'AGDATA Headquarters - Main Auditorium', 80, DATEADD(DAY, -3, GETUTCDATE()), DATEADD(DAY, 25, GETUTCDATE()),
    'https://images.unsplash.com/photo-1540575467063-178a50c2df87?w=800', 'https://teams.microsoft.com/innovation-summit')

-- EVENT 7: Upcoming - Team Building Day (Mar 20, 2026)
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

-- EVENT 8: Draft - Annual Awards Ceremony (May 3, 2026)
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
-- Points awarded on event completion date
-- ========================================
PRINT 'Adding event participants...'

-- EVENT 1: Code Sprint Q4 2025 (Completed Dec 7) - Harshal won 1st!
INSERT INTO EventParticipants (Id, EventId, UserId, PointsAwarded, EventRank, RegisteredAt, AwardedAt, AwardedBy, AttendanceStatus)
VALUES
(NEWID(), @Event1Id, @AdminId, 1000, 1, DATEADD(DAY, -80, GETUTCDATE()), DATEADD(DAY, -58, GETUTCDATE()), @Admin2Id, 'Attended'),  -- Harshal WON!
(NEWID(), @Event1Id, @Employee1Id, 600, 2, DATEADD(DAY, -78, GETUTCDATE()), DATEADD(DAY, -58, GETUTCDATE()), @Admin2Id, 'Attended'),
(NEWID(), @Event1Id, @Employee2Id, 400, 3, DATEADD(DAY, -75, GETUTCDATE()), DATEADD(DAY, -58, GETUTCDATE()), @Admin2Id, 'Attended'),
(NEWID(), @Event1Id, @Employee3Id, NULL, NULL, DATEADD(DAY, -74, GETUTCDATE()), NULL, NULL, 'Attended'),
(NEWID(), @Event1Id, @Employee4Id, NULL, NULL, DATEADD(DAY, -72, GETUTCDATE()), NULL, NULL, 'Attended')

-- EVENT 2: Tech Quiz (Completed Dec 20) - Sankalp won, Harshal 2nd!
INSERT INTO EventParticipants (Id, EventId, UserId, PointsAwarded, EventRank, RegisteredAt, AwardedAt, AwardedBy, AttendanceStatus)
VALUES
(NEWID(), @Event2Id, @Employee1Id, 600, 1, DATEADD(DAY, -52, GETUTCDATE()), DATEADD(DAY, -45, GETUTCDATE()), @AdminId, 'Attended'),  -- Sankalp WON!
(NEWID(), @Event2Id, @AdminId, 400, 2, DATEADD(DAY, -50, GETUTCDATE()), DATEADD(DAY, -45, GETUTCDATE()), @Admin2Id, 'Attended'),     -- Harshal 2nd
(NEWID(), @Event2Id, @Employee3Id, 200, 3, DATEADD(DAY, -49, GETUTCDATE()), DATEADD(DAY, -45, GETUTCDATE()), @AdminId, 'Attended'),
(NEWID(), @Event2Id, @Employee2Id, NULL, NULL, DATEADD(DAY, -48, GETUTCDATE()), NULL, NULL, 'Attended'),
(NEWID(), @Event2Id, @Employee5Id, NULL, NULL, DATEADD(DAY, -47, GETUTCDATE()), NULL, NULL, 'Attended')

-- EVENT 3: Weekend Hackathon (Completed Jan 16) - Amit won, Sankalp 2nd, Rahul 3rd
INSERT INTO EventParticipants (Id, EventId, UserId, PointsAwarded, EventRank, RegisteredAt, AwardedAt, AwardedBy, AttendanceStatus)
VALUES
(NEWID(), @Event3Id, @Employee2Id, 900, 1, DATEADD(DAY, -32, GETUTCDATE()), DATEADD(DAY, -18, GETUTCDATE()), @AdminId, 'Attended'),
(NEWID(), @Event3Id, @Employee1Id, 550, 2, DATEADD(DAY, -30, GETUTCDATE()), DATEADD(DAY, -18, GETUTCDATE()), @AdminId, 'Attended'),
(NEWID(), @Event3Id, @Employee4Id, 350, 3, DATEADD(DAY, -28, GETUTCDATE()), DATEADD(DAY, -18, GETUTCDATE()), @AdminId, 'Attended'),
(NEWID(), @Event3Id, @AdminId, NULL, NULL, DATEADD(DAY, -27, GETUTCDATE()), NULL, NULL, 'Attended'),  -- Harshal participated (no prize)
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
-- Calculated based on EVENT earnings only (no AdminAward feature)
-- ========================================
PRINT 'Creating points accounts...'

-- HARSHAL's Points Summary (MAIN DEMO USER - ADMIN):
-- Event 1 (Dec 7): +1000 (1st place Code Sprint)
-- Event 2 (Dec 20): +400 (2nd place Tech Quiz)  
-- Redemption (Jan 3): -400 (Amazon Rs.1000)
-- Redemption (Jan 20): -200 (Amazon Rs.500)
-- Redemption (Feb 2 - Pending): -200 (Flipkart)
-- Total Earned: 1000+400 = 1400
-- Total Redeemed: 400+200 = 600 (delivered) + 200 (pending)
-- Current Balance: 1400 - 600 = 800, with 200 pending = 600 available

INSERT INTO UserPointsAccounts (Id, UserId, CurrentBalance, TotalEarned, TotalRedeemed, PendingPoints, CreatedAt, LastUpdatedAt)
VALUES
-- Harshal (Admin): Events 1400, Redeemed 600, Pending 200, Balance 800
(NEWID(), @AdminId, 800, 1400, 600, 200, DATEADD(MONTH, -8, GETUTCDATE()), GETUTCDATE()),
-- Priya (Admin 2): No event earnings, no activity
(NEWID(), @Admin2Id, 0, 0, 0, 0, DATEADD(MONTH, -7, GETUTCDATE()), GETUTCDATE()),
-- Sankalp (MAIN DEMO USER - EMPLOYEE): Events 1750, Redeemed 800, Pending 200, Balance 950
(NEWID(), @Employee1Id, 950, 1750, 800, 200, DATEADD(MONTH, -6, GETUTCDATE()), GETUTCDATE()),
-- Amit: Events 1300, Redeemed 400, Balance 900
(NEWID(), @Employee2Id, 900, 1300, 400, 0, DATEADD(MONTH, -6, GETUTCDATE()), GETUTCDATE()),
-- Neha: Event 200, Redeemed 100, Balance 100
(NEWID(), @Employee3Id, 100, 200, 100, 0, DATEADD(MONTH, -5, GETUTCDATE()), GETUTCDATE()),
-- Rahul: Event 350, Redeemed 150, Balance 200
(NEWID(), @Employee4Id, 200, 350, 150, 0, DATEADD(MONTH, -4, GETUTCDATE()), GETUTCDATE()),
-- Ananya: No event earnings, no activity
(NEWID(), @Employee5Id, 0, 0, 0, 0, DATEADD(MONTH, -3, GETUTCDATE()), GETUTCDATE()),
-- Vikram: No event earnings, no activity
(NEWID(), @Employee6Id, 0, 0, 0, 0, DATEADD(MONTH, -2, GETUTCDATE()), GETUTCDATE())

-- ========================================
-- STEP 13: CREATE POINTS TRANSACTIONS
-- IMPORTANT: Using correct enum values!
-- TransactionType: 'Earned' or 'Redeemed'
-- TransactionSource: 'Event' or 'Redemption' (AdminAward feature not available)
-- Sorted in chronological order with correct BalanceAfter
-- ========================================
PRINT 'Creating points transactions...'

-- =====================
-- HARSHAL (Admin) - MAIN DEMO USER - Rich History
-- Timeline: Dec 7, 2025 to Feb 2, 2026
-- Total Earned: 1400 (Events only)
-- =====================
INSERT INTO UserPointsTransactions (Id, UserId, UserPoints, TransactionType, TransactionSource, SourceId, Description, Timestamp, BalanceAfter)
VALUES
-- Dec 7 (Day -58): Event 1 - 1st Place Code Sprint = 1000 pts
(NEWID(), @AdminId, 1000, 'Earned', 'Event', @Event1Id, '1st Place - AGDATA Code Sprint Q4 2025 - Built automated testing framework that reduced bug detection time by 40%', DATEADD(DAY, -58, GETUTCDATE()), 1000),
-- Dec 20 (Day -45): Event 2 - 2nd Place Tech Quiz = 400 pts
(NEWID(), @AdminId, 400, 'Earned', 'Event', @Event2Id, '2nd Place - Tech Quiz Championship - Scored 92/100 in technical knowledge assessment', DATEADD(DAY, -45, GETUTCDATE()), 1400),
-- Jan 3 (Day -31): Redemption = -400 pts
(NEWID(), @AdminId, 400, 'Redeemed', 'Redemption', @Prod2Id, 'Redeemed: Amazon Gift Card Rs.1000', DATEADD(DAY, -31, GETUTCDATE()), 1000),
-- Jan 20 (Day -14): Redemption = -200 pts
(NEWID(), @AdminId, 200, 'Redeemed', 'Redemption', @Prod1Id, 'Redeemed: Amazon Gift Card Rs.500', DATEADD(DAY, -14, GETUTCDATE()), 800),
-- Feb 2 (Day -1): Pending Redemption = -200 pts (Reserved)
(NEWID(), @AdminId, 200, 'Redeemed', 'Redemption', @Prod4Id, 'Pending: Flipkart Gift Card Rs.500', DATEADD(DAY, -1, GETUTCDATE()), 600)

-- =====================
-- SANKALP (Employee 1) - MAIN DEMO USER - Top Performer
-- Total Earned: 1750 (Events only)
-- =====================
INSERT INTO UserPointsTransactions (Id, UserId, UserPoints, TransactionType, TransactionSource, SourceId, Description, Timestamp, BalanceAfter)
VALUES
-- Dec 7 (Day -58): Event 1 - 2nd Place = 600 pts
(NEWID(), @Employee1Id, 600, 'Earned', 'Event', @Event1Id, '2nd Place - AGDATA Code Sprint Q4 2025 - Innovative API design with microservices architecture', DATEADD(DAY, -58, GETUTCDATE()), 600),
-- Dec 20 (Day -45): Event 2 - 1st Place = 600 pts
(NEWID(), @Employee1Id, 600, 'Earned', 'Event', @Event2Id, '1st Place - Tech Quiz Championship - Perfect Score! Answered all 100 questions correctly', DATEADD(DAY, -45, GETUTCDATE()), 1200),
-- Jan 5 (Day -29): Redemption = -200 pts
(NEWID(), @Employee1Id, 200, 'Redeemed', 'Redemption', @Prod1Id, 'Redeemed: Amazon Gift Card Rs.500', DATEADD(DAY, -29, GETUTCDATE()), 1000),
-- Jan 16 (Day -18): Event 3 - 2nd Place = 550 pts
(NEWID(), @Employee1Id, 550, 'Earned', 'Event', @Event3Id, '2nd Place - Weekend Hackathon 2026 - Built AI-powered feature for customer support', DATEADD(DAY, -18, GETUTCDATE()), 1550),
-- Jan 19 (Day -15): Redemption = -600 pts
(NEWID(), @Employee1Id, 600, 'Redeemed', 'Redemption', @Prod14Id, 'Redeemed: AGDATA Backpack', DATEADD(DAY, -15, GETUTCDATE()), 950),
-- Feb 2 (Day -1): Pending Redemption = -200 pts
(NEWID(), @Employee1Id, 200, 'Redeemed', 'Redemption', @Prod1Id, 'Pending: Amazon Gift Card Rs.500', DATEADD(DAY, -1, GETUTCDATE()), 750)

-- =====================
-- AMIT (Employee 2) - Strong Performer (minimal data)
-- Total Earned: 1300 (Events only)
-- =====================
INSERT INTO UserPointsTransactions (Id, UserId, UserPoints, TransactionType, TransactionSource, SourceId, Description, Timestamp, BalanceAfter)
VALUES
-- Dec 7 (Day -58): Event 1 - 3rd Place = 400 pts
(NEWID(), @Employee2Id, 400, 'Earned', 'Event', @Event1Id, '3rd Place - AGDATA Code Sprint Q4 2025 - Solid implementation', DATEADD(DAY, -58, GETUTCDATE()), 400),
-- Jan 16 (Day -18): Event 3 - 1st Place = 900 pts
(NEWID(), @Employee2Id, 900, 'Earned', 'Event', @Event3Id, '1st Place - Weekend Hackathon 2026 - Winning project!', DATEADD(DAY, -18, GETUTCDATE()), 1300),
-- Jan 25 (Day -9): Redemption = -400 pts
(NEWID(), @Employee2Id, 400, 'Redeemed', 'Redemption', @Prod2Id, 'Redeemed: Amazon Gift Card Rs.1000', DATEADD(DAY, -9, GETUTCDATE()), 900)

-- =====================
-- NEHA (Employee 3) - Minimal data
-- Total Earned: 200 (Event only)
-- =====================
INSERT INTO UserPointsTransactions (Id, UserId, UserPoints, TransactionType, TransactionSource, SourceId, Description, Timestamp, BalanceAfter)
VALUES
-- Dec 20 (Day -45): Event 2 - 3rd Place = 200 pts
(NEWID(), @Employee3Id, 200, 'Earned', 'Event', @Event2Id, '3rd Place - Tech Quiz Championship - Great effort!', DATEADD(DAY, -45, GETUTCDATE()), 200),
-- Jan 24 (Day -10): Redemption = -100 pts
(NEWID(), @Employee3Id, 100, 'Redeemed', 'Redemption', @Prod13Id, 'Redeemed: AGDATA Coffee Mug', DATEADD(DAY, -10, GETUTCDATE()), 100)

-- =====================
-- RAHUL (Employee 4) - Minimal data
-- Total Earned: 350 (Event only)
-- =====================
INSERT INTO UserPointsTransactions (Id, UserId, UserPoints, TransactionType, TransactionSource, SourceId, Description, Timestamp, BalanceAfter)
VALUES
-- Jan 16 (Day -18): Event 3 - 3rd Place = 350 pts
(NEWID(), @Employee4Id, 350, 'Earned', 'Event', @Event3Id, '3rd Place - Weekend Hackathon 2026 - Impressive for a new joiner!', DATEADD(DAY, -18, GETUTCDATE()), 350),
-- Jan 24 (Day -10): Redemption = -150 pts
(NEWID(), @Employee4Id, 150, 'Redeemed', 'Redemption', @Prod5Id, 'Redeemed: BookMyShow Voucher Rs.300', DATEADD(DAY, -10, GETUTCDATE()), 200)

-- =====================
-- PRIYA, ANANYA, VIKRAM - No event wins, no transactions
-- These users have 0 points (no AdminAward feature)
-- =====================

-- ========================================
-- STEP 14: CREATE REDEMPTIONS
-- Link to products with proper status workflow
-- Only for users with event earnings
-- ========================================
PRINT 'Creating redemptions...'

-- HARSHAL's Redemptions (MAIN DEMO USER)
-- Jan 3: Amazon Rs.1000 (400 pts) - Delivered
INSERT INTO Redemptions (Id, UserId, ProductId, PointsSpent, Status, RequestedAt, ApprovedAt, ApprovedBy, ProcessedAt, ProcessedBy, Quantity)
VALUES (NEWID(), @AdminId, @Prod2Id, 400, 'Delivered', DATEADD(DAY, -31, GETUTCDATE()), DATEADD(DAY, -30, GETUTCDATE()), @Admin2Id, DATEADD(DAY, -28, GETUTCDATE()), @Admin2Id, 1)

-- Jan 20: Amazon Rs.500 (200 pts) - Delivered
INSERT INTO Redemptions (Id, UserId, ProductId, PointsSpent, Status, RequestedAt, ApprovedAt, ApprovedBy, ProcessedAt, ProcessedBy, Quantity)
VALUES (NEWID(), @AdminId, @Prod1Id, 200, 'Delivered', DATEADD(DAY, -14, GETUTCDATE()), DATEADD(DAY, -13, GETUTCDATE()), @Admin2Id, DATEADD(DAY, -12, GETUTCDATE()), @Admin2Id, 1)

-- Feb 2: Flipkart Rs.500 (200 pts) - Pending
INSERT INTO Redemptions (Id, UserId, ProductId, PointsSpent, Status, RequestedAt, Quantity)
VALUES (NEWID(), @AdminId, @Prod4Id, 200, 'Pending', DATEADD(DAY, -1, GETUTCDATE()), 1)

-- SANKALP's Redemptions (MAIN DEMO USER)
-- Jan 5: Amazon Rs.500 (200 pts) - Delivered
INSERT INTO Redemptions (Id, UserId, ProductId, PointsSpent, Status, RequestedAt, ApprovedAt, ApprovedBy, ProcessedAt, ProcessedBy, Quantity)
VALUES (NEWID(), @Employee1Id, @Prod1Id, 200, 'Delivered', DATEADD(DAY, -29, GETUTCDATE()), DATEADD(DAY, -28, GETUTCDATE()), @AdminId, DATEADD(DAY, -26, GETUTCDATE()), @AdminId, 1)

-- Jan 19: AGDATA Backpack (600 pts) - Delivered
INSERT INTO Redemptions (Id, UserId, ProductId, PointsSpent, Status, RequestedAt, ApprovedAt, ApprovedBy, ProcessedAt, ProcessedBy, Quantity)
VALUES (NEWID(), @Employee1Id, @Prod14Id, 600, 'Delivered', DATEADD(DAY, -15, GETUTCDATE()), DATEADD(DAY, -14, GETUTCDATE()), @AdminId, DATEADD(DAY, -12, GETUTCDATE()), @AdminId, 1)

-- Feb 2: Amazon Rs.500 (200 pts) - Pending
INSERT INTO Redemptions (Id, UserId, ProductId, PointsSpent, Status, RequestedAt, Quantity)
VALUES (NEWID(), @Employee1Id, @Prod1Id, 200, 'Pending', DATEADD(DAY, -1, GETUTCDATE()), 1)

-- AMIT's Redemption (Minimal)
-- Jan 25: Amazon Rs.1000 (400 pts) - Delivered
INSERT INTO Redemptions (Id, UserId, ProductId, PointsSpent, Status, RequestedAt, ApprovedAt, ApprovedBy, ProcessedAt, ProcessedBy, Quantity)
VALUES (NEWID(), @Employee2Id, @Prod2Id, 400, 'Delivered', DATEADD(DAY, -9, GETUTCDATE()), DATEADD(DAY, -8, GETUTCDATE()), @AdminId, DATEADD(DAY, -7, GETUTCDATE()), @AdminId, 1)

-- NEHA's Redemption (Minimal)
-- Jan 24: Coffee Mug (100 pts) - Delivered
INSERT INTO Redemptions (Id, UserId, ProductId, PointsSpent, Status, RequestedAt, ApprovedAt, ApprovedBy, ProcessedAt, ProcessedBy, Quantity)
VALUES (NEWID(), @Employee3Id, @Prod13Id, 100, 'Delivered', DATEADD(DAY, -10, GETUTCDATE()), DATEADD(DAY, -9, GETUTCDATE()), @AdminId, DATEADD(DAY, -8, GETUTCDATE()), @AdminId, 1)

-- RAHUL's Redemption (Minimal)
-- Jan 24: BookMyShow (150 pts) - Delivered
INSERT INTO Redemptions (Id, UserId, ProductId, PointsSpent, Status, RequestedAt, ApprovedAt, ApprovedBy, ProcessedAt, ProcessedBy, Quantity)
VALUES (NEWID(), @Employee4Id, @Prod5Id, 150, 'Delivered', DATEADD(DAY, -10, GETUTCDATE()), DATEADD(DAY, -9, GETUTCDATE()), @AdminId, DATEADD(DAY, -8, GETUTCDATE()), @AdminId, 1)

-- NO redemptions for Priya, Ananya, Vikram (they have 0 points)

-- ========================================
-- STEP 15: ADMIN BUDGETS (NOT USED - AdminAward feature not available)
-- Keeping empty for future use
-- ========================================
PRINT 'Skipping admin budgets (AdminAward feature not available)...'

-- ========================================
-- VERIFICATION QUERIES
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
PRINT '============================================='
PRINT 'MAIN DEMO USERS:'
PRINT '============================================='
PRINT ''
PRINT 'HARSHAL BEHARE (Admin) - Points from Events only:'
PRINT '  - Code Sprint Q4 2025: 1st Place = 1000 pts (Dec 7)'
PRINT '  - Tech Quiz Championship: 2nd Place = 400 pts (Dec 20)'
PRINT '  - Total Earned: 1400 pts'
PRINT '  - Redeemed: Amazon Rs.1000 (-400), Amazon Rs.500 (-200)'
PRINT '  - Pending: Flipkart Rs.500 (-200)'
PRINT '  - CURRENT BALANCE: 800 pts (600 available)'
PRINT ''
PRINT 'SANKALP CHAKRE (Employee) - Points from Events only:'
PRINT '  - Code Sprint Q4 2025: 2nd Place = 600 pts (Dec 7)'
PRINT '  - Tech Quiz Championship: 1st Place = 600 pts (Dec 20)'
PRINT '  - Weekend Hackathon 2026: 2nd Place = 550 pts (Jan 16)'
PRINT '  - Total Earned: 1750 pts'
PRINT '  - Redeemed: Amazon Rs.500 (-200), AGDATA Backpack (-600)'
PRINT '  - Pending: Amazon Rs.500 (-200)'
PRINT '  - CURRENT BALANCE: 950 pts (750 available)'
PRINT '============================================='
PRINT ''

-- Data Summary
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

PRINT ''
PRINT 'TOP EMPLOYEES BY TOTAL POINTS EARNED (Events only):'
SELECT TOP 5 u.FirstName + ' ' + u.LastName AS Name, upa.CurrentBalance, upa.TotalEarned, upa.TotalRedeemed
FROM Users u
JOIN UserPointsAccounts upa ON u.Id = upa.UserId
WHERE upa.TotalEarned > 0
ORDER BY upa.TotalEarned DESC

PRINT ''
PRINT 'DEMO USER TRANSACTIONS:'
PRINT '--- HARSHAL (Admin) ---'
SELECT 
    t.Description,
    CASE WHEN t.TransactionType = 'Earned' THEN '+' + CAST(t.UserPoints AS VARCHAR) ELSE '-' + CAST(t.UserPoints AS VARCHAR) END AS Points,
    t.TransactionSource AS Source,
    CONVERT(VARCHAR, t.Timestamp, 106) AS Date,
    t.BalanceAfter
FROM UserPointsTransactions t
JOIN Users u ON t.UserId = u.Id
WHERE u.Email = 'Harshal.Behare@agdata.com'
ORDER BY t.Timestamp

PRINT ''
PRINT '--- SANKALP (Employee) ---'
SELECT 
    t.Description,
    CASE WHEN t.TransactionType = 'Earned' THEN '+' + CAST(t.UserPoints AS VARCHAR) ELSE '-' + CAST(t.UserPoints AS VARCHAR) END AS Points,
    t.TransactionSource AS Source,
    CONVERT(VARCHAR, t.Timestamp, 106) AS Date,
    t.BalanceAfter
FROM UserPointsTransactions t
JOIN Users u ON t.UserId = u.Id
WHERE u.Email = 'Sankalp.Chakre@agdata.com'
ORDER BY t.Timestamp

GO