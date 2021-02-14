using System;
using Microsoft.Xna.Framework;

namespace PixelArt {
    public class PanelSide : UIBack {
        public PanelSide(Rectangle rectangle) : base(rectangle) {
            texture = Textures.get("PanelSide");
            border = Color.LightGray;
            borderWidth = 1;
        }
    }
}