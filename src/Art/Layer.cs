using System.Drawing;
using Microsoft.Xna.Framework.Graphics;

namespace PixelArt {
    public class Layer {

        public static int nextID;

        public readonly int uniqueID;
        public Texture2D texture;

        public Layer(Texture2D texture) {
            this.texture = texture;

            uniqueID = nextID;
            nextID++;
        }

        public Layer(Layer layer) {
            texture = Textures.copy(layer.texture);
            uniqueID = layer.uniqueID;
        }


        public Layer copy() {
            return new Layer(this);
        }
    }
}