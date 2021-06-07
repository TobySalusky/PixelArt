using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

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

		public static HtmlNode[] nodeArr(params HtmlNode[] nodes) {
			return nodes;
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

				vars[name] = obj;
				types[name] = type;
				
				Logger.log($"{name}({type}): {obj}");
			}
		}

		/*CACHE_START*/
		public static class CacheData {

			public static string[] CachedInput() {
				return new string[]{ @"
<div flexDirection='row' dimens='100%' backgroundColor='black' alignX='flexEnd' alignY='center'>
    <div dimens={200} backgroundColor='red'/>
    <Test/>
</div>
", @"
const Test = () => {
    
    bool [val, setVal] = useState(false);


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
                    </div>}
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

	___node = newNode("div", props: new Dictionary<string, object> {["onTick"]=((Action)(()=>{
            if (val != ((int)timePassed() % 2 == 0)) {
                setVal(!val);    
            }
        }))}, childrenFunc: (Func<HtmlNode[]>) (() => nodeArr(((val) ? 
                    null :
                    newNode("div", props: new Dictionary<string, object> {["align"]="center"}, children: nodeArr(newNode("div", props: new Dictionary<string, object> {["backgroundColor"]="yellow", ["dimens"]=(150)}, textContent: ""), newNode("div", props: new Dictionary<string, object> {["backgroundColor"]="lightblue", ["dimens"]=(75)}, textContent: "")))))));
	return ___node;
}
HtmlNode node = newNode("div", props: new Dictionary<string, object> {["flexDirection"]="row", ["dimens"]="100%", ["backgroundColor"]="black", ["alignX"]="flexEnd", ["alignY"]="center"}, children: nodeArr(newNode("div", props: new Dictionary<string, object> {["dimens"]=(200), ["backgroundColor"]="red"}, textContent: ""), CreateTest("Test", textContent: "")));
setupNode(node);
return node;
			}
		}
/*CACHE_END*/
	}
}