using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace PixelArt {
    public class UITextInput : UIText {
        public UITextInput(Vector2 pos) : base(pos, null) {
            noHit = false;
            selectable = true;
            dimen = new Vector2(100, 30);
        }

        public override void update(MouseInfo mouse, KeyInfo keys, float deltaTime) {

            if (Main.selectedUI == this) {
                
                KeyboardState oldState = keys.oldState;
                KeyboardState newState = keys.newState;

                Keys[] arr = newState.GetPressedKeys();
                bool changed = false;
                bool shift = keys.down(Keys.LeftShift) || keys.down(Keys.RightShift);

                foreach (Keys key in arr) {
                    if (oldState.IsKeyUp(key)) {
                        changed = true;
                        string str = key.ToString();
                        bool single = str.Length == 1;
                        char c = str[0];
                        if (single && c >= 'A' && c <= 'Z') {
                            if (shift) text += c;
                            else text += (char) (c + 32);
                        } 
                        else if (str.Length == 2 && c == 'D' && str[1] >= '0' && str[1] <= '9') {
                            text += str[1];
                        }
                        else if (key == Keys.Back) {
                            text = text.Remove(text.Length - 1);
                        }
                        else if (key == Keys.Space) {
                            text += " ";
                        } 
                        else if (key == Keys.OemPeriod) { 
                            text += ".";
                        }
                        else if (key == Keys.OemComma) { 
                            text += ",";
                        }
                        else if (key == Keys.LeftShift || key == Keys.RightShift) { // TODO: don't call str update/changed
                            // nothing
                        } 
                        else { // default
                            text += str;
                        }
                    }
                }
                
            }

            base.update(mouse, keys, deltaTime);
        }
    }
}