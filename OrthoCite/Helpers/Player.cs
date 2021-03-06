﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.Maps.Tiled;
using MonoGame.Extended.Animations.SpriteSheets;
using MonoGame.Extended.Sprites;
using MonoGame.Extended.TextureAtlases;
using System.Collections.Generic;

namespace OrthoCite.Helpers
{
    public enum Direction
    {
        NONE,
        LEFT,
        RIGHT,
        UP,
        DOWN,
        ATTACK_TOP,
        ATTACK_LEFT,
        ATTACK_DOWN,
        ATTACK_RIGHT
    }

    public enum TypePlayer
    {
        WithSpriteSheet,
        WithTexture2D
    }

    public enum TypeDeplacement
    {
        WithKey,
        WithDirection
    }

    public class Player
    {

        public delegate void AttackEvent(Player player);
        public event AttackEvent playerAttack;
        public void AttackEventNow()
        {
            if (playerAttack != null)
                playerAttack(this);
        }

        RuntimeData _runtimeData;

        public SpriteSheetAnimator heroAnimations { set; get; }
        public Sprite heroSprite { set; get; }
        public Texture2D heroTexture { set; get; }

        public Vector2 positionVirt { set; get; }
        public Vector2 position { set; get; }
        
        public Direction actualDir { set; get; }
        public Direction lastDir { set; get; }
        public Dictionary<Direction, SpriteSheetAnimationData> spriteFactory { get; set; }
        public TypePlayer typePlayer { set; get; }
        public TypeDeplacement typeDeplacement { get; set; }
        public TiledTileLayer collisionLayer { set; get; }
        

        public int separeFrame { set; get; }
        public int actualFrame { set; get; }
        public int fastFrame { set; get; }
        public int lowFrame { set; get; }
        
        public int tileWidth;
        public int tileHeight;
        public int mapHeight;
        public int mapWidth; 

        public int gidCol { get; set; }


        string _texture;

        public Player(TypePlayer TypePlaYer, RuntimeData runtimeData, string texture)
             : this(TypePlaYer, new Vector2(0,0), runtimeData, texture){} 

        public Player(TypePlayer TypePlaYer, Vector2 PositionVirt, RuntimeData runtimeData, string texture)
        {
            
            positionVirt = PositionVirt;
            typePlayer = TypePlaYer;
            _runtimeData = runtimeData;
            _texture = texture;
            spriteFactory = new Dictionary<Direction, SpriteSheetAnimationData>();
        }


        public void LoadContent(ContentManager content)
        {
            tileHeight = collisionLayer.TileHeight;
            tileWidth = collisionLayer.TileWidth;
            mapHeight = collisionLayer.Height;
            mapWidth = collisionLayer.Width;

            if (typePlayer == TypePlayer.WithSpriteSheet)
            {
                heroTexture = content.Load<Texture2D>(_texture);
                var HeroAtlas = TextureAtlas.Create(heroTexture, 32, 32);
                var HeroWalkingFactory = new SpriteSheetAnimationFactory(HeroAtlas);

                HeroWalkingFactory.Add(Direction.NONE.ToString(), spriteFactory[Direction.NONE]);
                HeroWalkingFactory.Add(Direction.DOWN.ToString(), spriteFactory[Direction.DOWN]);
                HeroWalkingFactory.Add(Direction.LEFT.ToString(), spriteFactory[Direction.LEFT]);
                HeroWalkingFactory.Add(Direction.RIGHT.ToString(), spriteFactory[Direction.RIGHT]);
                HeroWalkingFactory.Add(Direction.UP.ToString(), spriteFactory[Direction.UP]);

                if (spriteFactory.ContainsKey(Direction.ATTACK_TOP)) HeroWalkingFactory.Add(Direction.ATTACK_TOP.ToString(), spriteFactory[Direction.ATTACK_TOP]);
                if (spriteFactory.ContainsKey(Direction.ATTACK_DOWN)) HeroWalkingFactory.Add(Direction.ATTACK_DOWN.ToString(), spriteFactory[Direction.ATTACK_DOWN]);
                if (spriteFactory.ContainsKey(Direction.ATTACK_LEFT)) HeroWalkingFactory.Add(Direction.ATTACK_LEFT.ToString(), spriteFactory[Direction.ATTACK_LEFT]);
                if (spriteFactory.ContainsKey(Direction.ATTACK_RIGHT)) HeroWalkingFactory.Add(Direction.ATTACK_RIGHT.ToString(), spriteFactory[Direction.ATTACK_RIGHT]);
                
               

                heroAnimations = new SpriteSheetAnimator(HeroWalkingFactory);
                heroSprite = heroAnimations.CreateSprite(positionVirt);

                playerAttack += goAttack;
                
            }
            else if (typePlayer == TypePlayer.WithTexture2D)
            {
                heroTexture = content.Load<Texture2D>(_texture);
            }

            actualDir = Direction.NONE;
            lastDir = actualDir;
            position = new Vector2(positionVirt.X * tileWidth, positionVirt.Y * tileHeight);
        }

        public void UpdateThePosition()
        {
            position = new Vector2(positionVirt.X * tileWidth, positionVirt.Y * tileHeight);
        }
       
        public void Draw(SpriteBatch spriteBatch)
        {
            if(typePlayer == TypePlayer.WithSpriteSheet)
            {
                if (lastDir == Direction.LEFT || lastDir == Direction.ATTACK_LEFT) heroSprite.Effect = SpriteEffects.FlipHorizontally;
                else heroSprite.Effect = SpriteEffects.None;
                
                spriteBatch.Draw(heroSprite);
            }
            else if(typePlayer == TypePlayer.WithTexture2D)
            {
                spriteBatch.Draw(heroTexture, position, Color.White);
            }
        }

        public void MoveUpChamp()
        {

            positionVirt += new Vector2(0, -1);
        }

        public void MoveDownChamp()
        {
            positionVirt += new Vector2(0, +1);
        }

        public void MoveLeftChamp()
        {
            positionVirt += new Vector2(-1, 0);
        }

        public void MoveRightChamp()
        {
            positionVirt += new Vector2(+1, 0);
        }

        private void goAttack(Player player)
        {
            if (player.lastDir == Direction.LEFT) player.heroAnimations.Play(Direction.ATTACK_LEFT.ToString());
            else if (player.lastDir == Direction.RIGHT) player.heroAnimations.Play(Direction.ATTACK_RIGHT.ToString());
            else if (player.lastDir == Direction.UP) player.heroAnimations.Play(Direction.ATTACK_TOP.ToString());
            else if (player.lastDir == Direction.DOWN) player.heroAnimations.Play(Direction.ATTACK_DOWN.ToString());
        }

        public void checkMove(KeyboardState keyboardState)
        {
            

            if (_runtimeData.AnswerBox.isVisible) return;


            if (separeFrame == 0)
            {
                
                if (typeDeplacement == TypeDeplacement.WithKey && actualDir == Helpers.Direction.NONE && keyboardState.GetPressedKeys().Length != 0)
                {

                    if (keyboardState.IsKeyDown(Keys.LeftShift)) actualFrame = fastFrame;
                    else actualFrame = lowFrame;

                    if (keyboardState.IsKeyDown(Keys.Down))
                    {
                        if (!ColDown()) { actualDir = Helpers.Direction.DOWN; }
                            lastDir = Helpers.Direction.DOWN;
                    }
                    else if (keyboardState.IsKeyDown(Keys.Up))
                    {
                        if (!ColUp()) { actualDir = Helpers.Direction.UP; }
                        lastDir = Helpers.Direction.UP;
                    }
                    else if (keyboardState.IsKeyDown(Keys.Left))
                    {
                        if (!ColLeft()) { actualDir = Helpers.Direction.LEFT; }
                            lastDir = Helpers.Direction.LEFT;
                    }
                    else if (keyboardState.IsKeyDown(Keys.Right))
                    {
                        if (!ColRight()) { actualDir = Helpers.Direction.RIGHT; } 
                        lastDir = Helpers.Direction.RIGHT;
                    }
                    heroAnimations.Play(lastDir.ToString());

                   separeFrame++;
                }

            }
            else if (separeFrame != 0)
            {
                
                if ((separeFrame > actualFrame + 2 && actualFrame == lowFrame) || (separeFrame == fastFrame && separeFrame >= actualFrame))
                {
                    if (actualDir == Helpers.Direction.DOWN && !ColDown()) MoveDownChamp();
                    if (actualDir == Helpers.Direction.UP && !ColUp()) MoveUpChamp();
                    if (actualDir == Helpers.Direction.LEFT && !ColLeft()) MoveLeftChamp();
                    if (actualDir == Helpers.Direction.RIGHT && !ColRight()) MoveRightChamp();


                   position = new Vector2(positionVirt.X * tileWidth, positionVirt.Y * tileHeight);


                    actualDir = Helpers.Direction.NONE;
                    separeFrame = 0;
                }
                else
                {
                    if (actualDir == Helpers.Direction.DOWN)
                    {
                        position += new Vector2(0, tileHeight / (actualFrame));
                    }
                    if (actualDir == Helpers.Direction.UP)
                    {
                        position += new Vector2(0, -(tileHeight / (actualFrame)));
                    }
                    if (actualDir == Helpers.Direction.LEFT)
                    {
                        position += new Vector2(-(tileWidth / (actualFrame )), 0);
                    }
                    if (actualDir == Helpers.Direction.RIGHT)
                    {
                        position += new Vector2(tileWidth / (actualFrame), 0);
                    }
                    separeFrame++;
                }
            }

            
        }

        public void MooveChamp(Direction d)
        {
            if(separeFrame == 0)
            {
                actualFrame = lowFrame;

                if (typeDeplacement == TypeDeplacement.WithDirection)
                {

                    if (d == Direction.DOWN)
                    {
                        if (!ColDown()) { actualDir = Helpers.Direction.DOWN; heroAnimations.Play(Helpers.Direction.DOWN.ToString()); }
                            lastDir = Helpers.Direction.DOWN;
                    }
                    else if (d == Direction.UP)
                    {
                        if (!ColUp()) { actualDir = Helpers.Direction.UP; heroAnimations.Play(Helpers.Direction.UP.ToString());} 
                        lastDir = Helpers.Direction.UP;
                        
                    }
                    else if (d == Direction.LEFT)
                    {
                        if (!ColLeft()) { actualDir = Helpers.Direction.LEFT; heroAnimations.Play(Helpers.Direction.LEFT.ToString()); }
                            lastDir = Helpers.Direction.LEFT;
                        
                    }
                    else if (d == Direction.RIGHT)
                    {
                        if (!ColRight()) { actualDir = Helpers.Direction.RIGHT; heroAnimations.Play(Helpers.Direction.RIGHT.ToString()); }
                            lastDir = Helpers.Direction.RIGHT;
                      
                    }

                    separeFrame++;

                }
            }

        }

        public bool ColUp()
        {
            if (positionVirt.Y <= 0) return true;
            foreach (TiledTile i in collisionLayer.Tiles)
            {
                if (i.X == positionVirt.X && i.Y == positionVirt.Y - 1 && i.Id == gidCol) return true;
                if (_runtimeData.Player != null && _runtimeData.Player != this && _runtimeData.Player.positionVirt.X == positionVirt.X && _runtimeData.Player.positionVirt.Y == positionVirt.Y - 1) return true;
                if(_runtimeData.Map != null) _runtimeData.Map.checkIfWeLaunchInstance(i);
                if (_runtimeData.DoorGame != null && _runtimeData.DoorGame.CheckColUp(i)) return true;
                if (_runtimeData.Rearranger != null && _runtimeData.Rearranger.CheckColUp(i)) return true;
                if (_runtimeData.ThrowGame != null) _runtimeData.ThrowGame.CheckBadTile(i);
                if (_runtimeData.StopGame != null && _runtimeData.StopGame.CheckColUp(i)) return true;

            }


            foreach(KeyValuePair<ListPnj,PNJ> i in _runtimeData.PNJ)
            {
                if (positionVirt.X == i.Value.PNJPlayer.positionVirt.X && positionVirt.Y - 1 == i.Value.PNJPlayer.positionVirt.Y) return true;
            }

            return false;
        }

        public bool ColDown()
        {

            if (positionVirt.Y >= mapHeight - 1) return true;
            foreach (TiledTile i in collisionLayer.Tiles)
            {
                if (i.X == positionVirt.X && i.Y == positionVirt.Y + 1 && i.Id == gidCol) return true;
                if (_runtimeData.Player != null && _runtimeData.Player != this && _runtimeData.Player.positionVirt.X == positionVirt.X && _runtimeData.Player.positionVirt.Y == positionVirt.Y + 1) return true;
                if (_runtimeData.ThrowGame != null) _runtimeData.ThrowGame.CheckBadTile(i);
                if (_runtimeData.StopGame != null && _runtimeData.StopGame.CheckColDown(i)) return true;
            }
            foreach (KeyValuePair<ListPnj, PNJ> i in _runtimeData.PNJ)
            {
                if (positionVirt.X == i.Value.PNJPlayer.positionVirt.X && positionVirt.Y + 1 == i.Value.PNJPlayer.positionVirt.Y) return true;
            }
           
            return false;
        }

        public bool ColLeft()
        {
            if (positionVirt.X <= 0) return true;
            foreach (TiledTile i in collisionLayer.Tiles)
            {
                if (i.X == positionVirt.X - 1 && i.Y == positionVirt.Y && i.Id == gidCol) return true;
                if (_runtimeData.Player != null && _runtimeData.Player != this && _runtimeData.Player.positionVirt.X == positionVirt.X - 1 && _runtimeData.Player.positionVirt.Y == positionVirt.Y) return true;
                if (_runtimeData.ThrowGame != null) _runtimeData.ThrowGame.CheckBadTile(i);
                if (_runtimeData.DoorGame != null && _runtimeData.DoorGame.CheckColLeft(i)) return true;
            }
            foreach (KeyValuePair<ListPnj, PNJ> i in _runtimeData.PNJ)
            {
                if (positionVirt.X - 1 == i.Value.PNJPlayer.positionVirt.X && _runtimeData.Player.positionVirt.Y == i.Value.PNJPlayer.positionVirt.Y) return true;
            }
            return false;
        }

        public bool ColRight()
        {
            if (positionVirt.X >= mapWidth - 1) return true;
            foreach (TiledTile i in collisionLayer.Tiles)
            {
                if (i.X == positionVirt.X + 1 && i.Y == positionVirt.Y && i.Id == gidCol) return true;
                if (_runtimeData.Player != null && _runtimeData.Player != this && _runtimeData.Player.positionVirt.X == positionVirt.X + 1 && _runtimeData.Player.positionVirt.Y == positionVirt.Y) return true;
                if (_runtimeData.ThrowGame != null) _runtimeData.ThrowGame.CheckBadTile(i);
                if (_runtimeData.DoorGame != null && _runtimeData.DoorGame.CheckColRight(i)) return true;
                if (_runtimeData.StopGame != null && _runtimeData.StopGame.CheckColRight(i)) return true;

            }
            foreach (KeyValuePair<ListPnj, PNJ> i in _runtimeData.PNJ)
            {
                if (positionVirt.X + 1 == i.Value.PNJPlayer.positionVirt.X && _runtimeData.Player.positionVirt.Y == i.Value.PNJPlayer.positionVirt.Y) return true;
            }
            return false;
        }


    }
}
