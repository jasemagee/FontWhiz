using System;
using System.Drawing;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;

namespace FontWhiz
{
	public class FontTableRenderer
	{
		public Font Font {
			get;
			private set;
		}

		public int AsciiSetSize {
			get;
			set;
		}

		public int ColumnCount {
			get;
			set;
		}

		public char[] Chars {
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

		public bool ShowGrid {
			get;
			set;
		}

		public float CellSize {
			get;
			private set;
		}

		public FontTableRenderer (Font font, Color fontColour, Color backgroundColour)
		{
			Font = font;
			FontColour = fontColour;
			BackgroundColour = backgroundColour;

			ShowGrid = true;
			AsciiSetSize = 256;
			ColumnCount = 16;

			Chars = new char[AsciiSetSize];
			for (int i = 0; i < AsciiSetSize; i++) {
				Chars [i] = (char)(i);

				/// IsControl chars are not representable (I think this is the right method?)
				/// so set them to empty
				if (char.IsControl (Chars [i]))
					Chars [i] = ' ';
			}

			using (Bitmap bitmap = new Bitmap(1,1)) {
				using (Graphics graphics =  Graphics.FromImage(bitmap)) {
					foreach (char c in Chars) {
						SizeF charSize = graphics.MeasureString (char.ToString (c), Font);
						CellSize = Math.Max (CellSize, charSize.Width);
						CellSize = Math.Max (CellSize, charSize.Height);
					}
				}
			}

		}

		private void RenderGrid (Graphics graphics)
		{
			if (!ShowGrid)
				return;

			Pen pen = new Pen (FontColour);

			float width = graphics.VisibleClipBounds.Width - 1;
			float height = graphics.VisibleClipBounds.Height - 1;

			PointF topLeft = new PointF (0, 0);
			PointF topRight = new PointF (width, 0);
			PointF bottomLeft = new PointF (0, height);
			PointF bottomRight = new PointF (width, height);

			//  __
			// |
			graphics.DrawLine (pen, topLeft, topRight);
			graphics.DrawLine (pen, topLeft, bottomLeft);
  
			// __|
			graphics.DrawLine (pen, bottomRight, bottomLeft);
			graphics.DrawLine (pen, bottomRight, topRight);

			for (int i = 0; i < ColumnCount; i++) {
				graphics.DrawLine (pen, 
				                   new PointF (i * CellSize, 0), 
				                   new PointF (i * CellSize, height));
			}

			int row = 0;
			foreach (char c in Chars) {
				graphics.DrawLine (pen,
				                   new PointF (0, row * CellSize),
				                   new PointF (width, row * CellSize));

				row++;
				if (row >= 16)
					row = 0;

			}
		}

		public void Render (Bitmap bitmap)
		{
			if (Directory.Exists ("work"))
				Directory.Delete ("work", true);

			DirectoryInfo outDirectory = Directory.CreateDirectory ("work");

			BackgroundColour = Color.AliceBlue;
			string background = BackgroundColour == Color.Transparent ? "transparent" : '\'' + BackgroundColour.ToImageMagickRgb () + '\'';

			// TODO: Background would be more consistent but it doesn't want to work?
			Process.Start ("convert", string.Format ("-size {0}x{0} xc:{1} work/blank.png", CellSize, background));

			string defaultArgs = string.Format ("-gravity center +antialias -size {0}x{0} -background {1} -fill '{2}' -font Helvetica -pointsize {3}",
			                                    CellSize, background, FontColour.ToImageMagickRgb (), Font.Size);

			foreach (char c in Chars) {
				// Only bother to make files for actual visible characters
				if (char.IsControl (c))
					continue;

				string outputChar = char.ToString (c);

				if (outputChar.Equals ("'"))
					outputChar += "\'";

				if (outputChar.Equals ("\\"))
					outputChar += "\\";

			
				string args = string.Format ("label:'{0}' work/{1}.png", outputChar, (int)c);
				Process.Start ("convert", defaultArgs + " " + args);
			}

			//FileInfo[] files = outDirectory.GetFiles ();
			string[] filenames = new string[AsciiSetSize];
			for (int i = 0; i < AsciiSetSize; i++) {
				filenames [i] = "work/blank.png";
				FileInfo[] matches = outDirectory.GetFiles (string.Format ("{0}.png", i));
				if (matches.Length > 0)
					filenames [i] = "work/" + matches [0].Name;
			}

			string joinedFilenames = string.Join (" ", filenames);
			string montageArgs = string.Format ("{0} +set label '' -geometry {1}x{1}+0+0 work/output.png",
			                                    joinedFilenames, CellSize);

			Process.Start ("montage", montageArgs);

//
//				RenderGrid (graphics);
//
//				graphics.Flush ();
//			}
		}
	}
}

