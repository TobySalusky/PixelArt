using System;
using System.Drawing;
using Microsoft.Xna.Framework.Graphics;
using Color = Microsoft.Xna.Framework.Color;

namespace PixelArt {
    
    public class Layer {

        public static int nextID; // TODO: unused + resets when file is loaded = fix pls
        
        public Texture2D texture;
        
        public int uniqueID;
        public string name;
        public bool visible = true;

        
        
        public Layer(Texture2D texture, string name = "Untitled") {
            this.texture = texture;

            this.name = name;
            
            uniqueID = nextID;
            nextID++;
        }

        public Layer(Layer layer) {
            texture = Textures.copy(layer.texture);
            uniqueID = layer.uniqueID;
            name = layer.name;
            visible = layer.visible;
        }

        public Layer(ClipboardLayer clip) {
            
            var arr = new Color[clip.r.Length];
            for (int i = 0; i < arr.Length; i++) {
                arr[i] = new Color(clip.r[i], clip.g[i], clip.b[i], clip.a[i]);
            }
            texture = Textures.toTexture(arr, clip.xPix, clip.yPix);

            uniqueID = clip.uniqueID;
            name = clip.name;
            visible = clip.visible;
        }


        public Layer copy() {
            return new Layer(this);
        }

        public Layer controlC() {
            return new Layer(texture);
        }
    }

    [Serializable]
    public class ClipboardLayer {
        
        public int uniqueID;
        public string name;
        public bool visible;

        public int[] r, g, b, a;
        public int xPix, yPix;
        
        public ClipboardLayer(Layer layer) {
            uniqueID = layer.uniqueID;
            name = layer.name;
            visible = layer.visible;

            var arr = Util.colorArray(layer.texture);
            r = new int[arr.Length];
            g = new int[arr.Length];
            b = new int[arr.Length];
            a = new int[arr.Length];
            
            for (int i = 0; i < arr.Length; i++) {
                Color c = arr[i];
                r[i] = c.R;
                g[i] = c.G;
                b[i] = c.B;
                a[i] = c.A;
            }
            
            xPix = layer.texture.Width;
            yPix = layer.texture.Height;
        }
    }
}