using System.Collections.Generic;

namespace PixelArt {
	public static class Macros {

		public static Dictionary<string, string> create(params string[] macroList) {
			
			Dictionary<string, string> macros = new Dictionary<string, string>();
			
			for (int i = 0; i < macroList.Length; i += 2) {
				string macroID = macroList[i];
				string value = macroList[i + 1];

				macros[macroID] = value;
			}

			return macros;
		}
	}
}