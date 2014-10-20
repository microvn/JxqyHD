﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Engine
{
    public class Player : Character
    {
        private int _doing;
        private int _desX;
        private int _desY;
        private int _belong;
        private int _fight;
        private int _money;
        private Dictionary<int, Utils.LevelDetail> _levelIni;
        private float _totalNonFightingTime;
        private const float MaxNonFightingTime = 7f;

        #region Public properties

        public Dictionary<int, Utils.LevelDetail> LevelIni
        {
            get { return _levelIni; }
            set { _levelIni = value; }
        }

        public int Money
        {
            get { return _money; }
            set { _money = value; }
        }

        public int Fight
        {
            get { return _fight; }
            set { _fight = value; }
        }

        public int Belong
        {
            get { return _belong; }
            set { _belong = value; }
        }

        public int DesY
        {
            get { return _desY; }
            set { _desY = value; }
        }

        public int DesX
        {
            get { return _desX; }
            set { _desX = value; }
        }

        public int Doing
        {
            get { return _doing; }
            set { _doing = value; }
        }

        #endregion

        public Player() { }

        public Player(string filePath)
            : base(filePath)
        {

        }

        private MouseState _lastMouseState;
        private KeyboardState _lastKeyboardState;
        public override void Update(GameTime gameTime)
        {
            var mouseState = Mouse.GetState();
            var keyboardState = Keyboard.GetState();
            var mouseScreenPosition = new Vector2(mouseState.X, mouseState.Y);
            var mouseWorldPosition = Globals.TheCarmera.ToWorldPosition(mouseScreenPosition);
            mouseWorldPosition = Map.ToPixelPosition(Map.ToTilePosition(mouseWorldPosition));

            if (mouseState.LeftButton == ButtonState.Pressed)
            {
                if(keyboardState.IsKeyDown(Keys.LeftShift) ||
                    keyboardState.IsKeyDown(Keys.RightShift))
                    RunTo(mouseWorldPosition);
                else if(keyboardState.IsKeyDown(Keys.LeftAlt) ||
                    keyboardState.IsKeyDown(Keys.RightAlt))
                    JumpTo(mouseWorldPosition);
                else if(keyboardState.IsKeyDown(Keys.LeftControl) ||
                    keyboardState.IsKeyDown(Keys.RightControl))
                    Attacking(mouseWorldPosition - PositionInWorld);
                else WalkTo(mouseWorldPosition);
            }
            if (mouseState.RightButton == ButtonState.Pressed &&
                _lastMouseState.RightButton == ButtonState.Released)
            {
                UseMagic(FlyIni, mouseWorldPosition);
            }
            if (keyboardState.IsKeyDown(Keys.V) &&
                _lastKeyboardState.IsKeyUp(Keys.V))
            {
                if(IsSitting())Standing();
                else Sitdown();
            }

            _lastMouseState = mouseState;
            _lastKeyboardState = keyboardState;
            base.Update(gameTime);
        }

        public new void Draw(SpriteBatch spriteBatch, IEnumerable<Npc> npcsInView)
        {
            var texture = GetCurrentTexture();
            if(texture == null) return;

            var data = new Color[texture.Width*texture.Height];
            texture.GetData(data);
            texture = new Texture2D(texture.GraphicsDevice, texture.Width, texture.Height);
            texture.SetData(data);

            var tilePosition = new Vector2(MapX, MapY);
            var start = tilePosition - new Vector2(3, 15);
            var end = tilePosition + new Vector2(3, 15);
            if (start.X < 0) start.X = 0;
            if (start.Y < 0) start.Y = 0;
            if (end.X > Globals.TheMap.MapColumnCounts) end.X = Globals.TheMap.MapColumnCounts;
            if (end.Y > Globals.TheMap.MapRowCounts) end.Y = Globals.TheMap.MapRowCounts;
            var textureRegion = new Rectangle();
            var region = RegionInWorld;
            foreach (var npc in npcsInView)
            {
                if(npc.MapY > MapY)
                    Collider.MakePixelCollidedTransparent(region, texture, npc.RegionInWorld, npc.GetCurrentTexture());
            }
            for (var y = (int)start.Y; y < (int)end.Y; y++)
            {
                for (var x = (int)start.X; x < (int)end.X; x++)
                {
                    Texture2D tileTexture;
                    if (y > MapY)
                    {
                        tileTexture = Globals.TheMap.GetTileTextureAndRegion(x, y, 1, ref textureRegion);
                        Collider.MakePixelCollidedTransparent(region, texture, textureRegion, tileTexture);
                    }
                    tileTexture = Globals.TheMap.GetTileTextureAndRegion(x, y, 2, ref textureRegion);
                    Collider.MakePixelCollidedTransparent(region, texture, textureRegion, tileTexture);
                }
            }
            Draw(spriteBatch, texture);
        }
    }
}
