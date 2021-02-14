using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace PixelArt {
    public static class Fonts {
        public static SpriteFont Arial;

        public static void loadFonts(ContentManager Content) {
            Arial = Content.Load<SpriteFont>("Arial");
        }
    }
}