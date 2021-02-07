using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PixelArt {
    public class UIScreen {
        
        public List<UIElement> uiElements = new List<UIElement>();
        public List<UITransition> uiTransitions = new List<UITransition>();


        public virtual void update(MouseInfo mouse, KeyInfo keys, float deltaTime) {

            for (int i = uiTransitions.Count - 1; i >= 0; i--) {
                UITransition transition = uiTransitions[i];

                if (transition.deleteFlag) {
                    uiTransitions.RemoveAt(i);
                    continue;
                }

                transition.update(deltaTime);
            }
            
            foreach (var element in uiElements) {
                element.update(mouse, keys, deltaTime);
            }
        }

        public virtual void renderUnder(Game game, SpriteBatch spriteBatch) { }
        public virtual void renderOver(Game game, SpriteBatch spriteBatch) { }

        public virtual void render(Game game, SpriteBatch spriteBatch) {
            // Rendering UI
            
            game.GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin(SpriteSortMode.Deferred,
                BlendState.NonPremultiplied,
                SamplerState.PointClamp,
                null, null, null, null);

            renderUnder(game, spriteBatch);

            foreach (var element in uiElements) {
                element.render(spriteBatch);
            }

            renderOver(game, spriteBatch);
            spriteBatch.End();
        }
    }


    /*public class MainMenuScreen : UIScreen {

        public Vector2 centerMouseOff;
        
        public MainMenuScreen() {
            uiElements.Add(new UIButton(() => Runner.uiScreen = null, new Vector2(300, 300), new Vector2(200, 100), "Start Game"));
        }

        public override void update(MouseInfo mouse, KeyInfo keys, float deltaTime) {
            base.update(mouse, keys, deltaTime);

            centerMouseOff = mouse.pos - (new Vector2(1920, 1080) / 2);
        }

        public override void renderUnder(Game game, SpriteBatch spriteBatch) {
            
            float scale = 15;
            Texture2D body = Textures.get("EyeBody");
            Texture2D eyeOut = Textures.get("EyeOuter");
            Texture2D eyeIn = Textures.get("EyeCenter");
            Vector2 center = new Vector2(1920, 1080) / 2;
            spriteBatch.Draw(body, Util.center(center, Util.textureVec(body) * scale), Color.White);

            Vector2 off = centerMouseOff / 20;
            off = Util.setMag(off, Math.Min(50, Util.mag(off)));
            Vector2 eyePos = center + off;
            spriteBatch.Draw(eyeOut, Util.center(eyePos, Util.textureVec(eyeOut) * scale), Color.White);
            spriteBatch.Draw(eyeIn, Util.center(eyePos + off / 2, Util.textureVec(eyeIn) * scale), Color.White);
        }
    }
    
    public class LevelSelectScreen : UIScreen {

        public LevelSelectScreen() {
            int perRow = 5;

            for (int i = 0; i < Runner.levels.Length; i++) {
                int r = i / perRow;
                int c = i % perRow;

                int index = i;
                uiElements.Add(new UIButton(() => {
                    Runner.changeLevel(Runner.levels[index]);
                    Runner.uiScreen = null;
                }, new Vector2(300, 300) + new Vector2(170, 170) * new Vector2(c, r), new Vector2(150, 150), "Level " + (i + 1)));
            }
        }

    }*/
}