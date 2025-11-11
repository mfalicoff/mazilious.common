
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mazilious.Common.Configuration.Settings;
using Microsoft.Extensions.Configuration;
using Polly;
using Polly.Retry;
using VaultSharp;
using VaultSharp.V1.AuthMethods;
using VaultSharp.V1.AuthMethods.Token;
using VaultSharp.V1.Commons;

namespace Mazilious.Common.Configuration;

/// <summary>
/// Configuration source that loads secrets from HashiCorp Vault.
/// </summary>
public class VaultConfigurationSource : IConfigurationSource
{
    public VaultSettings VaultSettings { get; set; } = new();

    public IConfigurationProvider Build(IConfigurationBuilder builder)
    {
        return new VaultConfigurationProvider(VaultSettings);
    }
}

/// <summary>
/// Configuration provider that retrieves secrets from HashiCorp Vault and makes them available to the configuration system.
/// </summary>
public class VaultConfigurationProvider(VaultSettings vaultSettings) : ConfigurationProvider
{
    private readonly VaultSettings _vaultSettings = vaultSettings;

    private static readonly ResiliencePipeline RetryPipeline = new ResiliencePipelineBuilder()
        .AddRetry(
            new RetryStrategyOptions
            {
                ShouldHandle = new PredicateBuilder().Handle<Exception>(ex =>
                    ex.Message.Contains("rate-limited", StringComparison.OrdinalIgnoreCase)
                ),
                MaxRetryAttempts = 5,
                Delay = TimeSpan.FromMilliseconds(100),
                BackoffType = DelayBackoffType.Exponential,
                UseJitter = true,
                OnRetry = args =>
                {
                    Console.WriteLine(
                        $"Rate limited while loading Vault secret. Retrying in {args.RetryDelay.TotalMilliseconds}ms (attempt {args.AttemptNumber + 1})"
                    );
                    return default;
                },
            }
        )
        .Build();

    public override void Load()
    {
        LoadAsync().GetAwaiter().GetResult();
    }

    private async Task LoadAsync()
    {
        try
        {
            IAuthMethodInfo authMethod = new TokenAuthMethodInfo(_vaultSettings.Token);
            VaultClientSettings vaultClientSettings = new(_vaultSettings.Address, authMethod);
            IVaultClient vaultClient = new VaultClient(vaultClientSettings);

            if (string.IsNullOrEmpty(_vaultSettings.SecretPath))
            {
                await LoadAllSecretsFromMountAsync(vaultClient);
            }
            else
            {
                await LoadSecretFromPathAsync(vaultClient, _vaultSettings.SecretPath);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to load secrets from Vault: {ex.Message}");
        }
    }

    private async Task LoadSecretFromPathAsync(IVaultClient vaultClient, string secretPath)
    {
        await RetryPipeline.ExecuteAsync(async cancellationToken =>
        {
            Secret<SecretData> secret = await vaultClient.V1.Secrets.KeyValue.V2.ReadSecretAsync(
                path: secretPath,
                mountPoint: _vaultSettings.MountPath
            );

            if (secret?.Data?.Data == null)
                return;

            IDictionary<string, object> data = secret.Data.Data;

            foreach (KeyValuePair<string, object> kvp in data)
            {
                // Add directly to configuration using the key from Vault
                // Keys should be in format like "MongoDbSettings:ConnectionString"
                Data[kvp.Key] = kvp.Value?.ToString() ?? string.Empty;
            }
        });
    }

    private async Task LoadAllSecretsFromMountAsync(IVaultClient vaultClient)
    {
        Secret<ListInfo> listResponse =
            await vaultClient.V1.Secrets.KeyValue.V2.ReadSecretPathsAsync(
                path: string.Empty,
                mountPoint: _vaultSettings.MountPath
            );

        if (listResponse?.Data?.Keys == null)
            return;

        // Process secrets sequentially to avoid overwhelming Vault with concurrent requests
        foreach (
            string secretPath in listResponse.Data.Keys.Where(secretPath =>
                !secretPath.EndsWith('/')
            )
        )
        {
            try
            {
                await LoadSecretFromPathAsync(vaultClient, secretPath);
                // Small delay between requests to avoid rate limiting
                await Task.Delay(50);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to load secret from path '{secretPath}': {ex.Message}");
            }
        }
    }
}
