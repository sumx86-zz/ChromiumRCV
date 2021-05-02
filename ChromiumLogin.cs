using System;
using System.Text;

namespace ChromeRCV
{
    internal sealed class ChromiumLogin
    {
        string _hostname;
        string _username;
        string _password;

        public string Hostname
        {
            get
            {
                return _hostname;
            }
        }

        public string Username
        {
            get
            {
                return _username;
            }
        }

        public string Password
        {
            get
            {
                return _password;
            }
        }

        public ChromiumLogin(string hostname, string username, string password)
        {
            _hostname = hostname;
            _username = username;
            _password = password;
        }

        public override string ToString()
        {
            return string.Format("{0} - {1} - {2}", _hostname, _username, _password);
        }
    }
}
