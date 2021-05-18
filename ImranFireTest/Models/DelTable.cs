using Google.Cloud.Firestore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImranFireTest.Models
{
    [FirestoreData]
    class DelTable
    {
        [FirestoreProperty]
        public int Id { get; set; }
        [FirestoreProperty]
        public int DelID { get; set; }
    }
}
