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
using ImranFireTest.Model;

namespace ImranFireTest
{
    public partial class Form1 : Form
    {
        //FirestoreDb db;
        static string party_ts = "0";
        static string party_del_ts = "0";
        static string ledger_ts = "0";
        static string ledger_del_ts = "0";
        static MongoHelper db;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //Initialize MongoDb
            string dbName=  ConfigurationManager.AppSettings["DbName"];
            db = new MongoHelper(dbName);

            lblStat.Text = "Initialized";
            lblStat.ForeColor = Color.DarkGreen;
        }

        #region Party Update Algorithm
        private async Task PartyUpdate()
        {
            //1
            party_ts = db.MaxTs("Party");
            //2
            SqlHelper h = new SqlHelper();
            SqlDataReader dr = h.GetReaderBySQL("SELECT * FROM web_vw_party WHERE ts>" + party_ts + " Order By ts");
            //3
            while (dr.Read())
            {
                Console.WriteLine(dr.GetString(1));
                AddToPartyCollection(dr);
            }
            dr.Close();
            Console.WriteLine("Party TimeStamp: " + party_ts);
            ///////////////////////////////////////////////////END PARTY ALGORITHM/////////////////////////////////////////////////////////////////
            lblStat.Text = "Running Party_Update Batch Operation till " + party_ts + " timestamp";

        }
        private static void AddToPartyCollection(SqlDataReader sqlReader)
        {
            Party p = new Party();
            p.Id = sqlReader.GetInt32(0);
            p.PartyName = sqlReader.GetString(1);
            p.PartyTypeId = sqlReader.GetInt32(2);
            p.Debit = (sqlReader[3] as int?).GetValueOrDefault();
            p.Credit = (sqlReader[4] as int?).GetValueOrDefault();
            p.ts = sqlReader.GetInt32(5);
            var result = db.LoadRecordById<Party>("Party", p.Id);
            if (result == null)
                db.InsertRecord<Party>("Party", p);
            else
                db.UpsertRecord<Party>("Party", result._Id, p);
            party_ts = p.ts.ToString();
        }

        #endregion

        #region Party Delete Algorithm
        private async Task PartyDelete()
        {
            ////////////////////////////////////////////////////////PARTY DEL ALGORITHM////////////////////////////////////////////////////////////////
            /* ALGORITHM
            1.Get Max Ts
            2.Get Results From SqlServer > ts
            3.loop start
                AddParty() Method
            4.loop end
            */
            ////////////////////////////////////////////////////////PARTY ALGORITHM////////////////////////////////////////////////////////////////
            //1
            party_del_ts = db.MaxTs("PartyDel");
            //2
            SqlHelper h = new SqlHelper();
            SqlDataReader dr = h.GetReaderBySQL("SELECT * FROM web_vw_party_del WHERE ts>" + party_del_ts + " Order By ts");
            //3
            while (dr.Read())
            {
                Console.WriteLine(dr.GetString(1));
                AddToPartyDelCollection(dr);
            }
            dr.Close();
            Console.WriteLine("Party Del TimeStamp: " + party_del_ts);
            ///////////////////////////////////////////////////END PARTY ALGORITHM/////////////////////////////////////////////////////////////////
            lblStat.Text = "Running Party_Delete Batch Operation till " + party_del_ts + " timestamp";
        }

        private static void AddToPartyDelCollection(SqlDataReader sqlReader)
        {
            PartyDelLog p = new PartyDelLog();
            p.DelId = sqlReader.GetInt32(0);
            p.ts = sqlReader.GetInt32(1);
            db.InsertRecord<PartyDelLog>("PartyDelLog", p);
            db.DeleteRecord<Party>("Party", p.DelId);
            party_del_ts = p.ts.ToString();
        }
        #endregion

        #region Ledger Update Algorithm
        private async Task LedgerUpdate()
        {
            ////////////////////////////////////////////////////////Ledger ALGORITHM////////////////////////////////////////////////////////////////
            //1
            ledger_ts = db.MaxTs("Ledger");
            //2
            SqlHelper h = new SqlHelper();
            SqlDataReader dr = h.GetReaderBySQL("SELECT * FROM web_vw_ledger WHERE ts>" + ledger_ts + " Order By ts");
            //3
            while (dr.Read())
            {
                Console.WriteLine(dr.GetString(1));
                AddToLedgerCollection(dr);
            }
            dr.Close();
            Console.WriteLine("Party TimeStamp: " + party_ts);
            ///////////////////////////////////////////////////END PARTY ALGORITHM/////////////////////////////////////////////////////////////////
            lblStat.Text = "Running Ledger_Update Batch Operation till " + ledger_ts + " timestamp";
        }

        private static void AddToLedgerCollection(SqlDataReader sqlReader)
        {
            Ledger g = new Ledger();
            g.Id = sqlReader.GetInt32(0);
            g.PartyID = sqlReader.GetInt32(1);
            if (sqlReader.IsDBNull(2))
            {
                g.VocNo = null;
            }
            else
            {
                g.VocNo = sqlReader.GetInt32(2);
            }

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
            var result = db.LoadRecordById<Ledger>("Ledger", g.Id);
            if (result == null)
                db.InsertRecord<Ledger>("Ledger", g);
            else
                db.UpsertRecord<Ledger>("Ledger", result._Id, g);
            ledger_ts = g.ts.ToString();
        }

        #endregion

        #region Ledger Delete Algorithm
        private async Task LedgerDelete()
        {
            //1
            ledger_del_ts = db.MaxTs("LedgerDelLog");
            //2
            SqlHelper h = new SqlHelper();
            SqlDataReader dr = h.GetReaderBySQL("SELECT * FROM tbl_ledger_del WHERE ts>" + ledger_del_ts + " Order By ts");
            //3
            while (dr.Read())
            {
                Console.WriteLine(dr.GetString(1));
                AddToLedgerDelCollection(dr);
            }
            dr.Close();
            Console.WriteLine("Ledger Del TimeStamp: " + ledger_del_ts);
            ///////////////////////////////////////////////////END PARTY ALGORITHM/////////////////////////////////////////////////////////////////
            lblStat.Text = "Running Ledger_Delete Batch Operation till " + ledger_del_ts + " timestamp";
        }

        private static void AddToLedgerDelCollection(SqlDataReader sqlReader)
        {
            LedgerDelLog p = new LedgerDelLog();
            p.DelId = sqlReader.GetInt32(0);
            p.ts = sqlReader.GetInt32(1);
            db.InsertRecord<LedgerDelLog>("PartyDelLog", p);
            db.DeleteRecord<Ledger>("Ledger", p.DelId);
            ledger_del_ts = p.ts.ToString();
        }
        #endregion

        private async void button1_Click(object sender, EventArgs e)
        {
            await PartyDelete();
        }

        private async void btnManualSync_Click(object sender, EventArgs e)
        {
            await UpdateAll();
        }
        async Task UpdateAll()
        {
            try
            {
                lblStat.Text = "Running Party_Update Batch Operation";
                lblStat.ForeColor = Color.LightGoldenrodYellow;
                await PartyUpdate();
                lblStat.Text = "Running Party_Delete Batch Operation";
                await PartyDelete();
                lblStat.Text = "Running Ledger_Update Batch Operation";
                await LedgerUpdate();
                lblStat.Text = "Running Ledger_Delete Batch Operation";
                await LedgerDelete();
                lblStat.Text = "All Batch Operations Completed.";
                lblStat.ForeColor = Color.DarkGreen;
            }
            catch (Exception ex)
            {

                lblStat.Text = ex.ToString();
                lblStat.ForeColor = Color.Red;
            }

        }
        private async void timer1_Tick(object sender, EventArgs e)
        {
            lblStat.Text = "Starting to sync with cloud";
            lblStat.ForeColor = Color.DarkGreen;
            await Task.Delay(10000);
            await UpdateAll();

        }

        private void btnStartTimer_Click(object sender, EventArgs e)
        {
            timer1.Enabled = true;
            btnStopTimer.Enabled = true;
            btnStartTimer.Enabled = false;
        }

        private void btnStopTimer_Click(object sender, EventArgs e)
        {
            timer1.Enabled = false;
            btnStopTimer.Enabled = false;
            btnStartTimer.Enabled = true;

        }

        private async void button4_Click(object sender, EventArgs e)
        {
            await LedgerUpdate();
        }

        private async void button5_Click(object sender, EventArgs e)
        {
            await LedgerDelete();
        }
    }
}
