using System.Collections.Generic;
using EmployeeManagementApi.Models.ConfigSettings;

namespace EmployeeManagementApi.Models
{
    public class MongoDbSettings
    {
        public string ConnectionString { get; set; }
        public string DatabaseId { get; set; }
        public int CosmosDbAutoScaleThroughput { get; set; }
        public int TtlInDays { get; set; }
        public IDictionary<string, CollectionSettings> Collections { get; set; }
    }
}
