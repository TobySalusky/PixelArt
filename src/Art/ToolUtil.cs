using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace PixelArt {
    public static class ToolUtil {

        public static Vector2 tl = new Vector2(0, 450);
        private static int changeInt;
        
        public static List<UIElement> genToolSettings(Tool tool) {
            changeInt++;
            var list = new List<UIElement>();

            if (tool == Tool.Ellipse || tool == Tool.Rect) {
                list.Add(new UIButton(() => ToolSettings.shapeFill = false, Util.tl(tl + new Vector2(20, 20),
                    new Vector2(70, 100))) {
                    colorFunc = () => !ToolSettings.shapeFill ? Color.Gray : Colors.background
                });
                list.Add(new UIButton(() => ToolSettings.shapeFill = true, Util.tl(tl + new Vector2(110, 20),
                    new Vector2(70, 100))) {
                    colorFunc = () => ToolSettings.shapeFill ? Color.Gray : Colors.background
                });
                
            } else { // brush settings
                // TODO: Non-linear slider
                list.Add(new UISlider(Util.tl(tl + new Vector2(20, 20), new Vector2(160, 40)),
                    x => ToolSettings.brush.size = x * (ToolSettings.brush.sizeRange.Y - ToolSettings.brush.sizeRange.X) + ToolSettings.brush.sizeRange.X,
                    () => ((ToolSettings.brush.sizeRange.Y - ToolSettings.brush.sizeRange.X) == 0) ? 1 : 
                        (ToolSettings.brush.size - ToolSettings.brush.sizeRange.X) / (ToolSettings.brush.sizeRange.Y - ToolSettings.brush.sizeRange.X)) {
                    color = Color.Gray, fillColor = Colors.background
                });
                
                list.Add(new UIText(tl + new Vector2(185, 30), () => "" + (int) ToolSettings.brush.size));
                
                for (int i = 0; i < ToolSettings.brushes.Length; i++) {
                    Vector2 pos = tl + new Vector2(20, 100) + Vector2.UnitY * 30 * i;
                    Brush brush = ToolSettings.brushes[i];
                    list.Add(new BrushButton(brush, 
                        Util.tl(pos, new Vector2(180, 30))));
                    list.Add(new UIText(pos, () => brush.name));
                    list.Add(new UIText(pos + 170 * Vector2.UnitX, () => brush.size.ToString("F1")) {rightAlign = true});
                }
            }

            int stay = changeInt;
            foreach (var element in list) {
                element.deleteCondition = () => changeInt != stay;
            }

            return list;
        }

    }
}