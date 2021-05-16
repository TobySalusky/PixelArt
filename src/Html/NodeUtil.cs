﻿using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Xna.Framework;
using SharpDX.DXGI;

namespace PixelArt {
	public static class NodeUtil {

		public static readonly Dictionary<string, Color> colorDict = genColorDict();

		public static Dictionary<string, Color> genColorDict() {
			
			Dictionary<string, Color> dict = new Dictionary<string, Color>();
			Type colorType = typeof(Color);

			PropertyInfo[] fields = colorType.GetProperties(BindingFlags.Public | BindingFlags.Static);

			foreach (var field in fields) {
				if (field.PropertyType == colorType) {
					dict[field.Name.ToLower()] = (Color) field.GetValue(null);
				}
			}

			return dict;
		}

		public static Color strToColor(string str) {
			
			return colorDict[str.ToLower()];
		}


		public static int widthFromProp(object prop, HtmlNode parent) {

			if (prop is string str) {
				if (str.Substring(str.Length - 1) == "%") {
					int maxWidth = (parent == null) ? Main.screenWidth : parent.width;

					return (int) (float.Parse(str.Substring(0, str.Length - 1))/100F * maxWidth);
				}
				
				return int.Parse(str);
			}

			return (int) prop;
		}

		public static int heightFromProp(object prop, HtmlNode parent) {

			if (prop is string str) {
				if (str.Substring(str.Length - 1) == "%") {
					int maxHeight = (parent == null) ? Main.screenHeight : parent.height;

					return (int) (float.Parse(str.Substring(0, str.Length - 1))/100F * maxHeight);
				}

				return int.Parse(str);
			}

			return (int) prop;
		}
		
		public static Color colorFromProp(object prop) {

			if (prop is string str) {
				return strToColor(str);
			}

			return (Color) prop;
		}

		public static float percentAsFloat(string percent) {
			return float.Parse(percent.Substring(0, percent.Length - 1)) / 100F;
		}
	}
}