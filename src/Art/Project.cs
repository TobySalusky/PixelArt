namespace PixelArt {
    public class Project { // save last camera position/scale TODO:
        
        public Canvas canvas;

        public Project(ProjectSave save) {
            canvas = save.canvasSave.toCanvas();
        }

        public Project(Canvas canvas) {
            this.canvas = canvas;
        }
    }
}