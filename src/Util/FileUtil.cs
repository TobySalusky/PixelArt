using System;
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

        public static string fileName(string path, bool withExtension = false) {
            string name = path.Substring(Math.Max(path.LastIndexOf("\\"), path.LastIndexOf("/")) + 1);
            if (!withExtension && name.Contains(".")) {
                name = name.Substring(0, name.LastIndexOf("."));
            }

            return name;
        }

        public static string withoutTrailingSlash(string path) {
            return lastSlash(path) == path.Length - 1 ? path.Substring(0, path.Length - 1) : path;
        }

        public static int lastSlash(string path) {
            return Math.Max(path.LastIndexOf("\\"), path.LastIndexOf("/"));
        }

        public static string dirIn(string filePath) {
            return filePath.Substring(0, lastSlash(filePath));
        }
    }
}