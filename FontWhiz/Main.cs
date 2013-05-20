using Gtk;

namespace FontWhiz
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			Application.Init ();

			if (!ImageMagickResolver.Resolved ()) {
				var builder = new System.Text.StringBuilder ("Hi there,");
				builder.AppendLine ();
				builder.AppendLine ("I was unable to find ImageMagick on your system which means...");
				builder.AppendLine ("a) It is not installed");
				builder.AppendLine ("b) It is in a directory I haven't checked");
				builder.AppendLine ("");
				builder.AppendLine ("If 'a' please exit the application and install ImageMagick");
				builder.AppendLine ("If 'b' then please edit the file '{0}' and insert the correct location for convert and montage");
				builder.AppendLine ();
				builder.AppendLine ("Please visit the FAQ <a href=\"http://fontwhiz.com\">http://fontwhiz.com</a> if you are unclear.");


				var messageDialog = new MessageDialog (null, 
				                                       DialogFlags.Modal, 
				                                       MessageType.Error, 
				                                       ButtonsType.Close, 
				                                       builder.ToString (), UserSettings.AppUserSettingsFile);

				messageDialog.WindowPosition = WindowPosition.Center;

				messageDialog.Run ();
				messageDialog.Destroy ();
				return;
			}

			MainWindow win = new MainWindow ();
			win.Show ();
			Application.Run ();
		}
	}
}
