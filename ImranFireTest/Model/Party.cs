using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ImranFireTest.Model
{
    class Party
    {
        [BsonId]
        public Guid _Id { get; set; }
        [BsonElement("Id")]
        public int Id { get; set; }
        [BsonElement("PartyName")]
        public string PartyName { get; set; }
        [BsonElement("PartyTypeId")]
        public int PartyTypeId { get; set; }
        [BsonElement("Debit")]
        public int? Debit { get; set; }
        [BsonElement("Credit")]
        public int? Credit { get; set; }
        [BsonElement("ts")]
        public int ts { get; set; }
    }

}
