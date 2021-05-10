using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Data.SQLite;

namespace ChromeRCV
{
    internal sealed class ChromiumCookie
    {
        private string _name;
        private string _value;
        private string _expires;
        private int _maxAge;
        private string _domain;
        private string _path;
        private Int64 _secure;
        private Int64 _httpOnly;
        private Int64 _sameSite;

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public string Value
        {
            get { return _value; }
            set { _value = value; }
        }

        public string Expires
        {
            get { return _expires; }
            set { _expires = value; }
        }

        public int MaxAge
        {
            get { return _maxAge; }
            set { _maxAge = value; }
        }

        public string Domain
        {
            get { return _domain; }
            set { _domain = value; }
        }

        public string Path
        {
            get { return _path; }
            set { _path = value; }
        }

        public Int64 HttpOnly
        {
            get { return _httpOnly; }
            set { _httpOnly = value; }
        }

        public Int64 Secure
        {
            get { return _secure; }
            set { _secure = value; }
        }

        public Int64 SameSite
        {
            get { return _sameSite; }
            set { _sameSite = value; }
        }

        public void Print()
        {
            Console.WriteLine($"{_name}={_value}; Expires={_expires}; Path={_path}; Domain={_domain}; {_secure}; {_httpOnly}; {_sameSite}");
        }
    }

    internal sealed class ChromiumCookiesManager
    {
        private string _browser;
        private string _browserPath;
        private string _tempFile;
        private byte[] _masterKey;
        private readonly string _cookiesPath = "\\User Data\\Default\\Cookies";

        public string Browser
        {
            get
            {
                return _browser;
            }
        }

        public ChromiumCookiesManager() { }

        public ChromiumCookiesManager Reinitialize(string browserPath)
        {
            _browserPath = browserPath;
            var path = browserPath + _cookiesPath;
            if (File.Exists(path) == false)
                return null;

            _masterKey = Crypt.GetMasterKey(browserPath + "\\User Data");
            if (_masterKey == null)
                return null;

            _browser = Utils.GetBrowserFromPath(browserPath);
            _tempFile = Utils.CreateTempFile(path);
            return this;
        }

        public ChromiumCookie ExtractCookieData(SQLiteDataReader reader)
        {
            string name     = (string)reader["name"];
            string domain   = (string)reader["host_key"];
            string path     = (string)reader["path"];
            string value    = Encoding.Default.GetString((byte[])reader["encrypted_value"]);

            Int64 expires   = Convert.ToInt64(reader["expires_utc"]);
            Int64 samesite  = Convert.ToInt64(reader["samesite"]);
            Int64 httponly  = Convert.ToInt64(reader["is_httponly"]);
            Int64 secure    = Convert.ToInt64(reader["is_secure"]);

            ChromiumCookie cookie = new ChromiumCookie();
            cookie.Name     = name;
            cookie.Value    = Crypt.DecryptData(value, _masterKey);
            cookie.Expires  = DateTime.FromFileTimeUtc(10 * Convert.ToInt64(reader["expires_utc"])).ToString();
            cookie.Domain   = domain;
            cookie.Path     = path;
            cookie.SameSite = samesite;
            cookie.HttpOnly = httponly;
            cookie.Secure   = secure;
            return cookie;
        }

        public List<ChromiumCookie> GetData()
        {
            List<ChromiumCookie> cookies = new List<ChromiumCookie>();
            using (SQLiteConnection conn = new SQLiteConnection("Data Source=" + _tempFile + ";Version=3;New=True;Compress=True;")) {
                conn.Open();
                using (SQLiteCommand comm = conn.CreateCommand()) {
                    comm.CommandText = "SELECT host_key, name, encrypted_value, path, is_secure, is_httponly, expires_utc, samesite FROM cookies";
                    //comm.CommandText = "SELECT * FROM cookies";
                    using (SQLiteDataReader reader = comm.ExecuteReader()) {

                        while(reader.Read()) {
                            cookies.Add(ExtractCookieData(reader));
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
            return cookies;
        }
    }
}
