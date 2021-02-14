using System;
using Microsoft.Xna.Framework;

namespace PixelArt {
    public struct HSV {
        public float hue;        // 0 - 360
        public float saturation; // 0 - 1
        public float value;      // 0 - 1

        public HSV(Color color) {
            float r = color.R / 255F;
            float g = color.G / 255F;
            float b = color.B / 255F;
            
            float cMax = Math.Max(Math.Max(r, g), b);
            float cMin = Math.Min(Math.Min(r, g), b);
            float delta = cMax - cMin;

            // HUE
            if (delta == 0) hue = 0;
            else if (cMax == r) hue = 60 * Util.modulus((g - b) / delta, 6);
            else if (cMax == g) hue = 60 * (((b - r) / delta) + 2);
            else hue = 60 * (((r - g) / delta) + 4);

            // SATURATION
            if (cMax == 0) saturation = 0;
            else saturation = delta / cMax;

            // VALUE
            value = cMax;
        }

        public HSV(float hue, float saturation, float value) {
            this.hue = hue;
            this.saturation = saturation;
            this.value = value;
        }

        public Color toColor() {
            float c = saturation * value;
            float x = c * (1 - Math.Abs((hue / 60) % 2 - 1));
            float m = value - c;

            Vector3 rgb;
            if (hue >= 0 && hue < 60) rgb = new Vector3(c, x, 0);
            else if (hue >= 60 && hue < 120) rgb = new Vector3(x,c,0);
            else if (hue >= 120 && hue < 180) rgb = new Vector3(0,c,x);
            else if (hue >= 180 && hue < 240) rgb = new Vector3(0,x,c);
            else if (hue >= 240 && hue < 300) rgb = new Vector3(x,0,c);
            else rgb = new Vector3(c,0,x);

            var (r, g, b) = rgb;

            return new Color((int) ((r + m) * 255), (int) ((g + m) * 255), (int) ((b + m) * 255));
        }
    }
}