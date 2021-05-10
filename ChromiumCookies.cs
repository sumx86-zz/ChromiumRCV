using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

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
        private bool _secure;
        private bool _httpOnly;
        private string _sameSite;

        public void Print()
        {
            // host_key
            // name
            // value
            // path
            // expires_utc
            // firstpartyonly or samesite
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

        public List<ChromiumCookie> GetData()
        {
            List<ChromiumCookie> cookies = new List<ChromiumCookie>();
            return cookies;
        }
    }
}