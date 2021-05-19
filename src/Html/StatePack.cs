using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace PixelArt {
	public class StatePack {

		public Dictionary<string, object> vars;
		public Dictionary<string, string> types;

		public Func<float, float> sin = (rad) => (float) Math.Sin(rad);
		public Func<float, float> cos = (rad) => (float) Math.Cos(rad);
		public Func<float> timePassed = () => Main.timePassed;
		public Func<float> deltaTime = () => Main.currentDeltaTime;

		public static HtmlNode newNode(string tag, Dictionary<string, object> props = null, string textContent = null, HtmlNode[] children = null) { 
			return new HtmlNode(tag, props, textContent, children);
		}

		public static HtmlNode[] nodeArr(params HtmlNode[] nodes) {
			return nodes;
		}

		public static void setupNode(object node) {
			HtmlNode htmlNode = ((HtmlNode) node);
			htmlNode.topDownInit();
			htmlNode.layoutDown();
		}

		public StatePack(params object[] varList) {
			vars = new Dictionary<string, object>();
			types = new Dictionary<string, string>();
			for (int i = 0; i < varList.Length; i += 2) {
				object obj = varList[i + 1];
				
				string name = (string) varList[i];
				string type = obj.GetType().ToString();

				type = type.Replace("]", ">"); // for functions and such
				type = Regex.Replace(type, @"`[0-9]*\[", "<");

				vars[name] = obj;
				types[name] = type;
				
				Logger.log($"{name}({type}): {obj}");
			}
		}
	}

}