# TTB Stock & Product Service (.NET 10 & PostgreSQL)

ASP.NET Core Web API template connecting to **PostgreSQL** (port 5432) with your exact database schema for `PRODUCT` and `STOCK` tables (`PRODUCT_ID` foreign key), full CRUD operations, inventory adjustment operations, CORS for `http://localhost:3000` & `http://localhost:3001`, and a ready-to-use **Postman Collection**.

---

## 🗄️ Database Schema

### Table: `PRODUCT`
```sql
CREATE TABLE PRODUCT (
    PRODUCT_ID INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    PRODUCT_CODE VARCHAR(50) UNIQUE NOT NULL,
    PRODUCT_NAME VARCHAR(255) UNIQUE NOT NULL,
    PRODUCT_PRICE INT,
    ACTIVE BOOLEAN DEFAULT true,
    CREATE_DATE TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP,
    CREATE_BY VARCHAR(255) DEFAULT 'SYSTEM',
    UPDATE_DATE TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP,
    UPDATE_BY VARCHAR(255) DEFAULT 'SYSTEM'
);
```

### Table: `STOCK`
```sql
CREATE TABLE STOCK (
    STOCK_ID INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    PRODUCT_ID INT,
    AMOUNT INT,
    ACTIVE BOOLEAN DEFAULT true,
    CREATE_DATE TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP,
    CREATE_BY VARCHAR(255) DEFAULT 'SYSTEM',
    UPDATE_DATE TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP,
    UPDATE_BY VARCHAR(255) DEFAULT 'SYSTEM',
    
    CONSTRAINT fk_product FOREIGN KEY(PRODUCT_ID) REFERENCES PRODUCT(PRODUCT_ID) ON DELETE CASCADE
);
```

### Table: `TRANSACTION_LOG`
```sql
CREATE TABLE TRANSACTION_LOG (
    TRANSACTION_ID INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    STOCK_ID INT,
    PRODUCT_ID INT,
    AMOUNT INT,
    CREATE_DATE TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP,
    CREATE_BY VARCHAR(255) DEFAULT 'SYSTEM',

    CONSTRAINT fk_transaction_log_product FOREIGN KEY(PRODUCT_ID) REFERENCES PRODUCT(PRODUCT_ID) ON DELETE SET NULL,
    CONSTRAINT fk_transaction_log_stock FOREIGN KEY(STOCK_ID) REFERENCES STOCK(STOCK_ID) ON DELETE SET NULL
);
```

---

## ⚙️ Configuration & Connection String

Configured in `appsettings.json` and `appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=ttb_stock_db;Username=add me;Password=add me;"
  },
  "Cors": {
    "AllowedOrigins": [
      "http://localhost:3000",
      "http://localhost:3001",
      "https://localhost:3000",
      "https://localhost:3001"
    ]
  }
}
```

---

## 🐳 Quick Start with Docker (PostgreSQL)

```bash
docker compose up -d
```

Starts PostgreSQL on port `5432` with username `"add me"`, password `"add me"`, database `"ttb_stock_db"`, and automatically initializes the tables from `scripts/init.sql`.

---

## 🏃 Running the .NET API

```bash
dotnet run
```

Endpoints available at:
- **API Base**: `http://localhost:5004`
- **Scalar API Interactive UI**: `http://localhost:5004/scalar/v1`
- **OpenAPI Spec**: `http://localhost:5004/openapi/v1.json`

---

## 📮 Postman Collection

Import `postman/ttb-stock-service.postman_collection.json` into Postman.

### Endpoints:
1. **Products**
   - `GET /api/products` (Pagination, keyword search, active filter, sorting)
   - `GET /api/products/{id}` (Get product + stock breakdown)
   - `POST /api/products` (Create product + optional initial amount)
   - `PUT /api/products/{id}` (Update product)
   - `DELETE /api/products/{id}` (Delete product & cascade stocks)
2. **Stocks & Checkout**
   - `GET /api/stocks` (Filter by product/active status)
   - `GET /api/stocks/product/{productId}` (All stocks for product)
   - `GET /api/stocks/{id}` (Stock details)
   - `POST /api/stocks` (Add stock entry)
   - `PUT /api/stocks/{id}` (Update stock amount/active)
   - `POST /api/stocks/adjust` (Stock operations: `1=Add`, `2=Deduct`, `3=Set`)
   - `DELETE /api/stocks/{id}` (Delete stock entry)
   - `POST /api/stocks/checkout` (Checkout items, FIFO stock deduction, multi-validation & transaction log)
   - `GET /api/stocks/transactions` (Audit logs filtered by productId or stockId)
