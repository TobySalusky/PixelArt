using Microsoft.Xna.Framework;

namespace PixelArt {
    public class Camera {

        public Vector2 pos;
        public float scale;
        public Vector2 screenCenter = new Vector2(1920, 1080) / 2;
        
        public Camera(Vector2 pos, float scale = 1F) {
            this.pos = pos;
            this.scale = scale;
        }

        public Rectangle worldViewRect() {
            return Util.center(pos, new Vector2(1920, 1080) / scale);
        }

        public Vector2 toWorld(Vector2 screenPos) {
            return (screenPos - screenCenter) / scale + pos;
        }

        public Vector2 toScreen(Vector2 worldPos) {
            return (worldPos - pos) * scale + screenCenter;
        }

        public Rectangle toScreen(Vector2 centerPos, Vector2 dimen) {
            return Util.center(toScreen(centerPos), dimen * scale);
        }
    }
}