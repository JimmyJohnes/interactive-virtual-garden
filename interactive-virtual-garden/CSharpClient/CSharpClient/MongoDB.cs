using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Driver;

namespace MongoDBOperations
{
    public class MongoDBHandler
    {
        private readonly IMongoDatabase database;

        public MongoDBHandler(string connectionString, string databaseName)
        {
            var client = new MongoClient(connectionString);
            database = client.GetDatabase(databaseName);
        }

        public void InsertDocument(string collectionName, BsonDocument document)
        {
            var collection = database.GetCollection<BsonDocument>(collectionName);
            collection.InsertOne(document);
            Console.WriteLine("Document inserted.");
        }

        public List<BsonDocument> ReadDocuments(string collectionName)
        {
            var collection = database.GetCollection<BsonDocument>(collectionName);
            return collection.Find(new BsonDocument()).ToList();
        }

        public void UpdateDocument(string collectionName, string Address, List<string> unlockables, int Score)
        {
            var collection = database.GetCollection<BsonDocument>(collectionName);
            var filter = Builders<BsonDocument>.Filter.Eq("address", Address);

            // Define the updates for Age and Unlockables
            var update = Builders<BsonDocument>.Update
                            .Set("score", Score)
                            .Set("unlockables", new BsonArray(unlockables));

            collection.UpdateOne(filter, update);
            Console.WriteLine("Document updated.");
        }


        public void DeleteDocument(string collectionName, string name)
        {
            var collection = database.GetCollection<BsonDocument>(collectionName);
            var filter = Builders<BsonDocument>.Filter.Eq("Name", name);
            collection.DeleteOne(filter);
            Console.WriteLine("Document deleted.");
        }

        public bool DoesAddressExist(string collectionName, string address)
        {
            var collection = database.GetCollection<BsonDocument>(collectionName);
            var filter = Builders<BsonDocument>.Filter.Eq("mac_address", address);
            var count = collection.Find(filter).Limit(1).CountDocuments();
            Console.WriteLine("count in db:" + count);
            return count > 0;
        }
    }
}
