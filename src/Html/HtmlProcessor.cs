using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.Xna.Framework;

namespace PixelArt {

	
	[SuppressMessage("ReSharper", "StringIndexOfIsCultureSpecific.1")]
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
			

			string headerContent = carrotDict[mainPair.openIndex].contents(node);

			string output = "";
			
			processHeader: {
				
				int firstSpace = headerContent.IndexOf(" ");
				string tag = (firstSpace == -1) ? headerContent : headerContent.Substring(0, firstSpace);
				string data = (firstSpace == -1) ? null : headerContent.Substring(firstSpace + 1).Trim();

				char firstTagLetter = tag.ToCharArray()[0];
				output = (firstTagLetter >= 'A' && firstTagLetter <= 'Z') ? $"Create{tag}(" : "newNode(";
				
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
								bool typeless = false;
								if (sep != -1) { 
									string returnType = jsx.Substring(0, sep).Trim();

									if (new Regex("^[a-zA-z<>~]+$").IsMatch(returnType)) { 
										jsx = jsx.Substring(sep + 1).Trim();
										if (returnType.EndsWith("~")) {
											returnType = returnType.Substring(0, returnType.Length - 1);
											jsx = $"({returnType})({jsx})"; // auto-cast
										}

										jsx = $"(Func^^{returnType}^)(() =^ ({jsx}))";
									} else {
										typeless = true;
									}
								} else {
									typeless = true;
								}

								if (typeless) { 
									jsx = $"(Func^^object^)(() =^ ({jsx.Trim()}))";
								}

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
			} else {
				string text = mainPair.htmlContents(node).Trim();
				
				if (text.Contains("{")) {
					output += $"textContent: (Func<string>)(()=^ $'{text}')";
				} else { 
					output += $"textContent: '{text}'";
				}
			}

			return output + ")";
		}

		public static string pairToNodeStr(string str, DelimPair htmlPair, Dictionary<int, DelimPair> carrotDict) {
			return str.Substring(htmlPair.openIndex,
				(carrotDict[htmlPair.closeIndex].closeIndex + 1) - htmlPair.openIndex);
		}

		public static string defineComponent(string code) {

			string before = "const ";
			string tagEtc = code.Substring(code.IndexOf(before) + before.Length);
			string tag = tagEtc.sub(0, tagEtc.minValidIndex(" ", "="));

			before = "return";
			string afterReturn = code.Substring(code.IndexOf(before) + before.Length).Trim();
			DelimPair pair = DelimPair.genPairDict(afterReturn, "(", ")")[0];
			string returnContents = pair.contents(afterReturn).Trim();

			string stateStr = "";
			state: { 
				string stateDefinitions = code.sub(code.IndexOf("{") + 1, code.IndexOf(before));
				string[] lines = stateDefinitions.Split(new [] { '\r', '\n' });
				foreach (string str in lines) {
					string line = str.Trim();
					const string stateOpen = "useState(";
					int stateIndex = line.IndexOf(stateOpen);
					
					if (stateIndex != -1) {
						string type = line.Substring(0, line.IndexOf(" "));
						string varNameContents = DelimPair.searchPairs(line, "[", "]", line.IndexOf("[")).contents(line);
						string[] varNames = varNameContents.Split(",").Select(s => s.Trim()).ToArray();
						
						string initValue = DelimPair.searchPairs(line, "(", ")", line.IndexOf("(")).contents(line);

						stateStr += $@"
{type} {varNames[0]} = {initValue};
Action<{type}> {varNames[1]} = (val) => {varNames[0]} = val;
";
					}
				}
			}
			return @$"
public HtmlNode Create{tag}(string tag, Dictionary<string, object> props = null, string textContent = null, HtmlNode[] children = null) {{
	{stateStr}
	return {stringifyNode(returnContents)};
}}
";
		}

		public static string applyMacros(string str, Dictionary<string, string> macros) { // TODO: allow recursive macros!
			Logger.log("hi");
			
			foreach (string macroID in macros.Keys) {
				if (macroID.Contains("(")) {

					int openInd = macroID.IndexOf("(");
					string paramString = macroID.Substring(openInd + 1, macroID.LastIndexOf(")") - openInd - 1);

					var paramNames = paramString.Split(",").Select(str => str.Trim()).ToArray();

					string find = "@" + macroID.Substring(0, openInd) + "(";
					int currIndex = str.IndexOf(find);
					while (currIndex != -1) {
						var pair = DelimPair.genPairDict(str, "(", ")")[currIndex+macroID.Substring(0, openInd).Length+1];

						string content = pair.contents(str);
						var valStrs = content.Split(",").Select(str => str.Trim()).ToArray();
								
						Logger.log(valStrs.Length);
								
						string macroStr = macros[macroID];
						for (int i = 0; i < paramNames.Length; i++) {
							Logger.log("hello",paramNames[i], valStrs[i]);
							macroStr = macroStr.Replace($"$${paramNames[i]}", valStrs[i]);
						}

						str = str.Substring(0, currIndex) + macroStr + str.Substring(pair.closeIndex + 1);

						currIndex = str.IndexOf(find);
					}
							
				} else {
					str = str.Replace($"@{macroID}", macros[macroID]);
				}
			}

			return str;
		}

		public static async Task<HtmlNode> genHTML(string code, StatePack pack, Dictionary<string, string> macros = null, string[] components = null) {

			if (macros != null) code = applyMacros(code, macros);
			
			removeOpenClosed: {
				while (code.Contains("/>")) {
					int endIndex = code.IndexOf("/>");
					DelimPair pair = DelimPair.genPairDict(code, "<", ">")[endIndex + 1];
					int startIndex = pair.openIndex;

					string str = pair.whole(code);
					string tag = (str.Contains(" ")) ? str.sub(1, str.IndexOf(" ")) :  str.sub(1, str.IndexOf("/"));

					str = str.Substring(0, str.Length - 2) + $"></{tag}>";

					code = code.Substring(0, startIndex) + str + code.Substring(endIndex + 2);
				}
			}

			Logger.log("OUTPUT HTML===============\n\n" + code);


			code = "HtmlNode node = " + stringifyNode(code) + ";";
			code += "\nsetupNode(node);";
			code += "\nreturn node;";

			string preHTML = @"
using PixelArt;
using Microsoft.Xna.Framework;

int a = 5;
Func<int> test = () => a;
Action<int> setTest = (i) => a = i;



";
			if (components != null) { 
				foreach (string component in components) {
					preHTML += defineComponent((macros == null) ? component : applyMacros(component, macros));
				}
			}

			code = preHTML + code;

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