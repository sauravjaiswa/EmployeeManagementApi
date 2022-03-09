using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EmployeeManagementApi.Models.EmployeeDto;
using Microsoft.VisualBasic.FileIO;

namespace EmployeeManagementApi.Repositories.MongoDb
{
    public interface IDocumentRepository<T> where T: class
    {
        Task InsertOneDocumentAsync(T document);

        Task<T> FindDocumentByIdAsync(string key, Guid documentId);
        
        Task<List<T>> FindDocumentsAsync();

        Task<bool> ReplaceDocumentByIdAsync(string documentKey, Guid documentId, T document);

        Task<bool> UpdateDocumentByIdAsync<TField>(string documentKey, Guid documentId, TField properties);

        Task<bool> DeleteOneDocumentAsync(string key, Guid documentId);
    }
}
