# 🚀 Quick Start - Create Database with EF Core

## ✅ Everything is Ready!

Your EF Core Code-First migration is complete and ready to create the database.

## 📋 Steps to Create Database:

### 1. Find Your SQL Server Name

Open **SSMS 21** and connect. Note the server name from the connection dialog.

Common server names:
- `localhost` or `(local)`
- `localhost\SQLEXPRESS`  
- `(localdb)\MSSQLLocalDB`
- Your computer name (e.g., `DESKTOP-ABC123`)

### 2. Update Connection String

Edit `RewardPointsSystem.Api/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER_NAME;Database=RewardPointsDB;Integrated Security=true;TrustServerCertificate=True"
  }
}
```

Replace `YOUR_SERVER_NAME` with your actual server name from step 1.

### 3. Run One Command

```powershell
dotnet ef database update --project RewardPointsSystem.Infrastructure --startup-project RewardPointsSystem.Api
```

### 4. Verify in SSMS

Refresh SSMS and you'll see:
- Database: `RewardPointsDB`
- 11 Tables: Users, Roles, UserRoles, PointsAccounts, PointsTransactions, Events, EventParticipants, Products, ProductPricing, InventoryItems, Redemptions

### 5. Add Stored Procedure (Optional)

In SSMS, open and run: `Database/02_SP_GetTop3EmployeesByRewards.sql`

---

## 📁 Files You Have

### Database Scripts (Manual SQL approach - if needed):
- `Database/01_CreateSchema.sql` - All tables
- `Database/02_SP_GetTop3EmployeesByRewards.sql` - Top 3 employees procedure
- `Database/03_InsertSampleData.sql` - Test data
- `Database/README.md` - Detailed manual setup guide

### EF Core Approach (Recommended):
- `EF_CORE_SETUP.md` - Complete EF Core guide
- Migration already created in `RewardPointsSystem.Infrastructure/Migrations/`

---

## 🎯 That's It!

One command creates everything:
```powershell
dotnet ef database update --project RewardPointsSystem.Infrastructure --startup-project RewardPointsSystem.Api
```

Then view your database in SSMS 21! 🎉
