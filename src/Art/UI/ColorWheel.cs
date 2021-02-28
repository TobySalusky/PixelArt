using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PixelArt {
        public class ColorWheel : UIButton {

        public bool held;
        public float lastSaturation;
        private Color baseColor;
        private HSV baseHSV;

        public HSV BaseHSV {
            get => baseHSV;
            set {
                baseHSV = value;
                baseColor = baseHSV.toColor();
                genTexture();
            }
        }

        public ColorWheel(Vector2 pos, Vector2 dimen) : base(null, pos, dimen, null) {
            genTexture();
            hoverGrow = false;
            
            setHue(0);
        }

        public override void render(SpriteBatch spriteBatch) {
            base.render(spriteBatch);
            
            Vector2 rectPos;
            if (held) {
                Vector2 amounts = xyAmountClamped(Main.lastMousePos());
                rectPos = xyAmountToScreen(amounts);
                lastSaturation = amounts.X;
            }
            else {
                HSV hsv = new HSV(Main.brushColor);
                if (hsv.hue != baseHSV.hue && hsv.saturation != 0 && hsv.value != 0) setHue(hsv.hue);
                
                Vector2 amounts = new Vector2(hsv.saturation, 1 - hsv.value);
                
                if (hsv.value == 0) amounts.X = lastSaturation;
                
                rectPos = xyAmountToScreen(amounts);
            }

            Util.drawRect(spriteBatch, rectPos, Vector2.One * 12, 4, Color.Black);
            Util.drawRect(spriteBatch, rectPos, Vector2.One * 10, 2, Color.LightGray);
        }

        public void setHue(float hue) {
            BaseHSV = new HSV(hue, 1, 1);
        }

        public void genTexture() {
            var (xPix, yPix) = Util.point(dimen);

            var arr = new Color[xPix * yPix];
            
            for (int x = 0; x < xPix; x++) {
                for (int y = 0; y < yPix; y++) {
                    arr[x + y * xPix] = colorAt((float)x / (xPix - 1), (float)y / (yPix - 1));
                }
            }

            texture = Textures.toTexture(arr, xPix, yPix);
        }

        public Color colorAt(Vector2 xyAmount) {
            return colorAt(xyAmount.X, xyAmount.Y);
        }

        public Color colorAt(float xAmount, float yAmount) {
            return Color.Lerp(Color.Lerp(Color.White, baseColor, xAmount), Color.Black, yAmount);
        }

        public override void clicked(MouseInfo mouse, KeyInfo keys, float deltaTime) {
            base.clicked(mouse, keys, deltaTime);
            held = true;
        }

        public override void update(MouseInfo mouse, KeyInfo keys, float deltaTime) {
            base.update(mouse, keys, deltaTime);

            if (mouse.leftUnpressed) held = false;
            
            if (held) Main.brushColor = colorAt(xyAmount(mouse.pos));
        }
    }

}