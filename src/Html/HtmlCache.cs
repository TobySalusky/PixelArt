namespace PixelArt {
	public static class HtmlCache {
		public static void CacheHtml(string[] input, string outputCode) {
			string[] cachedInput = StatePack.CacheData.CachedInput();

			if (input == null || input.Length == 0) return;
			
			if (cachedInput == null || cachedInput.Length != input.Length) { 
				UpdateCache(input, outputCode);
			} else {
				for (int i = 0; i < input.Length; i++) {
					if (input[i] != cachedInput[i]) { 
						UpdateCache(input, outputCode);
					}
				}
			}
		}

		public static async void UpdateCache(string[] input, string outputCode) {
			string path = @"D:\Users\Tobafett\Documents\GitHub\PixelArt\src\Html\StatePack.cs";
			string text = await System.IO.File.ReadAllTextAsync(path);
			Logger.log(text);

			text = text.Substring(0, text.IndexOf("/*CACHE_START*/")) +
			       text.Substring(text.IndexOf("/*CACHE_END*/"));
			
			await System.IO.File.WriteAllTextAsync(path, text);
		}
	}
}