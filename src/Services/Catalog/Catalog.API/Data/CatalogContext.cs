using Catalog.API.Entities;
using Catalog.API.Repositories;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace Catalog.API.Data
{
    public class CatalogContext : ICatalogContext
    {
        public CatalogContext(/*IConfiguration configuration*/ICatalogDatabaseSettings databaseSettings)
        {
            //var client = new MongoClient(configuration.GetValue<string>("CatalogDatabaseSettings:ConnectionString"));
            var client = new MongoClient(databaseSettings.ConnectionString);
            // var database = client.GetDatabase(configuration.GetValue<string>("CatalogDatabaseSettings:DatabaseName"));
            var database = client.GetDatabase(databaseSettings.DatabaseName);

            // Products = database.GetCollection<Product>(configuration.GetValue<string>("CatalogDatabaseSettings:CollectionName"));
            Products = database.GetCollection<Product>(databaseSettings.CollectionName);
            CatalogContextSeed.SeedData(Products);
        }
        public IMongoCollection<Product> Products { get; }
    }
}
