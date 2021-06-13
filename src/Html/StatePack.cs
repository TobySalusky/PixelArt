using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace PixelArt {
	public class StatePack {

		public static Dictionary<string, object> vars;
		public static Dictionary<string, string> types;

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
			vars = new Dictionary<string, object>();
			types = new Dictionary<string, string>();
			for (int i = 0; i < varList.Length; i += 2) {
				object obj = varList[i + 1];
				
				string name = (string) varList[i];
				string type = obj.GetType().ToString();

				type = type.Replace("]", ">"); // for functions and such
				type = Regex.Replace(type, @"`[0-9]*\[", "<");
				type = type.Replace("[>", "[]");

				vars[name] = obj;
				types[name] = type;
				
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
<div flexDirection='row' dimens='100%' backgroundColor='black' align='center'>

    {arr[10, 20, 50, 100, 200, 100, 50, 20, 10].map(n =^ <div flexDirection='row' align='center'>
        {nStream(n).map(i =^
            <div backgroundColor='red' width={1} height={i} />
        )}
        
        {nStream(n).map(i =^
            <div backgroundColor='red' width={1} height={n-i} />
        )}
    </div>)}
    
</div>
", @"
const Test = () => {
    
    bool [val, setVal] = useState(false);

    string[] colorArr = arr['red', 'orange', 'yellow', 'green', 'blue', 'purple'];

    return (
        <div onTick={()=^{
            if (val != ((int)@t % 2 == 0)) {
                setVal(!val);    
            }
        }}>
            {(val) ? 
                    null :
                    <div align='center'>
                        <div backgroundColor='yellow' dimens={150} />
                        <div backgroundColor='lightblue' dimens={75} />
                    </div>
            }
            
            {
                arr[1, 2].map((color, i) =^ <div backgroundColor='yellow' dimens={150 * i} />),
                arr[1, 2].map((color, i) =^ <div backgroundColor='yellow' dimens={150 * i} />)
            }
        </div>
    );
}
" };
			}

			public static HtmlNode CachedNode() {
				/*IMPORTS_DONE*/

HtmlNode CreateTest(string tag, Dictionary<string, object> props = null, string textContent = null, HtmlNode[] children = null) {
	HtmlNode ___node = null;
	
bool val = false;
Action<bool> setVal = (___val) => {
	val = ___val;
	___node.stateChangeDown();
};

string[] colorArr = (new []{"red", "orange", "yellow", "green", "blue", "purple"});
	___node = newNode("div", props: new Dictionary<string, object> {["onTick"]=((Action)(()=>{
            if (val != ((int)timePassed() % 2 == 0)) {
                setVal(!val);    
            }
        }))}, childrenFunc: (Func<HtmlNode[]>) (() => nodeArr(((val) ? 
                    null :
                    newNode("div", props: new Dictionary<string, object> {["align"]="center"}, children: nodeArr(newNode("div", props: new Dictionary<string, object> {["backgroundColor"]="yellow", ["dimens"]=(150)}, textContent: ""), newNode("div", props: new Dictionary<string, object> {["backgroundColor"]="lightblue", ["dimens"]=(75)}, textContent: "")))
            ), (
                (new []{1, 2}).Select((color, i) => newNode("div", props: new Dictionary<string, object> {["backgroundColor"]="yellow", ["dimens"]=(150 * i)}, textContent: "")).ToArray(),
                (new []{1, 2}).Select((color, i) => newNode("div", props: new Dictionary<string, object> {["backgroundColor"]="yellow", ["dimens"]=(150 * i)}, textContent: "")).ToArray()
            ))));
	return ___node;
}
HtmlNode node = newNode("div", props: new Dictionary<string, object> {["flexDirection"]="row", ["dimens"]="100%", ["backgroundColor"]="black", ["align"]="center"}, childrenFunc: (Func<HtmlNode[]>) (() => nodeArr(((new []{10, 20, 50, 100, 200, 100, 50, 20, 10}).Select(n => newNode("div", props: new Dictionary<string, object> {["flexDirection"]="row", ["align"]="center"}, childrenFunc: (Func<HtmlNode[]>) (() => nodeArr((nStream(n).Select(i =>
            newNode("div", props: new Dictionary<string, object> {["backgroundColor"]="red", ["width"]=(1), ["height"]=(i)}, textContent: "")
        ).ToArray()), (nStream(n).Select(i =>
            newNode("div", props: new Dictionary<string, object> {["backgroundColor"]="red", ["width"]=(1), ["height"]=(n-i)}, textContent: "")
        ).ToArray()))))).ToArray()))));
setupNode(node);
return node;
			}
		}
/*CACHE_END*/
	}
}