using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PixelArt {
    public static class Exporting {

        public static string pathError = "";
        public static string exportPath = Paths.exportPath;
        public static string exportName = "";

        public static ExportType exportType;

        public enum ExportType {
            png, allLayersPackaged, allLayers
        }

        public static void exportPopUp() {
            Main.exportOpen = true;

            Vector2 tl = Main.screenCenter - Main.screenDimen * 0.35F;

            var nameInput = new UITextInput(new Vector2(Main.screenCenter.X - 200, Main.screenCenter.Y - 200),
                    new Vector2(400, 90), (str) => exportName = str) {backColor = Colors.panel, text = exportName};
            Main.onNextUpdateStart.Add(() => Main.selectedUI = nameInput);
            
            List<UIElement> elements = new List<UIElement> {
                
                // Backdrop and buttons
                new UIBack(Main.screenCenter, Main.screenDimen) {texture = Textures.get("Darken"), clickFunc = () => Main.exportOpen = false},
                new UIBack(Main.screenCenter, Main.screenDimen * 0.7F) {color = Colors.exportBack, border = Color.LightGray, borderWidth = 5},
                new UIButton(() => Main.exportOpen = false, Main.screenCenter + new Vector2(580, -320), Vector2.One * 60, "Exit Export") {
                    texture = Textures.get("PanelSide"), topTexture = Textures.get("ExitButton"), topColor = Colors.exportMid
                },
                new UIButton(exportImage, Main.screenCenter + Vector2.UnitY * 250, new Vector2(800, 150), "Export as PNG") {
                    texture = Textures.get("PanelSide"), borderWidth = 1
                },
                new UIText("Export", Main.screenCenter + Vector2.UnitY * 250) {center = true},
                
                // Path input
                new UIText("Path:  ", tl + new Vector2(200, 100), new Vector2(100, 30), true), 
                new UITextInput(tl + new Vector2(200, 100), new Vector2(800, 30),
                    (str) => {
                        exportPath = str;
                        pathError = (isValidPath(exportPath)) ? "" : "Invalid Path";
                    }) {backColor = Colors.panel, text = exportPath},
                new UIText(tl + new Vector2(1000, 100), new Vector2(100, 30), () => pathError) {color = Colors.error},
                
                // Name input
                new UIText("Name:  ", new Vector2(Main.screenCenter.X - 200, Main.screenCenter.Y - 200) , new Vector2(100, 90), true),
                nameInput,
                
                // Export Types
                new ExportTypeButton(ExportType.png, Main.screenCenter + new Vector2(-250, 100)),
                new UIText(".png", Main.screenCenter + new Vector2(-250, 100)) {center = true},
                
                new ExportTypeButton(ExportType.allLayersPackaged, Main.screenCenter + new Vector2(0, 100)),
                new UIText("layers (packed)", Main.screenCenter + new Vector2(0, 100)) {center = true},
                
                new ExportTypeButton(ExportType.allLayers, Main.screenCenter + new Vector2(250, 100)),
                new UIText("layers (unpacked)", Main.screenCenter + new Vector2(250, 100)) {center = true},
                
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


                string name = (exportName == "") ? "Untitled " + Util.randInt(100000000) : exportName;
                string path = FileUtil.correctPath(exportPath);

                if (exportType == ExportType.png) {
                    Texture2D texture = Main.canvas.genSingleImage();

                    Textures.exportTexture(texture, exportPath, name);
                } else if (exportType == ExportType.allLayers || exportType == ExportType.allLayersPackaged) {
                    if (exportType == ExportType.allLayersPackaged) {
                        path += name + "/";
                        FileUtil.createDirIfNone(path);
                    }

                    for (int i = 0; i < Main.canvas.layers.Count; i++) {
                        Textures.exportTexture(Main.canvas.layers[i].texture, path, name + i);
                    }
                }
            }
        }
    }
}