﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PixelArt {
    public static class Util {

        private static Random rand = new Random();

        public static Point min(Point p1, Point p2) {
            return new Point(Math.Min(p1.X, p2.X), Math.Min(p1.Y, p2.Y));
        }

        public static Point max(Point p1, Point p2) {
            return new Point(Math.Max(p1.X, p2.X), Math.Max(p1.Y, p2.Y));
        }
        
        public static float nearestAngle(float angle, float targetAngle) {
            float diff = targetAngle - angle;
            if (Math.Abs(diff) < Maths.PI) {
                return angle;
            }
            
            if (diff > 0) {
                return angle + Maths.twoPI;
            }

            return angle - Maths.twoPI;
        }

        public static Vector2 setMag(Vector2 vec, float mag) {
            return Vector2.Normalize(vec) * mag;
        }


        public static int randomIntPN() {
            return randInt(0, 2) * 2 - 1;
        }

        public static bool between(Vector2 val, Vector2 tl, Vector2 br) {
            return val.X > tl.X && val.X < br.X && val.Y > tl.Y && val.Y < br.Y;
        }

        // TODO: optimise, honestly seems super inefficient
        public static Vector2 randomInOut(Rectangle inside, Rectangle outside) { // 'outside' should be a subsection of 'inside'
            float vol = area(inside) - area(outside);

            Rectangle r1 = new Rectangle(inside.X, inside.Y, inside.Width, outside.Y - inside.Y);
            Rectangle r2 = new Rectangle(inside.X, inside.Y, outside.X - inside.X, inside.Y);
            Rectangle r3 = new Rectangle(outside.X + outside.Width, inside.Y, (inside.Right - outside.Right), inside.Y);
            Rectangle r4 = new Rectangle(inside.X, outside.Y + outside.Height, inside.Width, inside.Y + inside.Height - (outside.Y + outside.Height));

            float chance = random();
            float parVol = area(r1) / vol;
            if (chance <= parVol) return randomIn(r1);
            
            parVol += area(r2) / vol;
            if (chance <= parVol) return randomIn(r2);
            
            parVol += area(r3) / vol;
            if (chance <= parVol) return randomIn(r3);
            return randomIn(r4);
        }

        public static Vector2 randomIn(Rectangle inside) {
            return new Vector2(inside.X + random(inside.Width), inside.Y + random(inside.Height));
        }

        public static float area(Rectangle rect) {
            return rect.Width * rect.Height;
        }

        public static Vector2 rotate(Vector2 vec, float rotateBy) {
            return Util.polar(mag(vec), angle(vec) + rotateBy);
        }

        public static Rectangle center(Vector2 pos, Vector2 dimen) {
            return new Rectangle((int) (pos.X - dimen.X / 2), (int) (pos.Y - dimen.Y / 2), (int)dimen.X, (int)dimen.Y);
        }
        
        public static Rectangle centerRounded(Vector2 pos, Vector2 dimen) {
            return new Rectangle((int) Math.Round(pos.X - dimen.X / 2), (int) Math.Round(pos.Y - dimen.Y / 2), (int)Math.Round(dimen.X), (int)Math.Round(dimen.Y));
        }

        public static Rectangle tl(Vector2 topLeft, Vector2 dimen) {
            return new Rectangle((int) (topLeft.X), (int) (topLeft.Y), (int)dimen.X, (int)dimen.Y);
        }

        public static float sinSmooth(float val, float max) {
            return (float) Math.Sin(val / max * Maths.halfPI);
        }

        public static float sinLerp(float valTime, float maxTime, float from, float to) {
            return from + sinSmooth(valTime, maxTime) * (to - from);
        }
        
        public static Vector2 sinLerp(float valTime, float maxTime, Vector2 from, Vector2 to) {
            return from + sinSmooth(valTime, maxTime) * (to - from);
        }
        
        public static Vector2 sinLerp(float val, Vector2 from, Vector2 to) {
            return from + sinSmooth(val, 1) * (to - from);
        }
        
        
        public static float revSinSmooth(float val, float max) {
            return (float) Math.Sin((1 - val / max) * Maths.halfPI);
        }

        public static float revSinLerp(float valTime, float maxTime, float from, float to) {
            return from + revSinSmooth(valTime, maxTime) * (to - from);
        }
        
        public static float modulus(float a, float b) { // floats??
            return (float) (a - b * Math.Floor(a / b));
        }

        public static int intMod(float a, float b) {
            return (int) (a - b * Math.Floor(a / b));
        }

        public static Vector2 polar(float mag, float angle) {
            return new Vector2((float) Math.Cos(angle) * mag, (float) Math.Sin(angle) * mag);
        }

        public static float angle(Vector2 vector) {
            return (float) Math.Atan2(vector.Y, vector.X);
        }
        
        public static float mag(Vector2 vector) {
            return Vector2.Distance(Vector2.Zero, vector);
        }

        public static float lessDiff(float val, float op1, float op2) {
            if (Math.Abs(val - op1) < Math.Abs(val - op2)) {
                return op1;
            }

            return op2;
        }
        

        public static string spacedName(string UnSpaced) {

            StringBuilder spaced = new StringBuilder();

            char[] arr = UnSpaced.ToCharArray();
            for (int i = 0; i < arr.Length; i++) {
                char c = arr[i];

                if (i > 0 && c >= 'A' && c <= 'Z') {
                    spaced.Append(" " + c);
                } else { 
                    spaced.Append(c.ToString());
                }
            }

            return spaced.ToString();
        }

        // FROM: https://stackoverflow.com/questions/8928464/for-an-object-can-i-get-all-its-subclasses-using-reflection-or-other-ways
        public static IEnumerable<Type> subClassesOf(Type super) {
            
            var subclasses =
                from assembly in AppDomain.CurrentDomain.GetAssemblies()
                from type in assembly.GetTypes()
                where type.IsSubclassOf(super)
                select type;
            return subclasses;
        }

        public static Vector2 textureVec(Texture2D texture) {
            return new Vector2(texture.Width, texture.Height);
        }

        public static int randInt(int startInc, int endExc) {
            return rand.Next(startInc, endExc);
        }
        
        public static int randInt(int endExc) {
            return rand.Next(0, endExc);
        }

        public static float random(float min, float max) {
            return (float) rand.NextDouble() * (max - min) + min;
        }

        public static float randomAngle() {
            return random(Maths.twoPI);
        }

        public static float random(float max) {
            return (float) rand.NextDouble() * max;
        }

        public static float random() { 
            return (float) rand.NextDouble();
        }

        public static float randomPN() {
            return random(-1, 1);
        }
        
        public static float randomPN(float maxPN) {
            return random(-maxPN, maxPN);
        }

        public static bool angleDir(float angle) { // true is facingLeft // TODO: WARNING: only works for default values given by atan2 (-pi to pi)
            float abs = Math.Abs(angle);
            return (abs > Maths.halfPI && abs < Maths.PI);
        }

        public static Vector2 toVec(Point point) { 
            return new Vector2(point.X, point.Y);
        }

        public static float heightToJumpPower(float jumpHeight, float gravity) {
            return (float) Math.Sqrt(jumpHeight * 2 * gravity);
        }

        public static Color[] colorArray(Texture2D texture) {
            var colorData = new Color[texture.Width * texture.Height];
            texture.GetData(colorData);

            return colorData;
        }

        public static bool chance(float chance) {
            return (rand.NextDouble() < chance);
        }

        public static Color randomColor(Texture2D texture) { // TODO:  WARNING:does not guarantee a non invisible color
            var arr = colorArray(texture);
            return arr[(int) (rand.NextDouble() * arr.Length)];
        }
        
        public static Color randomColor(Texture2D texture, Rectangle rect) { // TODO:  WARNING:does not guarantee a non invisible color
            var arr = colorArray(texture);
            int x = randInt(rect.Width)+ rect.X;
            int y = randInt(rect.Height)+ rect.Y;
            
            return arr[x + y * texture.Width];
        }

        public static bool isClassOrSub(Object obj, Type superClass) {
            return obj.GetType().IsSubclassOf(superClass) || obj.GetType() == superClass;
        }

        public static Rectangle useRatio(Vector2 dimen, Rectangle rect) {

            float ratio = dimen.Y / dimen.X;

            Vector2 newDimen;
            
            float height = ratio * rect.Width;
            if (height > rect.Height) {
                newDimen = new Vector2(rect.Height / ratio, rect.Height);
            } else {
                newDimen = new Vector2(rect.Width, ratio * rect.Width);
            }
            

            return center(middle(rect), newDimen);
        }

        public static Vector2 middle(Rectangle rect) {
            return new Vector2(rect.X + rect.Width / 2F, rect.Y + rect.Height / 2F);
        }

        // FROM: https://stackoverflow.com/questions/972307/how-to-loop-through-all-enum-values-in-c
        public static IEnumerable<T> GetValues<T>() {
            return Enum.GetValues(typeof(T)).Cast<T>();
        }
    }
}