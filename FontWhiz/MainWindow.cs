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
		fontbutton.FontSet += InstalledFontSet;
		fontfilechooserbutton.SelectionChanged += FromFileSelectionChanged;
		installedfontsradiobutton.Toggled += OnInputFontTypeGroupToggled;
		generatebutton.Released += OnGenerateButtonReleased;

		RefreshPreview ();
	}

	private void RefreshPreview ()
	{
		Font font = null;
		if (installedfontsradiobutton.Active) 
			font = FontResolver.FromName (fontbutton.FontName);
		else 
			font = FontResolver.FromFilename (fontfilechooserbutton.Filename, (int)sizespinbutton.Value);

		if (font == null)
			return;

		Color backgroundColour = backgroundcolorbutton.Color.ToDotNetColour ();

		if (transparentbackgroundcheckbutton.Active)
			backgroundColour = Color.Transparent;

		Color fontColour = fontcolorbutton.Color.ToDotNetColour ();


		var tableRenderer = new FontTableRenderer (font, fontColour, backgroundColour);

		var bitmap = new Bitmap ((int)(tableRenderer.CellSize * tableRenderer.ColumnCount), 
		                         (int)(tableRenderer.CellSize * tableRenderer.ColumnCount));


		tableRenderer.Render (bitmap);

		using (MemoryStream memoryStream = new MemoryStream()) {
			bitmap.Save (memoryStream, ImageFormat.Png);
			memoryStream.Position = 0;
			Gdk.Pixbuf pixbuf = new Gdk.Pixbuf (memoryStream);
			previewimage.Pixbuf = pixbuf;
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

		Font font = null;
		if (installedfontsradiobutton.Active) 
			font = FontResolver.FromName (fontbutton.FontName);
		else 
			font = FontResolver.FromFilename (fontfilechooserbutton.Filename, (int)sizespinbutton.Value);

		if (font == null)
			return;

		Color backgroundColour = backgroundcolorbutton.Color.ToDotNetColour ();

		if (transparentbackgroundcheckbutton.Active)
			backgroundColour = Color.Transparent;

		Color fontColour = fontcolorbutton.Color.ToDotNetColour ();


		var tableRenderer = new FontTableRenderer (font, fontColour, backgroundColour);

		var bitmap = new Bitmap ((int)(tableRenderer.CellSize * tableRenderer.ColumnCount), 
		                         (int)(tableRenderer.CellSize * tableRenderer.ColumnCount),
		                         PixelFormat.Format16bppRgb555);


		tableRenderer.Render (bitmap);

		bitmap.Save (outputfilechooserbutton.Filename, ImageFormat.Png);
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
	protected void InstalledFontSet (object sender, EventArgs e)
	{
		RefreshPreview ();
	}
	protected void FromFileSelectionChanged (object sender, EventArgs e)
	{
		RefreshPreview ();
	}
	protected void TransparentBackgroundToggled (object sender, EventArgs e)
	{
		backgroundcolorbutton.Sensitive = !transparentbackgroundcheckbutton.Active;
	}

}
