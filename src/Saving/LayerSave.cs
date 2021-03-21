using Microsoft.Xna.Framework;

namespace PixelArt {
    
    public class LayerSave {

        public string name;
        public bool visible;
        public int width, height;
        public int[] rgba;
        public int[] run;

        public LayerSave() { }

        public LayerSave(Layer layer) {

            name = layer.name;
            visible = layer.visible;
            width = layer.texture.Width;
            height = layer.texture.Height;
            
            var arr = Util.colorArray(layer.texture);
            int diffCount = 0;
            for (int i = 0; i < arr.Length; i++) {
                if (i == 0 || arr[i - 1] != arr[i]) {
                    diffCount++;
                }
            }
            
            rgba = new int[diffCount];
            run = new int[diffCount];

            int diffNum = 0;
            int currRun = 0, currRGB = RGBA.colorToRGBA(arr[0]);
            for (int i = 0; i < arr.Length; i++) {
                int thisRGB = RGBA.colorToRGBA(arr[i]);
                if (thisRGB == currRGB) {
                    currRun++;
                }
                else {
                    rgba[diffNum] = currRGB;
                    run[diffNum] = currRun;
                    diffNum++;

                    currRGB = thisRGB;
                    currRun = 1;
                }
            }
            rgba[diffCount - 1] = currRGB;
            run[diffCount - 1] = currRun;
        }

        public Layer toLayer() {

            var arr = new Color[width * height];
            int x = 0;
            for (int i = 0; i < run.Length; i++) {
                Color color = RGBA.fromRGBA(rgba[i]);
                for (int j = 0; j < run[i]; j++) {
                    arr[x] = color;
                    x++;
                }
            }
            
            return new Layer(Textures.toTexture(arr, width, height), name) { visible = visible};
        }
    }
}