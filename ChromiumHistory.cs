using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ChromeRCV
{
    internal sealed class ChromiumHistory
    {
        string _url;
        string _title;
        string _visitcount;

        public string Url
        {
            get { return _url; } set { _url = value; }
        }

        public string Title
        {
            get { return _title; } set { _title = value; }
        }

        public string VisitCount
        {
            get { return _visitcount; } set { _visitcount = value; }
        }

        public ChromiumHistory(string url, string title, string visitcount)
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

    }
}
