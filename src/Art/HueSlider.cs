using System;
using Microsoft.Xna.Framework;

namespace PixelArt {
    public class HueSlider : UISlider {

        public float lastVal;
        public Vector2 sv;
        
        public HueSlider(Vector2 pos, Vector2 dimen) :
            base(pos, dimen) {
            vertical = true;
            slideFunc = setHue;
            findSlideFunc = () => {
                float val = Main.colorWheel.BaseHSV.hue / 360;

                if (val == 0 && Math.Abs(1-lastVal) < 0.05F) return 1;
                
                return val;
            };
            
            genTexture();
        }

        public void genTexture() {
            int pix = (int) dimen.Y;

            var arr = new Color[pix];

            for (int i = 0; i < pix; i++) {
                arr[i] = new HSV(((float) i / (pix - 1)) * 360, 1, 1).toColor();
            }

            texture = Textures.toTexture(arr, 1, pix);
        }

        public override void clicked(MouseInfo mouse, KeyInfo keys, float deltaTime) {
            base.clicked(mouse, keys, deltaTime);
            HSV hsv = new HSV(Main.brushColor);
            sv = new Vector2(hsv.saturation, hsv.value);
            if (sv.Y == 0) {
                sv.X = Main.colorWheel.lastSaturation;
            }
        }

        public void setHue(float val) {
            Main.colorWheel.setHue(val * 360);
            Main.brushColor = new HSV(val * 360, sv.X, sv.Y).toColor();

            lastVal = val;
        }
    }
}