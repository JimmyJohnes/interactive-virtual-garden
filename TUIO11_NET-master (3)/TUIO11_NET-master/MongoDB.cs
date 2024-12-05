using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Driver;
using static TuioDemo;

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

        public void UpdateDocument(string collectionName, string address, int score, List<string> unlockables, List<int> phases, List<String> states,List<String> seeds,int presetNumbe)
        {
            var collection = database.GetCollection<BsonDocument>(collectionName);
            var filter = Builders<BsonDocument>.Filter.Eq("address", address);

            var update = Builders<BsonDocument>.Update
                .Set("score", new BsonInt32(score))
                .Set("unlockables", new BsonArray(unlockables)) 
                .Set("phases", new BsonArray(phases))           
                .Set("states", new BsonArray(states))           
                .Set("seeds", new BsonArray(seeds))
                .Set("preset",new BsonInt32(presetNumbe));

            collection.UpdateOne(filter, update);
            Console.WriteLine("Document updated.");
        }

        public void DeleteDocument(string collectionName, string name)
        {
            var collection = database.GetCollection<BsonDocument>(collectionName);
            var filter = Builders<BsonDocument>.Filter.Eq("name", name);
            collection.DeleteOne(filter);
            Console.WriteLine("Document deleted.");
        }

        public bool DoesAddressExist(string collectionName, string address)
        {
            var collection = database.GetCollection<BsonDocument>(collectionName);
            var filter = Builders<BsonDocument>.Filter.Eq("address", address);
            var count = collection.Find(filter).Limit(1).CountDocuments();
            return count > 0;
        }
        public Device GetUserDevice(string collectionName, string address)
        {
            var collection = database.GetCollection<BsonDocument>(collectionName);
            var filter = Builders<BsonDocument>.Filter.Eq("address", address);

            // Find the document based on the address
            var document = collection.Find(filter).FirstOrDefault();

            if (document == null)
            {
                Console.WriteLine("No document found with the specified address.");
                return null;
            }

            var device = new Device
            {
                name = document.Contains("name") ? document["name"].AsString : "",
                address = document.Contains("address") ? document["address"].AsString : "",
                score = document.Contains("score") ? document["score"].AsInt32 : 0,
                unlockables = document.Contains("unlockables") ? document["unlockables"].AsBsonArray.Select(u => u.AsString).ToList() : new List<string>(),
                seeds = document.Contains("seeds") ? document["seeds"].AsBsonArray.Select(s => s.AsString).ToList() : new List<string>(),
                states = document.Contains("states") ? document["states"].AsBsonArray.Select(s => s.AsString).ToList() : new List<string>(),
                phases = document.Contains("phases") ? document["phases"].AsBsonArray.Select(p => p.AsInt32).ToList() : new List<int>(),
                status = document.Contains("status") ? document["status"].AsString : ""  
            };

            return device;
        }
    }

}
