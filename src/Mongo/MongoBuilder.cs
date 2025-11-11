using System;
using Mazilious.Common.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;

namespace Mazilious.Common.Mongo;

public class MongoBuilder(IServiceCollection services, IConfiguration configuration)
{
    public IServiceCollection Services { get; } = services;

    public IConfiguration Configuration { get; } = configuration;
}

public static class MongoBuilderExtensions
{
    public static MongoBuilder ConfigureMongo(this MongoBuilder builder)
    {
        MongoDbSettings mongoSettings = builder.Configuration.GetRequiredSection<MongoDbSettings>();

        builder.Services.AddSingleton<IMongoClient>(sp =>
        {
            if (string.IsNullOrEmpty(mongoSettings.ConnectionString))
            {
                throw new InvalidOperationException(
                    "MongoDB connection string not found. Configure it in MongoDbSettings:ConnectionString or store it in Vault as 'ConnectionString'."
                );
            }

            return new MongoClient(mongoSettings.ConnectionString);
        });

        builder.Services.AddSingleton<IMongoDatabase>(serviceProvider =>
        {
            IMongoClient mongoClient = serviceProvider.GetRequiredService<IMongoClient>();

            if (string.IsNullOrEmpty(mongoSettings.DatabaseName))
            {
                throw new InvalidOperationException(
                    "MongoDB database name not found. Configure it in MongoDbSettings:DatabaseName or store it in Vault as 'DatabaseName'."
                );
            }

            return mongoClient.GetDatabase(mongoSettings.DatabaseName);
        });

        return builder;
    }
    
    public static MongoBuilder RegisterMongoCollection<T>(
        this MongoBuilder builder,
        string collectionName
    )
    {
        builder.Services.AddTransient<IMongoCollection<T>>(serviceProvider =>
        {
            IMongoDatabase mongoDatabase = serviceProvider.GetRequiredService<IMongoDatabase>();
            return mongoDatabase.GetCollection<T>(collectionName);
        });

        return builder;
    }
}