
using System.Security.Cryptography;
using System.Text;

namespace SFTPSyncLib
{
    public class DPAPIEncryption
    {
        public static string Encrypt(string plainText)
        {
            byte[] data = Encoding.UTF8.GetBytes(plainText);
            byte[] encrypted = ProtectedData.Protect(data, null, DataProtectionScope.CurrentUser);
            return Convert.ToBase64String(encrypted);
        }

        public static string Decrypt(string cipherText)
        {
            try
            {
                byte[] data = Convert.FromBase64String(cipherText);
                byte[] decrypted = ProtectedData.Unprotect(data, null, DataProtectionScope.CurrentUser);
                return Encoding.UTF8.GetString(decrypted);
            }
            catch (Exception)
            {
                //If we fail to decrypt, chanced are we either didn't have an encrypted password,
                //or the configuration file came from some other user account or system.
                //If we have an input string, return the same string, otherwise return blank.
                return String.IsNullOrWhiteSpace(cipherText) ? String.Empty : cipherText;
            }

        }
    }
}
