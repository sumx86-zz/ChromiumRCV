using System;
using System.Collections.Generic;
using System.IO;

namespace ChromeRCV
{
    internal sealed class ChromiumBookmark
    {
        private string _date_added;
        private string _guid;
        private int _id;
        private string _name;
        private string _type;
        private string _url;

        public string DateAdded
        {
            get { return _date_added; }
            set { _date_added = value; }
        }

        public string Guid
        {
            get { return _guid; }
            set { _guid = value; }
        }

        public int ID
        {
            get { return _id; }
            set { _id = value; }
        }

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public string Type
        {
            get { return _type; }
            set { _type = value; }
        }

        public string Url
        {
            get { return _url; }
            set { _url = value; }
        }

        public ChromiumBookmark(string date, string guid, int id, string name, string type, string url)
        {
            _date_added = date;
            _guid = guid;
            _id = id;
            _name = name;
            _type = type;
            _url = url;
        }

        public ChromiumBookmark() { }

        public void Print()
        {
            Console.WriteLine("URL  :  " + _url);
            Console.WriteLine("NAME :  " + _name);
            Console.WriteLine("ID   :  " + _id);
            Console.WriteLine("DATE :  " + _date_added + "\r\n");
        }
    }

    internal sealed class ChromiumBookmarksManager
    {
        private string _browser;
        private string _browserPath;
        private string _tempFile;
        private readonly string _bookmarksPath = "\\User Data\\Default\\Bookmarks";

        public string Browser
        {
            get
            {
                return _browser;
            }
        }
        public ChromiumBookmarksManager() { }

        public ChromiumBookmarksManager Reinitialize(string browserPath)
        {
            _browserPath = browserPath;
            _browser = Utils.GetBrowserFromPath(browserPath);
            
            var path = browserPath + _bookmarksPath;
            if(File.Exists(path) == false)
                return null;

            _tempFile = Utils.CreateTempFile(path);
            return this;
        }

        public List<ChromiumBookmark> GetData()
        {
            List<ChromiumBookmark> bookmarks = new List<ChromiumBookmark>();
            string data = File.ReadAllText(_tempFile);

            if(!string.IsNullOrEmpty(data)) {
                foreach(SimpleJSON.JSONNode mark in SimpleJSON.JSON.Parse(data)["roots"]["bookmark_bar"]["children"]) {
                    ChromiumBookmark bookmark = new ChromiumBookmark();
                    bookmark.DateAdded = DateTime.FromFileTime(10 * Convert.ToInt64((string)mark["date_added"])).ToString();
                    bookmark.Guid = mark["guid"];
                    bookmark.ID   = mark["id"];
                    bookmark.Name = mark["name"];
                    bookmark.Type = mark["type"];
                    bookmark.Url  = mark["url"];
                    bookmarks.Add(bookmark);
                }
            }
            try {
                File.Delete(_tempFile);
            }
            catch {
                Console.WriteLine($"Failed to delete {_tempFile.ToUpper()}");
            }
            return bookmarks;
        }
    }
}
