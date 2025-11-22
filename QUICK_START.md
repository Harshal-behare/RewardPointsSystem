# Reward Points System - Quick Start ⚡

## 🚀 Start the API (30 seconds)

### 1. **Database Migration** (First time only)
```bash
cd RewardPointsSystem.Infrastructure
dotnet ef database update --startup-project ../RewardPointsSystem.Api
```

### 2. **Run the API**
```bash
cd RewardPointsSystem.Api
dotnet run
```

### 3. **Open Swagger**
Navigate to: **http://localhost:5000**

---

## 🎯 Quick Demo Flow (5 minutes)

### **Step 1: Register User**
```
POST /api/v1/auth/register
```
```json
{
  "email": "demo@company.com",
  "password": "Demo123!",
  "confirmPassword": "Demo123!",
  "firstName": "Demo",
  "lastName": "User"
}
```
**Copy the `accessToken` from response**

### **Step 2: Authorize**
- Click "Authorize" button
- Enter: `Bearer <paste-token>`
- Click "Authorize"

### **Step 3: Get Current User**
```
GET /api/v1/auth/me
```
Shows authenticated user profile

### **Step 4: View Points Balance**
```
GET /api/v1/points/balance
```
Shows user's current points

### **Step 5: Browse Products**
```
GET /api/v1/products
```
Shows available products for redemption

---

## 📊 Key Endpoints

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| POST | `/api/v1/auth/register` | ❌ | Register new user |
| POST | `/api/v1/auth/login` | ❌ | Login |
| GET | `/api/v1/auth/me` | ✅ | Current user |
| POST | `/api/v1/auth/refresh` | ❌ | Refresh token |
| POST | `/api/v1/auth/logout` | ✅ | Logout |
| GET | `/api/v1/users` | ✅ Admin | All users |
| GET | `/api/v1/roles` | ✅ | All roles |
| POST | `/api/v1/events` | ✅ Admin | Create event |
| GET | `/api/v1/events` | ✅ | View events |
| GET | `/api/v1/products` | ✅ | Product catalog |
| GET | `/api/v1/points/balance` | ✅ | User balance |
| POST | `/api/v1/redemptions` | ✅ | Redeem points |
| GET | `/api/v1/admin/dashboard` | ✅ Admin | Admin stats |

---

## 🔑 Sample Data

### **Test Users:**
```json
// Employee
{
  "email": "employee@test.com",
  "password": "Test123!",
  "firstName": "John",
  "lastName": "Doe"
}

// For Admin role, manually update in database or seed data
```

### **Sample Event:**
```json
{
  "name": "Q4 2025 Sales",
  "description": "Quarterly sales competition",
  "eventDate": "2025-12-31T23:59:59Z",
  "totalPointsPool": 10000,
  "location": "HQ",
  "category": "Sales"
}
```

---

## ⚙️ Configuration

### **JWT Settings** (appsettings.json)
- Access Token: 15 minutes
- Refresh Token: 7 days
- Algorithm: HMACSHA256

### **Database**
- SQL Server
- Connection String in `appsettings.json`

---

## 🎤 Demo Talking Points

1. **Architecture:** Clean Architecture with DDD
2. **Security:** JWT auth with refresh tokens, PBKDF2 password hashing
3. **Validation:** FluentValidation on all inputs
4. **Documentation:** Swagger/OpenAPI with JWT support
5. **Scalability:** Stateless design, horizontal scaling ready

---

## 🔧 Troubleshooting

**Port already in use?**
```bash
# Change port in launchSettings.json
```

**Database connection error?**
```bash
# Check connection string in appsettings.json
# Ensure SQL Server is running
```

**401 Unauthorized?**
```bash
# Click Authorize button
# Enter: Bearer <your-token>
```

---

## 📚 Full Documentation

- **Demo Guide:** `DEMO_GUIDE.md` (detailed 15-min demo script)
- **Phase 7 Complete:** `PHASE_7_COMPLETE.md` (authentication details)
- **Project Plan:** `PLAN.md` (complete development roadmap)

---

✅ **Ready to Demo!** Open http://localhost:5000 and start exploring! 🚀
