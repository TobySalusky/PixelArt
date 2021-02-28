using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace PixelArt {
    public class Undo {

        public int layersCreated;
        public List<Layer> layers = new List<Layer>();
        public int selectedLayerIndex;

        public Undo(Canvas canvas) {
            foreach (var layer in canvas.layers) {
                layers.Add(layer.copy());
            }

            layersCreated = canvas.layersCreated;
            selectedLayerIndex = canvas.layerIndex;
        }

        public void apply(Canvas canvas) {
            // TODO: multilayer
            canvas.layers = layers;
            canvas.layerIndex = selectedLayerIndex;
            canvas.layer = canvas.layers[selectedLayerIndex];
            Main.updateLayerButtons = true;

            canvas.layersCreated = layersCreated;
        }
    }
}