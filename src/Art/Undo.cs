using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace PixelArt {
    public class Undo {
        public List<Layer> layers = new List<Layer>();

        public Undo(Canvas canvas) {
            foreach (var layer in canvas.layers) {
                layers.Add(layer.copy());
            }
        }

        public void apply(Canvas canvas) {
            // TODO: multilayer
            canvas.layers = layers;
        }
    }
}