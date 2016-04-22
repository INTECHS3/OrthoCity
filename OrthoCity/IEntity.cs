﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace OrthoCity
{
    interface IEntity
    {
        void LoadContent(ContentManager content);
        void UnloadContent();

        void Update(GameTime gameTime, KeyboardState keyboardState);
        void Draw(SpriteBatch spriteBatch);
    }
}
