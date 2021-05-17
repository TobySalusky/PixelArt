using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.Xna.Framework;

namespace PixelArt {

	
	public static class HtmlProcessor {

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
							string jsx = data.Substring(i + 1, fin - (i + 1)).Trim();

							if (currLabel.StartsWith("-")) { // dynamic value (auto generate Func)
								int sep = jsx.IndexOf(":");
								string returnType = jsx.Substring(0, sep);
								jsx = jsx.Substring(sep + 1).Trim();
								jsx = $"(Func^^{returnType}^)(() =^ ({jsx}))";
							} else if (Util.noSpaces(jsx).StartsWith("()=^")) {
								int sep = jsx.IndexOf("=^");
								jsx = jsx.Substring(sep + 2).Trim();
								jsx = $"(Action)(()=^{jsx})";
							}

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

		public static async Task<HtmlNode> genHTML(string code, StatePack pack, Dictionary<string, string> macros = null) {
			
			applyMacros: { // TODO: make function macros work when inner parenthesis are present!!!
				if (macros != null) {
					foreach (string macroID in macros.Keys) {
						if (macroID.Contains("(")) {

							int openInd = macroID.IndexOf("(");
							string paramString = macroID.Substring(openInd + 1, macroID.LastIndexOf(")") - openInd - 1);

							var paramNames = paramString.Split(",").Select(str => str.Trim()).ToArray();

							string find = "@" + macroID.Substring(0, openInd) + "(";
							int currIndex = code.IndexOf(find);
							while (currIndex != -1) {
								var pair = DelimPair.genPairDict(code, "(", ")")[currIndex+macroID.Substring(0, openInd).Length+1];

								string content = pair.contents(code);
								var valStrs = content.Split(",").Select(str => str.Trim()).ToArray();
								
								Logger.log(valStrs.Length);
								
								string macroStr = macros[macroID];
								for (int i = 0; i < paramNames.Length; i++) {
									Logger.log("hello",paramNames[i], valStrs[i]);
									macroStr = macroStr.Replace($"$${paramNames[i]}", valStrs[i]);
								}

								code = code.Substring(0, currIndex) + macroStr + code.Substring(pair.closeIndex + 1);

								currIndex = code.IndexOf(find);
							}
							
						} else {
							code = code.Replace($"@{macroID}", macros[macroID]);
						}
					}
				}
			}
			
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


			code = "HtmlNode node = " + stringifyNode(code) + ";";
			code += "\nsetupNode(node);";
			code += "\nreturn node;";

			code = "using PixelArt;\nusing Microsoft.Xna.Framework;\n" + code;

			foreach (string key in pack.vars.Keys) {
				code = code.Replace($"${key}", $"(({pack.types[key]})vars[\"{key}\"])");
			}
			code = code.Replace("'", "\"");
			code = code.Replace("^^", "<");
			code = code.Replace("^", ">");
			
			Logger.log("OUTPUT C#===============\n\n" + code);

			object htmlObj = await CSharpScript.EvaluateAsync(code, ScriptOptions.Default.WithImports("System", "System.Collections.Generic").AddReferences(
				typeof(HtmlNode).Assembly
				), pack);
			
			return (HtmlNode) htmlObj;
		}
	}
}