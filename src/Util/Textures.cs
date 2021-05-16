﻿using System;
using System.Collections;
using System.Collections.Generic;
 using System.Drawing;
 using System.IO;
 using Microsoft.Xna.Framework;
 using Microsoft.Xna.Framework.Graphics;
 using PixelArt;
 using Color = Microsoft.Xna.Framework.Color;

 namespace PixelArt {
    public class Textures {

        private static Dictionary<string, Texture2D> textures;
        public static Texture2D nullTexture, rect, invis, circle;

        public static void loadTextures() {

            textures = new Dictionary<string, Texture2D>();

            textures["pixel"] = genRect(Color.White);
            textures["rect"] = genRect(Color.White);
            textures["UIButton"] = genRect(new Color(Color.Black, 0.5F));
            textures["ToolButton"] = genRect(Color.Gray);
            textures["PanelSide"] = genRect(Colors.panel);
            textures["UIBack"] = genRect(Colors.exportBack);
            textures["Darken"] = genRect(new Color(Color.Black, 0.7F));
            textures["invis"] = genRect(new Color(1F,1F,1F,0F));

            processFolder(Paths.texturePath);

            rect = textures["rect"];
            invis = textures["invis"];
            
            nullTexture = textures["null"];

            circle = genCircle(500); // TODO: optimise, circle is slow and also kind of small/big
        }

        public static Image toImage(Texture2D texture) {
            
            MemoryStream mem = new MemoryStream();
            texture.SaveAsPng(mem, texture.Width, texture.Height);

            return Image.FromStream(mem);
        }

        public static Dictionary<string, Texture2D> debugTexturesGrab() {
            return textures;
        }

        public static Texture2D genCircle(int size) {
            Texture2D texture = new Texture2D(Main.getGraphicsDevice(), size, size);

            var arr = new Color[size * size];
            for (int i = 0; i < size; i++) { 
                for (int j = 0; j < size; j++) {
                    if (Util.mag(new Vector2(size / 2F - i, size / 2F - j)) <= size / 2F) {
                        arr[i + j * size] = Color.White;
                    }
                }
            }
            texture.SetData(arr);

            return texture;
        }

        public static Texture2D empty(int width, int height) {
            return genRect(Colors.erased, width, height);
        }
        
        public static Texture2D genTrans(int width, int height, int squareSize = 1) {
            Texture2D texture = new Texture2D(Main.getGraphicsDevice(), width, height);
            
            var arr = new Color[width * height];
            for (int x = 0; x < width; x++) {
                for (int y = 0; y < height; y++) {
                    bool dark = (x / squareSize % 2 == 0);
                    if (y / squareSize % 2 == 0) {
                        dark ^= true;
                    }

                    arr[x + y * width] = (dark) ? Color.LightGray : Color.White;
                }
            }
            
            texture.SetData(arr);
            return texture;
        }

        public static Texture2D genRect(Color rectColor) {
            Texture2D rect = new Texture2D(Main.getGraphicsDevice(), 1, 1);
            rect.SetData(new[] {rectColor});
            return rect;
        }

        public static Texture2D copy(Texture2D texture) {
            return toTexture(Util.colorArray(texture), texture.Width, texture.Height);
        }

        public static Texture2D toTexture(Color[] colArr, int x, int y) {
            Texture2D rect = new Texture2D(Main.getGraphicsDevice(), x, y);
            rect.SetData(colArr);
            return rect;
        }

        public static Texture2D genRect(Color rectColor, int x, int y) {
            Texture2D rect = new Texture2D(Main.getGraphicsDevice(), x, y);
            
            var arr = new Color[x * y];

            for (int r = 0; r < x; r++) {
                for (int c = 0; c < y; c++) {
                    arr[r + c * x] = rectColor;
                }
            }
            rect.SetData(arr);
            return rect;
        }
        
        private static void processFile(string path) { // assumes a png file...
            int start = path.LastIndexOf("\\") + 1;
            int pngIndex = path.LastIndexOf(".png");
            
            if (pngIndex != -1) {
                string filename = path.Substring(start, pngIndex - start);
                loadTexture(filename, path);
            }
        }

        private static void processFolder(string dirPath) {
            string [] files = Directory.GetFiles(dirPath);
            foreach (string file in files)
                processFile(file);

            // recursive calls
            string [] subDirs = Directory.GetDirectories(dirPath);
            foreach(string subDir in subDirs)
                processFolder(subDir);
        }

        private static void loadTexture(string identifier, string path) {
            Texture2D texture = Texture2D.FromFile(Main.getGraphicsDevice(), path);
            textures[identifier] = texture;
        }

        public static bool has(string identifier) {
            return textures.ContainsKey(identifier);
        }

        public static Texture2D get(string identifier) {
            
            textures.TryGetValue(identifier, out var texture);

            if (texture == null) {
                return textures["null"];
            }
            return texture;
        }

        public static void exportTexture(Texture2D texture, string location, string identifier) {
            Stream stream = File.Create(FileUtil.addPathToIdentifier(location, identifier + ".png"));
            texture.SaveAsPng( stream, texture.Width, texture.Height );
            stream.Dispose();
        }
        
        public static void exportTexture(Texture2D texture, string absolutePath) {
            Stream stream = File.Create(absolutePath);
            texture.SaveAsPng( stream, texture.Width, texture.Height );
            stream.Dispose();
        }
    }
}