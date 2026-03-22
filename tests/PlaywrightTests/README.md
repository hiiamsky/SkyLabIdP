# SKYLAB IdP E2E API 測試

這是SkyLab身分認證系統 (SKYLAB IdP) 的端對端 (E2E) API 測試專案，使用 Playwright C# 和 xUnit 框架。

## 🚀 快速開始

### 前置需求

1. **.NET 8.0 SDK**
2. **容器化 API 服務**: SKYLAB IdP API 運行在 `http://localhost:8083/`
3. **環境變數**: 設定必要的環境變數

### 環境變數設定

在執行測試前，請確保設定以下環境變數：

```bash
# API 金鑰
export SKYLABIDP_APIKEY="your-api-key-here"

# Redis 設定（如果需要）
export REDIS__HOST="localhost"
export REDIS__PASSWORD="your-redis-password"

# JWT RSA 私鑰（如果需要）
export JWT_RSA_PRIVATE_KEY="your-base64-encoded-rsa-key"
```

### 啟動 API 容器

```bash
# 建構容器
cd /Users/skyhsieh/Work/ERI/EnvProtection/IdP/SkyLabIdPWorktree/feature/20250807_E2E
docker build -f src/Dockerfile -t skylabidp-api .

# 啟動容器
docker run -p 8083:8080 --env-file .env skylabidp-api
```

### 執行測試

```bash
# 執行所有測試
dotnet test

# 執行特定測試類別
dotnet test --filter "ClassName~AuthenticationFlow"

# 執行特定測試方法
dotnet test --filter "MethodName~Login_WithValidCredentials"

# 產生測試報告
dotnet test --logger:trx --results-directory TestResults
```

## 🏗️ 測試架構

### 專案結構

```
PlaywrightTests/
├── Common/
│   ├── ApiConfig.cs          # API 配置和常數
│   └── ApiTestBase.cs        # 測試基礎類別
├── Models/
│   └── ApiModels.cs          # API 請求/回應模型
└── Tests/
    ├── AuthenticationFlowTests.cs       # 認證流程測試
    ├── MultiAuthHeadersTests.cs         # 多重認證標頭測試
    └── MultiTenantRegistrationTests.cs  # 多租戶註冊測試
```

### 基礎類別

所有測試都繼承 `ApiTestBase`，提供：
- HTTP 請求方法 (GET, POST, PUT, DELETE, PATCH)
- 認證標頭自動處理
- JSON 序列化/反序列化
- 錯誤處理和斷言方法

### 測試配置

- **基礎 URL**: `http://localhost:8083/skylabidp/api/v1`
- **超時時間**: 30 秒
- **並發執行**: 使用 `[Collection("Sequential")]` 防止測試間干擾

## 🎯 測試覆蓋範圍

### 1. 認證流程測試 (`AuthenticationFlowTests`)

基於使用者故事 `01_Authentication_Flow.md`：

- ✅ **成功登入**: 驗證 JWT 令牌產生
- ✅ **令牌刷新**: 測試 Refresh Token 機制
- ✅ **安全登出**: 驗證令牌撤銷和黑名單
- ✅ **JWKS 端點**: 驗證 RSA 公鑰提供
- ✅ **多租戶支援**: 測試不同租戶的登入
- ✅ **錯誤處理**: 無效憑證、過期令牌等

### 2. 多重認證標頭測試 (`MultiAuthHeadersTests`)

基於使用者故事 `03_Multi_Auth_Headers.md`：

- ✅ **三重認證**: X-Tenant-Id + X-API-key + JWT Bearer
- ✅ **公開端點**: 僅需基本標頭的端點
- ✅ **受保護端點**: 需要完整認證的端點
- ✅ **租戶隔離**: 跨租戶存取控制
- ✅ **標頭驗證**: 缺失或無效標頭處理
- ✅ **中間件順序**: 認證管道處理

### 3. 多租戶註冊測試 (`MultiTenantRegistrationTests`)

基於使用者故事 `02_Multi_Tenant_Registration.md`：

- ✅ **SkyLabMgm 註冊**: ServiceAgency 角色分配
- ✅ **開發者註冊**: SkyLab開發者專用端點
- ✅ **委員註冊**: 委員和助理註冊
- ✅ **CAEDP 註冊**: CAEDP 使用者註冊
- ✅ **協作機關註冊**: 預設權限服務
- ✅ **資料驗證**: 密碼強度、電子郵件格式
- ✅ **重複處理**: 重複電子郵件檢查

## 🔧 支援的租戶類型

| 租戶 ID | 描述 | 註冊端點 | 權限服務 |
|---------|------|----------|----------|
| `SkyLabmgm` | SkyLab管理系統 | `/Users/register` | `SkyLabMgmDefaultPermissionService` |
| `SkyLabcaedp` | SkyLab CAEDP | `/Users/register` | `SkyLabCaedpDefaultPermissionService` |
| `SkyLabdevelop` | SkyLab開發者 | `/Users/register-skylab-developer` | `SkyLabDevelopDefaultPermissionService` |
| `SkyLabcommittee` | SkyLab委員 | `/Users/register-skylab-committee` | `SkyLabCommitteeDefaultPermissionService` |
| `SkyLabcommitteeAssistant` | 委員助理 | `/Users/register-skylab-committee` | `SkyLabCommitteeDefaultPermissionService` |
| `SkyLabCollaborativeAgency` | 協作機關 | `/Users/register` | `DefaultPermissionService` |

## 🧪 測試資料管理

### ⚠️ 重要：預先建立測試使用者

**在執行認證相關測試之前**，必須確保以下測試使用者已經在資料庫中建立並啟用：

```csharp
// SkyLabMgm 測試使用者
UserName: "skylabmgm_test@example.com"
Password: "SkyLabMgm123!"
TenantId: "SkyLabmgm"
Status: Active

// 委員測試使用者
UserName: "committee_test@example.com"
Password: "Committee123!"
TenantId: "SkyLabcommittee"
Status: Active

// 開發者測試使用者
UserName: "develop_test@example.com"
Password: "Develop123!"
TenantId: "SkyLabdevelop"
Status: Active
```

**如果測試使用者不存在**，認證測試會失敗並顯示錯誤：
```
Login failed for tenant SkyLabdevelop, user develop_test@example.com. 
Error: Unknown error. Full response: {...,"messages":["帳號或密碼有錯"],"statusCode":403,...}
```

### 建立測試使用者的方法

1. **透過註冊 API**（推薦用於測試環境）
2. **直接插入資料庫**（需要管理員權限）
3. **使用管理介面**（如果有的話）

### 動態測試資料

大部分測試使用動態產生的測試資料：
- 隨機電子郵件地址
- 唯一的使用者名稱
- 符合要求的密碼

## 📊 測試執行報告

### 執行統計

執行測試後，可以在 `TestResults/` 目錄下找到詳細報告：

```bash
# 檢視測試結果摘要
dotnet test --verbosity normal

# 產生詳細的 HTML 報告（需要安裝工具）
dotnet tool install -g dotnet-reportgenerator-globaltool
reportgenerator -reports:"TestResults/*.trx" -targetdir:"TestResults/html" -reporttypes:Html
```

### 效能指標

測試包含效能驗證：
- **登入回應時間**: < 2 秒
- **註冊處理時間**: < 3 秒
- **認證驗證時間**: < 500ms
- **JWKS 端點回應**: < 500ms

## 🔍 疑難排解

### 常見問題

1. **容器未啟動**
   ```bash
   Error: connect ECONNREFUSED 127.0.0.1:8083
   ```
   **解決**: 確保 SKYLAB IdP API 容器正在運行

2. **API 金鑰錯誤**
   ```bash
   401 Unauthorized
   ```
   **解決**: 檢查 `SKYLABIDP_APIKEY` 環境變數是否設定正確

3. **租戶 ID 無效**
   ```bash
   400 Bad Request - Invalid tenant ID
   ```
   **解決**: 確認使用的租戶 ID 在 `TenantIds` 類別中定義

4. **測試使用者不存在**
   ```bash
   401 Unauthorized - Invalid credentials
   ```
   **解決**: 在資料庫中建立測試使用者帳戶

### 偵錯模式

啟用詳細日誌：

```bash
# 設定日誌等級
export PLAYWRIGHT_DEBUG=1

# 執行測試並顯示詳細輸出
dotnet test --verbosity diagnostic
```

## 🚀 持續整合

### GitHub Actions

可以在 CI/CD 管道中執行這些測試：

```yaml
name: E2E API Tests

on: [push, pull_request]

jobs:
  e2e-tests:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      
      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: 8.0.x
          
      - name: Start API Container
        run: |
          docker build -f src/Dockerfile -t skylabidp-api .
          docker run -d -p 8083:8080 --name skylabidp-api --env SKYLABIDP_APIKEY=${{ secrets.SKYLABIDP_APIKEY }} skylabidp-api
          
      - name: Wait for API
        run: sleep 10
        
      - name: Run E2E Tests
        env:
          SKYLABIDP_APIKEY: ${{ secrets.SKYLABIDP_APIKEY }}
        run: dotnet test tests/PlaywrightTests --logger:trx
        
      - name: Upload Test Results
        uses: actions/upload-artifact@v3
        with:
          name: test-results
          path: TestResults/
```

## 📝 貢獻指南

### 新增測試

1. 在 `doc/userstories/` 建立對應的使用者故事
2. 在 `Tests/` 目錄建立新的測試類別
3. 繼承 `ApiTestBase` 並使用 `[Collection("Sequential")]`
4. 遵循 Given-When-Then 模式編寫測試
5. 更新本 README 文件的測試覆蓋範圍

### 程式碼風格

- 使用 `async/await` 進行異步操作
- 測試方法使用描述性命名：`MethodName_Condition_ExpectedResult`
- 使用 Theory/InlineData 進行參數化測試
- 適當的註解和文檔說明

## 📞 支援

如有問題或需要協助，請：
1. 檢查本文檔的疑難排解段落
2. 查看使用者故事文檔 (`doc/userstories/`)
3. 聯繫開發團隊
