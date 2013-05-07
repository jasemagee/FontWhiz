using System;
using System.Drawing;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;

namespace FontWhiz
{
	public class FontRenderer
	{
		public FontRendererParams FontRendererParams {
			get;
			set;
		}

		public int AsciiSetSize {
			get;
			set;
		}

		public char[] Chars {
			get;
			set;
		}

		public float CellSize {
			get;
			private set;
		}

		public FontRenderer (FontRendererParams fontRendererParams)
		{
			FontRendererParams = fontRendererParams;

			AsciiSetSize = 256;

			Chars = new char[AsciiSetSize];
			for (int i = 0; i < AsciiSetSize; i++) {
				Chars [i] = (char)(i);

				/// IsControl chars are not representable (I think this is the right method?)
				/// so set them to empty
				if (char.IsControl (Chars [i]))
					Chars [i] = ' ';
			}
		}

		private void CalculateCellSize (DirectoryInfo directory)
		{
			var defaultArgs = GetGravSizeFontArgs ();

			foreach (char c in Chars) {
				// Only bother to make files for actual visible characters
				if (char.IsControl (c))
					continue;

				string outputChar = GetImageMagickSafeChar (c);

				string args = string.Format ("{0} label:'{1}' work/{2}.png", defaultArgs, outputChar, (int)c);	
				ProcessStartAndWaitForExit ("convert", args);
			}

			foreach (var file in directory.GetFiles()) {
				Image image = Image.FromFile (file.FullName);
				CellSize = Math.Max (CellSize, image.Width);
				CellSize = Math.Max (CellSize, image.Height);
				file.Delete ();
			}
		}

		public string Render ()
		{
			if (Directory.Exists ("work"))
				Directory.Delete ("work", true);

			DirectoryInfo outDirectory = Directory.CreateDirectory ("work");

			RenderFixed (outDirectory);
			return "work/output.png";
		}

		private void RenderFixed (DirectoryInfo outDirectory)
		{
			CalculateCellSize (outDirectory);

			string background = FontRendererParams.BackgroundColour.ToImageMagickRgb ();

			// TODO: Background would be more consistent but it doesn't want to work?
			ProcessStartAndWaitForExit ("convert", string.Format ("-size {0}x{0} xc:{1} work/blank.png", 
			                                                      CellSize, background)
			);

			var defaultArgs = GetAllArgs ();

			foreach (char c in Chars) {
				// Only bother to make files for actual visible characters
				if (char.IsControl (c))
					continue;

				string outputChar = GetImageMagickSafeChar (c);

				var args = string.Format ("{0} label:'{1}' work/{2}.png", defaultArgs, outputChar, (int)c);
				ProcessStartAndWaitForExit ("convert", args);
			}


			string[] filenames = new string[AsciiSetSize];
			for (int i = 0; i < AsciiSetSize; i++) {
				filenames [i] = "work/blank.png";
				FileInfo[] matches = outDirectory.GetFiles (string.Format ("{0}.png", i));
				if (matches.Length > 0)
					filenames [i] = matches [0].FullName;
			}

			var joinedFilenames = string.Join (" ", filenames);
			var montageArgs = string.Format (
				"{0} +set label '' -geometry +0+0 -background none -bordercolor none work/output.png",
				joinedFilenames, CellSize, background);

			ProcessStartAndWaitForExit ("montage", montageArgs);

		}

		private string GetGravSizeFontArgs ()
		{
			var defaultArgs = string.Format ("-gravity center -font {0} -pointsize {1}", 
			                                 FontRendererParams.Font, FontRendererParams.FontSize);

			if (!FontRendererParams.AntiAlias)
				defaultArgs += " +antialias";

			return defaultArgs;
		}

		private string GetAllArgs ()
		{
			string background = FontRendererParams.BackgroundColour.ToImageMagickRgb ();

			var defaultArgs = GetGravSizeFontArgs ();

			defaultArgs += string.Format (
				" -size {0}x{0} -background {1} -fill '{2}'",
				CellSize, background, FontRendererParams.FontColour.ToImageMagickRgb ());

			return defaultArgs;
		}

		private void ProcessStartAndWaitForExit (string filename, string args)
		{
			using (Process p = new Process ()) {
				p.StartInfo.FileName = filename;
				p.StartInfo.Arguments = args;
				p.StartInfo.UseShellExecute = false;
				p.Start ();
				p.WaitForExit ();
			}
		}

		private string GetImageMagickSafeChar (char c)
		{
			string outputChar = char.ToString (c);

			if (outputChar.Equals ("'"))
				outputChar += "\'";

			if (outputChar.Equals ("\\"))
				outputChar += "\\";

			return outputChar;

		}

		public static List<string> GetImageMagickFonts ()
		{
			var p = new Process ();
			p.StartInfo.UseShellExecute = false;
			p.StartInfo.RedirectStandardOutput = true;
			p.StartInfo.FileName = "convert";
			p.StartInfo.Arguments = "-list font";
			p.Start ();

			string output = p.StandardOutput.ReadToEnd ();
			p.WaitForExit ();

			const string IMAGE_MAGICK_FONT_DEF = "Font: ";

			string[] outputLines = output.Split ('\n');
			List<string> fonts = new List<string> ();
			foreach (string line in outputLines) {
				if (line.Contains (IMAGE_MAGICK_FONT_DEF)) {
					int fontStartIndex = line.LastIndexOf (IMAGE_MAGICK_FONT_DEF) + IMAGE_MAGICK_FONT_DEF.Length;
					string fontName = line.Substring (fontStartIndex);
					fonts.Add (fontName);
				}
			}

			return fonts;
		}

		public static bool IsImageMagickInstalled ()
		{
			var p = new Process ();
			p.StartInfo.UseShellExecute = false;
			p.StartInfo.RedirectStandardOutput = true;
			p.StartInfo.FileName = "convert";
			p.StartInfo.Arguments = "--version";
			p.Start ();

			string output = p.StandardOutput.ReadToEnd ();
			p.WaitForExit ();

			return output.ToLower ().Contains ("imagemagick");
		}

	}
}

