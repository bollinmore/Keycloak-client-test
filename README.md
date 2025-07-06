# Keycloak + C# (.NET) OIDC 登入常見錯誤 Q&A

## 目錄

1. [dotnet add package 失敗：No versions available for the package](#q1)
2. [dotnet restore 無法找到 package 或其依賴](#q2)
3. [執行程式後卡在 🔐 開始登入...](#q3)
4. [瀏覽器登入後顯示 Not Found，Terminal 沒有回應](#q4)
5. [Error redeeming code: invalid_request / Client Certification missing for MTLS HoK Token Binding](#q5)
6. [取得 Token 後下一步建議](#q6)

---

### <a name="q1"></a>
## Q1：dotnet add package 時出現 `There are no versions available for the package ...`

**說明：**  
你的 `dotnet` 工具沒連上 NuGet 官方套件來源（或是 source 設定有誤）。

**解法：**
1. 執行  
   ```bash
   dotnet nuget list source
   ```
   應該要有  
   ```
   nuget.org [Enabled]
   https://api.nuget.org/v3/index.json
   ```
2. 沒有的話，執行
   ```bash
   dotnet nuget add source https://api.nuget.org/v3/index.json --name nuget.org
   ```
3. 若還是不行，檢查網路、proxy、防火牆；  
   可考慮手動下載 `.nupkg` 離線安裝（參考 Q2）。

---

### <a name="q2"></a>
## Q2：dotnet restore 找不到 package 或其依賴

**說明：**  
通常因為公司網路、離線環境或 RestoreSources 設定錯誤。

**解法：**
1. 手動下載所有專案所需 `.nupkg`（含依賴套件）放到一個資料夾（如 `NuGetOfflinePackages`）。
2. 修改 `.csproj` 增加  
   ```xml
   <RestoreSources>$(MSBuildThisFileDirectory);/完整/本機/路徑/NuGetOfflinePackages</RestoreSources>
   ```
3. 再執行  
   ```bash
   dotnet restore
   ```

---

### <a name="q3"></a>
## Q3：執行程式後卡在 `🔐 開始登入...`

**說明：**  
C# OidcClient 在等 Keycloak 的 redirect，但沒收到回呼，常見於 port 綁定或防火牆/權限問題。

**排查步驟：**
- Keycloak 的 `Valid Redirect URIs` 必須和 `RedirectUri` 設定完全一致（例如都用 `http://127.0.0.1:8000/`，不能有多餘 `/` 或不同網域）。
- Mac 防火牆要允許 dotnet 或 terminal 網路連線。
- 測試 `nc -l 8000`，然後瀏覽器打 `http://127.0.0.1:8000/`，確定本機 port 能打通。
- 換用 `127.0.0.1` 或 `localhost`、不同 port 試試。
- 用 `sudo dotnet run` 測試是否是權限問題。

---

### <a name="q4"></a>
## Q4：瀏覽器登入後顯示 Not Found，Terminal 沒有回應

**說明：**  
大部分情況下 OidcClient 臨時 web server 沒正常啟動，或 redirect URL 不正確，導致 code 沒被程式收到。

**解法：**
- 依 Q3 檢查 port/redirect uri/防火牆。
- 確認 Keycloak 使用者帳號的「Required Actions」全為空（不要有 Update Profile、Verify Email 等）。
- 測試同一段 port 用最簡 HttpListener 程式是否能被本機網頁觸發。
- 確定 Keycloak Client 設定為 Public，沒有打開多餘的安全設定。

---

### <a name="q5"></a>
## Q5：Error redeeming code: `invalid_request / Client Certification missing for MTLS HoK Token Binding`

**說明：**  
Keycloak 啟用 Mutual TLS（MTLS）或 HoK Token Binding，client 沒提供 certificate，導致授權失敗。

**解法：**
- 進 Keycloak 後台 > Clients > [你的 client]
	- 關閉 **Mutual TLS Certificate Bound Access Tokens**
	- 關閉 **Holder-of-key token (HoK) required**
	- 設定為 **Public client**，不要啟用 Client Authentication

---

### <a name="q6"></a>
## Q6：取得 Access Token、ID Token 後，下一步建議？

**說明：**  
可用 token 呼叫受 Keycloak 保護的 API，在 Authorization Header 加上：

```csharp
client.DefaultRequestHeaders.Authorization =
	new AuthenticationHeaderValue("Bearer", accessToken);
```

也可解析 JWT 內容、設計權限控制、串接桌面應用、實作 Refresh Token 等進階功能。

---

## 參考文件

- Keycloak 官方文件: https://www.keycloak.org/documentation
- IdentityModel.OidcClient: https://github.com/IdentityModel/IdentityModel.OidcClient
- JWT 工具: https://jwt.io

---

**有任何其他錯誤或新案例，歡迎隨時更新本 Q&A！**
