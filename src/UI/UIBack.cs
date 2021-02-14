using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PixelArt {
    public class UIBack : UIButton {
        public Color border = new Color(1F, 1F, 1F, 0F);
        public int borderWidth = 5;
        public Color color = Color.White;
        
        public UIBack(Vector2 pos, Vector2 dimen) : base(null, pos, dimen, "UIBackground") {
            hoverGrow = false;
            texture = Textures.get("rect");
        }
        
        public UIBack(Rectangle rectangle) : this(Util.toVec(rectangle.Center), Util.toVec(rectangle.Size)) {}

        public override void render(SpriteBatch spriteBatch) {
            spriteBatch.Draw(Textures.get("rect"), Util.expand(drawRect(), borderWidth), border);
            base.render(spriteBatch);
        }

        public override Color findTint() {
            return color;
        }
    }
}