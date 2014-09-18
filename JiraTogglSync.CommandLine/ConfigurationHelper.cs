using System;
using System.Configuration;
using System.Text;
using System.Security;
using System.Security.Cryptography;

namespace JiraTogglSync.CommandLine
{
    public class ConfigurationHelper
    {
        static byte[] entropy = Encoding.Unicode.GetBytes("JiraTogglSync.Salt");

        public static string GetValueFromConfig(string key, Func<string> askForValue)
        {
            var value = ConfigurationManager.AppSettings[key];

            if (value != null)
                return value;

            value = askForValue();

            SaveConfig(key, value);

            return value;
        }

        public static string GetEncryptedValueFromConfig(string key, Func<string> askForValue)
        {
            var value = ConfigurationManager.AppSettings[key];

            if (value != null)
            {
                try
                {
                    return DecryptString(value);
                }
                catch (CryptographicException ex)
                {
                    throw new ApplicationException(string.Format("Cannot decrypt App.config's '{0}' value. Delete it and relaunch the app.", key), ex);
                }
            }

            value = askForValue();

            SaveConfig(key, EncryptString(value));

            return value;
        }

        private static void SaveConfig(string key, string value)
        {
            Configuration configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            configuration.AppSettings.Settings.Add(key, value);
            configuration.Save();

            ConfigurationManager.RefreshSection("appSettings");
        }

        private static string EncryptString(string input)
        {
            byte[] encryptedData = ProtectedData.Protect(
                Encoding.ASCII.GetBytes(input),
                entropy,
                DataProtectionScope.CurrentUser);

            return Convert.ToBase64String(encryptedData);
        }

        private static string DecryptString(string encryptedData)
        {
            byte[] decryptedData = ProtectedData.Unprotect(
                Convert.FromBase64String(encryptedData),
                entropy,
                DataProtectionScope.CurrentUser);

            return Encoding.ASCII.GetString(decryptedData);
        }
    }
}
