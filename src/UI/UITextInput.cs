using System;
using System.ComponentModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace PixelArt {
    public class UITextInput : UIText { // POS marks Top-left

        public static readonly char[] shiftNum = { 
            ')', '!', '@', '#', '$', '%', '^', '&', '*', '('
        };

        public Color backColor = Color.Gray;
        public Color unselectedBorder = Color.Gray;
        public Color selectBorder = Color.LightGray;

        public Action<string> stringAction;
        
        public UITextInput(Vector2 pos, Vector2 dimen, Action<string> stringAction = null) : base(pos, dimen, null) {
            noHit = false;
            selectable = true;
            this.dimen = dimen;

            this.stringAction = stringAction;
        }

        public virtual bool allowChange(string oldText, string newText) {
            return true;
        }

        public override bool mouseOver(Vector2 mousePos) {
            return Util.between(mousePos, pos, pos + dimen);
        }

        public override void render(SpriteBatch spriteBatch) {
            Rectangle rect = Util.tl(pos, dimen);
            spriteBatch.Draw(Textures.rect, rect, backColor);
            Util.drawRect(spriteBatch, rect, 1, this == Main.selectedUI ? selectBorder : unselectedBorder);
            base.render(spriteBatch);
        }

        public virtual void changeAction() { 
            stringAction?.Invoke(text);
        }

        public virtual void enterAction() {
            if (Main.selectedUI == this) { 
                changeAction();
                Main.selectedUI = null;
            }
            else {
                Logger.log("ERROR in UITextInput");
            }
        }

        public virtual void spaceAction() {
            text += ' ';
        }

        public override void notClicked(MouseInfo mouse, KeyInfo keys, float deltaTime) {
            if (this == Main.selectedUI)
                enterAction();
            
            base.notClicked(mouse, keys, deltaTime);
        }

        public override void update(MouseInfo mouse, KeyInfo keys, float deltaTime) {

            if (Main.selectedUI == this) {
                
                KeyboardState oldState = keys.oldState;
                KeyboardState newState = keys.newState;

                Keys[] arr = newState.GetPressedKeys();
                bool changed = false;
                bool shift = keys.down(Keys.LeftShift) || keys.down(Keys.RightShift);
                bool control = keys.down(Keys.LeftControl) || keys.down(Keys.RightControl);
                
                foreach (Keys key in arr) {
                    bool changeOnKey = false;
                    string oldText = text;
                    
                    if (oldState.IsKeyUp(key)) {
                        
                        changeOnKey = true;
                        
                        string str = key.ToString();
                        bool single = str.Length == 1;
                        char c = str[0];

                        if (single && c >= 'A' && c <= 'Z') {
                            if (shift || newState.CapsLock) text += c;
                            else text += (char) (c + 32);
                        } 
                        else if (str.Length == 2 && c == 'D' && str[1] >= '0' && str[1] <= '9') {
                            text += (shift) ? (shiftNum[int.Parse("" + str[1])]) : str[1];
                        }
                        else if (key == Keys.Back) {
                            if (text.Length > 0) {
                                if (control) {
                                    text = text.Remove(Math.Max(0, text.LastIndexOf(" ")));
                                }
                                else {
                                    text = text.Remove(text.Length - 1);
                                }
                            }
                        }
                        else if (key == Keys.Enter) {
                            enterAction();
                        } 
                        else if (key == Keys.Space) {
                            spaceAction();
                        } 
                        else if (key == Keys.OemPeriod) { 
                            text += (shift) ? '>' : '.';
                        }
                        else if (key == Keys.OemComma) { 
                            text += (shift) ? '<' : ',';
                        }
                        else if (key == Keys.OemQuestion) { 
                            text += (shift) ? '?' : '/';
                        }
                        else if (key == Keys.OemSemicolon) { 
                            text += (shift) ? ':' : ';';
                        }
                        else if (key == Keys.OemQuotes) { 
                            text += (shift) ? '"' : '\'';
                        }
                        else if (key == Keys.OemTilde) { 
                            text += (shift) ? '~' : '`';
                        }
                        else if (key == Keys.OemPipe) { 
                            text += (shift) ? '|' : '\\';
                        }
                        else if (key == Keys.LeftShift || key == Keys.RightShift 
                                                       || key == Keys.LeftControl || key == Keys.RightControl 
                                                       || key == Keys.CapsLock) { // TODO: don't call str update/changed
                            // nothing
                            changeOnKey = false;
                        } 
                        else { // default
                            text += str;
                        }
                    }

                    if (changeOnKey) {
                        changed = allowChange(oldText, text);
                    }
                }

                if (changed) {
                    changeAction();
                }
            }

            base.update(mouse, keys, deltaTime);
        }
    }
}