using MongoDB.Driver;

namespace Stock.API.Services
{
    public class MongoDBService
    {
        readonly IMongoDatabase _database;
        public MongoDBService(IConfiguration configuration)
        {
            MongoClient client = new(configuration["MongoDB:Server"]);
            _database = client.GetDatabase(configuration["MongoDB:DBName"]);
        }
        public IMongoCollection<T> GetCollection<T>() => _database.GetCollection<T>(typeof(T).Name.ToLowerInvariant());
    }
}
