﻿using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Engine.Gui
{
    public class TextGui : GuiItem
    {
        private int _startIndex;
        private int _endIndex;
        private readonly LinkedList<Info> _drawInfo = new LinkedList<Info>(); 
        public SpriteFont Font { set; get; }

        public string Text
        {
            set
            {
                TextStream = new StringBuilder(value);
                _startIndex = _endIndex = 0;
                Caculate();
            }
            get { return TextStream.ToString(); }
        }
        private StringBuilder TextStream { set; get; }
        public Color DefaultColor { get; set; }

        public TextGui(GuiItem parent, 
            Vector2 position,
            int width, 
            int height, 
            SpriteFont font, 
            string text)
        {
            Init(parent, position, width, height, font, text, Color.Black);
        }

        public TextGui(GuiItem parent,
            Vector2 position,
            int width,
            int height,
            SpriteFont font,
            string text,
            Color defaultColor)
        {
            Init(parent, position, width, height, font, text, defaultColor);
        }

        public void Init(GuiItem parent,
            Vector2 position,
            int width,
            int height,
            SpriteFont font,
            string text,
            Color defaultColor)
        {
            Parent = parent;
            Position = position;
            Width = width;
            Height = height;
            Font = font;
            DefaultColor = defaultColor;
            Text = text;// must be the last, because it invoke Caculate()
        }

        private bool IsReachRight(float x)
        {
            return x  >= Width;
        }

        private bool IsReachBottom(float y)
        {
            return y + Font.LineSpacing >= Height;
        }

        public bool NextPage()
        {
            return Caculate();
        }

        public bool Caculate()
        {
            _drawInfo.Clear();
            try
            {
                if (TextStream == null &&
                    _endIndex >= TextStream.Length)
                    return false;

                _startIndex = _endIndex;
                var x = 0f;
                var y = 0f;
                var endIndex = TextStream.Length;
                while (_endIndex < endIndex)
                {
                    var drawText = TextStream[_endIndex].ToString();
                    if (drawText == "<")
                    {
                        var text = new StringBuilder();
                        while (TextStream[++_endIndex] != '>')
                        {
                            text.Append(TextStream[_endIndex]);
                        }

                        switch (text.ToString())
                        {
                            case "color=Red":
                                DefaultColor = Color.Red;
                                break;
                            case "color=Black":
                                DefaultColor = Color.Black;
                                break;
                            case "enter":
                                y += Font.LineSpacing;
                                x = 0;
                                if (IsReachBottom(y))
                                {
                                    _endIndex++;
                                    return true;
                                }
                                break;
                       }
                    }
                    else if (drawText == "\n")
                    {
                        y += Font.LineSpacing;
                        x = 0;
                        if (IsReachBottom(y))
                        {
                            _endIndex++;
                            return true;
                        }
                    }
                    else
                    {
                        var stringWidth = Font.MeasureString(drawText).X;
                        if (IsReachRight(x + stringWidth))
                        {
                            y += Font.LineSpacing;
                            if (IsReachBottom(y))
                                return true;
                            x = 0f;
                        }
                        _drawInfo.AddLast(new Info(
                            drawText,
                            ScreenPosition + new Vector2(x, y),
                            DefaultColor));
                        x += stringWidth;
                    }
                    _endIndex++;
                }
            }
            catch (Exception)
            {
                _drawInfo.Clear();
                _endIndex = TextStream.Length;
                Log.LogMessageToFile("String [" + TextStream +"] format is bad!");
                return false;
            }
            return true;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!IsShow) return;
            base.Draw(spriteBatch);
            foreach (var info in _drawInfo)
            {
                spriteBatch.DrawString(Font, info.Text, info.Position, info.DrawColor);
            }
        }

        private struct Info
        {
            public string Text;
            public Vector2 Position;
            public Color DrawColor;

            public Info(string text, Vector2 position, Color drawColor)
            {
                Text = text;
                Position = position;
                DrawColor = drawColor;
            }
        }
    }
}