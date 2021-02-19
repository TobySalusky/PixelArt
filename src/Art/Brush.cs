using System;
using Microsoft.Xna.Framework;

namespace PixelArt {
    public class Brush {

        public float freq = 0.5F;
        public float size = 1;
        public string name = "Pencil";
        public Vector2 sizeRange = new Vector2(1, 1);

        public void brushBetween(Vector2 start, Vector2 end, Canvas canvas, Color[] arr) {
            
            Vector2 diff = end - start;
            float mag = Util.mag(diff);
            float angle = Util.angle(diff);
            
            for (float add = 0; add <= mag; add += freq) {
                Vector2 canvasPos = Util.polar(add, angle) + start;
                brushAt(canvasPos, canvas, arr);
            }
        }

        public virtual void brushAt(Vector2 canvasPos, Canvas canvas, Color[] arr) {
            Point pixel = canvas.toPixel(canvasPos);
            if (canvas.inBounds(pixel))
                canvas.setRGB(pixel, Main.brushColor);
        }
    }

    public class CircleBrush : Brush {
        
        public CircleBrush(float size) {
            this.size = size;
            name = "Brush";
            sizeRange = new Vector2(1, 100);
        }
        
        public override void brushAt(Vector2 canvasPos, Canvas canvas, Color[] arr) {

            if (size < 1.1F) {
                base.brushAt(canvasPos, canvas, arr);
                return;
            }

            float rad = size / 2;
            
            for (int x = (int)(canvasPos.X - rad) - 1; x < canvasPos.X + rad + 1; x++) {
                for (int y = (int)(canvasPos.Y - rad) - 1; y < canvasPos.Y + rad + 1; y++) {
                    Point pixel = new Point(x, y);
                    if (canvas.inBounds(pixel) && Util.mag(new Vector2(x + 0.5F, y + 0.5F) - canvasPos) < rad) {
                        canvas.setRGB(pixel, Main.brushColor);
                    }
                }
            }
        }
    }

    public class ClippingBrush : Brush {
        public ClippingBrush(float size) {
            this.size = size;
            name = "Clipping";
            sizeRange = new Vector2(1, 100);
        }
        
        public override void brushAt(Vector2 canvasPos, Canvas canvas, Color[] arr) {

            if (size < 1.1F) {
                base.brushAt(canvasPos, canvas, arr);
                return;
            }

            float rad = size / 2;
            
            for (int x = (int)(canvasPos.X - rad) - 1; x < canvasPos.X + rad + 1; x++) { // TODO: fix how being at size 1 screws up clipping effect (uses base)
                for (int y = (int)(canvasPos.Y - rad) - 1; y < canvasPos.Y + rad + 1; y++) {
                    Point pixel = new Point(x, y);
                    if (canvas.inBounds(pixel) && Util.mag(new Vector2(x + 0.5F, y + 0.5F) - canvasPos) < rad) {
                        if (canvas.getRGB(pixel) != Colors.erased) 
                            canvas.setRGB(pixel, Main.brushColor);
                    }
                }
            }
        }
    }
}