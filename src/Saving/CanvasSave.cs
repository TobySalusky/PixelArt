using System.Collections.Generic;

namespace PixelArt {
    public class CanvasSave {

        public int layersCreated;
        public LayerSave[] layerSaves;

        public CanvasSave() {}

        public CanvasSave(Canvas canvas) {

            layersCreated = canvas.layersCreated;
            
            layerSaves = new LayerSave[canvas.layers.Count];

            for (int i = 0; i < layerSaves.Length; i++) {
                layerSaves[i] = new LayerSave(canvas.layers[i]);
            }
        }

        public Canvas toCanvas() { 
            List<Layer> layers = new List<Layer>();

            foreach (var layerSave in layerSaves) {
                layers.Add(layerSave.toLayer());
            }
            
            Canvas canvas = new Canvas(layers[0].texture) {layers = layers, layersCreated = layersCreated};
            
            return canvas;
        }
    }
}