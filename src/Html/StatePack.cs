using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Microsoft.Xna.Framework;

namespace PixelArt {
	public class StatePack {

		public static Dictionary<string, object> ___vars;
		public static Dictionary<string, string> ___types;

		public static Func<float, float> sin = (rad) => (float) Math.Sin(rad);
		public static Func<float, float> cos = (rad) => (float) Math.Cos(rad);
		public static Func<float> timePassed = () => Main.timePassed;
		public static Func<float> deltaTime = () => Main.currentDeltaTime;

		public static float random(float max = 1F) {
			return Util.random(max);
		}

		public static HtmlNode newNode(string tag, Dictionary<string, object> props = null, object textContent = null, 
			HtmlNode[] children = null, Func<HtmlNode[]> childrenFunc = null) { 
			
			return new HtmlNode(tag, props, textContent, children, childrenFunc);
		}

		public static HtmlNode[] nodeArr(params object[] objs) {

			List<HtmlNode> nodes = new List<HtmlNode>();
			
			foreach (var elem in objs) {
				switch (elem) {
					case HtmlNode node:
						nodes.Add(node);
						break;
					case IEnumerable<HtmlNode> nodeArr:
						nodes.AddRange(nodeArr);
						break;
				}
			}
			return nodes.ToArray();
		}
		
		public static HtmlNode[] nodeArr(params HtmlNode[] nodes) {
			return nodes;
		}

		public static int[] nStream(int n) { 
			int[] arr = new int[n];

			for (int i = 0; i < n; i++) {
				arr[i] = i;
			}

			return arr;
		}

		public static void setupNode(object node) {
			HtmlNode htmlNode = ((HtmlNode) node);
			htmlNode.topDownInit();
			htmlNode.bottomUpInit();
			htmlNode.layoutDown();
		}

		public StatePack(params object[] varList) {
			___vars = new Dictionary<string, object>();
			___types = new Dictionary<string, string>();
			for (int i = 0; i < varList.Length; i += 2) {
				object obj = varList[i + 1];
				
				string name = (string) varList[i];
				string type = obj.GetType().ToString();

				type = type.Replace("]", ">"); // for functions and such
				type = Regex.Replace(type, @"`[0-9]*\[", "<");
				type = type.Replace("[>", "[]");

				___vars[name] = obj;
				___types[name] = type;
				
				Logger.log($"{name}({type}): {obj}");
			}
		}

		public static string StatePackAbsolutePath() {
			return TraceFilePath();
		}

		private static string TraceFilePath([CallerFilePath] string sourceFilePath = "")
		{
			return sourceFilePath;
		}

		/*CACHE_START*/
		public static class CacheData {

			public static string[] CachedInput() {
				return new string[]{ @"
<div class='back' flexDirection='row' dimens='100%' alignX='center' alignY='flexStart'>

    <div class='container'>
        {arr['jeffery', 'jim', 'bob'].map(str => 
            <div class='entry'>{str}</div>
        )}
    </div>
</div>
", @"
const Test = () => {
    

    return (
        <div></div>
    );
}
" };
			}

			public static HtmlNode CachedNode() {
				/*IMPORTS_DONE*/

HtmlNode CreateTest(string tag, Dictionary<string, object> props = null, string textContent = null, HtmlNode[] children = null) {
	HtmlNode ___node = null;
	
	___node = newNode("div", textContent: "");
	return ___node;
}
HtmlNode node = newNode("div", props: new Dictionary<string, object> {["class"]="back", ["flexDirection"]="row", ["dimens"]="100%", ["alignX"]="center", ["alignY"]="flexStart"}, children: nodeArr(newNode("div", props: new Dictionary<string, object> {["class"]="container"}, childrenFunc: (Func<HtmlNode[]>) (() => nodeArr(((new []{"jeffery", "jim", "bob"}).Select(str => 
            newNode("div", props: new Dictionary<string, object> {["class"]="entry"}, textContent: (Func<string>)(()=> ""+(str)+""))
        ).ToArray()))))));
setupNode(node);
return node;
			}
		}
/*CACHE_END*/
	}
}