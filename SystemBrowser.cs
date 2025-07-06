using IdentityModel.OidcClient.Browser;
using System;
using System.Diagnostics;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

public class SystemBrowser : IBrowser
{
	private readonly string _redirectUri;
	private readonly int _port;

	public SystemBrowser(int port)
	{
		_port = port;
		_redirectUri = $"http://127.0.0.1:{port}/";
	}

	public async Task<BrowserResult> InvokeAsync(BrowserOptions options, CancellationToken cancellationToken = default)
	{
		using (var listener = new HttpListener())
		{
			listener.Prefixes.Add(_redirectUri);
			listener.Start();

			OpenBrowser(options.StartUrl);

			var context = await listener.GetContextAsync();

			var response = context.Response;
			string responseString = "<html><head><meta http-equiv='refresh' content='10;url=http://localhost'></head><body>認證完成，請關閉此視窗。</body></html>";
			var buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
			response.ContentLength64 = buffer.Length;
			response.OutputStream.Write(buffer, 0, buffer.Length);
			response.OutputStream.Close();

			return new BrowserResult
			{
				Response = context.Request.Url.ToString(),
				ResultType = BrowserResultType.Success
			};
		}
	}

	private void OpenBrowser(string url)
	{
		try
		{
			if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
			{
				Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
			}
			else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
			{
				Process.Start("xdg-open", url);
			}
			else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
			{
				Process.Start("open", url);
			}
			else
			{
				throw new PlatformNotSupportedException();
			}
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Failed to launch browser: {ex}");
			throw;
		}
	}
}