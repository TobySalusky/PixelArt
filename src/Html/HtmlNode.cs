using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SharpDX.Direct2D1;
using SharpDX.Direct3D11;
using SpriteBatch = Microsoft.Xna.Framework.Graphics.SpriteBatch;

namespace PixelArt {
	public class HtmlNode {
		
		// Primary info
		public string tag;
		public string textContent;

		public HtmlNode parent;
		public HtmlNode[] children;
		
		public Dictionary<string, object> props;
		public Dictionary<string, object> funcs; // TODO:

		public List<Action> actionList;

		// Position
		public PositionType position;
		public int x, y;
		public int width, height;

		// Text
		public string fontFamily = "JetbrainsMono";
		public int fontSize = 18;
		public SpriteFont font;
		//public Vector2 textDimens = Vector2.Zero;
		// TODO: font weight

		// Appearance
		public int borderRadius = 0;
		public int borderWidth = 0;
		public Color borderColor = Color.Black;

		public Color backgroundColor = Color.Transparent, color = Color.Black;

		// Layout
		public float flex;
		public AlignType alignX, alignY;
		public DirectionType flexDirection = DirectionType.column;

		// ENUMS
		public enum PositionType { // TODO: implement absolute, relative, and others
			normal, absolute, relative
		}

		public enum DirectionType {
			row, column
		}

		public enum AlignType { 
			flexStart, flexEnd, start, end, center, spaceBetween, spaceAround, spaceEvenly
		}

		public HtmlNode(string tag, Dictionary<string, object> props = null, string textContent = null, HtmlNode[] children = null) {
			this.tag = tag;
			this.props = props;
			this.textContent = textContent;
			this.children = children;

			if (children != null) {
				foreach (HtmlNode child in children) {
					child.parent = this;
				}
			}
		}

		public void onWidthChange() {
			onResize();
		}
		
		public void onHeightChange() {
			onResize();
		}

		public void onResize() {
			// TODO:
			parent?.layoutDown();

			if (props.ContainsKey("borderRadius")) { // TODO: abstract to method
				if (props["borderRadius"] is string str) {
					float mult = NodeUtil.percentAsFloat(str);
					borderRadius = (int) Math.Min(mult * width, mult * height);
				} else { 
					borderRadius = (int) props["borderRadius"];
				}
			}
		}

		public void onFontChange() {
			font = Fonts.getFontSafe(fontFamily, fontSize);
			Vector2 textDimens = font.MeasureString(textContent);
			width = (int) textDimens.X;
			height = (int) textDimens.Y;
			onResize();
		}

		public void topDownInit() {
			processProps: {
				if (textContent == "") textContent = null;
				if (textContent != null) font = Fonts.getFontSafe(fontFamily, fontSize); // default
				
				if (props == null) goto finishProps;
				
				if (props.ContainsKey("position")) position = Enum.Parse<PositionType>((string)props["position"]);
				
				if (props.ContainsKey("x")) x = (int) props["x"];
				if (props.ContainsKey("y")) y = (int) props["y"];

				if (props.ContainsKey("dimens")) { 
					width = NodeUtil.widthFromProp(props["dimens"], parent);
					height = NodeUtil.heightFromProp(props["dimens"], parent);
				}

				if (props.ContainsKey("width")) width = NodeUtil.widthFromProp(props["width"], parent);
				if (props.ContainsKey("height")) height = NodeUtil.heightFromProp(props["height"], parent);
				if (props.ContainsKey("flex")) flex = (int) props["flex"];
				
				if (props.ContainsKey("fontFamily")) fontFamily = (string) props["fontFamily"];
				if (props.ContainsKey("fontSize")) fontSize = (int) props["fontSize"];

				if (textContent != null) {
					onFontChange();
				}
				
				if (props.ContainsKey("-fontSize")) {
					object funcProp = props["-fontSize"];
					if (funcProp is Func<int> intFunc) { 
						bindAction(() => {
							int initFontSize = fontSize;
							fontSize = intFunc();
							if (initFontSize != fontSize) onFontChange();
						});
					}
				}
				if (props.ContainsKey("-fontFamily")) {
					object funcProp = props["-fontFamily"];
					if (funcProp is Func<string> strFunc) { 
						bindAction(() => {
							string initFontFamily = fontFamily;
							fontFamily = strFunc();
							if (initFontFamily != fontFamily) onFontChange();
						});
					}
				}


				if (props.ContainsKey("-flex")) {
					object flexFuncProp = props["-flex"];
					if (flexFuncProp is Func<float> floatFunc) { 
						bindAction(() => {
							float initFlex = flex;
							flex = floatFunc();
							if (initFlex != flex) onResize();
						});
					}
				}

				if (props.ContainsKey("-width")) {
					object widthFuncProp = props["-width"];
					if (widthFuncProp is Func<string> strFunc) {
						bindAction(() => {
							int initWidth = width;
							width = NodeUtil.widthFromProp(strFunc(), parent);
							if (initWidth != width) onWidthChange();
						});
					} else if (widthFuncProp is Func<int> intFunc) { 
						bindAction(() => {
							int initWidth = width;
							width = intFunc();
							if (initWidth != width) onWidthChange();
						});
					}
				}
				
				if (props.ContainsKey("-height")) {
					object heightFuncProp = props["-height"];
					if (heightFuncProp is Func<string> strFunc) {
						bindAction(() => {
							int initHeight = height;
							height = NodeUtil.heightFromProp(strFunc(), parent);
							if (initHeight != height) onHeightChange();
						});
					} else if (heightFuncProp is Func<int> intFunc) { 
						bindAction(() => {
							int initHeight = height;
							height = intFunc();
							if (initHeight != height) onHeightChange();
						});
					}
				}


				if (props.ContainsKey("borderRadius")) {
					if (props["borderRadius"] is string str) {
						float mult = NodeUtil.percentAsFloat(str);
						borderRadius = (int) Math.Min(mult * width, mult * height);
					} else { 
						borderRadius = (int) props["borderRadius"];
					}
				}
				
				if (props.ContainsKey("borderWidth")) borderWidth = (int) props["borderWidth"];
				if (props.ContainsKey("borderColor")) borderColor = NodeUtil.colorFromProp(props["borderColor"]);

				if (props.ContainsKey("backgroundColor")) backgroundColor = NodeUtil.colorFromProp(props["backgroundColor"]);
				if (props.ContainsKey("color")) color = NodeUtil.colorFromProp(props["color"]);
				
				align: { 
					if (props.ContainsKey("flexDirection")) flexDirection = Enum.Parse<DirectionType>((string) props["flexDirection"]);

					AlignType alignMain = AlignType.flexStart, alignSub = AlignType.start;

					if (props.ContainsKey("justifyContent")) alignMain = Enum.Parse<AlignType>((string) props["justifyContent"]);
					if (props.ContainsKey("alignItems")) alignSub = Enum.Parse<AlignType>((string) props["alignItems"]);
					
					if (flexDirection == DirectionType.column) {
						alignX = alignSub;
						alignY = alignMain;
					} else { 
						alignX = alignMain;
						alignY = alignSub;
					}

					if (props.ContainsKey("alignX")) { 
						alignX = Enum.Parse<AlignType>((string) props["alignX"]);
						if (alignX != AlignType.start && alignX != AlignType.center && alignX != AlignType.end) { 
							flexDirection = DirectionType.row;
						}
					}
					
					if (props.ContainsKey("alignY")) { 
						alignY = Enum.Parse<AlignType>((string) props["alignY"]);
						if (alignY != AlignType.start && alignY != AlignType.center && alignY != AlignType.end) {
							flexDirection = DirectionType.column;
						}
					}
				}

				if (props.ContainsKey("ref")) {
					((Action<string>) props["ref"])($"testing ref! here is some width for your troubles {width}.");
				}

			} finishProps: { }

			if (children != null) {
				foreach (HtmlNode child in children) {
					child.topDownInit();
				}
			}
		}

		public void layoutDown() {
			if (children == null) return;

			int sumWidth = 0, sumHeight = 0;
			float sumFlex = 0;
			foreach (HtmlNode child in children) {
				sumWidth += child.width;
				sumHeight += child.height;
				sumFlex += child.flex;
			}

			if (flexDirection == DirectionType.column || sumFlex < 0.0001F) {
				switch (alignX) {
					case AlignType.spaceBetween: {

						if (children.Length == 1) {
							children[0].x = x;
							break;
						}

						int gap = (width - sumWidth) / (children.Length - 1);
						int thisX = x;
						foreach (HtmlNode child in children) {
							child.x = thisX;
							thisX += child.width + gap;
						}
						break;
					}
					case AlignType.spaceAround: {

						int gap = (width - sumWidth) / (children.Length);
						int thisX = x + gap / 2;
						foreach (HtmlNode child in children) {
							child.x = thisX;
							thisX += child.width + gap;
						}
						break;
					}
					case AlignType.spaceEvenly: {

						int gap = (width - sumWidth) / (children.Length + 1);
						int thisX = x + gap;
						foreach (HtmlNode child in children) {
							child.x = thisX;
							thisX += child.width + gap;
						}
						break;
					}
					case AlignType.center: {
						if (flexDirection == DirectionType.column) {
							foreach (HtmlNode child in children) {
								child.x = (x + width / 2) - child.width / 2;
							}
							break;
						}

						int thisX = (x + width / 2) - sumWidth / 2;
						foreach (HtmlNode child in children) {
							child.x = thisX;
							thisX += child.width;
						}
						break;
					}
					case AlignType.start: { 
						foreach (HtmlNode child in children) {
							child.x = x;
						}
						break;
					}
					case AlignType.end: { 
						foreach (HtmlNode child in children) {
							child.x = x + width - child.width;
						}
						break;
					}
					case AlignType.flexStart: { 
						int thisX = x;
						for (int i = 0; i < children.Length; i++) {
							children[i].x = thisX;
							thisX += children[i].width;
						}
						break;
					}
					case AlignType.flexEnd: { 
						int thisX = x + width;
						for (int i = children.Length - 1; i >= 0; i--) {
							thisX -= children[i].width;
							children[i].x = thisX;
						}
						break;
					}
				}
			} else {

				int nonFlexWidth = 0;
				foreach (HtmlNode child in children) {
					if (child.flex <= 0) nonFlexWidth += child.width;
				}
				
				float perFlex = (width - nonFlexWidth) / sumFlex;

				int thisX = 0;
				foreach (HtmlNode child in children) {
					child.x = thisX;
					if (child.flex > 0) child.width = (int) (perFlex * child.flex);
					thisX += child.width;
				}
			}

			if (flexDirection == DirectionType.row || sumFlex < 0.0001F) { 
				switch (alignY) {
					case AlignType.spaceBetween: {

						if (children.Length == 1) {
							children[0].y = y;
							break;
						}

						int gap = (height - sumHeight) / (children.Length - 1);
						int thisY = y;
						foreach (HtmlNode child in children) {
							child.y = thisY;
							thisY += child.height + gap;
						}
						break;
					}
					case AlignType.spaceAround: {

						int gap = (height - sumHeight) / (children.Length);
						int thisY = y + gap / 2;
						foreach (HtmlNode child in children) {
							child.y = thisY;
							thisY += child.height + gap;
						}
						break;
					}
					case AlignType.spaceEvenly: {

						int gap = (height - sumHeight) / (children.Length + 1);
						int thisY = y + gap;
						foreach (HtmlNode child in children) {
							child.y = thisY;
							thisY += child.height + gap;
						}
						break;
					}
					case AlignType.center: {
						if (flexDirection == DirectionType.row) {
							foreach (HtmlNode child in children) {
								child.y = (y + height / 2) - child.height / 2;
							}
							break;
						}

						int thisY = (y + height / 2) - sumHeight / 2;
						foreach (HtmlNode child in children) {
							child.y = thisY;
							thisY += child.height;
						}
						break;
					}
					case AlignType.start: { 
						foreach (HtmlNode child in children) {
							child.y = y;
						}
						break;
					}
					case AlignType.end: { 
						foreach (HtmlNode child in children) {
							child.y = y + height - child.height;
						}
						break;
					}
					case AlignType.flexStart: { 
						int thisY = y;
						for (int i = 0; i < children.Length; i++) {
							children[i].y = thisY;
							thisY += children[i].height;
						}
						break;
					}
					case AlignType.flexEnd: { 
						int thisY = y + height;
						for (int i = children.Length - 1; i >= 0; i--) {
							thisY -= children[i].height;
							children[i].y = thisY;
						}
						break;
					}
				}
			} else {
				int nonFlexHeight = 0;
				foreach (HtmlNode child in children) {
					if (child.flex <= 0) nonFlexHeight += child.height;
				}
				
				float perFlex = (height - nonFlexHeight) / sumFlex;

				int thisY = 0;
				foreach (HtmlNode child in children) {
					child.y = thisY;
					if (child.flex > 0) child.height = (int) (perFlex * child.flex);
					thisY += child.height;
				}
			}



			foreach (HtmlNode child in children) {
				child.layoutDown();
			}
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

		public void bindAction(Action action) { 
			if (actionList == null) actionList = new List<Action>();
			
			actionList.Add(action);
		}

		public void update(float deltaTime) {

			if (actionList != null) {
				foreach (Action action in actionList) {
					action.Invoke();
				}
			}

			if (children != null) { 
				foreach (HtmlNode child in children) {
					child.update(deltaTime);
				}
			}
		}

		public void tryRenderText(SpriteBatch spriteBatch) {
			
			if (textContent == null) return;
			if (color == Color.Transparent) return;
			
			spriteBatch.DrawString(font, textContent, new Vector2(x, y), color);
		}

		public void renderSelf(SpriteBatch spriteBatch) {

			if (borderWidth != 0 && borderColor != Color.Transparent) {
				if (borderRadius == 0) { 
					int doubleBorder = 2 * borderWidth;
					spriteBatch.Draw(Textures.rect, new Rectangle(x - borderWidth, y - borderWidth, width + doubleBorder, borderWidth), borderColor);
					spriteBatch.Draw(Textures.rect, new Rectangle(x - borderWidth, y, borderWidth, height), borderColor);
					spriteBatch.Draw(Textures.rect, new Rectangle(x + width, y, borderWidth, height), borderColor);
					spriteBatch.Draw(Textures.rect, new Rectangle(x - borderWidth, y + height, width + doubleBorder, borderWidth), borderColor);
				} else {
					
					int diameter = borderRadius * 2;
					spriteBatch.Draw(Textures.rect, new Rectangle(x + borderRadius, y - borderWidth, width - diameter, borderWidth), borderColor);
					spriteBatch.Draw(Textures.rect, new Rectangle(x - borderWidth, y+borderRadius, borderWidth, height-diameter), borderColor);
					spriteBatch.Draw(Textures.rect, new Rectangle(x + width, y + borderRadius, borderWidth, height - diameter), borderColor);
					spriteBatch.Draw(Textures.rect, new Rectangle(x + borderRadius, y + height, width - diameter, borderWidth), borderColor);

					if (backgroundColor == Color.Transparent) { 
						void drawArc(Vector2 center, float radius, float startAngle, float endAngle, int points) {
						
							float diff = (endAngle - startAngle) / (points - 1);
							for (int i = 1; i < points; i++) {
								Vector2 p1 = center + Util.polar(radius, startAngle + diff * (i - 1));
								Vector2 p2 = center + Util.polar(radius, startAngle + diff * i);
							
								Util.drawLineScreen(p1, p2, spriteBatch, borderColor, borderWidth);
							}
						}
						const int points = 25;
						float rad = borderRadius + borderWidth / 2F;
						drawArc(new Vector2(x+borderRadius, y+borderRadius), rad, 
							-Maths.halfPI, -Maths.PI, points);
						drawArc(new Vector2(x+width-borderRadius, y+borderRadius), rad, 
							0, -Maths.halfPI, points);
						drawArc(new Vector2(x+borderRadius, y+height-borderRadius), rad, 
							-Maths.PI, -Maths.PI * 3/2F, points);
						drawArc(new Vector2(x+width-borderRadius, y+height-borderRadius), rad, 
							-Maths.PI * 3/2F, -Maths.twoPI, points);
					} else {
						int doubleBorder = borderWidth * 2;
						spriteBatch.Draw(Textures.circle, new Rectangle(x-borderWidth, y-borderWidth,diameter, diameter+doubleBorder), borderColor);
						spriteBatch.Draw(Textures.circle, new Rectangle(x + width - diameter-borderWidth, y-borderWidth,diameter+doubleBorder, diameter+doubleBorder), borderColor);
						spriteBatch.Draw(Textures.circle, new Rectangle(x-borderWidth, y + height - diameter-borderWidth,diameter+doubleBorder, diameter+doubleBorder), borderColor);
						spriteBatch.Draw(Textures.circle, new Rectangle(x + width - diameter-borderWidth, y + height - diameter-borderWidth,diameter+doubleBorder, diameter+doubleBorder), borderColor);
					}
				}
			}
			
			if (backgroundColor != Color.Transparent) {
				if (borderRadius == 0) { 
					Rectangle renderRect = new Rectangle(x, y, width, height);
					spriteBatch.Draw(Textures.rect, renderRect, backgroundColor);
				} else { 
					int diameter = borderRadius * 2;
			
					spriteBatch.Draw(Textures.rect, new Rectangle(x + borderRadius, y, width - diameter, borderRadius), backgroundColor);
					spriteBatch.Draw(Textures.rect, new Rectangle(x, y + borderRadius, width, height - diameter), backgroundColor);
					spriteBatch.Draw(Textures.rect, new Rectangle(x + borderRadius, y + height - borderRadius, width - diameter, borderRadius), backgroundColor);
			
					spriteBatch.Draw(Textures.circle, new Rectangle(x, y,diameter, diameter), backgroundColor);
					spriteBatch.Draw(Textures.circle, new Rectangle(x + width - diameter, y,diameter, diameter), backgroundColor);
					spriteBatch.Draw(Textures.circle, new Rectangle(x, y + height - diameter,diameter, diameter), backgroundColor);
					spriteBatch.Draw(Textures.circle, new Rectangle(x + width - diameter, y + height - diameter,diameter, diameter), backgroundColor);
				}
			}
			
			tryRenderText(spriteBatch);
		}

		public void render(SpriteBatch spriteBatch) { 
			
			renderSelf(spriteBatch);
			
			if (children != null) { 
				foreach (HtmlNode child in children) {
					child.render(spriteBatch);
				}
			}
		}
	}
}