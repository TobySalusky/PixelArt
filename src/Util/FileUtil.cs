using System.IO;

namespace PixelArt {
    public static class FileUtil {
        
        public static string readTxtFile(string absolutePath) {
            return System.IO.File.ReadAllText(absolutePath);
        }

        public static string readTxtFile(string locationPath, string filename, string extension = ".txt") {
            return readTxtFile(locationPath + filename + extension);
        }

        public static string correctPath(string path) {
            if (!path.EndsWith("/") && !path.EndsWith("\\")) {
                return path + "/";
            }

            return path;
        }

        public static void createDirIfNone(string path) {
            Directory.CreateDirectory(path);
        }
        
        public static string addPathToIdentifier(string path, string identifier) {
            if (!path.EndsWith("/") && !path.EndsWith("\\")) {
                return path + "/" + identifier;
            }

            return path + identifier;
        }
    }
}