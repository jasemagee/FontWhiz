using System;
using Gtk;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Collections.Generic;
using FontWhiz;
using System.Threading;

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
			(sender, e) => PerformRefreshPreview ();

		foreach (var font in FontTableRenderer.GetImageMagickFonts()) 
			installedfontscombobox.AppendText (font);

		installedfontscombobox.Active = 0;
	}

	private class RefreshPreviewParams
	{
		public string Font {
			get;
			set;
		}

		public int FontSize {
			get;
			set;
		}

		public Color FontColour {
			get;
			set;
		}

		public Color BackgroundColour {
			get;
			set;
		}

		public bool AntiAlias {
			get;
			set;
		}

		public RefreshPreviewParams (string font, int fontSize, Color fontColour, Color backgroundColour, bool antiAlias)
		{
			Font = font;
			FontSize = fontSize;
			FontColour = fontColour;
			BackgroundColour = backgroundColour;
			AntiAlias = antiAlias;
			
		}
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

		previewbutton.Sensitive = false;

		Color backgroundColour = backgroundcolorbutton.Color.ToDotNetColour ();

		if (transparentbackgroundcheckbutton.Active)
			backgroundColour = Color.Transparent;

		Thread thread = new Thread (new ParameterizedThreadStart (RefreshPreview));
		thread.Start (new RefreshPreviewParams (font, 
		                                        sizespinbutton.ValueAsInt, 
		                                        fontcolorbutton.Color.ToDotNetColour (), 
		                                        backgroundColour,
		                                        antialiasingcheckbutton.Active)
		);
	}

	private void RefreshPreview (object objectParam)
	{
		var refreshPreviewParams = objectParam as RefreshPreviewParams;

		this.GdkWindow.Cursor = new Gdk.Cursor (Gdk.CursorType.Watch);
		try {
			var tableRenderer = new FontTableRenderer (refreshPreviewParams.Font, 
			                                           refreshPreviewParams.FontSize,
			                                           refreshPreviewParams.AntiAlias, 
			                                           refreshPreviewParams.FontColour, 
			                                           refreshPreviewParams.BackgroundColour);

			string preview = tableRenderer.Render ();

			// Do this on the main thread!
			Gtk.Application.Invoke (delegate {
				previewimage.File = preview;
				previewbutton.Sensitive = true;
			}
			);

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
		var fileChooserDialog = new FileChooserDialog (
			"Select a File", this,
			FileChooserAction.Save,
			 "Cancel", ResponseType.Cancel,
			"Save", ResponseType.Accept);

		//var fileFilter = new FileFilter ();
		//fileFilter.Name = "png";
		//fileFilter.AddMimeType ("image/png");
		//fileChooserDialog.AddFilter (fileFilter);


		// How do we do the extension?
		if (fileChooserDialog.Run () == (int)ResponseType.Accept) {
			File.Copy (previewimage.File, fileChooserDialog.Filename);
		}

		fileChooserDialog.Destroy ();
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

	//		using (MemoryStream memoryStream = new MemoryStream()) {
//			bitmap.Save (memoryStream, ImageFormat.Png);
//			memoryStream.Position = 0;
//			Gdk.Pixbuf pixbuf = new Gdk.Pixbuf (memoryStream);
//			previewimage.Pixbuf = pixbuf;
//		}

}
