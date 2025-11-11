namespace Mazilious.Common.Configuration.Settings;

/// <summary>
/// Configuration settings for connecting to HashiCorp Vault.
/// </summary>
public class VaultSettings
{
    public string Address { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public string MountPath { get; set; } = "kv";
    public string SecretPath { get; set; } = string.Empty;
}