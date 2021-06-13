﻿using System;
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

			
			DelimPair mainPair = htmlPairs[^1];
			
			List<string> childNodes = new List<string>();
			List<int> childNodesIndices = new List<int>();
			string mainInnerContents = "";
			int mainStartIndex = 0;
			foreach (var pair in htmlPairs) {
				if (pair.nestCount == 1) {
					string subNode = pairToNodeStr(node, pair, carrotDict);
					childNodes.Add(subNode);
					childNodesIndices.Add(pair.openIndex);
				} else if (pair.nestCount == 0) {
					mainStartIndex = carrotDict[pair.openIndex].closeIndex + 1;
					mainInnerContents = node.sub(mainStartIndex, pair.closeIndex);
				}
			}
			childNodesIndices = childNodesIndices.Select(i => i - mainStartIndex).ToList();
			Dictionary<int, string> childNodesIndicesDict = new Dictionary<int, string>();
			for (int i = 0; i < childNodes.Count; i++) {
				childNodesIndicesDict[childNodesIndices[i]] = childNodes[i];
			}

			string headerContent = carrotDict[mainPair.openIndex].contents(node);

			string output = "";
			
			processHeader: {
				
				int firstSpace = headerContent.IndexOf(" ");
				string tag = (firstSpace == -1) ? headerContent : headerContent.Substring(0, firstSpace);
				string data = (firstSpace == -1) ? null : headerContent.Substring(firstSpace + 1).Trim();

				Logger.log("efefeefef", tag);
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

				bool staticChildren = true;

				var bracketPairs = DelimPair.genPairs(mainInnerContents, "{", "}");
				List<DelimPair> jsxPairs = new List<DelimPair>();

				foreach (DelimPair bracketPair in bracketPairs) {
					var dict = mainInnerContents.nestAmountsRange((bracketPair.openIndex, bracketPair.closeIndex), 
						DelimPair.Carrots, DelimPair.CurlyBrackets, DelimPair.SquareBrackets, DelimPair.Parens, ("<", "</"));
					if (DelimPair.nestsAll(dict, 0)) {
						jsxPairs.Add(bracketPair);
						staticChildren = false;
					}
				}

				if (staticChildren) { 
					output += "children: nodeArr(";
					for (int i = 0; i < childNodes.Count; i++) {
						output += stringifyNode(childNodes[i]) + ((i + 1 < childNodes.Count) ? ", " : "");
					}

					output += ")";
				} else { // TODO: don't regenerate non-jsx segments (define them above creation code)

					int elemCount = 0;
					output += "childrenFunc: (Func^^HtmlNode[]^) (() =^ nodeArr(";

					int i = 0;
					while (i < mainInnerContents.Length) {
						var chars = mainInnerContents.ToCharArray();

						char c = chars[i];
						if (c == '<' || c == '{') {
							if (elemCount > 0) output += ", ";

							if (c == '<') {
								string thisChildNode = childNodesIndicesDict[i];
								output += stringifyNode(thisChildNode);
								i += thisChildNode.Length;
							} else {
								DelimPair bracketPair = mainInnerContents.searchPairs("{", "}", i);

								string jsxChild = bracketPair.contents(mainInnerContents);

								for (int j = childNodes.Count - 1; j >= 0; j--) {
									int childIndex = childNodesIndices[j];
									string childNodeStr = childNodesIndicesDict[childIndex];
									if (childIndex > bracketPair.openIndex && childIndex < bracketPair.closeIndex) {

										int childNewIndex = childIndex - (i + 1);
										jsxChild = jsxChild.Substring(0, childNewIndex) + stringifyNode(childNodeStr) 
										                                                + jsxChild.Substring(childNewIndex + childNodeStr.Length);
									}
								}

								output += $"({jsxChild})";

								i = bracketPair.closeIndex + 1;
							}

							elemCount++;
						} else {
							i++;
						}
					}
					
					output += "))";
				}

				object arr = (Func<HtmlNode[]>) (() => StatePack.nodeArr(null, null));

			} else {
				string text = mainPair.htmlContents(node).Trim();
				
				if (text.Contains("{")) {
					string textExpression = "";
					while (text.Contains("{")) {
						int index = text.IndexOf("{");
						DelimPair pair = text.searchPairs("{", "}", index);
						if (textExpression != "") textExpression += "+";
						textExpression += $"'{text.beforePair(pair)}'+({pair.contents(text)})";
						text = text.afterPair(pair);
					}

					textExpression += $"+'{text}'";

					string dynamicText = $"(Func<string>)(()=^ {textExpression})";
					output += $"textContent: {dynamicText}";
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

			before = "return"; // TODO: check nesting b/c returns in functions are possible
			string afterReturn = code.Substring(code.IndexOf(before) + before.Length).Trim();
			DelimPair pair = DelimPair.genPairDict(afterReturn, "(", ")")[0];
			string returnContents = pair.contents(afterReturn).Trim();
			returnContents = removeOpenClosed(returnContents);

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
						string afterType = line.Substring(line.IndexOf(" ") + 1);
						string varNameContents = afterType.searchPairs("[", "]", afterType.IndexOf("[")).contents(afterType);
						string[] varNames = varNameContents.Split(",").Select(s => s.Trim()).ToArray();
						
						string initValue = DelimPair.searchPairs(line, "(", ")", line.IndexOf("(")).contents(line);

						// TODO: use comparison in state action to check if there was a change
						stateStr += $@"
{type} {varNames[0]} = {initValue};
Action<{type}> {varNames[1]} = (___val) => {{
	{varNames[0]} = ___val;
	___node.stateChangeDown();
}};
";
					} else if (line != "") {
						if (line.StartsWith("var")) { // multi-inline var declarations
							var commas = line.allIndices(",").Where((index) => DelimPair.allNestOf(0,
								line.nestAmountsLen(index, 1,
									DelimPair.Parens, DelimPair.CurlyBrackets, DelimPair.SquareBrackets,
									DelimPair.Quotes, DelimPair.SingleQuotes, DelimPair.Carrots)));

							if (commas.Any()) {
								var declarations = line.splitWithout(commas);
								line = "";
								foreach (string declaration in declarations) {
									string varDeclaration = declaration.Trim();
									if (!varDeclaration.StartsWith("var ")) varDeclaration = "var " + varDeclaration;
									if (!varDeclaration.EndsWith(";")) varDeclaration += ";";
									line += varDeclaration + "\n";
								}
							}
						}

						stateStr += $"\n{line}";
					}
				}
			}
			
			var namedArrayElements = new Dictionary<string, string>();
			generateNamedArrayElements: {
				const string namedElRegex = @"}\s*=";


				var match = Regex.Match(stateStr, namedElRegex);
				while (match.Success) {

					DelimPair bracketPair = stateStr.searchPairs("{", "}", match.Index);

					string content = bracketPair.contents(stateStr);
					string[] elementNames = content.Split(",");

					int nameEnd = -1;
					string arrName = "";
					var chars = stateStr.ToCharArray();
					for (int i = bracketPair.openIndex - 1; i >= 0; i--) {
						if (nameEnd == -1) {
							if (chars[i] != ' ') nameEnd = i;
						}
						else if (chars[i] == ' ') {
							arrName = stateStr.sub(i + 1, nameEnd + 1);
							break;
						}
					}

					for (int i = 0; i < elementNames.Length; i++) {
						namedArrayElements[$"{arrName}.{elementNames[i].Trim()}"] = $"{arrName}[{i}]";
					}

					stateStr = bracketPair.removeFrom(stateStr);
					match = Regex.Match(stateStr, namedElRegex);
				}
			}

			string output = @$"
HtmlNode Create{tag}(string tag, Dictionary<string, object> props = null, string textContent = null, HtmlNode[] children = null) {{
	HtmlNode ___node = null;
	{stateStr}
	___node = {stringifyNode(returnContents)};
	return ___node;
}}
";
			
			
			
			applyNamedArrayElements: {
				foreach (string key in namedArrayElements.Keys) {
					output = output.Replace(key, namedArrayElements[key]);
				}
			}

			return output;
		}

		public static string applyMacros(string str, Dictionary<string, string> macros) { // TODO: allow recursive macros!
			
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

		public static string removeOpenClosed(string code) {
			while (code.Contains("/>")) {
				int endIndex = code.IndexOf("/>");
				DelimPair pair = DelimPair.genPairDict(code, "<", ">")[endIndex + 1];
				int startIndex = pair.openIndex;

				string str = pair.whole(code);
				string tag = (str.Contains(" ")) ? str.sub(1, str.IndexOf(" ")) :  str.sub(1, str.IndexOf("/"));

				str = str.Substring(0, str.Length - 2) + $"></{tag}>";

				code = code.Substring(0, startIndex) + str + code.Substring(endIndex + 2);
			}

			return code;
		}

		public static async Task<HtmlNode> genHTML(string code, StatePack pack, Dictionary<string, string> macros = null, string[] components = null) {

			// cache ====
			string inputString = code;
			string[] inputArr = new List<string> {inputString}.Concat(components ?? new string[]{}).ToArray();

			if (HtmlSettings.useCache && HtmlCache.IsCached(inputArr)) {
				Logger.log("Using Cached HTML");
				return HtmlCache.UseCache();
			}
			
			// code generation
			code = code.Replace("=>", "=^");

			if (macros != null) code = applyMacros(code, macros);

			code = removeOpenClosed(code);

			Logger.log("OUTPUT HTML===============\n\n" + code);


			code = "HtmlNode node = " + stringifyNode(code) + ";";
			code += "\nsetupNode(node);";
			code += "\nreturn node;";

			string preHTML = @"
using System.Linq;
using PixelArt;
using Microsoft.Xna.Framework;
/*IMPORTS_DONE*/
";
			if (components != null) { 
				foreach (string component in components) {
					preHTML += defineComponent((macros == null) ? component : applyMacros(component, macros));
				}
			}

			code = preHTML + code;

			foreach (string key in StatePack.vars.Keys) {
				code = code.Replace($"${key}", $"(({StatePack.types[key]})vars[\"{key}\"])");
			}

			mapToSelect: {
				while (code.Contains(".map(")) {
					int index = code.IndexOf(".map(");
					DelimPair pair = code.searchPairs("(", ")", index + 4);

					code = code.Substring(0, index) + $".Select({pair.contents(code)}).ToArray()" + code.Substring(pair.closeIndex + 1);
				}
			}
			code = code.Replace("'", "\"");
			code = code.Replace("^^", "<");
			code = code.Replace("^", ">");
			
			
			inlineArray: {

				int minIndex() => code.minValidIndex("arr(", "arr[");

				while (minIndex() != -1) {
					int index = minIndex();

					string type = "";
					int bracketIndex = index + 3;
					if (code.Substring(bracketIndex, 1) == "(") {
						DelimPair pair = code.searchPairs("(", ")", bracketIndex);
						bracketIndex = pair.closeIndex + 1;
						type = pair.contents(code);
					}

					DelimPair contentPair = code.searchPairs("[", "]", bracketIndex);
					string arrContents = contentPair.contents(code);
					code = code.Substring(0, index) + $"(new {type}[]{{{arrContents}}})" + code.Substring(contentPair.closeIndex + 1);
				}
			}


			Logger.log("OUTPUT C#===============\n\n" + code);

			

			object htmlObj = await CSharpScript.EvaluateAsync(code, ScriptOptions.Default.WithImports("System", "System.Collections.Generic").AddReferences(
				typeof(HtmlNode).Assembly
				), pack);
			
			HtmlNode returnNode = (HtmlNode) htmlObj;
			
			if (HtmlSettings.generateCache) { // Only caches when node generation is successful
				string toCache = code.Substring(code.IndexOf("/*IMPORTS_DONE*/"));
				HtmlCache.CacheHtml(inputArr, toCache);
			}
			
			return returnNode;
		}
	}
}