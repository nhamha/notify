using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using ServerCore.Utilities.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoDBHelper
{
    public class MongoDBClient
    {
        private IMongoClient _client = null;//new MongoClient("mongodb://localhost:27018");
        private IMongoDatabase _dataBase = null;//= _client.GetDatabase("ReTransaction");
        public MongoDBClient()
        {
        }

        public MongoDBClient(string connectionStr, string dataBaseName)
        {
            ConnectMongoDB(connectionStr, dataBaseName);
        }

        public MongoDBClient(string host, int port, string dataBaseName)
        {
            string conn = string.Format("mongodb://{0}:{1}", host, port);
            ConnectMongoDB(conn, dataBaseName);
        }

        public MongoDBClient(string host, int port, string userName, string password, string dataBaseName)
        {
            string conn = string.Format("mongodb://{0}:{1}@{2}:{3}", userName, password, host, port);
            ConnectMongoDB(conn, dataBaseName);
        }

        public void ConnectMongoDB(string connectionStr, string dataBaseName)
        {
            try
            {
                _client = new MongoClient(connectionStr);
                _dataBase = _client.GetDatabase(dataBaseName);
            }
            catch(Exception ex)
            {
                _client = null;
                _dataBase = null;
                NLogManager.Exception(ex);
            }
        }

        public void CreateCollection(string collectionName)
        {
            try
            {
                _dataBase.CreateCollection(collectionName);
            }
            catch(Exception e)
            {
                NLogManager.Exception(e);
            }
        }

        public void RunCommand(string cmd)
        {
            //try
            //{
            //    var res = _dataBase.RunCommand<string>(cmd);

            //    var collect = _dataBase.GetCollection<BsonDocument>("");

            //    var commandResult = collect.Ru(aggregationCommand);
            //    var response = commandResult.Response;
            //    foreach (BsonDocument result in response["results"].AsBsonArray)
            //    {
            //        // process result
            //    }

            //}
            //catch (Exception e)
            //{

            //}
        }

        public void InsertOne(string collectionName, BsonDocument document)
        {
            try
            {
                var collection = _dataBase.GetCollection<BsonDocument>(collectionName);
                collection.InsertOne(document);
            }
            catch(Exception e)
            {
                NLogManager.Exception(e);
            }
        }

        public async void InsertOneAsync(string collectionName, BsonDocument document)
        {
            try
            {
                var collection = _dataBase.GetCollection<BsonDocument>(collectionName);
                await collection.InsertOneAsync(document);
            }
            catch (Exception e)
            {
                NLogManager.Exception(e);
            }
        }

        public void InsertMany(string collectionName, IEnumerable<BsonDocument> documents)
        {
            try
            {
                var collection = _dataBase.GetCollection<BsonDocument>(collectionName);
                collection.InsertMany(documents);
            }
            catch (Exception e)
            {
                NLogManager.Exception(e);
            }
        }

        public async void InsertManyAsync(string collectionName, IEnumerable<BsonDocument> documents)
        {
            try
            {
                var collection = _dataBase.GetCollection<BsonDocument>(collectionName);
                await collection.InsertManyAsync(documents);
            }
            catch (Exception e)
            {
                NLogManager.Exception(e);
            }
        }

        public List<T> FindAllDocuments<T>(string collectionName, SortDefinition<BsonDocument> sort = null)
        {
            try
            {
                //var builder = Builders<BsonDocument>.Sort;
                //var sort = builder.Ascending("x").Descending("y");

                var collection = _dataBase.GetCollection<BsonDocument>(collectionName);
                List<BsonDocument> list = null;
                if(sort == null)
                    list = collection.Find(new BsonDocument()).ToList();
                else
                    list = collection.Find(new BsonDocument()).Sort(sort).ToList();

                var listT = new List<T>();
                foreach (var docs in list)
                {
                    var obj = BsonSerializer.Deserialize<T>(docs);
                    listT.Add((T)obj);
                }
                return listT;
            }
            catch (Exception e)
            {
                NLogManager.Exception(e);
            }
            return null;
        }

        public async Task<List<T>> FindAllDocumentsAsync<T>(string collectionName, SortDefinition<BsonDocument> sort = null)
        {
            try
            {
                var collection = _dataBase.GetCollection<BsonDocument>(collectionName);

                List<BsonDocument> list = null;
                if (sort == null)
                    list = await collection.Find(new BsonDocument()).ToListAsync();
                else
                    list = await collection.Find(new BsonDocument()).Sort(sort).ToListAsync();

                var listT = new List<T>();

                foreach (var docs in list)
                {
                    var obj = BsonSerializer.Deserialize<T>(docs);
                    listT.Add((T)obj);
                }
                return listT;
            }
            catch (Exception e)
            {
                NLogManager.Exception(e);
            }
            return null;
        }

        public long Count(string collectionName)
        {
            try
            {
                var collection = _dataBase.GetCollection<BsonDocument>(collectionName);
                return collection.Count(new BsonDocument());
            }
            catch (Exception e)
            {
                NLogManager.Exception(e);
            }
            return 0;
        }

        public async Task<long> CountAsync(string collectionName)
        {
            try
            {
                var collection = _dataBase.GetCollection<BsonDocument>(collectionName);
                return await collection.CountAsync(new BsonDocument());
            }
            catch (Exception e)
            {
                NLogManager.Exception(e);
            }
            return 0;
        }

        public List<T> Find<T>(string collectionName, FilterDefinition<BsonDocument> filter, SortDefinition<BsonDocument> sort = null)
        {
            try
            {
                //var builder = Builders<BsonDocument>.Filter;
                //var filters = builder.Eq("x", 10) & builder.Lt("y", 20);

                var collection = _dataBase.GetCollection<BsonDocument>(collectionName);
                List<BsonDocument> list = null;

                if (sort == null)
                    list = collection.Find(filter).ToList();
                else
                    list = collection.Find(filter).Sort(sort).ToList();

                var listT = new List<T>();
                foreach (var docs in list)
                {
                    var obj = BsonSerializer.Deserialize<T>(docs);
                    listT.Add((T)obj);
                }
                return listT;
            }
            catch (Exception e)
            {
                NLogManager.Exception(e);
            }
            return null;
        }

        public async Task<List<T>> FindAsync<T>(string collectionName, FilterDefinition<BsonDocument> filter, SortDefinition<BsonDocument> sort = null)
        {
            try
            {
                //var builder = Builders<BsonDocument>.Filter;
                //var filters = builder.Eq("x", 10) & builder.Lt("y", 20);

                var collection = _dataBase.GetCollection<BsonDocument>(collectionName);
                List<BsonDocument> list = null;
                if (sort == null)
                    list = await collection.Find(filter).ToListAsync();
                else
                    list = await collection.Find(filter).Sort(sort).ToListAsync();

                var listT = new List<T>();
                foreach (var docs in list)
                {
                    var obj = BsonSerializer.Deserialize<T>(docs);
                    listT.Add((T)obj);
                }
                return listT;
            }
            catch (Exception e)
            {
                NLogManager.Exception(e);
            }
            return null;
        }

        public List<T> FindProjection<T>(string collectionName, ProjectionDefinition<BsonDocument> projection, SortDefinition<BsonDocument> sort = null)
        {
            try
            {
                //var builder = Builders<BsonDocument>.Filter;
                //var filters = builder.Eq("x", 10) & builder.Lt("y", 20);
                //var projection = Builders<BsonDocument>.Projection.Include("x").Include("y").Exclude("_id");

                var collection = _dataBase.GetCollection<BsonDocument>(collectionName);
                List<BsonDocument> list = null;

                if (sort == null)
                    list = collection.Find(new BsonDocument()).Project(projection).ToList();
                else
                    list = collection.Find(new BsonDocument()).Project(projection).Sort(sort).ToList();

                var listT = new List<T>();
                foreach (var docs in list)
                {
                    var obj = BsonSerializer.Deserialize<T>(docs);
                    listT.Add((T)obj);
                }
                return listT;
            }
            catch (Exception e)
            {
                NLogManager.Exception(e);
            }
            return null;
        }

        public async Task<List<T>> FindProjectionAsync<T>(string collectionName, ProjectionDefinition<BsonDocument> projection, SortDefinition<BsonDocument> sort = null)
        {
            try
            {
                //var builder = Builders<BsonDocument>.Filter;
                //var filters = builder.Eq("x", 10) & builder.Lt("y", 20);

                var collection = _dataBase.GetCollection<BsonDocument>(collectionName);
                List<BsonDocument> list = null;
                if (sort == null)
                    list = await collection.Find(new BsonDocument()).Project(projection).ToListAsync();
                else
                    list = await collection.Find(new BsonDocument()).Project(projection).Sort(sort).ToListAsync();

                var listT = new List<T>();
                foreach (var docs in list)
                {
                    var obj = BsonSerializer.Deserialize<T>(docs);
                    listT.Add((T)obj);
                }
                return listT;
            }
            catch (Exception e)
            {
                NLogManager.Exception(e);
            }
            return null;
        }

        public long UpdateOne(string collectionName, FilterDefinition<BsonDocument> filter, UpdateDefinition<BsonDocument> update)
        {
            try
            {
                //var filterd = Builders<BsonDocument>.Filter.Eq("i", 10);
                //var updated = Builders<BsonDocument>.Update.Set("i", 110);

                var collection = _dataBase.GetCollection<BsonDocument>(collectionName);
                var res = collection.UpdateOne(filter, update);
                if (res.IsModifiedCountAvailable)
                    return res.ModifiedCount;
            }
            catch (Exception e)
            {
                NLogManager.Exception(e);
            }
            return 0;
        }

        public long UpdateOne(string collectionName, FilterDefinition<BsonDocument> filter, string fieldName, object val)
        {
            try
            {
                var update = Builders<BsonDocument>.Update.Set(fieldName, val);
                var collection = _dataBase.GetCollection<BsonDocument>(collectionName);
                var res = collection.UpdateOne(filter, update);
                if(res.IsModifiedCountAvailable)
                    return res.ModifiedCount;
            }
            catch (Exception e)
            {
                NLogManager.Exception(e);
            }
            return 0;
        }

        public long UpdateMany(string collectionName, FilterDefinition<BsonDocument> filter, UpdateDefinition<BsonDocument> update)
        {
            try
            {
                //var filterd = Builders<BsonDocument>.Filter.Eq("i", 10);
                //var updated = Builders<BsonDocument>.Update.Set("i", 110);

                var collection = _dataBase.GetCollection<BsonDocument>(collectionName);
                var res = collection.UpdateMany(filter, update);
                if (res.IsModifiedCountAvailable)
                    return res.ModifiedCount;
            }
            catch (Exception e)
            {
                NLogManager.Exception(e);
            }
            return 0;
        }

        public async Task<long> UpdateManyAsync(string collectionName, FilterDefinition<BsonDocument> filter, UpdateDefinition<BsonDocument> update)
        {
            try
            {
                var collection = _dataBase.GetCollection<BsonDocument>(collectionName);
                var res = await collection.UpdateManyAsync(filter, update);
                if (res.IsModifiedCountAvailable)
                    return res.ModifiedCount;
            }
            catch (Exception e)
            {
                NLogManager.Exception(e);
            }
            return 0;
        }

        public long DeleteOne(string collectionName, FilterDefinition<BsonDocument> filter)
        {
            try
            {
                var collection = _dataBase.GetCollection<BsonDocument>(collectionName);
                var res = collection.DeleteOne(filter);
                return res.DeletedCount;
            }
            catch (Exception e)
            {
                NLogManager.Exception(e);
            }
            return 0;
        }

        public async Task<long> DeleteOneAsync(string collectionName, FilterDefinition<BsonDocument> filter)
        {
            try
            {
                var collection = _dataBase.GetCollection<BsonDocument>(collectionName);
                var res = await collection.DeleteOneAsync(filter);
                return res.DeletedCount;
            }
            catch (Exception e)
            {
                NLogManager.Exception(e);
            }
            return 0;
        }

        public long DeleteMany(string collectionName, FilterDefinition<BsonDocument> filter)
        {
            try
            {
                var collection = _dataBase.GetCollection<BsonDocument>(collectionName);
                var res = collection.DeleteMany(filter);
                return res.DeletedCount;
            }
            catch (Exception e)
            {
                NLogManager.Exception(e);
            }
            return 0;
        }

        public async Task<long> DeleteManyAsync(string collectionName, FilterDefinition<BsonDocument> filter)
        {
            try
            {
                var collection = _dataBase.GetCollection<BsonDocument>(collectionName);
                var res = await collection.DeleteManyAsync(filter);
                return res.DeletedCount;
            }
            catch (Exception e)
            {
                NLogManager.Exception(e);
            }
            return 0;
        }
    }
}
