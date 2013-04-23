using System;
using Gtk;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Collections.Generic;
using FontWhiz;

public partial class MainWindow: Gtk.Window
{	
	public MainWindow (): base (Gtk.WindowType.Toplevel)
	{
		Build ();

		/// GUI designer appears to hate me and keeps forgetting these
		installedfontsradiobutton.Toggled += OnInputFontTypeGroupToggled;
		generatebutton.Released += OnGenerateButtonReleased;

		transparentbackgroundcheckbutton.Toggled += 
			(sender, e) => backgroundcolorbutton.Sensitive = !transparentbackgroundcheckbutton.Active;

		previewbutton.Clicked += 
			(sender, e) => RefreshPreview ();

		foreach (var font in FontTableRenderer.GetImageMagickFonts()) 
			installedfontscombobox.AppendText (font);

		installedfontscombobox.Active = 0;
	}

	private void RefreshPreview ()
	{
		this.GdkWindow.Cursor = new Gdk.Cursor (Gdk.CursorType.Watch);
		try {
	
			string font = string.Empty;
			if (installedfontsradiobutton.Active) 
				font = installedfontscombobox.ActiveText;
			else 
				font = fontfilechooserbutton.Filename;

			if (string.IsNullOrEmpty (font))
				return;

			Color backgroundColour = backgroundcolorbutton.Color.ToDotNetColour ();

			if (transparentbackgroundcheckbutton.Active)
				backgroundColour = Color.Transparent;

			Color fontColour = fontcolorbutton.Color.ToDotNetColour ();


			var tableRenderer = new FontTableRenderer (font, sizespinbutton.ValueAsInt, antialiasingcheckbutton.Active, fontColour, backgroundColour);


			string preview = tableRenderer.Render ();

//		using (MemoryStream memoryStream = new MemoryStream()) {
//			bitmap.Save (memoryStream, ImageFormat.Png);
//			memoryStream.Position = 0;
//			Gdk.Pixbuf pixbuf = new Gdk.Pixbuf (memoryStream);
//			previewimage.Pixbuf = pixbuf;
//		}

			previewimage.File = preview;
		} finally {
			this.GdkWindow.Cursor = new Gdk.Cursor (Gdk.CursorType.Arrow);
		}
	}
	
	protected void OnDeleteEvent (object sender, DeleteEventArgs a)
	{
		Application.Quit ();
		a.RetVal = true;
	}
	protected void OnGenerateButtonReleased (object sender, EventArgs e)
	{
		if (string.IsNullOrEmpty (outputfilechooserbutton.Filename))
			return;
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
}
