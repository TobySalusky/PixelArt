namespace PixelArt {
    public static class ToolSettings {
        
        public static bool shapeFill = false;
        public static Brush brush;
        public static Brush[] brushes = {
            new CircleBrush(1F),
            new CircleBrush(3F),
            new CircleBrush(5),
            new CircleBrush(10),
            new CircleBrush(15),
            new ClippingBrush(30), 
        };

        public static void loadTools() {
            brush = brushes[0];
        }


    }
}