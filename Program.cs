using IdentityModel.OidcClient;
using IdentityModel.OidcClient.Browser;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace KeycloakOfflineDemo
{
    class Program
    {
        static async Task Main()
        {
            var options = new OidcClientOptions
            {
                Authority = "http://127.0.0.1:8080/realms/my-app-realm",
                ClientId = "my-desktop-app",
                Scope = "openid profile email",
                RedirectUri = "http://127.0.0.1:8000/",
                Browser = new SystemBrowser(8000),
                Policy = new Policy { RequireIdentityTokenSignature = false }
            };

            var oidcClient = new OidcClient(options);

            Console.WriteLine("🔐 開始登入...");
            var result = await oidcClient.LoginAsync();

            if (result.IsError)
            {
                Console.WriteLine($"❌ 登入失敗: {result.Error}");
                return;
            }

            Console.WriteLine("✅ 登入成功!");
            Console.WriteLine($"👤 使用者: {result.User.Identity.Name}");
            Console.WriteLine($"🔑 Access Token: {result.AccessToken}");
            Console.WriteLine($"🪪 ID Token: {result.IdentityToken}");
        }
    }
}
