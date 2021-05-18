using Google.Cloud.Firestore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImranFireTest.Models
{
    [FirestoreData]
    class Ledger
    {
        [FirestoreProperty]
        public int Id { get; set; }
        [FirestoreProperty]
        public int PartyID { get; set; }
        [FirestoreProperty]
        public int VocNo { get; set; }
        [FirestoreProperty]
        public DateTime Date { get; set; }
        [FirestoreProperty]
        public string TType { get; set; }
        [FirestoreProperty]
        public string Description { get; set; }
        [FirestoreProperty]
        public Int64? Debit { get; set; }
        [FirestoreProperty]
        public Int64? Credit { get; set; }
        [FirestoreProperty]
        public int ts { get; set; }

    }
}
