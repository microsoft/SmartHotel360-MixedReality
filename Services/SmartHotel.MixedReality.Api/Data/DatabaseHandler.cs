using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using MongoDb.Bson.NodaTime;
using MongoDB.Driver;
using SmartHotel.MixedReality.Api.Anchors;
using SmartHotel.MixedReality.Api.Topology;

namespace SmartHotel.MixedReality.Api.Data
{
    public interface IDatabaseHandler<T>
    {
        Task<List<T>> Find(Expression<Func<T, bool>> filter);
        Task ReplaceOneAsync(Expression<Func<T, bool>> filter, T entity);
        Task DeleteOneAsync(Expression<Func<T, bool>> filter);
        Task<T> InsertOneAsync(T entity);
        Task<T> FindOne(Expression<Func<T, bool>> filter);
        Task<List<T>> FindIn(Expression<Func<T, string>> field, IEnumerable<string> items);
    }
    
    public class DatabaseHandler<T> : IDatabaseHandler<T>
    {
        private readonly DatabaseSettings _config;
        private readonly MongoClient _documentClient;

        public DatabaseHandler(IOptions<DatabaseSettings> databaseSettings)
        {
            _config = databaseSettings.Value;
            _documentClient = new MongoClient(_config.MongoDbConnectionString);
        }
        
        private IMongoCollection<T> GetAnchorSetMongoCollection()
        {
            IMongoDatabase db = _documentClient.GetDatabase(_config.MongoDbName);
            IMongoCollection<T> anchorTable = db.GetCollection<T>(typeof(T).Name);
            return anchorTable;
        }

        public async Task<List<T>> FindIn(Expression<Func<T, string>> field, IEnumerable<string> items)
        {
            IMongoCollection<T> collection = GetAnchorSetMongoCollection();
            FilterDefinitionBuilder<T> f = new FilterDefinitionBuilder<T>();            

            return await collection.Find(f.In(field, items)).ToListAsync();
        }


        public async Task<List<T>> Find(Expression<Func<T, bool>> filter)
        {
            IMongoCollection<T> collection = GetAnchorSetMongoCollection();
            return await collection.Find(filter).ToListAsync();
        }

        public async Task<T> FindOne(Expression<Func<T, bool>> filter)
        {
            IMongoCollection<T> collection = GetAnchorSetMongoCollection();
            return await collection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task ReplaceOneAsync(Expression<Func<T, bool>> filter, T entity)
        {
            IMongoCollection<T> collection = GetAnchorSetMongoCollection();
            await collection.ReplaceOneAsync(filter, entity, new UpdateOptions(){ IsUpsert = true });
        }
        public async Task DeleteOneAsync(Expression<Func<T, bool>> filter)
        {
            IMongoCollection<T> collection = GetAnchorSetMongoCollection();
            await collection.DeleteOneAsync(filter);
        }
        public async Task<T> InsertOneAsync(T entity)
        {
            IMongoCollection<T> collection = GetAnchorSetMongoCollection();
            await collection.InsertOneAsync(entity);
            return entity;
        }
    }
}
