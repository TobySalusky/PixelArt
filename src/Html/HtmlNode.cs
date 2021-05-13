namespace PixelArt {
	public class HtmlNode {
		public string tag;
		public HtmlNode[] children;
		public string textContent;

		public override string ToString() {
			return $"{tag}Node{{{(textContent != null ? " textContent: " + textContent : "")}{(children != null ? " children: " + stringifyChildren() : "")}}}";
		}

		public string stringifyChildren() {
			string str = "[";
			foreach (HtmlNode child in children) {
				str += child.ToString();
			}

			return str + "]";
		}
	}
}