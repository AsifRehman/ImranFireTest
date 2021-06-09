using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ImranFireTest.Model
{
    class PartyDelLog
    {
        [BsonId]
        public Guid _Id { get; set; }
        [BsonElement("DelId")]
        public int DelId { get; set; }
        [BsonElement("ts")]
        public int ts { get; set; }
    }

}
