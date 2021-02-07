using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Xna.Framework;

namespace PixelArt {
    public class UITransition {
        
        public float time, endTime = 1F;
        public UIElement element;

        public bool deleteFlag = false;
        
        public UITransition(UIElement element) {
            this.element = element;
        }

        public virtual void onApply(float deltaTime) { }
        public virtual void onDelete(float deltaTime) { }

        public virtual void update(float deltaTime) {
            time += deltaTime;

            if (time >= endTime) {
                deleteFlag = true;
                onDelete(deltaTime);
            } else {
                onApply(deltaTime);
            }
        }
    }

    public class SinSlide : UITransition {

        public Vector2 startPos, endPos;
        
        public SinSlide(UIElement element, Vector2 startPos, Vector2 endPos) : base(element) {
            this.startPos = startPos;
            this.endPos = endPos;
        }

        public override void onApply(float deltaTime) {
            element.pos = startPos + (endPos - startPos) * Util.sinSmooth(time, endTime);
        }

        public override void onDelete(float deltaTime) {
            element.pos = endPos;
        }
    }

    public class SlideIn : SinSlide {
        public SlideIn(UIElement element) : base(element, new Vector2(
            Util.lessDiff(element.startPos.X, -element.startDimen.X / 2, 1920 + element.startDimen.X / 2), element.startPos.Y), element.startPos) { }
    }
    
    public class SlideOut : SinSlide {
        public SlideOut(UIElement element) : base(element, element.startPos, new Vector2(
            Util.lessDiff(element.startPos.X, -element.startDimen.X / 2, 1920 + element.startDimen.X / 2), element.startPos.Y)) { }

    }
}