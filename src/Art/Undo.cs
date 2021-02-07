using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace PixelArt {
    public class Undo {
        public List<Color[]> arrs = new List<Color[]>();

        public Undo(Canvas canvas) {
            arrs.Add(Util.colorArray(canvas.layer.texture));
        }

        public void apply(Canvas canvas) {
            // TODO: multilayer
            canvas.layer.texture.SetData(arrs[0]);
        }
    }
}