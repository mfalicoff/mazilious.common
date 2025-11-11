using Mazilious.Common.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;

namespace Mazilious.Common.Mongo;

public static class ServiceCollectionExtensions
{
    public static MongoBuilder AddMongo(this IServiceCollection services, IConfiguration configuration)
    {
        MongoBuilder builder = new(services, configuration);
        return builder;
    }
}