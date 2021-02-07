using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PixelArt {
    public class UIElement {

        public Vector2 pos, dimen;
        public Vector2 startPos, startDimen;
        public Texture2D texture;

        public virtual void update(MouseInfo mouse, KeyInfo keys, float deltaTime) {
            
            if (mouseOver(mouse.pos)) { 
                hovered(mouse, keys, deltaTime);
            }
            else {
                notHovered(mouse, keys, deltaTime);
            }

            if (mouse.leftPressed && mouseOver(mouse.pos)) {
                clicked(mouse, keys, deltaTime);
            }
        }

        public virtual void hovered(MouseInfo mouse, KeyInfo keys, float deltaTime) {
        
        }
        
        public virtual void notHovered(MouseInfo mouse, KeyInfo keys, float deltaTime) {
        
        }


        public virtual void clicked(MouseInfo mouse, KeyInfo keys, float deltaTime) {
            
        }

        public virtual void render(SpriteBatch spriteBatch) {
            spriteBatch.Draw(texture, drawRect(), Color.White);
        }

        public virtual Rectangle drawRect() {
            return Util.center(pos, dimen);
        }

        public static bool mouseOver(Vector2 mousePos, Vector2 pos, Vector2 dimen) {
            return Util.between(mousePos, pos - dimen / 2, pos + dimen / 2);
        }

        public virtual bool mouseOver(Vector2 mousePos) {
            return mouseOver(mousePos, pos, dimen);
        }

    }
}