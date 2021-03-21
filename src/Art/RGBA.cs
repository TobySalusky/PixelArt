using Microsoft.Xna.Framework;

namespace PixelArt {
    public static class RGBA {

        public static int colorToRGBA(Color color) {
            
            int rgba = color.A | (color.B << 8) | (color.G << 16) | (color.R << 24);
            
            return rgba;
        }

        public static Color fromRGBA(int rgba) { 
            int r = (rgba >> 24) & 0x000000FF;
            int g = (rgba >> 16) & 0x000000FF;
            int b = (rgba >> 8) & 0x000000FF;
            int a = rgba & 0x000000FF;
            return new Color(r, g, b, a);
        }
    }
}