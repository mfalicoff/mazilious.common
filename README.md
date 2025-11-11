# Mazilious.Common

A collection of common utilities and helpers, providing reusable components for configuration management, MongoDB integration, and HashiCorp Vault support.

## Features

### 🔧 Configuration Extensions
- **Vault Integration**: Seamlessly integrate HashiCorp Vault as a configuration source
- **Configuration Binding**: Simplified configuration binding to strongly-typed options classes
- **Override Support**: Support for `appsettings.override.json` for local development

### 🗄️ MongoDB Integration
- **Fluent Builder Pattern**: Easy-to-use MongoDB configuration with a fluent API
- **Dependency Injection**: Full support for .NET dependency injection
- **Type-Safe Collections**: Register MongoDB collections with compile-time type safety

## Installation

```bash
dotnet add package Mazilious.Common
```

## Usage

### Configuration with Vault

Configure your application to use HashiCorp Vault for secrets management:

```csharp
using Mazilious.Common.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Configure configuration sources including Vault
builder.Configuration.ConfigureConfiguration();
```

**appsettings.json:**
```json
{
  "VaultSettings": {
    "Address": "https://vault.example.com",
    "Token": "your-vault-token",
    "MountPath": "kv",
    "SecretPath": "myapp/secrets"
  }
}
```

### Binding Configuration to Options

Bind configuration sections to strongly-typed options classes:

```csharp
using Mazilious.Common.Configuration;

// Automatically binds the section name matching the class name
services.BindFromConfiguration<MySettings>(configuration);
```

### MongoDB Setup

Configure MongoDB with the fluent builder pattern:

```csharp
using Mazilious.Common.Mongo;

var builder = WebApplication.CreateBuilder(args);

// Add MongoDB with collections
builder.Services
    .AddMongo(builder.Configuration)
    .ConfigureMongo()
    .RegisterMongoCollection<User>("users")
    .RegisterMongoCollection<Product>("products");
```

**appsettings.json:**
```json
{
  "MongoDbSettings": {
    "ConnectionString": "mongodb://localhost:27017",
    "DatabaseName": "myapp"
  }
}
```

**Using MongoDB in your services:**
```csharp
public class UserService
{
    private readonly IMongoCollection<User> _users;

    public UserService(IMongoCollection<User> users)
    {
        _users = users;
    }

    public async Task<User> GetUserAsync(string id)
    {
        return await _users.Find(u => u.Id == id).FirstOrDefaultAsync();
    }
}
```

## Configuration Classes

### VaultSettings
```csharp
public class VaultSettings
{
    public string Address { get; set; }      // Vault server address
    public string Token { get; set; }        // Authentication token
    public string MountPath { get; set; }    // Mount path (default: "kv")
    public string SecretPath { get; set; }   // Path to secrets
}
```

### MongoDbSettings
```csharp
public record MongoDbSettings
{
    public string ConnectionString { get; set; }  // MongoDB connection string
    public string DatabaseName { get; set; }      // Database name
}
```

## Requirements

- .NET 9.0 or later

## Dependencies

- Microsoft.Extensions.Configuration
- Microsoft.Extensions.DependencyInjection
- MongoDB.Driver
- VaultSharp
- Polly

## License

MIT

## Repository

[https://github.com/mfalicoff/mazilious.common](https://github.com/mfalicoff/mazilious.common)

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

