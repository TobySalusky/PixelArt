namespace PixelArt {
	public static class Str {
		public static string sub(this string str, int startInclusive, int endExclusive) {
			return str.Substring(startInclusive, endExclusive - startInclusive);
		}
		
		public static string sub(this string str, int startInclusive) {
			return str.Substring(startInclusive);
		}
	}
}