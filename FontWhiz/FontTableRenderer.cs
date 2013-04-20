using System;
using System.Drawing;
using System.Collections.Generic;

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
			AsciiSetSize = 255;
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
			using (Graphics graphics = Graphics.FromImage (bitmap)) {

				graphics.Clear (BackgroundColour); // <-- Changing Color.White works just fine!
				graphics.Flush ();

//				graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SingleBitPerPixel;
//				graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;
//				graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.Low;
//				graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighSpeed;

//				graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
//				graphics.CompositingMode  = System.Drawing.Drawing2D.CompositingMode.SourceOver;

				var path = new System.Drawing.Drawing2D.GraphicsPath ();
				SolidBrush brush = new SolidBrush (FontColour);


				int row = 0;
				int col = 0;
				foreach (char c in Chars) {
//					graphics.DrawString ("A", 
//					               Font,
//					               new SolidBrush (FontColour), 
//					               new PointF (row * CellSize, col * CellSize));
//
//					graphics.Flush ();

					path.AddString (char.ToString (c),
					               new FontFamily (Font.FontFamily.Name),
					               (int)Font.Style, graphics.DpiY * Font.Size / 72,					              
					               new PointF (row * CellSize, col * CellSize),
					               new StringFormat ());

					//path.AddString (
					//	new PointF (width - CellSize, height - CellSize));
					path.CloseFigure ();
					//graphics.Flush ();

					row++;
					if (row >= 16) {
						col++;
						row = 0;
					}
				}

				float width = graphics.VisibleClipBounds.Width - 1;
				float height = graphics.VisibleClipBounds.Height - 1;


				path.AddString ("A",
					               new FontFamily (Font.FontFamily.Name),
					               (int)Font.Style, graphics.DpiY * Font.Size / 72,					              
					               new PointF (width - 24, height - 24),
					               new StringFormat ());

				graphics.FillPath (brush, path);

				RenderGrid (graphics);

				graphics.Flush ();
			}
		}
	}
}

