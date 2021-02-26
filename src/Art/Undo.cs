using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace PixelArt {
    public class Undo {
        public List<Layer> layers = new List<Layer>();
        public int selectedLayerIndex;

        public Undo(Canvas canvas) {
            foreach (var layer in canvas.layers) {
                layers.Add(layer.copy());
            }

            selectedLayerIndex = canvas.selectedLayerIndex;
        }

        public void apply(Canvas canvas) {
            // TODO: multilayer
            canvas.layers = layers;
            canvas.selectedLayerIndex = selectedLayerIndex;
            canvas.layer = canvas.layers[selectedLayerIndex];
            Main.updateLayerButtons = true;
        }
    }
}