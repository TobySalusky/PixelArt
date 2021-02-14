using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PixelArt {
    public class UIText : UIElement {
        
        public SpriteFont font;
        public Color color;
        public string text = "";

        public Func<string> findString;
        
        public UIText(Vector2 tl, Func<string> findString) {
            pos = tl;
            font = Fonts.Arial;
            this.findString = findString;
            noHit = true;
            color = Color.White;
        }

        public override void update(MouseInfo mouse, KeyInfo keys, float deltaTime) {
            base.update(mouse, keys, deltaTime);

            text = findString.Invoke();
        }

        public override void render(SpriteBatch spriteBatch) {
            spriteBatch.DrawString(font, text, pos, color);
        }
    }
}