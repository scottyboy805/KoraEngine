using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace KoraGame
{
    [Serializable]
    [StructLayout(LayoutKind.Explicit)]
    public struct Color32
    {
        public static readonly Color32 Clear = new Color32(0, 0, 0, 0);
        public static readonly Color32 White = new Color32(255, 255, 255, 255);
        public static readonly Color32 Black = new Color32(0, 0, 0, 0);
        public static readonly Color32 Red = new Color32(255, 0, 0, 255);
        public static readonly Color32 Green = new Color32(0, 255, 0, 255);
        public static readonly Color32 Blue = new Color32(0, 0, 255, 255);
        public static readonly Color32 Yellow = (Color32)Color.Yellow;
        public static readonly Color32 Cyan = (Color32)Color.Cyan;
        public static readonly Color32 Magenta = (Color32)Color.Magenta;

        #region XNAColors
        public static readonly Color32 AliceBlue = new Color32(0xf0f8ffff);
        public static readonly Color32 AntiqueWhite = new Color32(0xfaebd7ff);
        public static readonly Color32 Aqua = new Color32(0x00ffffff);
        public static readonly Color32 Aquamarine = new Color32(0x7fffd4ff);
        public static readonly Color32 Azure = new Color32(0xf0ffffff);
        public static readonly Color32 Beige = new Color32(0xf5f5dcff);
        public static readonly Color32 Bisque = new Color32(0xffe4c4ff);
        public static readonly Color32 BlanchedAlmond = new Color32(0xffebcdff);
        public static readonly Color32 BlueViolet = new Color32(0x8a2be2ff);
        public static readonly Color32 Brown = new Color32(0xa52a2aff);
        public static readonly Color32 BurlyWood = new Color32(0xdeb887ff);
        public static readonly Color32 CadetBlue = new Color32(0x5f9ea0ff);
        public static readonly Color32 Chartreuse = new Color32(0x7fff00);
        public static readonly Color32 Chocolate = new Color32(0xd2691eff);
        public static readonly Color32 Coral = new Color32(0xff7f50ff);
        public static readonly Color32 CornflowerBlue = new Color32(0x6495edff);
        public static readonly Color32 Cornsilk = new Color32(0xfff8dcff);
        public static readonly Color32 Crimson = new Color32(0xdc143cff);
        public static readonly Color32 DarkBlue = new Color32(0x00008bff);
        public static readonly Color32 DarkCyan = new Color32(0x008b8bff);
        public static readonly Color32 DarkGoldenrod = new Color32(0xb8860bff);
        public static readonly Color32 DarkGray = new Color32(0xa9a9a9ff);
        public static readonly Color32 DarkGreen = new Color32(0x006400ff);
        public static readonly Color32 DarkKhaki = new Color32(0xbdb76bff);
        public static readonly Color32 DarkMagenta = new Color32(0x8b008bff);
        public static readonly Color32 DarkOliveGreen = new Color32(0x556b2f);
        public static readonly Color32 DarkOrange = new Color32(0xff8c00ff);
        public static readonly Color32 DarkOrchid = new Color32(0x9932ccff);
        public static readonly Color32 DarkRed = new Color32(0x8b0000ff);
        public static readonly Color32 DarkSalmon = new Color32(0xe9967aff);
        public static readonly Color32 DarkSeaGreen = new Color32(0x8fbc8bff);
        public static readonly Color32 DarkSlateBlue = new Color32(0x483d8bff);
        public static readonly Color32 DarkSlateGray = new Color32(0x2f4f4fff);
        public static readonly Color32 DarkTurquoise = new Color32(0x00ced1ff);
        public static readonly Color32 DarkViolet = new Color32(0x9400d3ff);
        public static readonly Color32 DeepPink = new Color32(0xff1493ff);
        public static readonly Color32 DeepSkyBlue = new Color32(0x00bfffff);
        public static readonly Color32 DimGray = new Color32(0x696969ff);
        public static readonly Color32 DodgerBlue = new Color32(0x1e90ffff);
        public static readonly Color32 Firebrick = new Color32(0xb22222ff);
        public static readonly Color32 FloralWhite = new Color32(0xfffaf0ff);
        public static readonly Color32 ForestGreen = new Color32(0x228b22ff);
        public static readonly Color32 Fuchsia = new Color32(0xff00ffff);
        public static readonly Color32 Gainsboro = new Color32(0xdcdcdcff);
        public static readonly Color32 GhostWhite = new Color32(0xf8f8ffff);
        public static readonly Color32 Gold = new Color32(0xffd700ff);
        public static readonly Color32 Goldenrod = new Color32(0xdaa520ff);
        public static readonly Color32 Gray = new Color32(0x808080ff);
        public static readonly Color32 GreenYellow = new Color32(0xadff2fff);
        public static readonly Color32 Honeydew = new Color32(0xf0fff0ff);
        public static readonly Color32 HotPink = new Color32(0xff69b4ff);
        public static readonly Color32 IndianRed = new Color32(0xcd5c5cff);
        public static readonly Color32 Indigo = new Color32(0x4b0082ff);
        public static readonly Color32 Ivory = new Color32(0xfffff0ff);
        public static readonly Color32 Khaki = new Color32(0xf0e68cff);
        public static readonly Color32 Lavender = new Color32(0xe6e6faff);
        public static readonly Color32 LavenderBlush = new Color32(0xfff0f5ff);
        public static readonly Color32 LawnGreen = new Color32(0x7cfc00ff);
        public static readonly Color32 LemonChiffon = new Color32(0xfffacdff);
        public static readonly Color32 LightBlue = new Color32(0xadd8e6ff);
        public static readonly Color32 LightCoral = new Color32(0xf08080ff);
        public static readonly Color32 LightCyan = new Color32(0xe0ffffff);
        public static readonly Color32 LightGoldenrodYellow = new Color32(0xfafad2ff);
        public static readonly Color32 LightGreen = new Color32(0x90ee90ff);
        public static readonly Color32 LightGray = new Color32(0xd3d3d3ff);
        public static readonly Color32 LightPink = new Color32(0xffb6c1ff);
        public static readonly Color32 LightSalmon = new Color32(0xffa07aff);
        public static readonly Color32 LightSeaGreen = new Color32(0x20b2aaff);
        public static readonly Color32 LightSkyBlue = new Color32(0x87cefaff);
        public static readonly Color32 LightSlateGray = new Color32(0x778899ff);
        public static readonly Color32 LightSteelBlue = new Color32(0xb0c4deff);
        public static readonly Color32 LightYellow = new Color32(0xffffe0ff);
        public static readonly Color32 Lime = new Color32(0x00ff00ff);
        public static readonly Color32 LimeGreen = new Color32(0x32cd32ff);
        public static readonly Color32 Linen = new Color32(0xfaf0e6ff);
        public static readonly Color32 Maroon = new Color32(0x800000ff);
        public static readonly Color32 MediumAquamarine = new Color32(0x66cdaaff);
        public static readonly Color32 MediumBlue = new Color32(0x0000cdff);
        public static readonly Color32 MediumOrchid = new Color32(0xba55d3ff);
        public static readonly Color32 MediumPurple = new Color32(0x9370dbff);
        public static readonly Color32 MediumSeaGreen = new Color32(0x3cb371ff);
        public static readonly Color32 MediumSlateBlue = new Color32(0x7b68eeff);
        public static readonly Color32 MediumSpringGreen = new Color32(0x00fa9aff);
        public static readonly Color32 MediumTurquoise = new Color32(0x48d1ccff);
        public static readonly Color32 MediumVioletRed = new Color32(0xc71585ff);
        public static readonly Color32 MidnightBlue = new Color32(0x191970ff);
        public static readonly Color32 MintCream = new Color32(0xf5fffaff);
        public static readonly Color32 MistyRose = new Color32(0xffe4e1ff);
        public static readonly Color32 Moccasin = new Color32(0xffe4b5ff);
        public static readonly Color32 NavajoWhite = new Color32(0xffdeadff);
        public static readonly Color32 Navy = new Color32(0x000080ff);
        public static readonly Color32 OldLace = new Color32(0xfdf5e6ff);
        public static readonly Color32 Olive = new Color32(0x808000ff);
        public static readonly Color32 OliveDrab = new Color32(0x6b8e23ff);
        public static readonly Color32 Orange = new Color32(0xffa500ff);
        public static readonly Color32 OrangeRed = new Color32(0xff4500ff);
        public static readonly Color32 Orchid = new Color32(0xda70d6ff);
        public static readonly Color32 PaleGoldenrod = new Color32(0xeee8aaff);
        public static readonly Color32 PaleGreen = new Color32(0x98fb98ff);
        public static readonly Color32 PaleTurquoise = new Color32(0xafeeeeff);
        public static readonly Color32 PaleVioletRed = new Color32(0xdb7093ff);
        public static readonly Color32 PapayaWhip = new Color32(0xffefd5ff);
        public static readonly Color32 PeachPuff = new Color32(0xffdab9ff);
        public static readonly Color32 Peru = new Color32(0xcd853fff);
        public static readonly Color32 Pink = new Color32(0xffc0cbff);
        public static readonly Color32 Plum = new Color32(0xdda0ddff);
        public static readonly Color32 PowderBlue = new Color32(0xb0e0e6ff);
        public static readonly Color32 Purple = new Color32(0x800080ff);
        public static readonly Color32 RosyBrown = new Color32(0xbc8f8fff);
        public static readonly Color32 RoyalBlue = new Color32(0x4169e1ff);
        public static readonly Color32 SaddleBrown = new Color32(0x8b4513ff);
        public static readonly Color32 Salmon = new Color32(0xfa8072ff);
        public static readonly Color32 SandyBrown = new Color32(0xf4a460ff);
        public static readonly Color32 SeaGreen = new Color32(0x2e8b57ff);
        public static readonly Color32 SeaShell = new Color32(0xfff5eeff);
        public static readonly Color32 Sienna = new Color32(0xa0522dff);
        public static readonly Color32 Silver = new Color32(0xc0c0c0ff);
        public static readonly Color32 SkyBlue = new Color32(0x87ceebff);
        public static readonly Color32 SlateBlue = new Color32(0x6a5acdff);
        public static readonly Color32 SlateGray = new Color32(0x708090ff);
        public static readonly Color32 Snow = new Color32(0xfffafaff);
        public static readonly Color32 SpringGreen = new Color32(0x00ff7fff);
        public static readonly Color32 SteelBlue = new Color32(0x4682b4ff);
        public static readonly Color32 Tan = new Color32(0xd2b48cff);
        public static readonly Color32 Teal = new Color32(0x008080ff);
        public static readonly Color32 Thistle = new Color32(0xd8bfd8ff);
        public static readonly Color32 Tomato = new Color32(0xff6347ff);
        public static readonly Color32 Turquoise = new Color32(0x40e0d0ff);
        public static readonly Color32 Violet = new Color32(0xee82eeff);
        public static readonly Color32 Wheat = new Color32(0xf5deb3ff);
        public static readonly Color32 WhiteSmoke = new Color32(0xf5f5f5ff);
        public static readonly Color32 YellowGreen = new Color32(0x9acd32ff);
        #endregion

        /// <summary>
        /// The Color Value in a ABGR 32-bit unsigned integer
        /// </summary>
        [FieldOffset(0)]
        public uint RGBA;
        [DataMember]
        [FieldOffset(0)]
        public byte R;
        [DataMember]
        [FieldOffset(1)]
        public byte G;
        [DataMember]
        [FieldOffset(2)]
        public byte B;
        [DataMember]
        [FieldOffset(3)]
        public byte A;

        // Properties
        public float RFloat
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return R / 255f; }
        }

        public float GFloat
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return G / 255f; }
        }

        public float BFloat
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return B / 255f; }
        }

        public float AFloat
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return A / 255f; }
        }

        public string HexRGB
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return ToHexString("RGB"); }
        }

        public string HexRGBA
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return ToHexString("RGBA"); }
        }

        // Constructor
        public Color32(uint rgba)
        {
            this.R = 0;
            this.G = 0;
            this.B = 0;
            this.A = 0;
            this.RGBA = rgba;

            this.RGBA = (rgba & 0x000000FFU) << 24 | (rgba & 0x0000FF00U) << 8 |
                        (rgba & 0x00FF0000U) >> 8 | (rgba & 0xFF000000U) >> 24;
        }

        public Color32(byte r, byte g, byte b)
        {
            this.RGBA = 0;
            this.R = r;
            this.G = g;
            this.B = b;
            this.A = 255;
        }

        public Color32(byte r, byte g, byte b, byte a)
        {
            this.RGBA = 0;
            this.R = r;
            this.G = g;
            this.B = b;
            this.A = a;
        }

        public Color32(int r, int g, int b)
        {
            RGBA = 0;
            R = (byte)r;
            G = (byte)g;
            B = (byte)b;
            A = 255;
        }

        public Color32(int r, int g, int b, int a)
        {
            RGBA = 0;
            R = (byte)r;
            G = (byte)g;
            B = (byte)b;
            A = (byte)a;
        }

        // Methods
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FillArray(byte[] arr, int offset, int count = 4)
        {
            if (count > 0) arr[offset] = R;
            if (count > 1) arr[offset + 1] = G;
            if (count > 2) arr[offset + 2] = B;
            if (count > 3) arr[offset + 3] = A;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Color32 Lerp(in Color32 a, in Color32 b, float t)
        {
            t = Math.Clamp(t, 0, 1);
            Color32 result;
            result.RGBA = 0;
            result.R = (byte)((float)(int)a.R + (float)(b.R - a.R) * t);
            result.G = (byte)((float)(int)a.G + (float)(b.G - a.G) * t);
            result.B = (byte)((float)(int)a.B + (float)(b.B - a.B) * t);
            result.A = (byte)((float)(int)a.A + (float)(b.A - a.A) * t);
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Color32 LerpExtrap(in Color32 a, in Color32 b, float t)
        {
            Color32 result;
            result.RGBA = 0;
            result.R = (byte)((float)(int)a.R + (float)(b.R - a.R) * t);
            result.G = (byte)((float)(int)a.G + (float)(b.G - a.G) * t);
            result.B = (byte)((float)(int)a.B + (float)(b.B - a.B) * t);
            result.A = (byte)((float)(int)a.A + (float)(b.A - a.A) * t);
            return result;
        }

        public static Color32 FromArray(byte[] arr, int offset, int count = 4)
        {
            Color32 result = new Color32();
            if (count > 0) result.R = arr[offset];
            if (count > 1) result.G = arr[offset + 1];
            if (count > 2) result.B = arr[offset + 2];
            if (count > 3) result.A = arr[offset + 3];
            return result;
        }

        #region Object Overrides
        public override string ToString()
        {
            return string.Format("({0}, {1}, {2}, {3})", R, G, B, A);
        }

        public override bool Equals(object obj)
        {
            if (obj is Color32)
                return Equals((Color32)obj);

            return false;
        }

        public bool Equals(Color32 other)
        {
            return RGBA == other.RGBA;
        }

        public override int GetHashCode()
        {
            return R.GetHashCode() ^ (G.GetHashCode() << 2) ^ (B.GetHashCode() >> 2) ^ (A.GetHashCode() >> 1);
        }
        #endregion

        #region Operators
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Color32 operator +(in Color32 a, in Color32 b)
        {
            Color32 result;
            result.RGBA = 0;
            result.R = (byte)(a.R + b.R);
            result.G = (byte)(a.G + b.G);
            result.B = (byte)(a.B + b.B);
            result.A = (byte)(a.A + b.A);
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Color32 operator -(in Color32 a, in Color32 b)
        {
            Color32 result;
            result.RGBA = 0;
            result.R = (byte)(a.R - b.R);
            result.G = (byte)(a.G - b.G);
            result.B = (byte)(a.B - b.B);
            result.A = (byte)(a.A - b.A);
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Color32 operator *(in Color32 a, in Color32 b)
        {
            Color32 result;
            result.RGBA = 0;
            result.R = (byte)(a.R * b.R);
            result.G = (byte)(a.G * b.G);
            result.B = (byte)(a.B * b.B);
            result.A = (byte)(a.A * b.A);
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Color32 operator /(in Color32 a, in Color32 b)
        {
            Color32 result;
            result.RGBA = 0;
            result.R = (byte)(a.R / b.R);
            result.G = (byte)(a.G / b.G);
            result.B = (byte)(a.B / b.B);
            result.A = (byte)(a.A / b.A);
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Color32 a, Color32 b)
        {
            return a.RGBA == b.RGBA;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Color32 a, Color32 b)
        {
            return a.RGBA != b.RGBA;
        }
        #endregion

        #region Conversion
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator Color(in Color32 color)
        {
            Color result;
            result.R = (float)color.R / 255f;
            result.G = (float)color.G / 255f;
            result.B = (float)color.B / 255f;
            result.A = (float)color.A / 255f;
            return result;
        }
        #endregion

        #region HexString
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Color32 FromHexRGB(string value)
        {
            return FromHexString("RGB", value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Color32 FromHexRGBA(string value)
        {
            return FromHexString("RGBA", value);
        }

        private string ToHexString(string components)
        {
            const string HEX = "0123456789ABCDEF";
            Span<char> result = stackalloc char[components.Length * 2];

            for (int i = 0; i < components.Length; i++)
            {
                switch (components[i])
                {
                    case 'R':
                    case 'r':
                        result[i * 2 + 0] = HEX[(R & 0xf0) >> 4];
                        result[i * 2 + 1] = HEX[(R & 0x0f)];
                        break;
                    case 'G':
                    case 'g':
                        result[i * 2 + 0] = HEX[(G & 0xf0) >> 4];
                        result[i * 2 + 1] = HEX[(G & 0x0f)];
                        break;
                    case 'B':
                    case 'b':
                        result[i * 2 + 0] = HEX[(B & 0xf0) >> 4];
                        result[i * 2 + 1] = HEX[(B & 0x0f)];
                        break;
                    case 'A':
                    case 'a':
                        result[i * 2 + 0] = HEX[(A & 0xf0) >> 4];
                        result[i * 2 + 1] = HEX[(A & 0x0f)];
                        break;
                }
            }

            return new string(result);
        }

        private static Color32 FromHexString(string components, ReadOnlySpan<char> value)
        {
            // skip past useless string data (ex. if the string was 0xffffff or #ffffff)
            if (value.Length > 0 && value[0] == '#')
                value = value.Slice(1);
            if (value.Length > 1 && value[0] == '0' && (value[1] == 'x' || value[1] == 'X'))
                value = value.Slice(2);

            var color = Black;

            for (int i = 0; i < components.Length && i * 2 + 2 <= value.Length; i++)
            {
                switch (components[i])
                {
                    case 'R':
                    case 'r':
                        if (byte.TryParse(value.Slice(i * 2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var r))
                            color.R = r;
                        break;
                    case 'G':
                    case 'g':
                        if (byte.TryParse(value.Slice(i * 2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var g))
                            color.G = g;
                        break;
                    case 'B':
                    case 'b':
                        if (byte.TryParse(value.Slice(i * 2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var b))
                            color.B = b;
                        break;
                    case 'A':
                    case 'a':
                        if (byte.TryParse(value.Slice(i * 2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var a))
                            color.A = a;
                        break;
                }
            }

            return color;
        }
        #endregion
    }
}
