using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PixelArt {
    public class UISlider : UIElement {

        public Action<float> slideFunc;
        public Func<float> findSlideFunc;

        public float slideAmount;
        public bool held;
        
        public bool vertical;

        public UISlider(Vector2 pos, Vector2 dimen, Action<float> slideFunc = null, Func<float> findSlideFunc = null) {
            this.slideFunc = slideFunc;
            this.findSlideFunc = findSlideFunc;
            this.pos = pos;
            this.dimen = dimen;
            
            texture = Textures.get("UIButton");
        }

        public override void render(SpriteBatch spriteBatch) {
            base.render(spriteBatch);
            renderHandle(spriteBatch);
        }

        public override void clicked(MouseInfo mouse, KeyInfo keys, float deltaTime) {
            held = true;
        }

        public override void update(MouseInfo mouse, KeyInfo keys, float deltaTime) {
            base.update(mouse, keys, deltaTime);

            if (mouse.leftUnpressed) held = false;

            if (held) {
                slideAmount = findSlideAmount(mouse.pos);
                slideFunc.Invoke(slideAmount);
            }
            else {
                if (findSlideFunc != null) slideAmount = findSlideFunc.Invoke();
            }
        }

        public float findSlideAmount(Vector2 mousePos) {
            Vector2 amounts = xyAmountClamped(mousePos);
            
            return vertical ? amounts.Y : amounts.X;
        }

        public Vector2 findHandlePos() {
            return xyAmountToScreen(vertical ? new Vector2(0.5F, slideAmount) : new Vector2(slideAmount, 0.5F));
        }

        public void renderHandle(SpriteBatch spriteBatch) {
            Util.drawRect(spriteBatch, findHandlePos(), Vector2.One * 12, 4, Color.Black);
            Util.drawRect(spriteBatch, findHandlePos(), Vector2.One * 10, 2, Color.LightGray);
        }
    }
}