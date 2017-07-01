using System;
using System.Configuration;
using System.Text;
using System.Security.Cryptography;

namespace JiraTogglSync.CommandLine
{
	public class ConfigurationHelper
	{
		private static readonly byte[] Entropy = Encoding.Unicode.GetBytes("JiraTogglSync.Salt");

		public static string GetValueFromConfig(string key, Func<string> askForValue, string defaultValue = null, Func<string, bool> isValueValid = null)
		{
			var value = ConfigurationManager.AppSettings[key];

			if (value != null)
				return value;

			value = AskForValueOrUseDefault(askForValue, defaultValue);

			while (isValueValid != null && !isValueValid(value))
			{
				value = AskForValueOrUseDefault(askForValue, defaultValue);
			}

			SaveConfig(key, value);

			return value;
		}

		private static string AskForValueOrUseDefault(Func<string> askForValue, string defaultValue)
		{
			var value = askForValue();

			if (string.IsNullOrEmpty(value))
				value = defaultValue ?? "";

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
					throw new ApplicationException($"Cannot decrypt App.config's '{key}' value. Delete it and relaunch the app.", ex);
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
					Entropy,
					DataProtectionScope.CurrentUser);

			return Convert.ToBase64String(encryptedData);
		}

		private static string DecryptString(string encryptedData)
		{
			byte[] decryptedData = ProtectedData.Unprotect(
					Convert.FromBase64String(encryptedData),
					Entropy,
					DataProtectionScope.CurrentUser);

			return Encoding.ASCII.GetString(decryptedData);
		}
	}
}
