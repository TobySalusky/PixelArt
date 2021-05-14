using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using SharpDX.Direct3D11;

namespace PixelArt {

	
	public static class HtmlProcessor {
		
		public static async void process(string code, StatePack pack) {
			foreach (string key in pack.vars.Keys) {
				code = code.Replace($"${key}", $"(({pack.types[key]})vars[\"{key}\"])");
			}

			//code = Regex.Replace(code, @"[^\\]'", "\"");
			//code = code.Replace("\\'", "'");
			code = code.Replace("'", "\"");
			
			/*var lines = code.Split('\r', '\n');
			code = "";
			foreach (string line in lines) {
				string newLine = line.Trim();
			}*/
			
			Logger.log(code);
			var hi = await CSharpScript.EvaluateAsync(code, ScriptOptions.Default.WithImports("System"), pack);
			
			Logger.log(hi);
		}

		public static string stringifyNode(string node) {
			node = node.Trim();
			
			var htmlPairs = DelimPair.genPairs(node, "<", "</");
			var carrotDict = DelimPair.genPairDict(node, "<", ">");

			
			DelimPair mainPair = htmlPairs[htmlPairs.Count - 1];
			
			List<string> childNodes = new List<string>();
			foreach (var pair in htmlPairs) {
				if (pair.nestCount == 1) {
					string subNode = pairToNodeStr(node, pair, carrotDict);
					childNodes.Add(subNode);
				}
			}

			string output = "newNode(";

			string headerContent = carrotDict[mainPair.openIndex].contents(node);
			
			processHeader: {
				
				int firstSpace = headerContent.IndexOf(" ");
				string tag = (firstSpace == -1) ? headerContent : headerContent.Substring(0, firstSpace);
				string data = (firstSpace == -1) ? null : headerContent.Substring(firstSpace + 1).Trim();

				output += $"'{tag}', ";
				
				processData: {
					if (data == null) goto finishData;

					var quoteDict = DelimPair.genPairDict(data, "'", "'"); // TODO: quote pairing will need to be more complex
					var bracketDict = DelimPair.genPairDict(data, "{", "}");

					int lastFin = 0;
					string currLabel = "InvalidProp";
					Dictionary<string, string> props = new Dictionary<string, string>();

					var chars = data.ToCharArray();
					for (int i = 0; i < data.Length; i++) {
						if (chars[i] == '=') {
							currLabel = data.Substring(lastFin, i - lastFin).Trim();
						}

						if (chars[i] == '\'') {
							int fin = quoteDict[i].closeIndex;
							string str = data.Substring(i + 1, fin - (i + 1));
							props[currLabel] = $"'{str}'";
							lastFin = fin + 1;
							i = fin;
						}

						if (chars[i] == '{') {
							int fin = bracketDict[i].closeIndex;
							string jsx = data.Substring(i + 1, fin - (i + 1));
							props[currLabel] = $"({jsx})";
							lastFin = fin + 1;
							i = fin;
						}
					}

					string propStr = "props: new Dictionary<string, object> {";

					var keys = props.Keys;
					string startWith = "";
					foreach (string key in keys) {
						propStr += $"{startWith}['{key}']={props[key]}";
						startWith = ", ";
					}

					propStr += "}, ";
					output += propStr;

					
				} finishData: { }
			}

			if (childNodes.Count > 0) {
				output += "children: nodeArr(";
				for (int i = 0; i < childNodes.Count; i++) {
					output += stringifyNode(childNodes[i]) + ((i + 1 < childNodes.Count) ? ", " : "");
				}

				output += ")";
			}
			else {
				output += $"textContent: '{mainPair.htmlContents(node).Trim()}'";
			}

			return output + ")";
		}

		public static string pairToNodeStr(string str, DelimPair htmlPair, Dictionary<int, DelimPair> carrotDict) {
			return str.Substring(htmlPair.openIndex,
				(carrotDict[htmlPair.closeIndex].closeIndex + 1) - htmlPair.openIndex);
		}

		public static async Task<HtmlNode> genHTML(string code, StatePack pack) {

			removeOpenClosed: {
				string newCode = "";
				int lastIndex = 0;
				var matches = Regex.Matches(code, @"\<([a-zA-Z0-9]+)(\s([a-zA-Z0-9${}:=\[\]\'\s]+))?(/?)\>");

				foreach (Match match in matches) {

					string str = $"<{match.Groups[1].Value}{(match.Groups[3].Value == "" ? "" : " " + match.Groups[3].Value.Trim())}>";
					if (match.Groups[4].Value == "/") str += $"</{match.Groups[1].Value}>";

					newCode += code.Substring(lastIndex, match.Index - lastIndex) + str;
					lastIndex = match.Index + match.Length;
				}

				newCode += code.Substring(lastIndex);
			
				code = newCode;
			}
			Logger.log("OUTPUT HTML===============\n\n" + code);


			code = "return " + stringifyNode(code) + ";";


			foreach (string key in pack.vars.Keys) {
				code = code.Replace($"${key}", $"(({pack.types[key]})vars[\"{key}\"])");
			}
			code = code.Replace("'", "\"");
			
			Logger.log("OUTPUT C#===============\n\n" + code);
			
			object htmlObj = await CSharpScript.EvaluateAsync(code, ScriptOptions.Default.WithImports("System", "System.Collections.Generic"), pack);
			/*
			Logger.log("\n");
			Logger.log(hi);*/

			return (HtmlNode) htmlObj;
		}

		public static void test() {

			const string html = @"
<div>
	<div>
		<p width={$a*3} height={100} title='test'>Hi</p>
		<p>Hi2</p>
		<p/>
		<p></p>
	</div>

	<h1>
		what?
	</h1>
</div>
";
			const int a = 107;
			
			genHTML(html, new StatePack("a", a));
		}
	}
}