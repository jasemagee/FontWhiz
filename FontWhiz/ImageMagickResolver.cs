using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization;

namespace FontWhiz
{
	public class ImageMagickResolver
	{
		private static UserSettings mResolvedSettings = null;
		public static UserSettings ResolvedSettings {
			get {
				if (!Resolved ())
					throw new InvalidProgramException ("Settings must be resolved prior to use");

				return mResolvedSettings;	
			}
		}
		public static bool Resolved ()
		{
			UserSettings defaultUserSettings = GetDefaultUserSettingsForPlatform ();
			UserSettings userSettings = UserSettings.Load (defaultUserSettings);

			bool valid =  IsUserSettingsValid (userSettings);

			if (valid)
				mResolvedSettings = userSettings;
			else
				File.Delete (UserSettings.AppUserSettingsFile);
			return valid;
		}

		private static UserSettings GetDefaultUserSettingsForPlatform ()
		{
			string expectedLocation = string.Empty;
			string expectedConvertName = string.Empty;
			string expectedMontageName = string.Empty;

			int p = (int)Environment.OSVersion.Platform;
			if ((p == 4) || (p == 6) || (p == 128)) {
				expectedLocation = "/usr/bin/";
				expectedConvertName = "convert";
				expectedMontageName = "montage";
			} else {
				string programFiles = Environment.GetFolderPath (Environment.SpecialFolder.ProgramFilesX86);
				string imageMagickFolder = "C:\\Program Files (x86)\\ImageMagick\\";

				foreach (var directory in Directory.GetDirectories(programFiles)) {
					if (directory.Contains ("ImageMagick"))
						imageMagickFolder = directory;
				}

				expectedLocation =imageMagickFolder;
				expectedConvertName = "convert.exe";
				expectedMontageName = "montage.exe";
			}


			return new UserSettings (
				Path.Combine (expectedLocation, expectedConvertName),
				Path.Combine (expectedLocation, expectedMontageName)
			);
		}


		private static bool IsUserSettingsValid (UserSettings userSettings)
		{
			if (!File.Exists (userSettings.ConvertLocation))
				return false;

			if (!IsVersionInfoCorrect (userSettings.ConvertLocation))
				return false;

			if (!File.Exists (userSettings.MontageLocation))
				return false;

			if (!IsVersionInfoCorrect (userSettings.MontageLocation))
				return false;

			return true;

		}

		private static bool IsVersionInfoCorrect (string filename)
		{
			string output = string.Empty;
			using (Process p = new Process ()) {
				p.StartInfo.UseShellExecute = false;
				p.StartInfo.RedirectStandardOutput = true;
				p.StartInfo.RedirectStandardError = true;
				p.StartInfo.CreateNoWindow = true;
				p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
				p.StartInfo.FileName = filename;
				p.StartInfo.Arguments = "--version";
				p.Start ();

				output = p.StandardOutput.ReadToEnd ();
				p.WaitForExit ();		
			}

			return output.ToLower ().Contains ("imagemagick");
		}
	}
}

