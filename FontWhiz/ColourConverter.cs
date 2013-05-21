using System.Drawing;

namespace FontWhiz
{
	public static class ColourConverter
	{
		public static Color ToDotNetColour (ushort r, ushort g, ushort b)
		{
			return Color.FromArgb (255,
			                       r / 257,
			                       g / 257,
			                       b / 257);

		}

		public static Color ToDotNetColour (this Gdk.Color gdkColour)
		{
			return ToDotNetColour (gdkColour.Red,
			                      gdkColour.Green,
			                      gdkColour.Blue);
		}

		public static string ToImageMagickRgb (this Color colour)
		{
			if (colour == Color.Transparent)
				return "transparent";
			else 
				return string.Format ("rgb({0},{1},{2})",
			                     colour.R,
			                     colour.G,
			                     colour.B);
		}
	}
}

