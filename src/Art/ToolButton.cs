using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PixelArt {
    public class ToolButton : UIButton {

        public Texture2D toolTexture;
        public Tool tool;
        
        public ToolButton(Tool tool, Vector2 pos) : base(() => Main.tool = tool, pos, Vector2.One * 32, tool.ToString()) {
            this.tool = tool;
            texture = Textures.rect;
            toolTexture = Textures.get(tool + "Tool");
            if (toolTexture == Textures.nullTexture) toolTexture = Textures.invis;
        }

        public override Color findTint() {
            return (Main.tool == tool) ? Color.LightGray : Color.Gray;
        }

        public override void render(SpriteBatch spriteBatch) {
            base.render(spriteBatch);
            spriteBatch.Draw(toolTexture, drawRect(), Color.White);
        }
    }
}