namespace PixelArt {
    public static class FileUtil {
        
        public static string readTxtFile(string absolutePath) {
            return System.IO.File.ReadAllText(absolutePath);
        }

        public static string readTxtFile(string locationPath, string filename, string extension = ".txt") {
            return readTxtFile(locationPath + filename + extension);
        }

        public static string addPathToIdentifier(string path, string identifier) {
            if (!path.EndsWith("/") && !path.EndsWith("\\")) {
                return path + "/" + identifier;
            }

            return path + identifier;
        }
    }
}