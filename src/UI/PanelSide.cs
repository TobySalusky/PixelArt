using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PixelArt {
    public class PanelSide : UIBack {
        public int side;
        public PanelSide(Rectangle rectangle, int side) : base(rectangle) {
            texture = Textures.get("PanelSide");
            border = Color.LightGray;
            this.side = side;
        }

        public override void render(SpriteBatch spriteBatch) {
            base.render(spriteBatch);
            Util.drawLineScreen(pos + new Vector2(side * 0.5F, -0.5F) * dimen, pos + new Vector2(side * 0.5F, 0.5F) * dimen, spriteBatch, border, 1);
        }
    }
}