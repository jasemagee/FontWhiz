using System;
using Gtk;
using System.Drawing;
using System.IO;
using FontWhiz;
using System.Threading;

public partial class MainWindow: Gtk.Window
{	
	public MainWindow (): base (Gtk.WindowType.Toplevel)
	{
		Build ();

		/// GUI designer appears to hate me and keeps forgetting these
		EnterNotifyEvent += HandleEnterNotifyEvent;

		installedfontsradiobutton.Toggled += OnInputFontTypeGroupToggled;
		generatebutton.Released += OnGenerateButtonReleased;

		transparentbackgroundcheckbutton.Toggled += 
			(sender, e) => backgroundcolorbutton.Sensitive = !transparentbackgroundcheckbutton.Active;

		previewbutton.Clicked += 
			(sender, e) => PerformRefreshPreview ();

		this.KeyReleaseEvent += HandleKeyReleaseEvent;
	}

	private void PerformRefreshPreview ()
	{
		// I *think* this ok on a different thread.. only reading values? 
		string font = string.Empty;
		if (installedfontsradiobutton.Active) 
			font = installedfontscombobox.ActiveText;
		else 
			font = fontfilechooserbutton.Filename;

		if (string.IsNullOrEmpty (font)) 
			return;

		outputframe.Sensitive = false;
		this.GdkWindow.Cursor = new Gdk.Cursor (Gdk.CursorType.Watch);

		Color backgroundColour = backgroundcolorbutton.Color.ToDotNetColour ();

		if (transparentbackgroundcheckbutton.Active)
			backgroundColour = Color.Transparent;

		Thread thread = new Thread (new ParameterizedThreadStart (RefreshPreview));
		thread.Start (new FontRendererParams (font, 		                                        
		                                      sizespinbutton.ValueAsInt, 		                                        
		                                      fontcolorbutton.Color.ToDotNetColour (),                                        
		                                      backgroundColour,
		                                      antialiasingcheckbutton.Active,
		                                      true)
		);
	}

	private void RefreshPreview (object objectParam)
	{
		var refreshPreviewParams = objectParam as FontRendererParams;
		var tableRenderer = new FontRenderer (refreshPreviewParams);
		string preview = tableRenderer.Render ();

		// Do this on the main thread!
		Gtk.Application.Invoke (delegate {
			previewimage.File = preview;
			labelWidth.LabelProp = previewimage.Pixbuf.Width.ToString ();
			labelHeight.LabelProp = previewimage.Pixbuf.Height.ToString ();
			labelCellSize.LabelProp = tableRenderer.CellSize.ToString ();
			outputframe.Sensitive = true;
			this.GdkWindow.Cursor = new Gdk.Cursor (Gdk.CursorType.Arrow);
		}
		);
	}
	

	protected void OnGenerateButtonReleased (object sender, EventArgs e)
	{
		var fileChooserDialog = new FileChooserDialog (
			"Select a PNG", this,
			FileChooserAction.Save,
			 "Cancel", ResponseType.Cancel,
			"Save", ResponseType.Accept);

	
		var fileFilter = new FileFilter ();
		fileFilter.Name = "png";
		fileFilter.AddMimeType ("image/png");
		fileFilter.AddPattern ("*.png");

		fileChooserDialog.AddFilter (fileFilter);

		if (fileChooserDialog.Run () == (int)ResponseType.Accept) {
			string filename = fileChooserDialog.Filename;
			bool continueSave = true;

			if (System.IO.Path.GetExtension (filename) != ".png")
				filename = System.IO.Path.ChangeExtension (filename, "png");


			if (File.Exists (filename)) {
				var messageDialog = new MessageDialog (this,
				                                      DialogFlags.Modal,
				                                      MessageType.Question,
				                                      ButtonsType.OkCancel,
				                                      "{0} already exists. Overwrite?", 
				                                       filename);

				if (messageDialog.Run () != (int)ResponseType.Ok)
					continueSave = false;

				messageDialog.Destroy ();
			}

			if (continueSave)
				File.Copy (previewimage.File, filename, true);
		}

		fileChooserDialog.Destroy ();
	}

	void HandleEnterNotifyEvent (object o, EnterNotifyEventArgs args)
	{	
		bool installed = FontRenderer.IsImageMagickInstalled ();

		if (!installed) {
			var messageDialog = new MessageDialog (this, DialogFlags.Modal,
		                                       MessageType.Error,
		                                       ButtonsType.Close,
		                                       "You must have ImageMagick installed in " + 
				"order to use this software. See the FAQ at <a href=\"http://fontwhiz.com\">http://fontwhiz.com</a>"
			);

			messageDialog.Run ();
			messageDialog.Destroy ();
			Application.Quit ();
		} else {
			foreach (var font in FontRenderer.GetImageMagickFonts()) 
			installedfontscombobox.AppendText (font);
		
		installedfontscombobox.Active = 0;
		}
	}

	protected void OnInputFontTypeGroupToggled (object sender, EventArgs e)
	{
		if (installedfontsradiobutton.Active) {
			installedfontsframe.Sensitive = true;
			fromfileframe.Sensitive = false;
		} else {
			installedfontsframe.Sensitive = false;
			fromfileframe.Sensitive = true;
		}
	}

	void HandleKeyReleaseEvent (object o, KeyReleaseEventArgs args)
	{
		/// F knows if this is correct. Calling Destory() doesn't close the app 
		/// or call OnDeleteEvent
		if (args.Event.Key == Gdk.Key.Escape)
			Application.Quit ();
	}

	protected void OnDeleteEvent (object sender, DeleteEventArgs a)
	{
		Application.Quit ();
		a.RetVal = true;
	}

	//		using (MemoryStream memoryStream = new MemoryStream()) {
//			bitmap.Save (memoryStream, ImageFormat.Png);
//			memoryStream.Position = 0;
//			Gdk.Pixbuf pixbuf = new Gdk.Pixbuf (memoryStream);
//			previewimage.Pixbuf = pixbuf;
//		}
}
