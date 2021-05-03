using System;
using System.Collections.Generic;
using System.IO;

namespace ChromeRCV
{
    class Program
    {
        private static string LocalAppdata = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

        private static List<string> Browsers = new List<string>
        {
            LocalAppdata + "\\Google\\Chrome",
            LocalAppdata + "\\Google(x86)\\Chrome",
            LocalAppdata + "\\Microsoft\\Edge"
        };

        public static Dictionary<string, List<ChromiumLogin>> GetLogins()
        {
            Dictionary<string, List<ChromiumLogin>> d = new Dictionary<string, List<ChromiumLogin>>();

            foreach (var browserPath in Browsers) {
                if (Directory.Exists(browserPath)) {
                    byte[] key = Crypt.GetMasterKey(browserPath + "\\User Data");
                    if (key == null)
                        continue;

                    List<ChromiumLogin> logins = new List<ChromiumLogin>();
                    var browser = Utils.GetBrowserFromPath(browserPath);
                    var loginDataPath = browserPath + "\\User Data\\Default\\Login Data";
                    ChromiumLoginManager.GetData(loginDataPath, key, ref logins);
                    d.Add(browser, logins);
                }
            }
            return d;
        }

        public static void Usage()
        {
            Console.WriteLine(
                @"usage: ./ChromeRCV arg
Arguments: 
    cookies   - show all chromium cookies
    passwords - show all chromium passwords
    history   - show all chromium history
    bookmarks - show all chromium bookmarks"
            );
            Console.ReadKey();
        }

        static void Main(string[] args)
        {
            Dictionary<string, List<ChromiumLogin>> loginData = GetLogins();
            foreach (KeyValuePair<string, List<ChromiumLogin>> entry in loginData) {
                var browser = entry.Key;
                Console.WriteLine($"==={browser.ToUpper()}===");

                foreach (var login in entry.Value) {
                    if (!string.IsNullOrEmpty(login.Password)) {
                        login.Print();
                    }
                }
            }
            Console.ReadKey();
        }

        #region cmdvars
            //private static bool logins  = false;
            //private static bool cookies = false;
            //private static bool history = false;
        #endregion
    }
}
