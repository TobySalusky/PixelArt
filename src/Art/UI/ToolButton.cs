using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PixelArt {
    public class ToolButton : UIButton {

        public Tool tool;
        
        public ToolButton(Tool tool, Vector2 pos) : base(() => Main.tool = tool, pos, Vector2.One * 32, tool.ToString()) {
            this.tool = tool;
            texture = Textures.rect;
            topTexture = Textures.get(tool + "Tool");
            if (topTexture == Textures.nullTexture) topTexture = null;
        }

        public override Color findTint() {
            return (Main.tool == tool) ? Color.LightGray : Color.Gray;
        }
    }
}