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
                
                Logger.log(1111);
            }

            int stay = changeInt;
            foreach (var element in list) {
                element.deleteCondition = () => changeInt != stay;
            }

            return list;
        }

    }
}