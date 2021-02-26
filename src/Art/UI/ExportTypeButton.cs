using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PixelArt {
    public class ExportTypeButton : UIButton {

        public Exporting.ExportType type;

        public ExportTypeButton(Exporting.ExportType type, Vector2 pos) : base(() => Exporting.exportType = type, pos, new Vector2(200, 40), type.ToString()) {
            this.type = type;
            hoverGrow = false;
            
            colorFunc = () => Exporting.exportType == type ? Colors.background : 
                (hover) ? Color.Lerp(Colors.panel, Colors.background, 0.3F) : Colors.panel;

            borderWidth = 1;
        }
    }
}