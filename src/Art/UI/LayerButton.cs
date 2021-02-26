using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PixelArt {
    public class LayerButton : UIButton {

        public static List<LayerButton> layerButtons = new List<LayerButton>();

        public Layer layer;

        public LayerButton(Layer layer, Vector2 pos, string name = "Untitled Layer") : base(null, pos, new Vector2(150, 80), name) {
            this.layer = layer;
            clickFunc = () => {
                Main.canvas.selectedLayerIndex = Main.canvas.layers.IndexOf(layer);
                Main.canvas.layer = layer;
            };

            colorFunc = () => Main.canvas.layer == layer ? Color.Gray : 
                (hover) ? Color.Lerp(Colors.background, Color.Gray, 0.3F) : Colors.background;

            hoverGrow = false;

            borderWidth = 1;
            border = Color.White;
        }

        public override void render(SpriteBatch spriteBatch) {
            base.render(spriteBatch);

            Rectangle image = Util.expand(drawRect(), -10);
            
            spriteBatch.Draw(layer.texture, image, Color.White);
        }

        public static void handleLayerButtons() {
            if (Main.updateLayerButtons) {
                var layers = Main.canvas.layers;

                foreach (var button in layerButtons) {
                    button.delete = true;
                }
                layerButtons.Clear(); // TODO: relink
                
                foreach ((int i, Layer layer) in layers.Enumerate()) {
                    
                    Vector2 pos = Main.screenDimen - new Vector2(175 / 2F, 160 + 90 * i);
                    
                    layerButtons.Add(new LayerButton(layer, pos));
                }
                
                Main.uiElements.AddRange(layerButtons);
                
                Main.updateLayerButtons = false;
            }
        }
    }
    
    public static class Extensions // https://gist.github.com/Zodt/09c484c224f8a8bd11d96fe3ab962904
    {
        public static IEnumerable<(int, TGeneralized)> Enumerate<TGeneralized>
            (this IEnumerable<TGeneralized> array) => array.Select((item, index) => (index , item));
    }
}