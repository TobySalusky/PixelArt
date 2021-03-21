using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PixelArt {
    public class PanelTop : UIBack {
        public int dir;
        public PanelTop(Rectangle rectangle, int dir) : base(rectangle) {
            texture = Textures.get("PanelSide");
            border = Color.LightGray;
            this.dir = dir;
        }

        public override void render(SpriteBatch spriteBatch) {
            base.render(spriteBatch);
            Util.drawLineScreen(pos + new Vector2(-0.5F, dir * 0.5F) * dimen, pos + new Vector2(0.5F, dir * 0.5F) * dimen, spriteBatch, border, 1);
        }
    }
}