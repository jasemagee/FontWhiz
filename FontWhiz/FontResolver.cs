using System;
using System.Drawing;
using System.Drawing.Text;

namespace FontWhiz
{
	public class FontResolver
	{
		public static Font FromFilename (string filename, int size)
		{
			FontFamily fontFamily = LoadFromFile (filename);
			return new Font (fontFamily, size);
		}

		public static Font FromName (string fontName)
		{
			Font font = null;
			var fontFamilyDesc = Pango.FontDescription.FromString (fontName);
			FontStyle style = FromPangoStyle (fontFamilyDesc.Style, fontFamilyDesc.Weight);

			/// Seriously, what the fuck?
			int notBullshitSize = (int)(fontFamilyDesc.Size / Pango.Scale.PangoScale);

			/// The chooser shows these options but they won't resolve automagically
			/// a installed font. Bit balls.
			if (fontFamilyDesc.Family == "Mono") {
				font = new Font (FontFamily.GenericMonospace, notBullshitSize, style);
			} else if (fontFamilyDesc.Family == "Sans") {
				font = new Font (FontFamily.GenericSansSerif, notBullshitSize, style);
			} else if (fontFamilyDesc.Family == "Serif") {
				font = new Font (FontFamily.GenericSerif, notBullshitSize, style);
			} else {
				InstalledFontCollection installedFonts = new InstalledFontCollection ();
				var test = new FontFamily (fontFamilyDesc.Family, installedFonts);
				font = new Font (test, fontFamilyDesc.Size);
			}


			return font;
		}

		// Pango Style sounds like some sort of dance
		private static FontStyle FromPangoStyle (Pango.Style pangoStyle, Pango.Weight pangoWeight)
		{
			FontStyle result = FontStyle.Regular;

			if (pangoWeight == Pango.Weight.Bold)
				result |= FontStyle.Bold;

			if (pangoStyle == Pango.Style.Italic)
				result |= FontStyle.Italic;

			return FontStyle.Regular;
		}

		private static FontFamily LoadFromFile (string filename)
		{
			var fontCollection = new PrivateFontCollection ();
			fontCollection.AddFontFile (filename);
			return fontCollection.Families [0];
		}
	}
}

