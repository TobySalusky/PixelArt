using System;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Windows.Forms;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Keys = Microsoft.Xna.Framework.Input.Keys;

namespace PixelArt {
    public class Canvas {

        public Vector2 pos, dimen;
        
        public Texture2D background;
        public int xPix, yPix;
        public int area;
        public Vector2 pixDimen;
        public Rectangle canvasRect;
        
        public List<Layer> layers = new List<Layer>();
        public Layer layer;
        public Color[] layerColor;

        public bool changesMade = false;

        public List<Undo> undos = new List<Undo>();

        public int layerIndex = 0;

        public bool hasPreview;
        public Texture2D previewTexture;
        public Color[] previewColor;

        public bool grid = false;
        public Texture2D gridTexture;

        public Point tileDimen;
        public bool tileGrid = false;
        public Texture2D tileGridTexture;

        public int layersCreated = 1;
        
        // SPECIFICS
        
        // control-z
        public const float firstHoldZ = 0.3F, afterHoldZ = 0.075F;
        public float holdTimeZ;
        
        // Tools
        // general
        public Point beginPixel;
        public Vector2 beginMousePos;
        public bool tempTool;
        public Tool switchedToFrom;
        public bool usingTool;

        // selection type tools
        public bool selecting, hasSelection, movingSelection;
        public Texture2D selectionTexture;
        public Rectangle selectRect;
        public Point startRectTl;

        public Canvas(int pixSize) : this(pixSize, pixSize) {}

        public Canvas(int xPix, int yPix) : this(Textures.genRect(Colors.erased, xPix, yPix)) {}

        public Canvas(string str) : this(Textures.get(str)) { }

        public Canvas(Texture2D texture) {

            layers.Add(new Layer(texture, nameLayer()));
            xPix = texture.Width;
            yPix = texture.Height;
            area = xPix * yPix;
            pixDimen = new Vector2(xPix, yPix);

            canvasRect = new Rectangle(0, 0, xPix, yPix);
            
            pos = Vector2.Zero;
            dimen = new Vector2(1, (float) yPix / xPix) * 100;

            layerIndex = 0;
            
            background = Textures.genRect(Colors.canvasBack);
            //background = Textures.genTrans(xPix, yPix);
        }

        public void input(float deltaTime, KeyInfo keys, MouseInfo mouse) {

            layer = layers[layerIndex];
            layerColor = Util.colorArray(layer.texture); // very laggy for larger canvases
            Vector2 mousePos = toCanvas(mouse.pos);
            Vector2 lastMousePos = toCanvas(Main.lastMousePos());
            Point pixel = toPixel(mousePos);

            hasPreview = false;

            // GRABBING
            if (keys.pressed(Keys.G) && !hasSelection) {
                tempTool = true;
                selectRect = new Rectangle(0, 0, xPix, yPix);
                hasSelection = true;
                switchedToFrom = Main.tool;
                Main.tool = Tool.RectSelect;
            }
            
            // DUPLICATE LAYER
            if (keys.pressed(Keys.D) && keys.shift) {
                duplicateLayer();
            }

            // NEW LAYER
            if (keys.pressed(Keys.N) && !keys.control) {
                addLayerAbove();
            }
            
            // DELETE LAYER
            if (keys.pressed(Keys.X) || keys.pressed(Keys.Delete)) {
                addUndo();
                if (layers.Count == 1) {
                    iterBetween(canvasRect, (x, y) => setRGB(x, y, Colors.erased));
                } else {
                    layers.RemoveAt(layerIndex);
                    if (layerIndex >= layers.Count) layerIndex = layers.Count - 1;
                    layer = layers[layerIndex];
                    Main.updateLayerButtons = true;
                }
            }

            switch (Main.tool) {
                case Tool.Brush:

                    if (mouse.leftPressed) {
                        addUndo();
                        usingTool = true;
                        ToolSettings.brush.brushAt(mousePos, this, layerColor);
                    }

                    if (mouse.leftDown && usingTool) {
                        ToolSettings.brush.brushBetween(lastMousePos, mousePos, this, layerColor);
                    }
                    break;
                
                case Tool.Eraser:

                    Color saveColor = Main.brushColor;
                    Main.brushColor = Colors.erased;
                    if (mouse.leftPressed) {
                        addUndo();
                        usingTool = true;
                        ToolSettings.brush.brushAt(mousePos, this, layerColor);
                    }

                    if (mouse.leftDown && usingTool) {
                        ToolSettings.brush.brushBetween(lastMousePos, mousePos, this, layerColor);
                    }

                    Main.brushColor = saveColor;
                    break;
                
                case Tool.ColorPick:
                    if (mouse.leftPressed) {
                        if (inBounds(pixel)) {
                            var full = genSingleImage();
                            Main.brushColor = getRGB(pixel, Util.colorArray(full));
                        }
                    }
                    break;
                
                case Tool.FillBucket:
                    if (mouse.leftPressed) {
                        addUndo();
                        fillAt(pixel);
                    }
                    break;
                
                case Tool.Rect:

                    if (mouse.leftPressed) {
                        usingTool = true;
                        beginPixel = pixel;
                        addUndo();
                    }

                    if (mouse.leftDown && usingTool) {
                        
                        Point endPixel = pixel;

                        if (keys.down(Keys.LeftShift)) {
                            Vector2 start = new Vector2(beginPixel.X + 0.5F, beginPixel.Y + 0.5F);
                            Vector2 diff = mousePos - start;
                            diff = Maths.signEach(diff) * Maths.max(Maths.mags(diff));

                            endPixel = toPixel(start + diff);
                        }

                        
                        Point p1 = Util.max(Util.min(endPixel, beginPixel), new Point(0, 0));
                        Point p2 = Util.min(Util.max(endPixel, beginPixel), new Point(xPix - 1, yPix - 1));

                        hasPreview = true;
                        clearPreview();

                        void Fill(int x, int y) {
                            setRGB(x, y, Main.brushColor, previewColor);
                        }

                        if (ToolSettings.shapeFill) {
                            iterBetween(p1, p2, Fill);
                        }
                        else {
                            iterBetween(p1, new Point(p2.X, p1.Y), Fill);
                            iterBetween(p1, new Point(p1.X, p2.Y), Fill);
                            iterBetween(new Point(p2.X, p1.Y + 1), p2, Fill);
                            iterBetween(new Point(p1.X + 1, p2.Y), p2, Fill);
                        }
                    }

                    if (mouse.leftUnpressed && usingTool) {
                        bindPreview();
                        usingTool = false;
                    }
                    break;
                
                case Tool.Ellipse:

                    if (mouse.leftPressed) {
                        beginPixel = pixel;
                        usingTool = true;
                        addUndo();
                    }

                    if (mouse.leftDown && usingTool) {

                        Point endPixel = pixel;

                        if (keys.down(Keys.LeftShift)) {
                            Vector2 start = new Vector2(beginPixel.X + 0.5F, beginPixel.Y + 0.5F);
                            Vector2 diff = mousePos - start;
                            diff = Maths.signEach(diff) * Maths.max(Maths.mags(diff));

                            endPixel = toPixel(start + diff);
                        }

                        Point p1 = Util.min(endPixel, beginPixel);
                        Point p2 = Util.max(endPixel, beginPixel);

                        Vector2 center = new Vector2(p1.X + p2.X + 1, p1.Y + p2.Y + 1) / 2F;
                        Vector2 dimen = new Vector2(p2.X + 1 - p1.X, p2.Y + 1 - p1.Y);

                        hasPreview = true;

                        void CircleFill(int x, int y) {
                            if (inBounds(x, y) && Util.inEllipse(new Vector2(x + 0.5F, y + 0.5F), center, dimen)) 
                                setRGB(x, y, Main.brushColor, previewColor);
                        }
                        
                        void CircleDraw(int x, int y) {
                            Vector2 vec = new Vector2(x + 0.5F, y + 0.5F);
                            if (inBounds(x, y) && 
                                Util.inEllipse(vec, center, dimen) &&
                                !Util.inEllipse(vec, center, dimen - Vector2.One * 2)) 
                                setRGB(x, y, Main.brushColor, previewColor);
                        }

                        clearPreview();
                        Action<int, int> call = CircleFill;
                        if (!ToolSettings.shapeFill) call = CircleDraw;
                        iterBetween(Util.max(p1, new Point(0, 0)), Util.min(p2, new Point(xPix - 1, yPix - 1)), call);
                    }

                    if (mouse.leftUnpressed && usingTool) {
                        bindPreview();
                        usingTool = false;
                    }
                    break;
                
                case Tool.RectSelect:
                    
                    if (movingSelection) {
                        Vector2 off = mousePos - beginMousePos;
                        if (keys.down(Keys.LeftShift)) {
                            off = Maths.signEach(off) * Maths.removeMin(Maths.abs(off));
                        }

                        Point diff = new Point((int) Math.Round(off.X), (int) Math.Round(off.Y));

                        selectRect.Location = startRectTl + diff;

                        if (keys.pressed(Keys.Space) || keys.pressed(Keys.Enter) || mouse.leftPressed) { 
                            bindSelection();
                            hasSelection = false;
                            movingSelection = false;
                            selectionTexture = null;

                            if (tempTool) {
                                Main.tool = switchedToFrom;
                            }
                        }

                        if (mouse.rightPressed) {
                            hasSelection = false;
                            movingSelection = false;
                            selectionTexture = null;

                            useUndo();
                            
                            if (tempTool) {
                                Main.tool = switchedToFrom;
                            }
                        }

                        break;
                    }

                    if (mouse.leftPressed) {
                        beginPixel = pixel;
                        selectionTexture = null;
                        selecting = true;
                    }

                    if (selecting) {

                        if (mouse.leftDown) {

                            Point p1 = Util.min(Util.max(Util.min(pixel, beginPixel), new Point(0, 0)), new Point(xPix - 1, yPix - 1));
                            Point p2 = Util.min(Util.max(Util.max(pixel, beginPixel), new Point(0, 0)), new Point(xPix - 1, yPix - 1));

                            selectRect = new Rectangle(p1.X, p1.Y, p2.X - p1.X + 1, p2.Y - p1.Y + 1);
                        }

                        if (mouse.leftUnpressed) {
                            hasSelection = !(selectRect.Width == 1 && selectRect.Height == 1);
                            
                            
                            
                            selecting = false;
                        }
                    }
                    
                    // removing selection
                    if (hasSelection && (keys.pressed(Keys.Space) || keys.pressed(Keys.Enter))) { 
                        hasSelection = false;
                        selectionTexture = null;
                    }

                    // deleting
                    if (hasSelection && (keys.pressed(Keys.X) || keys.pressed(Keys.Delete) || keys.pressed(Keys.Back))) {
                        iterBetween(selectRect, (x, y) => {
                            setRGB(x, y, Colors.erased);
                        });

                        hasSelection = false;
                        selectionTexture = null;
                    }

                    // grab
                    if (hasSelection && keys.pressed(Keys.G)) {
                        // snaps to bounds of image
                        int minX = selectRect.X + selectRect.Width - 1, maxX = selectRect.X;
                        int minY = selectRect.Y + selectRect.Height - 1, maxY = selectRect.Y;
                        iterBetween(selectRect, (x, y) => {
                            if (getRGB(x, y) != Colors.erased) {
                                minX = Math.Min(minX, x);
                                maxX = Math.Max(maxX, x);
                                minY = Math.Min(minY, y);
                                maxY = Math.Max(maxY, y);
                            }
                        });
                        if (minX <= maxX && minY <= maxY) { 
                            selectRect = new Rectangle(minX, minY, maxX - minX + 1, maxY - minY + 1);
                        }

                        addUndo();
                        
                        Color[] arr = new Color[(selectRect.Width) * (selectRect.Height)];

                        int i = 0;
                        iterBetween(selectRect, (x, y) => { // TODO:remove a +1 +1 somewhere
                            arr[i] = getRGB(x, y);
                            setRGB(x, y, Colors.erased);
                            i++;
                        });
                        selectionTexture = Textures.toTexture(arr, selectRect.Width, selectRect.Height);
                        beginMousePos = mousePos;
                        startRectTl = selectRect.Location;
                        movingSelection = true;
                    }

                    break;
            }

            if (keys.down(Keys.Z) && keys.down(Keys.LeftControl)) {
                if (keys.pressed(Keys.Z)) {
                    holdTimeZ = firstHoldZ;
                    useUndo();
                }
                else {
                    holdTimeZ -= deltaTime;
                    if (holdTimeZ < 0) {
                        useUndo();
                        holdTimeZ = afterHoldZ;
                    }
                }
            }

            if (changesMade) {
                layer.texture.SetData(layerColor);
                changesMade = false;
            }

            previewTexture = hasPreview ? Textures.toTexture(previewColor, xPix, yPix) : null;
        }

        public void duplicateLayer() {
            pasteLayer(new Layer(Textures.copy(layer.texture), nameLayer()));
        }

        public void copyLayerToClipboard() { // TODO: reimplement outside pasting

            Layer copyLayer;
            //Texture2D copyTexture;
            if (hasSelection) {
                var arr = new Color[area];
                for (int i = 0; i < arr.Length; i++) {
                    arr[i] = Colors.erased;
                }
                

                iterBetween(selectRect, (x, y) => {
                    arr[x + y * xPix] = getRGB(x, y);
                });
                copyLayer = new Layer(Textures.toTexture(arr, xPix, yPix));

                /*var textureArr = new Color[(selectRect.Width) * (selectRect.Height)]; // TODO: extract to func (duped code)
                int j = 0;
                iterBetween(selectRect, (x, y) => {
                    textureArr[j] = getRGB(x, y);
                    j++;
                });
                copyTexture = Textures.toTexture(textureArr, selectRect.Width, selectRect.Height);*/
            }
            else {
                copyLayer = layer.controlC();
                //copyTexture = layer.texture;
            }

            // copies layer as ClipboardLayer
            System.Windows.Forms.Clipboard.Clear();
            DataFormats.Format format = DataFormats.GetFormat(typeof(ClipboardLayer).FullName);

            IDataObject dataObj = new DataObject();
            dataObj.SetData(format.Name, false, new ClipboardLayer(copyLayer));
            System.Windows.Forms.Clipboard.SetDataObject(dataObj, false);
        }

        public void pasteLayer(Layer pasteLayer) {
            addUndo();

            pasteLayer.name = nameLayer();
            
            layers.Insert(layerIndex + 1, pasteLayer);
            layerIndex++;
            layer = pasteLayer;
            
            Main.updateLayerButtons = true;
        }

        public void pasteTexture(Texture2D pasteTexture) {
            addLayerAbove();
            selectionTexture = pasteTexture;
            selectRect = new Rectangle(0, 0, selectionTexture.Width, selectionTexture.Height);
            bindSelection();
            selectionTexture = null;
            hasSelection = false;
            selecting = false;
            movingSelection = false;
        }

        public string nameLayer() {
            layersCreated++;
            return "L" + (layersCreated - 1);
        }

        public void addLayerAbove() {
            addUndo();
            layers.Insert(layerIndex + 1, new Layer(Textures.genRect(Colors.erased, xPix, yPix), nameLayer()));
            layerIndex++;
            layer = layers[layerIndex];
            Main.updateLayerButtons = true;
        }

        public void switchOff(Tool lastTool) {
            // remember temp-switching exits
            usingTool = false;
            switch (lastTool) {
                case Tool.RectSelect:
                    selecting = false;
                    hasSelection = false;
                    movingSelection = false;
                    selectionTexture = null;
                    break;
            }
        }

        public void makeTiled(int tileSize) {
            makeTiled(tileSize, tileSize);
        }

        public void makeTiled(int xTile, int yTile) {
            tileDimen = new Point(xTile, yTile);
            tileGrid = true;
            
            int mult = 12;
            int xPixels = xPix * mult;
            int yPixels = yPix * mult;
            var col = new Color[xPixels * yPixels];

            int multX = xTile * mult;
            int multY = yTile * mult;
            
            for (int x = 0; x < xPixels; x++) {
                for (int y = 0; y < yPixels; y++) {
                    col[x + y * xPixels] =
                        (x % multX == multX - 1 || x % multX == 0 || y % multY == multY - 1 || y % multY == 0)
                            ? Color.Black
                            : Colors.erased;
                }
            }

            tileGridTexture = Textures.toTexture(col, xPixels, yPixels);
        }
        
        public void genGridTexture() {
            int mult = 32;
            int xPixels = xPix * mult;
            int yPixels = yPix * mult;
            var col = new Color[xPixels * yPixels];

            for (int x = 0; x < xPixels; x++) {
                for (int y = 0; y < yPixels; y++) {
                    col[x + y * xPixels] =
                        (x % mult == mult - 1 || x % mult == 0 || y % mult == mult - 1 || y % mult == 0)
                            ? Color.Gray
                            : Colors.erased;
                }
            }

            gridTexture = Textures.toTexture(col, xPixels, yPixels);
        }

        public void bindPreview() {
            GraphicsDevice g = Main.instance.GraphicsDevice;
            SpriteBatch spriteBatch = Main.spriteBatch;
            RenderTarget2D renderTarget = new RenderTarget2D(
                g,
                xPix,
                yPix,
                false,
                g.PresentationParameters.BackBufferFormat,
                DepthFormat.Depth24);
            
            g.SetRenderTarget(renderTarget);

            g.Clear(Colors.erased);
            spriteBatch.Begin(SpriteSortMode.Deferred,
                BlendState.NonPremultiplied,
                SamplerState.PointClamp);
            
            Rectangle rect = new Rectangle(0, 0, xPix, yPix);
            spriteBatch.Draw(layer.texture, rect, Color.White);
            spriteBatch.Draw(previewTexture, rect, Color.White);

            spriteBatch.End();
            g.SetRenderTarget(null);

            layer.texture = renderTarget;
        }
        
        public void bindSelection() {
            GraphicsDevice g = Main.instance.GraphicsDevice;
            SpriteBatch spriteBatch = Main.spriteBatch;
            RenderTarget2D renderTarget = new RenderTarget2D(
                g,
                xPix,
                yPix,
                false,
                g.PresentationParameters.BackBufferFormat,
                DepthFormat.Depth24);
            
            g.SetRenderTarget(renderTarget);

            g.Clear(Colors.erased);
            spriteBatch.Begin(SpriteSortMode.Deferred,
                BlendState.NonPremultiplied,
                SamplerState.PointClamp);
            
            Rectangle rect = new Rectangle(0, 0, xPix, yPix);
            spriteBatch.Draw(layer.texture, rect, Color.White);
            spriteBatch.Draw(selectionTexture, selectRect, Color.White);

            spriteBatch.End();
            g.SetRenderTarget(null);

            layer.texture = renderTarget;
        }

        public Texture2D genSingleImage() {
            GraphicsDevice g = Main.instance.GraphicsDevice;
            SpriteBatch spriteBatch = Main.spriteBatch;
            RenderTarget2D renderTarget = new RenderTarget2D(
                g,
                xPix,
                yPix,
                false,
                g.PresentationParameters.BackBufferFormat,
                DepthFormat.Depth24);
            
            g.SetRenderTarget(renderTarget);

            g.Clear(Colors.erased);
            spriteBatch.Begin(SpriteSortMode.Deferred,
                BlendState.NonPremultiplied,
                SamplerState.PointClamp);
            
            Rectangle rect = new Rectangle(0, 0, xPix, yPix);
            foreach (var layer in layers) {
                if (layer.visible)
                    spriteBatch.Draw(layer.texture, rect, Color.White);
            }

            spriteBatch.End();
            g.SetRenderTarget(null);

            return renderTarget;
        }

        public void fillAt(Point pixel) {
            
            if (!inBounds(pixel)) return;

            var full = Util.colorArray(genSingleImage());

            Color onColor = getRGB(pixel, full);
            
            var points = new List<Point> {pixel};

            setRGB(pixel, Main.brushColor);
            while (points.Count > 0) {
                Point point = points[^1];
                points.RemoveAt(points.Count - 1);

                Point test = new Point(point.X - 1, point.Y);
                if (test.X >= 0 && getRGB(test, full) == onColor && getRGB(test) != Main.brushColor) {
                    points.Add(test);
                    setRGB(test, Main.brushColor);
                }
                
                test = new Point(point.X + 1, point.Y);
                if (test.X < xPix && getRGB(test, full) == onColor && getRGB(test) != Main.brushColor) {
                    points.Add(test);
                    setRGB(test, Main.brushColor);
                }
                
                test = new Point(point.X, point.Y - 1);
                if (test.Y >= 0 && getRGB(test, full) == onColor && getRGB(test) != Main.brushColor) {
                    points.Add(test);
                    setRGB(test, Main.brushColor);
                }
                
                test = new Point(point.X, point.Y + 1);
                if (test.Y < yPix && getRGB(test, full) == onColor && getRGB(test) != Main.brushColor) {
                    points.Add(test);
                    setRGB(test, Main.brushColor);
                }
            }
        }

        public void clearPreview() {
            previewColor = new Color[area];
            for (int i = 0; i < previewColor.Length; i++) {
                previewColor[i] = Colors.erased;
            }
        }

        public void iterBetween(Rectangle rect, Action<int, int> action) {
            iterBetween(new Point(rect.X, rect.Y), new Point(rect.X + rect.Width - 1, rect.Y + rect.Height - 1), action);
        }

        public void iterBetween(Point start, Point endInc, Action<int, int> action) { // end is inclusive
            for (int y = start.Y; y <= endInc.Y; y++) {
                for (int x = start.X; x <= endInc.X; x++) {
                    action.Invoke(x, y);
                }
            }
        }

        public void render(Camera camera, SpriteBatch spriteBatch) {
            Rectangle renderRect = camera.toScreen(pos, dimen);
            
            spriteBatch.Draw(background, renderRect, Color.White);

            foreach (var layer in layers) {
                
                if (!layer.visible) continue;
                
                spriteBatch.Draw(layer.texture, renderRect, Color.White);
                
                if (layer == this.layer) {
                    if (hasPreview)
                        spriteBatch.Draw(previewTexture, renderRect, Color.White);

                    if (selectionTexture != null) {
                        Rectangle snip = new Rectangle(0,0,selectRect.Width,selectRect.Height); // TODO:
                        if (selectRect.X < 0) {
                            snip.X -= selectRect.X;
                            snip.Width += selectRect.X;
                        }
                        if (selectRect.X + selectRect.Width - 1 >= xPix) {
                            snip.Width -= (selectRect.X + selectRect.Width - xPix);
                        }

                        if (selectRect.Y < 0) {
                            snip.Y -= selectRect.Y;
                            snip.Height += selectRect.Y;
                        }
                        if (selectRect.Y + selectRect.Height - 1 >= yPix) {
                            snip.Height -= (selectRect.Y + selectRect.Height - yPix);
                        }

                        spriteBatch.Draw(selectionTexture, canvasToScreen(new Rectangle(selectRect.X + snip.X, selectRect.Y + snip.Y, snip.Width, snip.Height)), snip, Color.White);
                    }
                }
            }

            if (grid) {
                if (gridTexture == null) {
                    genGridTexture();
                }

                spriteBatch.Draw(gridTexture, renderRect, Color.White);
            }

            if (tileGrid) {
                spriteBatch.Draw(tileGridTexture, renderRect, Color.White);
            }

            renderGizmos(camera, spriteBatch);
        }

        public void renderGizmos(Camera camera, SpriteBatch spriteBatch) {
            if (Main.tool == Tool.RectSelect && (selecting || hasSelection)) {
                Rectangle screenRect = canvasToScreen(selectRect);
                
                Vector2 tl = new Vector2(screenRect.X, screenRect.Y);
                Vector2 tr = tl + new Vector2(screenRect.Width, 0);
                Vector2 bl = tl + new Vector2(0, screenRect.Height);
                Vector2 br = tl + new Vector2(screenRect.Width, screenRect.Height);

                if (!movingSelection) {
                    Color cover = new Color(0.8F, 0.8F, 1F, 0.5F);
                    Util.renderCutRect(camera.toScreen(pos, dimen), screenRect, spriteBatch, cover);
                }
                
                if (movingSelection) {
                    Util.drawRect(spriteBatch, Util.expand(screenRect, 4), 4, Color.Orange);
                    Util.drawRect(spriteBatch, Util.expand(screenRect, 3), 2, Color.Black);
                }
                else {
                    Color outline = Color.Black;
                    const float thick = 2;
                    Util.dotLineScreen(tl, tr, spriteBatch, outline, thick);
                    Util.dotLineScreen(tr, br, spriteBatch, outline, thick);
                    Util.dotLineScreen(br, bl, spriteBatch, outline, thick);
                    Util.dotLineScreen(bl, tl, spriteBatch, outline, thick);
                }
            }
        }

        public void addUndo() {
            undos.Add(new Undo(this));
        }

        public void useUndo() {
            int index = undos.Count - 1;
            
            if (index < 0) return;
            
            undos[index].apply(this);
            undos.RemoveAt(index);

            layerColor = Util.colorArray(layer.texture);
        }

        public bool inBounds(int x, int y) {
            return (x >= 0 && x < xPix && y >= 0 && y < yPix);
        }

        public bool inBounds(Point pixel) {
            return inBounds(pixel.X, pixel.Y);
        }
        
        public void setRGB(Point point, Color color) {
            layerColor[point.X + point.Y * xPix] = color;
            changesMade = true;
        }
        
        public void setRGB(Point point, Color color, Color[] arr) {
            arr[point.X + point.Y * xPix] = color;
            changesMade = true;
        }
        
        public void setRGB(int x, int y, Color color, Color[] arr) {
            arr[x + y * xPix] = color;
            changesMade = true;
        }
        
        public void setRGB(int x, int y, Color color) {
            layerColor[x + y * xPix] = color;
            changesMade = true;
        }
        
        public Color getRGB(Point point) {
            return layerColor[point.X + point.Y * xPix];
        }
        
        public Color getRGB(int x, int y) {
            return layerColor[x + y * xPix];
        }
        
        public Color getRGB(Point point, Color[] arr) {
            return arr[point.X + point.Y * xPix];
        }
        
        public Color getRGB(int x, int y, Color[] arr) {
            return arr[x + y * xPix];
        }

        public Vector2 toCanvas(Vector2 screenPos) {
            return (Main.camera.toWorld(screenPos) - (pos - dimen/2)) / dimen * pixDimen;
        }
        
        public Vector2 canvasToWorld(Vector2 canvasPos) {
            return (canvasPos * dimen / pixDimen) + (pos - dimen/2);
        }

        public Rectangle canvasToScreen(Rectangle rect) {
            Vector2 p1 = canvasToWorld(new Vector2(rect.X, rect.Y));
            Vector2 p2 = canvasToWorld(new Vector2(rect.X + rect.Width, rect.Y + rect.Height));
            
            Vector2 pos = (p1 + p2) / 2;
            Vector2 dimen = p2 - p1;

            return Main.camera.toScreen(pos, dimen);
        }

        public Point toPixel(Vector2 onCanvas) {
            return new Point((int)Math.Floor(onCanvas.X),(int)Math.Floor(onCanvas.Y));
        }
    }
}