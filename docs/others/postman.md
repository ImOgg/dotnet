# Postman 使用筆記

## Environment Variables（環境變數）

### 用途
將重複使用的值（如 base URL、token）存成變數，避免每次手動輸入。

---

### 設定 Base URL

**步驟：**

1. 左側欄點 **Environments** → 點 **+** 新增
2. 命名 environment（例如 `Local`）
3. 新增變數：

| Variable  | Initial Value                  | Current Value                  |
|-----------|-------------------------------|-------------------------------|
| `baseUrl` | `https://localhost:5001/api`  | `https://localhost:5001/api`  |

4. 點 **Save**
5. 右上角下拉選單選 `Local`（預設是 `No Environment`）

**使用方式：**
```
{{baseUrl}}/members
{{baseUrl}}/users/login
```

---

### 自動儲存 Token

**步驟：**

1. 在 environment 新增變數 `token`（Current Value 先留空）
2. 在「登入」請求的 **Tests** 頁籤加入腳本：

```javascript
const res = pm.response.json();
pm.environment.set("token", res.token);
```

3. 其他需要驗證的請求，在 **Authorization** 頁籤：
   - Type 選 `Bearer Token`
   - Token 填 `{{token}}`

這樣登入後 token 自動寫入，不需要每次手動複製貼上。

---

### 常用變數範例

| Variable  | 說明              |
|-----------|-----------------|
| `baseUrl` | API base URL     |
| `token`   | JWT Bearer Token |
