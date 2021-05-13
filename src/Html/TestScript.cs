using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;

namespace PixelArt {

	public class Person {
		public string name = "none";
		public int money = 0;
	}

	public class Package {

		public Dictionary<string, object> vars;
		public Dictionary<string, string> types;

		public HtmlNode newNode(string tag, string textContent = null, HtmlNode[] children = null) { 
			return new HtmlNode {tag=tag, textContent=textContent, children=children};
		}

		public HtmlNode[] nodeArr(params HtmlNode[] nodes) {
			return nodes;
		}

		public Package(params object[] varList) {
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

	public static class TestScript {
		
		public static async void process(string code, Package pack) {
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
		
		public static async void genHTML(string code, Package pack) {

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
			
			/*removeLines: { 
				string newCode = "";
				
				var lines = code.Split('\r', '\n');
				foreach (string line in lines) {
					newCode += line.Trim();
				}

				code = newCode;
			}*/
			
			Logger.log("OUTPUT HTML===============\n\n" + code);
			
			processOpen: {
				string newCode = "";
				int lastIndex = 0;
				var matches = Regex.Matches(code, @"\<([a-zA-Z0-9]+)(\s([a-zA-Z0-9${}:=\[\]\'\s]+))?\>");

				foreach (Match match in matches) {

					string str = $"newNode(\"{match.Groups[1].Value}\", children: nodeArr(";

					newCode += code.Substring(lastIndex, match.Index - lastIndex) + str;
					lastIndex = match.Index + match.Length;
				}

				newCode += code.Substring(lastIndex);
			
				code = newCode;
			}

			processClose: {
				string newCode = "";
				int lastIndex = 0;
				var matches = Regex.Matches(code, @"\<(/[a-zA-Z0-9]+)\>");

				foreach (Match match in matches) {

					const string str = ")),";

					newCode += code.Substring(lastIndex, match.Index - lastIndex) + str;
					lastIndex = match.Index + match.Length;
				}

				newCode += code.Substring(lastIndex);
			
				code = newCode;

				code = "return " + code.Substring(0, code.LastIndexOf(",")).Trim() + ";";
			}

			fixText: {
				string newCode = "";
				int lastIndex = 0;
				// TODO: fix, not working (use parser system)
				var matches = Regex.Matches(code, @"children: nodeArr\((.*?)\)");

				foreach (Match match in matches) {
					Logger.log(match.Value);
					

					string contents = match.Groups[2].Value;

					string str = contents.StartsWith("newNode(") ? match.Value : $"textContent: \"{contents}\"";

					newCode += code.Substring(lastIndex, match.Index - lastIndex) + str;
					lastIndex = match.Index + match.Length;
				}

				newCode += code.Substring(lastIndex);
				code = newCode;

				code = code.Replace(",))", "))"); // TODO: scuffed, will screw stuff later
			}
			

			foreach (string key in pack.vars.Keys) {
				code = code.Replace($"${key}", $"(({pack.types[key]})vars[\"{key}\"])");
			}
			code = code.Replace("'", "\"");

			//code = "return newNode(\"div\", children: nodeArr(newNode(\"p\", textContent: \"hello\")));";
			
			Logger.log("OUTPUT C#===============\n\n" + code);
			
			var hi = await CSharpScript.EvaluateAsync(code, ScriptOptions.Default.WithImports("System"), pack);
			
			Logger.log(hi);
		}

		public static void test() {
			
			const string html = @"
<div>
	<div>
		<p>Hi</p>
		<p>Hi2</p>
		<p>Hi3</p>
	</div>

	<h1>Sup bro</h1>
</div>
";
			genHTML(html, new Package());
		}
	}
}