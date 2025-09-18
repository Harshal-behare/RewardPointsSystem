# AGDATA Reward Points System — Milestone 1

Console-based, in-memory C# app demonstrating reward points logic for AGDATA internal events.

## 🎯 Scope (Milestone 1)

- Entities: `User`, `Admin`, `Product`, `Event`, `Redemption`, `PointsTransaction`
- OOP: Encapsulation, inheritance (`Admin : User`), interfaces for services
- In-memory services & storage using `List<T>`
- Validations: duplicate users, positive points, sufficient balance, non-negative stock
- Redemption flow with balance/stock checks
- (Optional if implemented) Transaction history for Earn/Redeem

## 🗂 Project Structure

```
RewardPointsSystem
├── Models
│   ├── User.cs
│   ├── Admin.cs
│   ├── Product.cs
│   ├── Event.cs
│   ├── Redemption.cs
│   └── PointsTransaction.cs
├── Interfaces
│   ├── IUserService.cs
│   ├── IProductService.cs
│   ├── IEventService.cs
│   └── IRedemptionService.cs
├── Services
│   ├── UserService.cs
│   ├── ProductService.cs
│   ├── EventService.cs
│   ├── RedemptionService.cs
│   └── (Optional) PointsTransactionService.cs
└── Program.cs
```

## ▶️ Run Locally

- Requirements: **.NET 8 SDK**, Visual Studio 2022+
- Open solution → Set `RewardPointsSystem` as Startup Project → **Ctrl+F5**

## ✅ What’s Implemented

- Add users, preventing duplicates (email/employeeId)
- Add products with validations (required points > 0, stock ≥ 0)
- Create events (and optionally award points via events)
- Add points to user (positive only)
- Redeem product: validates balance & stock, updates both
- (Optional) Transaction history printing in console

## 🧪 Ideas for Next Milestones

- Persist data via EF Core + SQLite
- Web API (Minimal API) and/or UI
- Unit tests (xUnit), CI pipeline
- Roles/permissions for Admin vs User
- Concurrency/locking for stock and balance

## 📄 License

Private / Educational use for Boot Camp.
