using System.Collections.Generic;

namespace PixelArt {
		public class DelimPair { // TODO: gen ordered list
		
		public int openIndex, openLen;
		public int closeIndex, closeLen;
		public int nestCount;

		public DelimPair(int openIndex, int closeIndex, int openLen = 1, int closeLen = 1, int nestCount = 0) {
			this.openIndex = openIndex;
			this.closeIndex = closeIndex;
			this.openLen = openLen;
			this.closeLen = closeLen;
			this.nestCount = nestCount;
		}

		public string whole(string str) {
			return str.Substring(openIndex, closeIndex - openIndex + 1);
		}

		public string contents(string str) {
			return str.Substring(openIndex + 1, closeIndex - openIndex - 1);
		}

		public string htmlContents(string str) {
			string content = contents(str);
			return content.Substring(content.IndexOf(">") + 1);
		}

		public static Dictionary<int, DelimPair> genPairDict(string str, string open, string close) {
			var list = genPairs(str, open, close);
			Dictionary<int, DelimPair> dict = new Dictionary<int, DelimPair>();

			foreach (var pair in list) {
				dict[pair.openIndex] = pair;
				dict[pair.closeIndex] = pair;
			}

			return dict;
		}

		public static List<DelimPair> genPairs(string str, string open, string close) {
			
			Stack<int> stack = new Stack<int>();
			List<DelimPair> pairs = new List<DelimPair>();

			int openLen = open.Length, closeLen = close.Length;
			for (int i = 0; i < str.Length; i++) {
				if (i <= str.Length - openLen && (i > str.Length - closeLen || str.Substring(i, closeLen) != close || (open == close && stack.Count == 0)) &&
				    str.Substring(i, openLen) == open) {
					stack.Push(i);
					continue;
				}

				if (i <= str.Length - closeLen && str.Substring(i, closeLen) == close) {
					pairs.Add(new DelimPair(stack.Pop(), i, openLen, closeLen));
				}
			}

			foreach (var pair in pairs) {
				foreach (var other in pairs) {
					if (pair == other) continue;

					if (pair.openIndex > other.openIndex && pair.closeIndex < other.closeIndex) pair.nestCount++;
				}
			}

			return pairs;
		}

		/*public static List<DelimPair> genOrderedPairs(string str, string open, string close) { 
			
		}*/
	}

}