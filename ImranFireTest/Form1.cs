using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Google.Cloud.Firestore;
using ImranFireTest.Models;

namespace ImranFireTest
{
    public partial class Form1 : Form
    {
        FirestoreDb db;
        static string party_ts = "0";
        static string party_del_id = "0";
        static string ledger_ts = "0";
        static string ledger_del_id = "0";

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            var fireStoreJsonFile = ConfigurationManager.AppSettings["fireStoreJsonFile"];
            string path = AppDomain.CurrentDomain.BaseDirectory + fireStoreJsonFile;
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", path);
            var fireStoreName = ConfigurationManager.AppSettings["FireStoreName"];

            db = FirestoreDb.Create(fireStoreName);
            //MessageBox.Show("connected");
            lblStat.Text = "Connected";
            lblStat.ForeColor = Color.DarkGreen;

        }

        private async void button1_Click(object sender, EventArgs e)
        {
            Query qry = db.Collection("Party").WhereEqualTo("id", 243020001);
            QuerySnapshot snp = await qry.GetSnapshotAsync();
            foreach (DocumentSnapshot dc in snp.Documents)
            {
                Console.WriteLine(dc.Id);

            }

        }

        #region "Party Update"
        private async Task Party_Update()
        {
            if (party_ts == "0")
            {
                CollectionReference col = db.Collection("Party");
                Query qry = col.OrderByDescending("ts").Limit(1);
                QuerySnapshot querySnapshot = await qry.GetSnapshotAsync();
                if (querySnapshot.Count == 0)
                {
                    party_ts = "0";
                }
                else
                {
                    foreach (DocumentSnapshot dc in querySnapshot.Documents)
                    {
                        Dictionary<string, object> _party = dc.ToDictionary();
                        party_ts = _party["ts"].ToString();
                    }
                }
            }
            string connetionString;
            SqlConnection sqlCnn;
            SqlCommand sqlCmd;
            string sql;

            connetionString = ConfigurationManager.ConnectionStrings["Db1"].ConnectionString;
            sql = "SELECT PartyNameID, PartyName, PartyTypeID,  Debit, Credit, CAST(ts AS INT) ts FROM tbl_Party WHERE ts>" + party_ts + " ORDER BY ts";

            sqlCnn = new SqlConnection(connetionString);
            try
            {
                sqlCnn.Open();
                sqlCmd = new SqlCommand(sql, sqlCnn);
                SqlDataReader sqlReader = sqlCmd.ExecuteReader();
                while (sqlReader.Read())
                {
                    Party p = new Party();
                    p.Id = sqlReader.GetInt32(0);
                    p.PartyName = sqlReader.GetString(1);
                    p.PartyTypeId = sqlReader.GetInt32(2);
                    p.Debit = (sqlReader[3] as int?).GetValueOrDefault();
                    p.Credit = (sqlReader[4] as int?).GetValueOrDefault();
                    p.ts = sqlReader.GetInt32(5);

                    AddOrUpdate_Party_Store(p);
                }
                sqlReader.Close();
                sqlCmd.Dispose();
                sqlCnn.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Can not open connection ! " + ex.ToString());
            }

        }
        private async void AddOrUpdate_Party_Store(Party p)
        {
            Query qry = db.Collection("Party").WhereEqualTo("Id", p.Id);
            QuerySnapshot snp = await qry.GetSnapshotAsync();
            if (snp.Count() > 0)
            {
                Update_Party_Store(snp.Documents[0].Id, p);
            }
            else
            {
                CollectionReference doc = db.Collection("Party");
                await doc.AddAsync(p);
            }
        }
        private async void Update_Party_Store(string docID, Party upd)
        {
            DocumentReference doc = db.Collection("Party").Document(docID);
            DocumentSnapshot snp = await doc.GetSnapshotAsync();
            if (snp.Exists)
            {
                Party cur = snp.ConvertTo<Party>();
                bool isSame = cur.Id == upd.Id;
                if (isSame) isSame = cur.PartyName == upd.PartyName;
                if (isSame) isSame = cur.PartyTypeId == upd.PartyTypeId;
                if (isSame) isSame = cur.Debit == upd.Debit;
                if (isSame) isSame = cur.Credit == upd.Credit;
                if (isSame) isSame = cur.ts == upd.ts;
                if (!isSame) await doc.SetAsync(upd);
            }
        }
        private async Task
Party_Delete()
        {
            if (party_del_id == "0")
            {
                DocumentReference doc = db.Collection("DelRecord").Document("del");
                DocumentSnapshot snp = await doc.GetSnapshotAsync();
                if (!snp.Exists)
                {
                    party_del_id = "0";
                }
                else
                {
                    Dictionary<string, object> del = snp.ToDictionary();
                    party_del_id = del["PartyId"].ToString();
                }
            }
            string connetionString = null;
            SqlConnection sqlCnn;
            SqlCommand sqlCmd;
            string sql;

            connetionString = ConfigurationManager.ConnectionStrings["Db1"].ConnectionString;
            sql = "SELECT id, delID FROM tbl_Party_Del WHERE id>" + party_del_id + " ORDER BY id";

            sqlCnn = new SqlConnection(connetionString);
            try
            {
                sqlCnn.Open();
                sqlCmd = new SqlCommand(sql, sqlCnn);
                SqlDataReader sqlReader = sqlCmd.ExecuteReader();
                while (sqlReader.Read())
                {
                    DelTable p = new DelTable();
                    p.Id = sqlReader.GetInt32(0);
                    p.DelID = sqlReader.GetInt32(1);
                    Delete_Party_Store(p);
                }
                sqlReader.Close();
                sqlCmd.Dispose();
                sqlCnn.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Can not open connection ! " + ex.ToString());
            }

        }
        private async void Delete_Party_Store(DelTable p)
        {
            Query qry = db.Collection("Party").WhereEqualTo("Id", p.DelID);
            QuerySnapshot q = await qry.GetSnapshotAsync();

            foreach (DocumentSnapshot documentSnapshot in q.Documents)
            {
                DocumentReference doc = db.Collection("Party").Document(documentSnapshot.Id);
                await doc.DeleteAsync();
            }

            DocumentReference delDoc = db.Collection("DelRecord").Document("del");
            DocumentSnapshot snp = await delDoc.GetSnapshotAsync();
            if (snp.Exists)
            {
                DelRecord upd = snp.ConvertTo<DelRecord>();
                upd.PartyId = p.Id;
                await delDoc.SetAsync(upd);
                party_del_id = p.Id.ToString();
            }
        }
        #endregion

        #region "Ledger Update"
        private async Task Ledger_Update()
        {
            if (ledger_ts == "0")
            {

                CollectionReference col = db.Collection("Ledger");
                Query qry = col.OrderByDescending("ts").Limit(1);
                QuerySnapshot querySnapshot = await qry.GetSnapshotAsync();
                if (querySnapshot.Count == 0)
                {
                    ledger_ts = "0";
                }
                else
                {
                    foreach (DocumentSnapshot dc in querySnapshot.Documents)
                    {
                        Dictionary<string, object> _party = dc.ToDictionary();
                        ledger_ts = _party["ts"].ToString();
                    }
                }
            }
            string connetionString = null;
            SqlConnection sqlCnn;
            SqlCommand sqlCmd;
            string sql;

            connetionString = ConfigurationManager.ConnectionStrings["Db1"].ConnectionString;
            sql = "SELECT Id, PartyID, VocNo,  Date, TType, Description, NetDebit as Debit, NetCredit as Credit, CAST(ts AS INT) ts FROM tbl_Ledger WHERE ts>" + ledger_ts + " ORDER BY ts";

            sqlCnn = new SqlConnection(connetionString);
            try
            {
                sqlCnn.Open();
                sqlCmd = new SqlCommand(sql, sqlCnn);
                SqlDataReader sqlReader = sqlCmd.ExecuteReader();
                while (sqlReader.Read())
                {
                    Ledger g = new Ledger();
                    g.Id = sqlReader.GetInt32(0);
                    g.PartyID = sqlReader.GetInt32(1);
                    g.VocNo = sqlReader.GetInt32(2);
                    g.Date = sqlReader.GetDateTime(3).ToUniversalTime();
                    g.TType = sqlReader.GetString(4);
                    g.Description = sqlReader.IsDBNull(5) ? null : sqlReader.GetString(5);
                    if (sqlReader.IsDBNull(6))
                    {
                        g.Debit = null;
                    }
                    else
                    {
                        g.Debit = sqlReader.GetInt64(6);
                    }
                    if (sqlReader.IsDBNull(7))
                    {
                        g.Credit = null;
                    }
                    else
                    {
                        g.Credit = sqlReader.GetInt64(7);
                    }
                    g.ts = sqlReader.GetInt32(8);

                    AddOrUpdate_Ledger_Store(g);
                }
                sqlReader.Close();
                sqlCmd.Dispose();
                sqlCnn.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Can not open connection ! " + ex.ToString());
            }

        }
        private async void AddOrUpdate_Ledger_Store(Ledger g)
        {
            Query qry = db.Collection("Ledger").WhereEqualTo("Id", g.Id);
            QuerySnapshot snp = await qry.GetSnapshotAsync();
            if (snp.Count() > 0)
            {
                Update_Ledger_Store(snp.Documents[0].Id, g);
            }
            else
            {
                CollectionReference doc = db.Collection("Ledger");
                await doc.AddAsync(g);
            }
        }

        private async void Update_Ledger_Store(string docID, Ledger upd)
        {
            DocumentReference doc = db.Collection("Ledger").Document(docID);
            Dictionary<string, object> data = new Dictionary<string, object>()
            {
                    { "Id", upd.Id },
                    { "PartyID", upd.PartyID },
                    { "VocNo", upd.VocNo },
                    { "Date", upd.Date },
                    { "TType", upd.TType },
                    { "Description", upd.Description },
                    { "ts", upd.ts }
            };

            DocumentSnapshot snp = await doc.GetSnapshotAsync();
            if (snp.Exists)
            {
                Ledger cur = snp.ConvertTo<Ledger>();
                bool isSame = cur.Id == upd.Id;
                if (isSame) isSame = cur.PartyID == upd.PartyID;
                if (isSame) isSame = cur.VocNo == upd.VocNo;
                if (isSame) isSame = cur.Date == upd.Date;
                if (isSame) isSame = cur.TType == upd.TType;
                if (isSame) isSame = cur.Description == upd.Description;
                if (isSame) isSame = cur.ts == upd.ts;
                if (!isSame) await doc.SetAsync(upd);
            }
        }
        private async Task
Ledger_Delete()
        {
            if (ledger_del_id == "0")
            {
                DocumentReference doc = db.Collection("DelRecord").Document("del");
                DocumentSnapshot snp = await doc.GetSnapshotAsync();
                if (!snp.Exists)
                {
                    ledger_del_id = "0";
                }
                else
                {
                    Dictionary<string, object> del = snp.ToDictionary();
                    ledger_del_id = del["LedgerId"].ToString();
                }
            }
            string connetionString = null;
            SqlConnection sqlCnn;
            SqlCommand sqlCmd;
            string sql;
            connetionString = ConfigurationManager.ConnectionStrings["Db1"].ConnectionString;
            sql = "SELECT id, delID FROM tbl_Ledger_Del WHERE id>" + ledger_del_id + " ORDER BY id";

            sqlCnn = new SqlConnection(connetionString);
            try
            {
                sqlCnn.Open();
                sqlCmd = new SqlCommand(sql, sqlCnn);
                SqlDataReader sqlReader = sqlCmd.ExecuteReader();
                while (sqlReader.Read())
                {
                    DelTable p = new DelTable();
                    p.Id = sqlReader.GetInt32(0);
                    p.DelID = sqlReader.GetInt32(1);
                    Delete_Ledger_Store(p);
                }
                sqlReader.Close();
                sqlCmd.Dispose();
                sqlCnn.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Can not open connection ! " + ex.ToString());
            }

        }

        private async void Delete_Ledger_Store(DelTable d)
        {
            Query qry = db.Collection("Ledger").WhereEqualTo("Id", d.DelID);
            QuerySnapshot q = await qry.GetSnapshotAsync();

            foreach (DocumentSnapshot documentSnapshot in q.Documents)
            {
                DocumentReference doc = db.Collection("Ledger").Document(documentSnapshot.Id);
                await doc.DeleteAsync();
            }

            DocumentReference delDoc = db.Collection("DelRecord").Document("del");
            DocumentSnapshot snp = await delDoc.GetSnapshotAsync();
            if (snp.Exists)
            {
                DelRecord upd = snp.ConvertTo<DelRecord>();
                upd.LedgerId = d.Id;
                await delDoc.SetAsync(upd);
                party_del_id = d.Id.ToString();
            }
        }
        #endregion

        private async void button2_Click(object sender, EventArgs e)
        {
            try
            {
                lblStat.Text = "Running Party_Update Batch Operation";
                lblStat.ForeColor = Color.LightGoldenrodYellow;
                await Party_Update();
                lblStat.Text = "Running Ledger_Update Batch Operation";
                await Ledger_Update();
                lblStat.Text = "Running Party_Delete Batch Operation";
                await Party_Delete();
                lblStat.Text = "Running Ledger_Delete Batch Operation";
                await Ledger_Delete();
                lblStat.Text = "All Batch Operations Completed.";
                lblStat.ForeColor = Color.DarkGreen;
            }
            catch (Exception ex)
            {

                lblStat.Text = ex.ToString();
                lblStat.ForeColor = Color.Red;
            }
        }
    }
}
