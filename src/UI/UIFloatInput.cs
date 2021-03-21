using System;
using Microsoft.Xna.Framework;

namespace PixelArt {
    public class UIFloatInput : UITextInput {

        public Action<float> floatAction;
        public Func<float, float> limiter;
        public Func<object> findFloat;
        public float defaultVal = 0F;
        
        public UIFloatInput(Vector2 pos, Vector2 dimen, Action<float> floatAction = null, Func<float, float> limiter = null, Func<object> findFloat = null) : base(pos, dimen, null) {
            this.floatAction = floatAction;
            this.limiter = limiter;
            this.findFloat = findFloat;
        }

        public override void update(MouseInfo mouse, KeyInfo keys, float deltaTime) {
            base.update(mouse, keys, deltaTime);
            
            if (findFloat != null && this != Main.selectedUI) {
                text = findFloat.Invoke().ToString();
            }
        }

        public override void spaceAction() {
            enterAction();
        }

        public override void enterAction() {
            base.enterAction();
            if (limiter != null)
                text = "" + limiter.Invoke(asFloat());
        }

        public override bool allowChange(string oldText, string newText) {
            if (newText == "" || float.TryParse(newText, out float num)) {
                return true;
            }

            text = oldText;
            return false;
        }

        public float asFloat() {
            return text == "" ? defaultVal : float.Parse(text);
        }

        public override void changeAction(bool enter = false) {
            float val = asFloat();
            if (limiter != null) {
                float newVal = limiter.Invoke(val);
                if (val != newVal && text != "")
                    text = "" + newVal;
                val = newVal;
            }

            floatAction?.Invoke(val);
        }
    }
}