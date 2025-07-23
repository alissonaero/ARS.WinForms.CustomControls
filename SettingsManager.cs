using ARS.WinForms.Properties;
using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Xml.Linq;
using System.Xml.XPath;

namespace ARS.WinForms
{
	/*
	/// ******** 🧠 What this class does *****
	/// 
	///	- Export(string settingsFilePath)
	//	- Saves current user settings(Properties.Settings.Default.Save()).
	///	- Writes the settings to a specified file path using ConfigurationManager.OpenExeConfiguration.
	///	- Import(string settingsFilePath, string pAppSettingsXmlName)
	///	- Loads an external settings file.
	///	- Extracts the XML node corresponding to the settings group (e.g., "MyApp.Properties.Settings").
	///	- Replaces the current settings with the imported ones.
	///	- Reloads the settings to apply changes.
	*/

	/// <summary>
	/// This is a custom utility for importing and exporting user settings in a Windows Forms application using the .settings infrastructure.
	/// It gives your app the ability to persist and transfer user-specific configuration between machines or sessions.
	/// </summary>
	public class SettingsManager
	{
		/// <summary>
		/// Imports settings from an XML file and updates the specified settings group in the application's configuration.
		/// </summary>
		/// <remarks>This method updates the application's configuration by replacing the raw XML of the specified
		/// settings group with the corresponding settings from the provided XML file. After the import, the configuration is
		/// saved and the settings are reloaded to reflect the changes.  If an error occurs during the import process, the
		/// application falls back to the last saved state of the settings.</remarks>
		/// <param name="settingsFilePath">The file path to the XML file containing the settings to import. Must be a valid file path.</param>
		/// <param name="settingsGroupName">The name of the settings group to update. Must match an existing settings group in the application's
		/// configuration.</param>
		/// <exception cref="FileNotFoundException">Thrown if the specified <paramref name="settingsFilePath"/> does not exist.</exception>
		/// <exception cref="ApplicationException">Thrown if an error occurs during the import process. The inner exception provides additional details.</exception>
		public void Import(string settingsFilePath, string settingsGroupName)
		{
			if (!File.Exists(settingsFilePath))
				throw new FileNotFoundException($"Settings file not found: {settingsFilePath}");

			try
			{
				var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal);
				
				var settingsXml = XDocument.Load(settingsFilePath);

				var settingsNode = settingsXml.XPathSelectElements($"//{settingsGroupName}").SingleOrDefault() ?? throw new InvalidOperationException($"Settings group '{settingsGroupName}' not found in XML.");
				
				var section = (config.GetSectionGroup("userSettings")?.Sections[settingsGroupName]) ?? throw new ConfigurationErrorsException($"Settings section '{settingsGroupName}' not found in current config.");
				
				section.SectionInformation.SetRawXml(settingsNode.ToString());
				
				config.Save(ConfigurationSaveMode.Modified);

				ConfigurationManager.RefreshSection("userSettings");
				
				Settings.Default.Reload();
			}
			catch (Exception ex)
			{
				Settings.Default.Reload(); // fallback to last saved state
				throw new ApplicationException("Failed to import settings.", ex);
			}
		}

		/// <summary>
		/// Exports the application's user settings to a specified file.
		/// </summary>
		/// <remarks>This method saves the current user settings and writes them to the specified file. The exported
		/// settings can be used for backup or transfer purposes.</remarks>
		/// <param name="settingsFilePath">The full path of the file where the settings will be saved.  This must be a valid file path and the caller must
		/// have write permissions to the specified location.</param>
		/// <exception cref="ApplicationException">Thrown if the export operation fails due to an error, such as an invalid file path or insufficient permissions.
		/// The inner exception provides additional details about the failure.</exception>
		public void Export(string settingsFilePath)
		{
			try
			{
				Settings.Default.Save();

				var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal);
				
				config.SaveAs(settingsFilePath);
			}
			catch (Exception ex)
			{
				throw new ApplicationException("Failed to export settings.", ex);
			}
		}
	}
}
