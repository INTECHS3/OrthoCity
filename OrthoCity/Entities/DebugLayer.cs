﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace OrthoCity.Entities
{
    class DebugLayer : IEntity
    {
        SpriteFont _font;
        double _refreshRate;
        KeyboardState _keyboardState;

        void IEntity.LoadContent(ContentManager content)
        {
            _font = content.Load<SpriteFont>("debug");
        }

        void IEntity.UnloadContent()
        {
        }

        void IEntity.Update(GameTime gameTime, KeyboardState keyboardState)
        {
            _refreshRate = gameTime.ElapsedGameTime.TotalMilliseconds;
        }

        void IEntity.Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.DrawString(_font, _refreshRate.ToString() + "ms", new Vector2(10, 10), Color.Black);
        }
    }
}
