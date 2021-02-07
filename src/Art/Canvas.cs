using System;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PixelArt {
    public class Canvas {

        public Vector2 pos, dimen;
        
        public Texture2D background;
        public int xPix, yPix;
        
        public List<Layer> layers = new List<Layer>();
        public Layer layer;
        public Color[] layerColor;

        public bool changesMade = false;

        public Canvas(int xPix, int yPix) : this(Textures.genRect(new Color(1F, 1F, 1F, 0F), xPix, yPix)) {}

        public Canvas(Texture2D texture) {

            background = Textures.get("rect");
            
            layers.Add(new Layer(texture));
            xPix = texture.Width;
            yPix = texture.Height;
            
            pos = Vector2.Zero;
            dimen = new Vector2(1, (float) yPix / xPix) * 100;

            layer = layers[0];
            layerColor = Util.colorArray(layer.texture);
        }

        public void input(float deltaTime, KeyInfo keys, MouseInfo mouse) {

            Point pixel = inPixel(toCanvas(mouse.pos));

            switch (Main.tool) {
                case Tool.Brush:
                    
                    if (mouse.leftDown) {
                        if (inBounds(pixel)) {
                            setColor(pixel, Main.brushColor);
                        }
                    }
                    break;
            }

            if (changesMade) {
                layer.texture.SetData(layerColor);
                changesMade = false;
            }
        }

        public void render(Camera camera, SpriteBatch spriteBatch) {
            Rectangle renderRect = camera.toScreen(pos, dimen);
            
            spriteBatch.Draw(background, renderRect, Color.White);

            foreach (var layer in layers) {
                spriteBatch.Draw(layer.texture, renderRect, Color.White);
            }
        }
        
        
        public bool inBounds(int x, int y) {
            return (x >= 0 && x < xPix && y >= 0 && y < yPix);
        }

        public bool inBounds(Point pixel) {
            return inBounds(pixel.X, pixel.Y);
        }

        public void setColor(Point point, Color color) {
            layerColor[point.X + point.Y * xPix] = color;
            changesMade = true;
        }
        
        public void setColor(int x, int y, Color color) {
            layerColor[x + y * xPix] = color;
            changesMade = true;
        }

        public Vector2 toCanvas(Vector2 screenPos) {
            return (Main.camera.toWorld(screenPos) - (pos - dimen/2)) / dimen * new Vector2(xPix, yPix);
        }

        public Point inPixel(Vector2 onCanvas) {
            return new Point((int)Math.Floor(onCanvas.X),(int)Math.Floor(onCanvas.Y));
        }
    }
}