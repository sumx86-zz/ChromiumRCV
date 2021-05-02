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
            Console.WriteLine("HOSTNAME:  " + _hostname);
            Console.WriteLine("USERNAME:  " + _username);
            Console.WriteLine("PASSWORD:  " + _password + "\r\n");
        }
    }
}
