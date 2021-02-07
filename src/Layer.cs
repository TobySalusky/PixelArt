using Microsoft.Xna.Framework.Graphics;

namespace PixelArt {
    public struct Layer {

        public Layer(Texture2D texture) {
            this.texture = texture;
        }

        public Texture2D texture;
    }
}