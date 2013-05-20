using Gtk;

namespace FontWhiz
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			Application.Init ();

			if (!ImageMagickResolver.Resolve ()) {
				var messageDialog = new MessageDialog (null, DialogFlags.Modal, 
				                                       MessageType.Error, ButtonsType.Close,
				                                      "You must have ImageMagick installed in " + 
					"order to use this software. See the FAQ " +
					"at <a href=\"http://fontwhiz.com\">http://fontwhiz.com</a>"
				);

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
