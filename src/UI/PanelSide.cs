using System;
using Microsoft.Xna.Framework;

namespace PixelArt {
    public class PanelSide : UIButton {
        public PanelSide(Rectangle rectangle) : base(null, Util.toVec(rectangle.Center), Util.toVec(rectangle.Size), "PanelSide") {
            hoverGrow = false;
            texture = Textures.get("PanelSide");
        }
    }
}