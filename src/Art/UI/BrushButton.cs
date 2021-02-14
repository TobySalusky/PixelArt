using System;
using Microsoft.Xna.Framework;

namespace PixelArt {
    public class BrushButton : UIButton {
        
        public BrushButton(Brush brush, Rectangle rectangle) : base(null, rectangle, "BrushButton") {
            clickFunc = () => ToolSettings.brush = brush;
            colorFunc = () => ToolSettings.brush == brush ? Color.Gray : 
                    (hover) ? Color.Lerp(Colors.background, Color.Gray, 0.3F) : Colors.background;
            
            border = Color.LightGray;
            borderWidth = 1;

            hoverGrow = false;
        }
    }
}