using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PixelArt {
    public class UIButton : UIElement {
        
        public Action clickFunc;

        public bool hoverGrow;
        public float hoverTime;
        public const float hoverSpeed = 5;
        public const float hoverMult = 1.1F;
        public string name;
        
        public UIButton(Action clickFunc, Vector2 pos, Vector2 dimen, string name = "Untitiled") {
            this.clickFunc = clickFunc;
            this.pos = pos;
            this.dimen = dimen;
            this.name = name;

            hoverGrow = true;
            
            startPos = pos;
            startDimen = dimen;
            
            texture = Textures.get("UIButton");
        }

        public override void render(SpriteBatch spriteBatch) {
            base.render(spriteBatch);
            //Vector2 nameDimen = Fonts.arial.MeasureString(name);
            //spriteBatch.DrawString(Fonts.arial, name, pos - nameDimen / 2, Color.White);
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
            clickFunc?.Invoke();
        }
    }
}