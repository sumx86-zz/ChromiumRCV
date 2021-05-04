using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Data.SQLite;

namespace ChromeRCV
{
    internal sealed class ChromiumHistory
    {
        private string _url;
        private string _title;
        private int _visitcount;

        public string Url
        {
            get { return _url; }
            set { _url = value; }
        }

        public string Title
        {
            get { return _title; }
            set { _title = value; }
        }

        public int VisitCount
        {
            get { return _visitcount; }
            set { _visitcount = value; }
        }

        public ChromiumHistory(string url, string title, int visitcount)
        {
            _url = url;
            _title = title;
            _visitcount = visitcount;
        }

        public ChromiumHistory() { }

        public void Print()
        {
            Console.WriteLine("URL    :  " + _url);
            Console.WriteLine("TITLE  :  " + _title);
            Console.WriteLine("VISITS :  " + _visitcount + "\r\n");
        }
    }

    internal sealed class ChromiumHistoryManager
    {
        private string _browser;
        private string _browserPath;
        private string _tempFile;
        private readonly string _historyPath = "\\User Data\\Default\\History";

        public string Browser
        {
            get
            {
                return _browser;
            }
        }

        public ChromiumHistoryManager() { }

        public ChromiumHistoryManager Reinitialize(string browserPath)
        {
            _browserPath = browserPath;
            var path = browserPath + _historyPath;
            if (File.Exists(path) == false)
                return null;

            _browser = Utils.GetBrowserFromPath(browserPath);
            _tempFile = Utils.CreateTempFile(path);
            return this;
        }

        public List<ChromiumHistory> GetData()
        {
            List<ChromiumHistory> history = new List<ChromiumHistory>();
            using (SQLiteConnection conn = new SQLiteConnection("Data Source=" + _tempFile + ";Version=3;New=True;Compress=True;")) {
                conn.Open();
                using (SQLiteCommand comm = conn.CreateCommand()) {
                    comm.CommandText = "SELECT url, title, visit_count FROM urls ORDER BY visit_count";
                    using (SQLiteDataReader reader = comm.ExecuteReader()) {

                        while (reader.Read()) {
                            string url = reader["url"].ToString();
                            string title = reader["title"].ToString();
                            int visitcount = Convert.ToInt32(reader["visit_count"]);

                            var c = new ChromiumHistory();
                            c.Url = url;
                            c.Title = title;
                            c.VisitCount = visitcount;
                            history.Add(c);
                        }
                    }
                }
                conn.Close();
            }
            try {
                File.Delete(_tempFile);
            }
            catch {
                Console.WriteLine($"Failed to delete {_tempFile.ToUpper()}");
            }
            return history;
        }
    }
}
