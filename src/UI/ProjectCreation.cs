using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace PixelArt {
    public static class ProjectCreation {
        public static UIIntInput xInput, yInput;
        
        public static void createPopup() {

            Main.popupOpen = true;
            
            Vector2 tl = Main.screenCenter - Main.screenDimen * 0.35F;

            xInput = new UIIntInput(new Vector2(Main.screenCenter.X - 250, Main.screenCenter.Y - 50),
                new Vector2(200, 50)) {backColor = Colors.panel};
            yInput = new UIIntInput(new Vector2(Main.screenCenter.X + 50, Main.screenCenter.Y - 50),
                new Vector2(200, 50)) {backColor = Colors.panel};

            var format = new UITextInput(new Vector2(Main.screenCenter.X - 200, Main.screenCenter.Y - 200),
                new Vector2(400, 90), useText) {backColor = Colors.panel};
            Main.onNextUpdateStart.Add(() => Main.selectedUI = format);
            
            List<UIElement> elements = new List<UIElement> {
                
                // Backdrop and buttons
                new UIBack(Main.screenCenter, Main.screenDimen) {texture = Textures.get("Darken"), clickFunc = () => Main.popupOpen = false},
                new UIBack(Main.screenCenter, Main.screenDimen * 0.7F) {color = Colors.exportBack, border = Color.LightGray, borderWidth = 5},
                new UIButton(() => Main.popupOpen = false, Main.screenCenter + new Vector2(580, -320), Vector2.One * 60) {
                    texture = Textures.get("PanelSide"), topTexture = Textures.get("ExitButton"), topColor = Colors.exportMid
                },
                
                // Full input
                new UIText("Format:  ", new Vector2(Main.screenCenter.X - 200, Main.screenCenter.Y - 200) , new Vector2(100, 90), true),
                format,
                
                // X/Y input
                new UIText("x:  ", new Vector2(Main.screenCenter.X - 250, Main.screenCenter.Y - 50), new Vector2(0, 50), true),
                xInput,
                new UIText("y:  ", new Vector2(Main.screenCenter.X + 50, Main.screenCenter.Y - 50), new Vector2(0, 50), true),
                yInput,
                
                // Create button
                new UIButton(create, Main.screenCenter + Vector2.UnitY * 250, new Vector2(800, 150)) {
                    texture = Textures.get("PanelSide"), borderWidth = 1
                },
                new UIText("Create", Main.screenCenter + Vector2.UnitY * 250) {center = true},
            };


            foreach (var element in elements) {
                element.deleteCondition = () => !Main.popupOpen;
            }
            Main.uiElements.AddRange(elements);
        }

        public static void create() {
            int x = xInput.asInt();
            int y = yInput.asInt();

            if (x > 0 && y > 0) { 
                Main.setCanvas(new Canvas(x, y));
                
                Main.popupOpen = false;
            }
            else { 
                // TODO: ERROR popup
            }
        }

        public static void useText(string text) {

            int x = -1, y = -1;
            
            string thisNum = "";
            bool num = false;

            void addNum(int num) {
                if (x == -1) {
                    x = num;
                }
                else if (y == -1) {
                    y = num;
                }
            }

            var arr = text.ToCharArray();
            for (int i = 0; i < text.Length; i++) {
                char c = arr[i];

                if (c >= '0' && c <= '9') {
                    if (!num) { 
                        num = true;
                        thisNum = "";
                    }
                    thisNum += "" + c;
                }
                else { 
                    if (num) addNum(int.Parse(thisNum));
                    num = false;
                }
            }
            if (num) addNum(int.Parse(thisNum));

            if (y == -1) y = x;

            if (x != -1) { 
                xInput.setInt(x);
                yInput.setInt(y);
            }
        }
    }
}