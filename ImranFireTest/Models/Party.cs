using Google.Cloud.Firestore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImranFireTest.Models
{
    [FirestoreData]
    class Party
    {
        [FirestoreProperty]
        public int Id { get; set; }
        [FirestoreProperty]
        public string PartyName { get; set; }
        [FirestoreProperty("PartyTypeId")]
        public int PartyTypeId { get; set; }
        [FirestoreProperty]
        public int Debit { get; set; }
        [FirestoreProperty]
        public int Credit { get; set; }
        [FirestoreProperty]
        public int ts { get; set; }

    }
}
