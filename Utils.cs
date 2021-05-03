using System;
using System.IO;

namespace ChromeRCV
{
    class Utils
    {
        // creates a temporary duplicate of the file at "filePath"
        public static string CreateTempFile(string filePath)
        {
            if(File.Exists(filePath) == false)
                return string.Empty;
            
            string localAppdata = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string newPath = localAppdata + "//Temp//" + Path.GetRandomFileName();
            File.Copy(filePath, newPath);
            return newPath;
        }

        public static string GetBrowserFromPath(string path)
        {
            var items = path.Split('\\');
            return string.Concat(new string[] { items[items.Length - 2], " ", items[items.Length - 1] });
        }
    }
}
