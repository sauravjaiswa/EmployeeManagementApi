using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EmployeeManagementApi.Models.ConfigSettings;
using EmployeeManagementApi.Models.EmployeeDto;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Driver;

namespace EmployeeManagementApi.Repositories.MongoDb
{
    public class MongoDocumentRepository<T>: IDocumentRepository<T> where T: class
    {
        private readonly IMongoCollection<T> collection;

        public MongoDocumentRepository(IMongoDatabase database, CollectionSettings collectionSettings)
        {
            collection = database.GetCollection<T>(collectionSettings.CollectionId);
        }

        public async Task InsertOneDocumentAsync(T document)
        {
            await collection.InsertOneAsync(document);
        }

        public async Task<T> FindDocumentByIdAsync(string key, Guid documentId)
        {
            var result = await collection.Find(Builders<T>.Filter.Eq(key, documentId)).FirstOrDefaultAsync();

            return result;
        }
        
        public async Task<List<T>> FindDocumentsAsync()
        {
            var result = await collection.Find(Builders<T>.Filter.Empty).ToListAsync();

            return result;
        }

        public async Task<bool> ReplaceDocumentByIdAsync(string documentKey, Guid documentId, T document)
        {
            var actionResult =
                await collection.ReplaceOneAsync(Builders<T>.Filter.Eq(documentKey, documentId), document,
                    new ReplaceOptions
                    {
                        IsUpsert = true
                    });

            return actionResult.IsAcknowledged && actionResult.ModifiedCount > 0;
        }

        public async Task<bool> UpdateDocumentByIdAsync<TFields>(string documentKey, Guid documentId, TFields properties)
        {
            var updateDefinition = Builders<T>.Update.Set("LastModified", DateTimeOffset.UtcNow);

            foreach (var prop in typeof(TFields).GetProperties())
            {
                var updatedData = prop.GetValue(properties);

                if (updatedData != null)
                {
                    updateDefinition = updateDefinition.Set(prop.Name, updatedData);
                }
            }

            var result =
                await collection.UpdateOneAsync(Builders<T>.Filter.Eq(documentKey, documentId), updateDefinition);

            return result.IsAcknowledged && result.ModifiedCount > 0;
        }

        public async Task<bool> DeleteOneDocumentAsync(string key, Guid documentId)
        {
            var actionResult = await collection.DeleteOneAsync(Builders<T>.Filter.Eq(key, documentId));

            return actionResult.IsAcknowledged && actionResult.DeletedCount > 0;
        }
    }
}
