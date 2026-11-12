using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using ManiaAPI.MCP.Configuration;
using Microsoft.AspNetCore.DataProtection;

namespace ManiaAPI.MCP.Accounts;

public sealed class LinkedAccountStore(McpOptions options, IDataProtectionProvider protection)
{
    private readonly IDataProtector protector = protection.CreateProtector("ManiaAPI.MCP.LinkedTokens.v1");

    public async Task<LinkedTokens?> ReadAsync(string caller, string provider, CancellationToken cancellationToken)
    {
        var path = PathFor(caller, provider);

        if (!File.Exists(path))
        {
            return null;
        }

        var encrypted = await File.ReadAllBytesAsync(path, cancellationToken);

        try
        {
            var plaintext = protector.CreateProtector(caller, provider).Unprotect(encrypted);

            return JsonSerializer.Deserialize(plaintext, AccountJsonContext.Default.LinkedTokens);
        }
        catch (Exception exception) when (exception is CryptographicException or JsonException)
        {
            throw new InvalidOperationException("The linked account cannot be decrypted. Check the shared data-protection key ring.");
        }
    }

    public async Task WriteAsync(string caller, string provider, LinkedTokens tokens, CancellationToken cancellationToken)
    {
        var path = PathFor(caller, provider);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);

        var plaintext = JsonSerializer.SerializeToUtf8Bytes(tokens, AccountJsonContext.Default.LinkedTokens);
        var encrypted = protector.CreateProtector(caller, provider).Protect(plaintext);
        var temporary = path + "." + Guid.NewGuid().ToString("N") + ".tmp";

        try
        {
            await File.WriteAllBytesAsync(temporary, encrypted, cancellationToken);
            File.Move(temporary, path, overwrite: true);
        }
        finally
        {
            if (File.Exists(temporary))
            {
                File.Delete(temporary);
            }
        }
    }

    public async Task<IAsyncDisposable> LockAsync(string caller, string provider, CancellationToken cancellationToken)
    {
        var path = PathFor(caller, provider) + ".lock";
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);

        for (var attempt = 0; ; attempt++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                return new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
            }
            catch (IOException) when (attempt < 100)
            {
                await Task.Delay(100, cancellationToken);
            }
        }
    }

    public void Delete(string caller, string provider)
    {
        File.Delete(PathFor(caller, provider));
    }

    private string PathFor(string caller, string provider)
    {
        if (!options.AccountLinking.Enabled || string.IsNullOrWhiteSpace(options.AccountLinking.StorePath))
        {
            throw new ArgumentException("Player account linking is not configured.");
        }

        if (provider is not ("trackmania" or "maniaplanet"))
        {
            throw new ArgumentException("The provider must be trackmania or maniaplanet.");
        }

        var filename = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(caller + "\n" + provider)));

        return Path.Combine(Path.GetFullPath(options.AccountLinking.StorePath), filename + ".tokens");
    }
}
