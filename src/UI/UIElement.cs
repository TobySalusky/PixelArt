using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PixelArt {
    public class UIElement {

        public Vector2 pos, dimen;
        public Vector2 startPos, startDimen;
        public Texture2D texture;

        public Func<bool> deleteCondition;
        public bool delete;

        public bool noHit;
        public bool selectable;

        public bool hover = false;
        
        public virtual void update(MouseInfo mouse, KeyInfo keys, float deltaTime) {
            
            if (mouseOver(mouse.pos)) { 
                hovered(mouse, keys, deltaTime);
                hover = true;
            }
            else {
                notHovered(mouse, keys, deltaTime);
                hover = false;
            }

            if (!noHit && mouse.leftPressed && mouseOver(mouse.pos) && !Main.uiHit) {
                clicked(mouse, keys, deltaTime);
                Main.uiHit = true;
                if (selectable) {
                    Main.selectedUI = this;
                }
            }

            if (deleteCondition != null) {
                if (deleteCondition.Invoke()) {
                    delete = true;
                }
            }
        }

        public virtual Color findTint() {
            return Color.White;
        }

        public Vector2 xyAmount(Vector2 mousePos) {
            return (mousePos - (pos - dimen / 2)) / dimen;
        }
        public Vector2 xyAmountClamped(Vector2 mousePos) {
            var (x, y) = xyAmount(mousePos);
            return new Vector2(Math.Clamp(x, 0, 1), Math.Clamp(y, 0, 1));
        }

        public Vector2 xyAmountToScreen(Vector2 xyAmount) {
            return (pos - dimen / 2) + dimen * xyAmount;
        }

        public Vector2 clampTo(Vector2 mousePos) {
            return xyAmountToScreen(xyAmountClamped(mousePos));
        }

        public virtual void hovered(MouseInfo mouse, KeyInfo keys, float deltaTime) {
        
        }
        
        public virtual void notHovered(MouseInfo mouse, KeyInfo keys, float deltaTime) {
        
        }


        public virtual void clicked(MouseInfo mouse, KeyInfo keys, float deltaTime) {
            
        }

        public virtual void render(SpriteBatch spriteBatch) {
            spriteBatch.Draw(texture, drawRect(), findTint());
        }

        public Rectangle drawRect() {
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