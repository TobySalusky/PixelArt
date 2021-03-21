using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PixelArt {
    public enum FileTabType {
        PNG, Folder
    }
    public class FileTab : UIButton {

        public FileTabType fileType;
        public Texture2D image;
        public string path, name, shortName;
        
        public SpriteFont font = Fonts.Arial;
        
        public FileTab(string path, FileTabType fileType) : base(null, Vector2.Zero, Vector2.One * 150, "FileTab") {
            clickFunc = () => FileOpenScreen.open(this);
            this.fileType = fileType;
            colorFunc = () => Colors.panel;
            
            this.path = path;
            name = FileUtil.fileName(path, true);
            shortName = Util.cutoffString(name, font, Width - 20);

            borderWidth = 1;
            border = (fileType == FileTabType.Folder) ? Color.Orange : Color.White;
            
            image = fileType == FileTabType.Folder ? Textures.get("FileTabFolder") : Texture2D.FromFile(Main.getGraphicsDevice(), path);

        }

        public override void render(SpriteBatch spriteBatch) {
            base.render(spriteBatch);
            spriteBatch.DrawString(Fonts.Arial, shortName, pos - dimen / 2 + Vector2.One * 10, border);
            Rectangle rect = drawRect();

            rect.Y += 35;
            rect.Height -= 45;
            rect.X += 10;
            rect.Width -= 20;

            rect = Util.useRatio( Util.textureVec(image), rect);
            if (fileType == FileTabType.PNG) {
                spriteBatch.Draw(Textures.rect, Util.expand(rect, 1), Color.White);
                spriteBatch.Draw(Textures.rect, rect, Colors.canvasBack);
            }

            spriteBatch.Draw(image, rect, Color.White);
        }
    }
}