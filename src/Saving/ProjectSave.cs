namespace PixelArt {
    public class ProjectSave {
        
        public CanvasSave canvasSave;
        
        public ProjectSave() {}

        public ProjectSave(Project project) {
            canvasSave = new CanvasSave(project.canvas);
        }
    }
}