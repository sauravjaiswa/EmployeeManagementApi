namespace EmployeeManagementApi.Repositories.MongoDb
{
    public interface IMongoDocumentRepositoryFactory
    {
        IDocumentRepository<T> GetDocumentRepository<T>() where T : class;
    }
}
