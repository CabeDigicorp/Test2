using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace CommonResources
{
    public class ColorsHelper
    {

        static Dictionary<string, ColorInfo> _coloriMacchina = null;

        static ColorsHelper()
        {
            _coloriMacchina = new Dictionary<string, ColorInfo>();
            ColorInfo colInfo = null;
            if (_coloriMacchina.TryGetValue(MyColorsEnum.Transparent.ToString(), out colInfo))
            {
                _coloriMacchina.Add(colInfo.Name, colInfo);
            }
            _coloriMacchina = ColorInfo.ColoriInstallatiInMacchina.ToDictionary(item => item.Name, item => item);
        }

        public static ColorInfo GetColorInfoByName(string colorName)
        {
            ColorInfo colInfo = null;
            _coloriMacchina.TryGetValue(colorName, out colInfo);
            return colInfo;
        }

        public static List<string> OrderedColorsName
        {
            get => new List<string>()
            {
                "Transparent",
                "Pink",
                "LightPink",
                "HotPink",
                "DeepPink",
                "PaleVioletRed",
                "MediumVioletRed",
                "LightSalmon",
                "Salmon",
                "DarkSalmon",
                "LightCoral",
                "IndianRed",
                "Crimson",
                "FireBrick",
                "DarkRed",
                "Red",
                "OrangeRed",
                "Tomato",
                "Coral",
                "DarkOrange",
                "Orange",
                "Yellow",
                "LightYellow",
                "LemonChiffon",
                "LightGoldenrodYellow",
                "PapayaWhip",
                "Moccasin",
                "PeachPuff",
                "PaleGoldenrod",
                "Khaki",
                "DarkKhaki",
                "Gold",
                "Cornsilk",
                "BlanchedAlmond",
                "Bisque",
                "NavajoWhite",
                "Wheat",
                "BurlyWood",
                "Tan",
                "RosyBrown",
                "SandyBrown",
                "Goldenrod",
                "DarkGoldenrod",
                "Peru",
                "Chocolate",
                "SaddleBrown",
                "Sienna",
                "Brown",
                "Maroon",
                "DarkOliveGreen",
                "Olive",
                "OliveDrab",
                "YellowGreen",
                "LimeGreen",
                "Lime",
                "LawnGreen",
                "Chartreuse",
                "GreenYellow",
                "SpringGreen",
                "MediumSpringGreen",
                "LightGreen",
                "PaleGreen",
                "DarkSeaGreen",
                "MediumAquamarine",
                "MediumSeaGreen",
                "SeaGreen",
                "ForestGreen",
                "Green",
                "DarkGreen",
                "Aqua",
                "Cyan",
                "LightCyan",
                "PaleTurquoise",
                "Aquamarine",
                "Turquoise",
                "MediumTurquoise",
                "DarkTurquoise",
                "LightSeaGreen",
                "CadetBlue",
                "DarkCyan",
                "Teal",
                "LightSteelBlue",
                "PowderBlue",
                "LightBlue",
                "SkyBlue",
                "LightSkyBlue",
                "DeepSkyBlue",
                "DodgerBlue",
                "CornflowerBlue",
                "SteelBlue",
                "RoyalBlue",
                "Blue",
                "MediumBlue",
                "DarkBlue",
                "Navy",
                "MidnightBlue",
                "Lavender",
                "Thistle",
                "Plum",
                "Violet",
                "Orchid",
                "Fuchsia",
                "Magenta",
                "MediumOrchid",
                "MediumPurple",
                "BlueViolet",
                "DarkViolet",
                "DarkOrchid",
                "DarkMagenta",
                "Purple",
                "Indigo",
                "DarkSlateBlue",
                "SlateBlue",
                "MediumSlateBlue",
                "White",
                "Snow",
                "Honeydew",
                "MintCream",
                "Azure",
                "AliceBlue",
                "GhostWhite",
                "WhiteSmoke",
                "Seashell",
                "Beige",
                "OldLace",
                "FloralWhite",
                "Ivory",
                "AntiqueWhite",
                "Linen",
                "LavenderBlush",
                "MistyRose",
                "Gainsboro",
                "LightGray",
                "Silver",
                "DarkGray",
                "Gray",
                "DimGray",
                "LightSlateGray",
                "SlateGray",
                "DarkSlateGray",
                "Black",

            };
        }

        /// <summary>
        /// Colori molto diversi fra loro
        /// </summary>
        public static List<string> DissimilarColors
        {
            get => new List<string>()
            {
                "OliveDrab",
                "SkyBlue",
                "Gold",
                "DarkGray",
                "Orange",
                "SteelBlue",
                "DarkSlateBlue",
                "LightSlateGray",
                "DarkSlateGray",
                "Lavender",
                "MediumAquamarine",
                "CadetBlue",
                "Moccasin",
                "Crimson",
                "Salmon",
                "DarkRed",
                "Chocolate",
                "Teal",
                "BlueViolet",
                "DeepPink",
            };
        }

        public static System.Windows.Media.SolidColorBrush Blend(System.Windows.Media.SolidColorBrush color, System.Windows.Media.SolidColorBrush backColor, double amount = 0.5)
        {
            byte a = (byte)((color.Color.A * amount) + backColor.Color.A * (1 - amount));
            byte r = (byte)((color.Color.R * amount) + backColor.Color.R * (1 - amount));
            byte g = (byte)((color.Color.G * amount) + backColor.Color.G * (1 - amount));
            byte b = (byte)((color.Color.B * amount) + backColor.Color.B * (1 - amount));
            return new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(a, r, g, b));
        }

        public static SolidColorBrush Convert(MyColorsEnum myColEnum)
        {
            switch (myColEnum)
            {
                case MyColorsEnum.EntitySelectionColor:// "LightGray":
                    return Application.Current.Resources["EntitySelectionColor"] as SolidColorBrush;
                case MyColorsEnum.AlertColor:
                    return Application.Current.Resources["AlertColor"] as SolidColorBrush;
                case MyColorsEnum.ErrorColor:
                    return Application.Current.Resources["ErrorColor"] as SolidColorBrush;
                case MyColorsEnum.DisabledColor:
                    return Application.Current.Resources["DisabledColor"] as SolidColorBrush;
                case MyColorsEnum.DesktopBrush:
                    return SystemColors.DesktopBrush;
                case MyColorsEnum.HighlightBrush:
                    return SystemColors.HighlightBrush;
                case MyColorsEnum.GradientActiveCaptionBrush:
                    return SystemColors.GradientActiveCaptionBrush;
                case MyColorsEnum.ActiveCaptionBrush:
                    return SystemColors.ActiveCaptionBrush;
                case MyColorsEnum.Transparent:
                    return new SolidColorBrush(Colors.Transparent);
                default:
                    return new SolidColorBrush(Colors.Transparent);

            }
        }

        public static SolidColorBrush Convert(string colorName)
        {
            if (colorName != null)
            {
                ColorInfo colorInfo = null;
                if (_coloriMacchina.TryGetValue(colorName, out colorInfo))
                {
                    return colorInfo.SampleBrush;
                }
            }

            return new SolidColorBrush(Colors.Transparent);
        }
    }


    public class ColorInfo
    {
        public string Name { get; set; }
        public Color Color { get; set; }
        public static List<ColorInfo> ColoriInstallatiInMacchina
        {
            get
            {
                return (List<ColorInfo>)(from System.Reflection.PropertyInfo property in typeof(Colors).GetProperties()
                                         select new ColorInfo(property.Name, (Color)property.GetValue(null, null))).OrderBy(f => f.HexValue).ToList();
            }
        }

        public SolidColorBrush SampleBrush
        {
            get { return new SolidColorBrush(Color); }
        }
        public string HexValue
        {
            get { return Color.ToString(); }
        }

        public ColorInfo(string color_name, Color color)
        {
            Name = color_name;
            Color = color;
        }
    }


    public enum MyColorsEnum
    {
        ActiveCaptionBrush = 0,
        DesktopBrush,
        GradientActiveCaptionBrush,
        HighlightBrush,
        EntitySelectionColor,
        AlertColor,
        ErrorColor,
        Transparent,
        DisabledColor,
    }
}
