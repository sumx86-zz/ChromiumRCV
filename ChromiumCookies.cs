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
        private string _domain;
        private string _path;
        private string _secure;
        private string _httpOnly;
        private string _sameSite;

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

        public string HttpOnly
        {
            get { return _httpOnly; }
            set { _httpOnly = value; }
        }

        public string Secure
        {
            get { return _secure; }
            set { _secure = value; }
        }

        public string SameSite
        {
            get { return _sameSite; }
            set { _sameSite = value; }
        }

        // source: https://chromium.googlesource.com/chromium/src/net/+/master/extras/sqlite/sqlite_persistent_cookie_store.cc
        public enum CookieSameSite
        {
            SameSiteUnspecified = -1,
            SameSiteNoRestriction = 0,
            SameSiteLax = 1,
            SameSiteStrict = 2,
            // Deprecated, mapped to SameSiteUnspecified.
            SameSiteExtended = 3
        };

        public string SameSiteToString(int samesite)
        {
            switch(samesite) {
                case (int) CookieSameSite.SameSiteNoRestriction:
                    return "no-restriction";
                case (int) CookieSameSite.SameSiteLax:
                    return "lax";
                case (int) CookieSameSite.SameSiteStrict:
                    return "Strict";
                case (int) CookieSameSite.SameSiteUnspecified:
                case (int) CookieSameSite.SameSiteExtended:
                    return "unspecified";
                default:
                    return "";
            }
        }

        public void Print()
        {
            Console.WriteLine($"{_name}={_value}; Expires={_expires}; Path={_path}; Domain={_domain}; {_secure}; {_httpOnly}; Samesite={_sameSite}");
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
            string name   = (string)reader["name"];
            string domain = (string)reader["host_key"];
            string path   = (string)reader["path"];
            byte[] value  = (byte[])reader["encrypted_value"];

            Int64 expires = Convert.ToInt64(reader["expires_utc"]);
            int samesite  = Convert.ToInt32(reader["samesite"]);
            int httponly  = Convert.ToInt32(reader["is_httponly"]);
            int secure    = Convert.ToInt32(reader["is_secure"]);

            ChromiumCookie cookie = new ChromiumCookie();
            cookie.Name     = name;
            cookie.Value    = Crypt.DecryptData(value, _masterKey);
            cookie.Expires  = DateTime.FromFileTimeUtc(10 * expires).ToString();
            cookie.Domain   = domain;
            cookie.Path     = path;
            cookie.SameSite = cookie.SameSiteToString(samesite);
            cookie.HttpOnly = Convert.ToBoolean(httponly) ? "HttpOnly" : "";
            cookie.Secure   = Convert.ToBoolean(secure)   ? "Secure"   : "";
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