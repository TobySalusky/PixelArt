using Microsoft.Xna.Framework.Graphics;

namespace PixelArt {
    public class Layer {
        
        public Texture2D texture;

        public Layer(Texture2D texture) {
            this.texture = texture;
        }

        public Layer copy() {
            return new Layer(Textures.copy(texture));
        }
    }
}