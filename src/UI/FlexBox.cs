using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace PixelArt {

    public enum FlexStartType { 
        auto, start
    }

    public class FlexBox {

        public List<UIElement> elements;
        public Rectangle rect;
        public float gap;
        public int maxPer;

        public FlexStartType startType = FlexStartType.auto;

        public FlexBox(List<UIElement> elements, Rectangle rect, float gap = 10, int maxPer = -1) {
            this.elements = elements;
            this.rect = rect;
            this.gap = gap;
            this.maxPer = maxPer;
        }

        
        public void apply() {
            
            if (elements.Count == 0) return;

            Vector2 elDimen = elements[0].startDimen;

            int perRow = calcCols();

            float midInd = perRow / 2F - 0.5F;
            Vector2 start = new Vector2(rect.X + rect.Width / 2F, rect.Y + elDimen.Y / 2F);

            if (rect.Height == 0) start.Y = rect.Y;

            for (int i = 0; i < elements.Count; i++) { 
                Vector2 pos = start + new Vector2((i % perRow - midInd) * (elDimen.X + gap), (i / perRow) * (elDimen.Y + gap));
                elements[i].pos = pos;
            }
        }

        public int calcRows() {
            int cols = calcCols();
            return elements.Count / cols + ((elements.Count % cols == 0) ? 0 : 1);
        }

        public int calcCols() {
            Vector2 dimen = Util.dimen(rect);
            Vector2 elDimen = elements[0].startDimen;
            int perRow = (int) ((dimen.X + gap) / (elDimen.X + gap));
            if (startType == FlexStartType.auto) perRow = Math.Min(elements.Count, perRow);
            if (maxPer != -1) perRow = Math.Min(perRow, maxPer);

            return perRow;
        }
    }
}