using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SharpDX.Direct3D11;

namespace PixelArt {
	public class HtmlNode {
		public string tag;
		public HtmlNode[] children;
		public string textContent;
		public Dictionary<string, object> props;

		public int x, y;
		public int width, height;
		public Color backgroundColor = Color.Red, color;

		public HtmlNode(string tag, Dictionary<string, object> props = null, string textContent = null, HtmlNode[] children = null) {
			this.tag = tag;
			this.props = props;
			this.textContent = textContent;
			this.children = children;

			processProps: { 
				if (props == null) goto finishProps;
				
				if (props.ContainsKey("x")) x = (int) props["x"];
				if (props.ContainsKey("y")) y = (int) props["y"];
				if (props.ContainsKey("width")) width = (int) props["width"];
				if (props.ContainsKey("height")) height = (int) props["height"];
				
				if (props.ContainsKey("backgroundColor")) backgroundColor = (Color) props["backgroundColor"];
				if (props.ContainsKey("color")) color = (Color) props["color"];
			} finishProps: { }
		}

		public override string ToString() {
			return $"{tag}Node{{{(props != null ? "props: "+Util.stringifyDict(props) : "")}{(textContent != null ? " textContent: " + textContent : "")}{(children != null ? " children: " + stringifyChildren() : "")}}}";
		}

		public string stringifyChildren() {
			string str = "[";
			for (int i = 0; i < children.Length; i++) {
				str += children[i].ToString() + ((i + 1 < children.Length) ? ", " : "");
			}
			
			return str + "]";
		}

		public void render(SpriteBatch spriteBatch) { 
			Rectangle renderRect = new Rectangle(x, y, width, height);
            
			spriteBatch.Draw(Textures.rect, renderRect, backgroundColor);

			if (children != null) { 
				foreach (HtmlNode child in children) {
					child.render(spriteBatch);
				}
			}
		}
	}
}