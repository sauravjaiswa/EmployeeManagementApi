using EmployeeManagementApi.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace EmployeeManagementApi.Repositories.MongoDb
{
    public class MongoDocumentRepositoryFactory : IMongoDocumentRepositoryFactory
    {
        private readonly IMongoClient mongoClient;
        private readonly MongoDbSettings mongoDbSettings;

        public MongoDocumentRepositoryFactory(IMongoClient mongoClient, IOptions<MongoDbSettings> mongoDbSettings)
        {
            this.mongoClient = mongoClient;
            this.mongoDbSettings = mongoDbSettings.Value;
        }

        public IDocumentRepository<T> GetDocumentRepository<T>() where T : class
        {
            var collectionSettings = mongoDbSettings.Collections[typeof(T).Name];

            return new MongoDocumentRepository<T>(mongoClient.GetDatabase(mongoDbSettings.DatabaseId), collectionSettings);
        }
    }
}
