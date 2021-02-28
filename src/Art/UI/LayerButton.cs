using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Windows.Forms;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Keys = Microsoft.Xna.Framework.Input.Keys;

namespace PixelArt {
    public class LayerButton : UIButton {

        public static List<LayerButton> layerButtons = new List<LayerButton>();

        public Layer layer;

        public bool held;
        public float grabOff;
        public float targetY;

        public UIButton visibleButton;
        
        public static Texture2D transBack = Textures.genTrans(96, 54, 6);
        public static int lastIndex = -1;

        public LayerButton(Layer layer, Vector2 pos, string name = "Untitled Layer") : base(null, pos,
            new Vector2(150, 80), name) {
            this.layer = layer;
            clickFunc = () => {
                Main.canvas.layerIndex = Main.canvas.layers.IndexOf(layer);
                Main.canvas.layer = layer;
            };

            colorFunc = () => Main.canvas.layer == layer ? Color.Gray :
                (hover) ? Color.Lerp(Colors.background, Color.Gray, 0.3F) : Colors.background;

            hoverGrow = false;

            borderWidth = 1;
            border = Color.White;

            targetY = pos.Y;

            visibleButton = new UIButton(() => layer.visible ^= true, Vector2.Zero, Vector2.One * 20) {
                border = Color.White, trueBorder = true, borderWidth = 1, colorFunc = () => layer.visible ? Color.LightGray : Colors.erased, hoverGrow = false
            };
        }

        public override void clicked(MouseInfo mouse, KeyInfo keys, float deltaTime) {
            if (!visibleButton.mouseOver(mouse.pos)) {
                base.clicked(mouse, keys, deltaTime);
            
                held = true;
                grabOff = mouse.pos.Y - pos.Y;
                lastIndex = yToIndex(pos.Y);
            }
        }

        public override void update(MouseInfo mouse, KeyInfo keys, float deltaTime) {

            if (held) {
                if (mouse.leftUnpressed) {
                    held = false;
                }
                else {
                    drag(this, mouse);
                }
            }

            base.update(mouse, keys, deltaTime);

            if (!held && pos.Y != targetY) {
                if (Util.diffUnder(targetY - pos.Y, 0.1F)) {
                    pos.Y = targetY;
                }
                else {
                    pos.Y += (targetY - pos.Y) * deltaTime * 25;
                }
            }
            
            visibleButton.pos = pos + new Vector2(50, 0);
            visibleButton.update(mouse, keys, deltaTime);
        }

        public override void render(SpriteBatch spriteBatch) {
            base.render(spriteBatch);

            Rectangle dr = drawRect();
            const int width = 96, height = 54;
            Rectangle imageMax = new Rectangle((int) (dr.X + (dimen.X - width) / 2F),
                (int) (dr.Y + (dimen.Y - height) / 2F), width, height);
            Rectangle image = Util.useRatio(Util.textureVec(texture), imageMax);

            spriteBatch.Draw(transBack, image, new Rectangle(0, 0, image.Width, image.Height), Color.White);
            spriteBatch.Draw(layer.texture, image, Color.White);
            
            spriteBatch.DrawString(Fonts.Arial, layer.name, pos - dimen / 2 + new Vector2(5, 5), Color.White);
            
            visibleButton.render(spriteBatch);
        }

        public static void drag(LayerButton button, MouseInfo mouse) {
            float y = mouse.pos.Y - button.grabOff;

            button.pos.Y = y;
            int index = yToIndex(y);

            if (index != lastIndex) {
                
                layerButtons.RemoveAt(lastIndex);
                Main.canvas.layers.RemoveAt(lastIndex);
                
                layerButtons.Insert(index, button);
                Main.canvas.layers.Insert(index, button.layer);
                
                foreach ((int i, LayerButton element) in layerButtons.Enumerate()) {
                    element.targetY = indexToY(i);
                }

                Main.canvas.layerIndex = index;
                lastIndex = index;
                
            }
            
            Main.renderAgain.Add(button);
        }

        public static float indexToY(int index) {
            return Main.screenHeight - (160 + 90 * index);
        }

        public static int yToIndex(float y) {

            y -= Main.screenHeight;
            y *= -1;
            y -= 160;
            y += 90 / 2F;

            int index = (int) (y / 90F);
            
            return Math.Clamp(index, 0, layerButtons.Count - 1);
        }

        public static void handleLayerButtons(MouseInfo mouse, KeyInfo keys, float deltaTime) {
            if (Main.updateLayerButtons) {
                var layers = Main.canvas.layers;

                foreach (var button in layerButtons) {
                    button.delete = true;
                }
                layerButtons.Clear(); // TODO: relink
                
                foreach ((int i, Layer layer) in layers.Enumerate()) {
                    
                    Vector2 pos = new Vector2(Main.screenWidth - 175 / 2F, indexToY(i));
                    
                    layerButtons.Add(new LayerButton(layer, pos));
                    layerButtons[i].update(mouse, keys, deltaTime);
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