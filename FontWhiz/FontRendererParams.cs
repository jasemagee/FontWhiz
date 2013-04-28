using System.Drawing;

namespace FontWhiz
{
	public class FontRendererParams
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

		public bool Pack {
			get;
			set;
		}

		public FontRendererParams (string font, int fontSize, Color fontColour, Color backgroundColour, bool antiAlias)
		{
			Font = font;
			FontSize = fontSize;
			FontColour = fontColour;
			BackgroundColour = backgroundColour;
			AntiAlias = antiAlias;
			Pack = false;
			
		}
	}
}

