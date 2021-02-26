using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PixelArt {
    public class UIText : UIElement {
        
        public SpriteFont font;
        public Color color;
        public string text = "";

        public Func<string> findString;

        public bool rightAlign = false;
        public Vector2 bindPos;
        
        public Vector2 textOffset;

        public float fontHeight;

        public bool center = false;
        
        public UIText(Vector2 pos, Func<string> findString) {
            this.pos = pos;
            bindPos = pos;
            font = Fonts.Arial;
            this.findString = findString;
            noHit = true;
            color = Color.White;
            texture = Textures.invis;

            fontHeight = font.MeasureString("TEST").Y;

            dimen = new Vector2(0, fontHeight);
        }


        public UIText(Vector2 pos, Vector2 dimen, Func<string> findString) : this(pos, findString){
            this.dimen = dimen;
            textOffset = new Vector2(10, dimen.Y / 2 - fontHeight / 2);
        }

        public UIText(string text, Vector2 pos) : this(pos, new Vector2(-1, -1), null) {
            this.text = text;
        }
        
        public UIText(string text, Vector2 pos, Vector2 dimen, bool rightAlign = false) : this(pos, dimen, null) {
            this.text = text;
            this.rightAlign = rightAlign;
        }

        public override void update(MouseInfo mouse, KeyInfo keys, float deltaTime) {
            base.update(mouse, keys, deltaTime);

            if (findString != null)
                text = findString.Invoke();
            if (rightAlign) {
                pos.X = bindPos.X - font.MeasureString(text).X;
            }
        }

        public virtual Vector2 textPos() { //TODO: scuffed, rework
            if (center) {
                Vector2 textSize = font.MeasureString(text);
                return pos - textSize / 2;
            }

            return (rightAlign) ? pos + new Vector2(-1, 1) * textOffset : pos + textOffset;
        }

        public override void render(SpriteBatch spriteBatch) {
            spriteBatch.DrawString(font, text, textPos(), color);
        }
    }
}