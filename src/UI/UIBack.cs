using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PixelArt {
    public class UIBack : UIButton {
        public Color color = Color.White;
        
        public UIBack(Vector2 pos, Vector2 dimen) : base(null, pos, dimen, "UIBackground") {
            hoverGrow = false;
            texture = Textures.get("rect");
        }
        
        public UIBack(Rectangle rectangle) : this(Util.toVec(rectangle.Center), Util.toVec(rectangle.Size)) {}

        public override Color findTint() {
            return color;
        }
    }
}