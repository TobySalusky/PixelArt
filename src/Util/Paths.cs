using System.IO;

namespace PixelArt {
    public static class Paths {

        public static string solutionPath, assetPath, texturePath, exportPath, fontPath, cssPath;

        static Paths() {
            string path = Path.GetFullPath("hi");
            solutionPath = path.Substring(0, path.IndexOf("bin\\Debug"));
            assetPath = solutionPath + "Assets\\";
            texturePath = assetPath + "Textures\\";
            fontPath = assetPath + "Fonts\\";
            exportPath = assetPath + "Exports\\";
            cssPath = assetPath + "CSS\\";
        }

    }
}