using System;

namespace PixelArt {
    public static class Logger {
        public static void log(string str) {
            Console.WriteLine(str);
        }
        
        public static void log(float str) {
            Console.WriteLine(str);
        }
        
        public static void log(int str) {
            Console.WriteLine(str);
        }
        
        public static void log(object str) {
            Console.WriteLine(str);
        }
    }
}