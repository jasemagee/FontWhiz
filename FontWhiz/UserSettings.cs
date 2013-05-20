using System;
using System.Runtime.Serialization;
using System.IO;

namespace FontWhiz
{
	[DataContract]
	public class UserSettings
	{
		[DataMember]
		public string ConvertLocation {
			get;
			set;
		}
		[DataMember]
		public string MontageLocation {
			get;
			set;
		}

		public UserSettings (string convertLocation, string montageLocation)
		{
			ConvertLocation = convertLocation;
			MontageLocation = montageLocation;
		}

		private static string AppUserSettingsFile {
			get {
				return Path.Combine (AppDataFolder, Path.ChangeExtension (AppName, "xml"));
			}
		}

		private static string AppDataFolder {
			get {
				return Path.Combine (GlobalAppDataFolder, AppName);
			}
		}

		private static string GlobalAppDataFolder {
			get {
				return Environment.GetFolderPath (Environment.SpecialFolder.ApplicationData);
			}
		}

		private static string AppName {
			get {
				return System.Reflection.Assembly.GetExecutingAssembly ().GetName ().Name;
			}
		}


		public static void Write (string filename, UserSettings userSettings)
		{
			DataContractSerializer s = new DataContractSerializer (typeof(UserSettings));
			using (FileStream fs = File.Open(filename, FileMode.Create)) {
				s.WriteObject (fs, userSettings);
			}
		}
	
		
		public static UserSettings Load (UserSettings defaultUserSettings)
		{
			if (!Directory.Exists (AppDataFolder))
				Directory.CreateDirectory (AppDataFolder);

			if (!File.Exists (AppUserSettingsFile)) {
				UserSettings.Write (AppUserSettingsFile, defaultUserSettings);
			}

			DataContractSerializer s = new DataContractSerializer (typeof(UserSettings));

			using (FileStream fs = File.Open(AppUserSettingsFile, FileMode.Open)) {
				object s2 = s.ReadObject (fs);
				if (s2 == null)
					return null;
				else
					return (UserSettings)s2;
			}
		}
	}
}

