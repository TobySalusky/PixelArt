using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PixelArt {
    public class Canvas {

        public Vector2 pos, dimen;
        
        public Texture2D background;
        public int xPix, yPix;
        
        public List<Layer> layers = new List<Layer>();
        public Color[] layerColor;

        public Canvas(int xPix, int yPix) : this(Textures.genRect(new Color(1F, 1F, 1F, 0F), xPix, yPix)) {}

        public Canvas(Texture2D texture) {

            background = Textures.get("rect");
            
            layers.Add(new Layer(texture));
            xPix = texture.Width;
            yPix = texture.Height;
            
            pos = Vector2.Zero;
            dimen = new Vector2(1, (float) yPix / xPix) * 100;
        }

        public void render(Camera camera, SpriteBatch spriteBatch) {
            Rectangle renderRect = camera.toScreen(pos, dimen);
            
            spriteBatch.Draw(background, renderRect, Color.White);

            foreach (var layer in layers) {
                spriteBatch.Draw(layer.texture, renderRect, Color.White);
            }
        }
    }
}