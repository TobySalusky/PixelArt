using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PixelArt {
    public static class Exporting {

        public static string pathError = "";
        public static string exportPath = Paths.exportPath;
        public static string exportName = "";
        
        public static void exportPopUp() {
            Main.exportOpen = true;

            Vector2 tl = Main.screenCenter - Main.screenDimen * 0.35F;
            
            List<UIElement> elements = new List<UIElement> {
                
                // Backdrop and buttons
                new UIBack(Main.screenCenter, Main.screenDimen) {texture = Textures.get("Darken")},
                new UIBack(Main.screenCenter, Main.screenDimen * 0.7F) {color = Colors.exportBack, border = Color.LightGray},
                new UIButton(() => Main.exportOpen = false, Main.screenCenter + new Vector2(580, -320), Vector2.One * 60, "Exit Export") {
                    texture = Textures.get("PanelSide"), topTexture = Textures.get("ExitButton"), topColor = Colors.exportMid
                },
                new UIButton(exportImage, Main.screenCenter + Vector2.UnitY * 250, new Vector2(800, 150), "Export as PNG") {
                    texture = Textures.get("PanelSide")
                },
                
                // Path input
                new UIText("Path:  ", tl + new Vector2(200, 100), new Vector2(100, 30), true), 
                new UITextInput(tl + new Vector2(200, 100), new Vector2(800, 30),
                    (str) => {
                        exportPath = str;
                        pathError = (isValidPath(exportPath)) ? "" : "Invalid Path";
                    }) {backColor = Colors.panel, text = exportPath},
                new UIText(tl + new Vector2(1000, 100), new Vector2(100, 30), () => pathError) {color = Colors.error},
                
                // Name input
                new UIText("Name:  ", new Vector2(Main.screenCenter.X - 200, Main.screenCenter.Y), new Vector2(100, 90), true),
                new UITextInput(new Vector2(Main.screenCenter.X - 200, Main.screenCenter.Y), new Vector2(400, 90), (str) => exportName = str) 
                    {backColor = Colors.panel, text = exportName},
                
                
            };


            foreach (var element in elements) {
                element.deleteCondition = () => !Main.exportOpen;
            }
            Main.uiElements.AddRange(elements);
        }

        public static bool isValidPath(string path) {
            return Directory.Exists(path);
        }

        public static void exportImage() {

            if (isValidPath(exportPath)) {
                Main.exportOpen = false;

                Texture2D texture = Main.canvas.genSingleImage();

                Textures.exportTexture(texture, exportPath, (exportName == "") ? "Untitled " + Util.randInt(100000000) : exportName);
            }
        }
    }
}