using Google.Cloud.Firestore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImranFireTest.Models
{
    [FirestoreData]
    class DelRecord
    {
        [FirestoreProperty]
        public int PartyId { get; set; }
        [FirestoreProperty]
        public int LedgerId { get; set; }
    }
}
