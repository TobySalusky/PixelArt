using System;
using Microsoft.Xna.Framework;

namespace PixelArt {
    public class UIIntInput : UITextInput {

        public Action<int> intAction;
        public Func<int, int> limiter;
        public Func<object> findInt;
        public int defaultVal = 0;
        
        public UIIntInput(Vector2 pos, Vector2 dimen, Action<int> intAction = null, Func<int, int> limiter = null, Func<object> findInt = null) : base(pos, dimen, null) {
            this.intAction = intAction;
            this.limiter = limiter;
            this.findInt = findInt;
        }

        public override void update(MouseInfo mouse, KeyInfo keys, float deltaTime) {
            base.update(mouse, keys, deltaTime);
            
            if (findInt != null && this != Main.selectedUI) {
                text = findInt.Invoke().ToString();
            }
        }

        public void setInt(int num) {
            text = "" + num;
            intAction?.Invoke(num); // TODO: use limiter?
        }

        public override void spaceAction() {
            enterAction();
        }

        public override void enterAction() {
            base.enterAction();
            if (limiter != null)
                text = "" + limiter.Invoke(asInt());
            
            if (text != "") text = "" + asInt();
        }

        public override bool allowChange(string oldText, string newText) {
            if (newText == "" || int.TryParse(newText, out int num)) {
                return true;
            }

            text = oldText;
            return false;
        }

        public int asInt() {
            return text == "" ? defaultVal : int.Parse(text);
        }

        public override void changeAction(bool enter = false) {
            int val = asInt();
            if (limiter != null) {
                int newVal = limiter.Invoke(val);
                if (val != newVal && text != "")
                    text = "" + newVal;
                val = newVal;
            }

            intAction?.Invoke(val);
        }
    }
}