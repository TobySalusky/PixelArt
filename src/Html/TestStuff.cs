using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;

namespace PixelArt {
	public class TestStuff {
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
	}
}