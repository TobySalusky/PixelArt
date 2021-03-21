using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PixelArt {
    public class UIButton : UIElement {
        
        public Action clickFunc;

        public bool hoverGrow;
        public float hoverTime;
        public const float hoverSpeed = 7.5F;
        public float hoverMult = 1.1F;
        public string name;

        public Texture2D topTexture;
        public Color topColor = Color.White;

        public Func<Color> colorFunc;

        public bool trueBorder = false; // true to not draw inside (used for transparency)
        public Color border = Color.Gray;
        public int borderWidth = 0;
        
        public UIButton(Action clickFunc, Vector2 pos, Vector2 dimen, string name = "Untitiled") {
            this.clickFunc = clickFunc;
            this.pos = pos;
            this.dimen = dimen;
            this.name = name;

            hoverGrow = true;
            
            startPos = pos;
            startDimen = dimen;
            
            texture = Textures.rect;
        }
        
        public UIButton(Action clickFunc, Rectangle rectangle, string name = "Untitiled") : this(clickFunc, Util.toVec(rectangle.Center), Util.toVec(rectangle.Size), name) {}

        public override void render(SpriteBatch spriteBatch) {
            Rectangle dr = drawRect();
            
            if (borderWidth > 0) {
                if (trueBorder == true) {
                    Util.renderCutRect(Util.expand(dr, borderWidth), dr, spriteBatch, border);
                }
                else {
                    spriteBatch.Draw(Textures.get("rect"), Util.expand(dr, borderWidth), border);
                }
            }
            base.render(spriteBatch);
            if (topTexture != null) {
                spriteBatch.Draw(topTexture, dr, topColor);
            }
        }

        public override Color findTint() {
            if (colorFunc != null) {
                return colorFunc.Invoke();
            }

            return base.findTint();
        }


        public override void update(MouseInfo mouse, KeyInfo keys, float deltaTime) {

            if (hoverGrow) {
                hoverTime = Math.Clamp(hoverTime, 0, 1);
                dimen = Util.sinLerp(hoverTime, startDimen, startDimen * hoverMult);
            }

            base.update(mouse, keys, deltaTime);
        }

        public override void hovered(MouseInfo mouse, KeyInfo keys, float deltaTime) {
            hoverTime += deltaTime * hoverSpeed;
        }

        public override void notHovered(MouseInfo mouse, KeyInfo keys, float deltaTime) {
            hoverTime -= deltaTime * hoverSpeed;
        }
        
        public override void clicked(MouseInfo mouse, KeyInfo keys, float deltaTime) {
            base.clicked(mouse, keys, deltaTime);
            clickFunc?.Invoke();
        }
    }
}