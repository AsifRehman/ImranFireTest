using MongoDB.Driver;
using MongoDB.Bson;
using System.Collections.Generic;
using System;
using System.Configuration;
using ImranFireTest.Model;
using System.Reflection;

namespace ImranFireTest
{
    public class MongoHelper
    {
        //private string conStr = "mongodb://asif:cosoft@cluster0-shard-00-00.k6lme.mongodb.net:27017,cluster0-shard-00-01.k6lme.mongodb.net:27017,cluster0-shard-00-02.k6lme.mongodb.net:27017/myFirstDatabase?ssl=true&replicaSet=atlas-9yuzik-shard-0&authSource=admin&retryWrites=true&w=majority";
        private string conStr = ConfigurationManager.AppSettings["MongoCon"];

        private IMongoDatabase db;
        public MongoHelper(string database)
        {
            var client = new MongoClient(conStr);
            db = client.GetDatabase(database);

        }

        public void InsertRecord<T>(string table, T record)
        {
            var col = db.GetCollection<T>(table);
            col.InsertOne(record);
        }

        public List<T> ReadRecords<T>(string table, T record)
        {
            var col = db.GetCollection<T>(table);
            return col.Find(new BsonDocument()).ToList();
        }

        public string MaxTs(string table)
        {
            var col = db.GetCollection<BsonDocument>(table);
            var result = col.Find(new BsonDocument()).SortByDescending(m => m["ts"]).Limit(1).FirstOrDefault();
            if (result == null)
                return "0";
            else
                return result.GetValue("ts").ToString();
        }
        public T LoadRecordById<T>(string table, int id)
        {
            var col = db.GetCollection<T>(table);
            var filter = Builders<T>.Filter.Eq("Id", id);

            return col.Find(filter).FirstOrDefault();
        }
        public void UpsertRecord<T>(string table, Guid _id, T record)
        {
            var col = db.GetCollection<T>(table);
            var result = col.ReplaceOne(
                new BsonDocument("_id", _id),
                record);
        }
        //public void UpsertLedger(Guid _id, Ledger record)
        //{
        //    var filter = Builders<BsonDocument>.Filter.Eq("_id", _id);
        //    var update = Builders<BsonDocument>.Update.Set("class_id", 483);

        //    var col = db.GetCollection<Ledger>("Ledger");
        //}
        public void DeleteRecord<T>(string table, int id)
        {
            var col = db.GetCollection<T>(table);
            var filter = Builders<T>.Filter.Eq("Id", id);
            col.DeleteOne(filter);
        }

    }
}
