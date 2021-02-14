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
                SamplerState.PointClamp);

            renderUnder(game, spriteBatch);

            foreach (var element in uiElements) {
                element.render(spriteBatch);
            }

            renderOver(game, spriteBatch);
            spriteBatch.End();
        }
    }
}