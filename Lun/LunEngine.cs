global using static Lun.LunEngine;
global using System;
global using System.Collections.Generic;
global using System.Linq;

using Lun.SFML.Graphics;
using Lun.SFML.Window;
using Lun.Shapes;
using System.Reflection;

namespace Lun
{
    public static class LunEngine
    {
        // Dispositivos
        static readonly Sprite                _sprite      = new Sprite();
        static readonly LargeSprite           _spritelarge = new LargeSprite();
        static readonly RectangleShape        rec          = new RectangleShape();
        static readonly CircleShape           cir          = new CircleShape();
        static readonly RoundedRectangleShape roundrec     = new RoundedRectangleShape();
        static readonly LineShape             lineshape    = new LineShape();

        internal static RenderTarget target;

        static List<RenderTarget> renders = new List<RenderTarget>(); 
        static List<Camera2D>     cams    = new List<Camera2D>();

        static Camera2D currentCamera;


        // Vertices
        static Vertex[] lines     = new Vertex[2];
        static Vertex[] gradients = new Vertex[4];


        // Font
        internal static Font gameFont;
        internal static Text _text;


        /// <summary>
        /// Desenha a textura
        /// </summary>
        /// <param name="target"></param>
        /// <param name="texture"></param>
        /// <param name="destination"></param>
        /// <param name="source"></param>
        /// <param name="origin"></param>
        /// <param name="color"></param>
        /// <param name="states"></param>
        public static void DrawTexture(Texture texture, Rectangle destination, Rectangle source, Color color, Vector2 origin, float rotation, RenderStates states)
        {
            if (texture == null) return;

            if (texture.type == TextureTypes.Normal)
            {
                var scale = destination.size / source.size;

                _sprite.Texture = texture.GetTexture();
                _sprite.Position = destination.position.Round();
                _sprite.Scale = scale;
                _sprite.TextureRect = (IntRect)source;
                _sprite.Color = color;
                _sprite.Origin = origin;
                _sprite.Rotation = rotation;
                target.Draw(_sprite, states);
            }
            else
            {
                var scale = destination.size / source.size;

                _spritelarge.Texture = texture.GetLargeTexture();
                _spritelarge.Position = destination.position.Round();
                _spritelarge.Scale = scale;
                _spritelarge.Color = color;
                _spritelarge.Origin = origin;
                _spritelarge.Rotation = rotation;
                target.Draw(_spritelarge, states);
            }
        }

        /// <summary>
        /// Desenha a textura
        /// </summary>
        /// <param name="target"></param>
        /// <param name="texture"></param>
        /// <param name="destination"></param>
        /// <param name="source"></param>
        /// <param name="color"></param>
        /// <param name="origin"></param>
        /// <param name="rotation"></param>
        public static void DrawTexture(Texture texture, Rectangle destination, Rectangle source, Color color, Vector2 origin, float rotation)
            => DrawTexture(texture, destination, source, color, origin, rotation, RenderStates.Default);

        /// <summary>
        /// Desenha a textura
        /// </summary>
        /// <param name="target"></param>
        /// <param name="texture"></param>
        /// <param name="destination"></param>
        /// <param name="source"></param>
        /// <param name="color"></param>
        /// <param name="origin"></param>
        public static void DrawTexture(Texture texture, Rectangle destination, Rectangle source, Color color, Vector2 origin)
            => DrawTexture(texture, destination, source, color, origin, 0);

        /// <summary>
        /// Desenha a textura
        /// </summary>
        /// <param name="target"></param>
        /// <param name="texture"></param>
        /// <param name="destination"></param>
        /// <param name="source"></param>
        /// <param name="color"></param>
        public static void DrawTexture(Texture texture, Rectangle destination, Rectangle source, Color color)
            => DrawTexture(texture, destination, source, color, Vector2.Zero);

        /// <summary>
        /// Desenha a textura
        /// </summary>
        /// <param name="target"></param>
        /// <param name="texture"></param>
        /// <param name="destination"></param>
        /// <param name="source"></param>
        public static void DrawTexture(Texture texture, Rectangle destination, Rectangle source)
            => DrawTexture(texture, destination, source, Color.White);

        /// <summary>
        /// Desenha a textura
        /// </summary>
        /// <param name="target"></param>
        /// <param name="texture"></param>
        /// <param name="destination"></param>
        /// <param name="source"></param>
        public static void DrawTexture(Texture texture, Rectangle destination)
            => DrawTexture(texture, destination, new Rectangle(Vector2.Zero, texture.size), Color.White);

        /// <summary>
        /// Desenha a textura
        /// </summary>
        /// <param name="target"></param>
        /// <param name="texture"></param>
        /// <param name="destination"></param>
        public static void DrawTexture(Texture texture, Rectangle destination, Color color)
            => DrawTexture(texture, destination, new Rectangle(Vector2.Zero, texture.size), color);

        /// <summary>
        /// Desenha a textura
        /// </summary>
        /// <param name="target"></param>
        /// <param name="texture"></param>
        public static void DrawTexture(Texture texture, Vector2 position)
            => DrawTexture(texture, new Rectangle(position, texture.size));

        /// <summary>
        /// Desenha a textura
        /// </summary>
        /// <param name="target"></param>
        /// <param name="texture"></param>
        public static void DrawTexture(Texture texture, Vector2 position, Color color)
            => DrawTexture(texture, new Rectangle(position, texture.size), color);

        public static void DrawRenderTexture(RenderTexture render, Vector2 position, Color color)
        {            
            render.Texture.Smooth = true;
            _sprite.Texture = render.Texture;
            _sprite.Position = position;
            _sprite.Scale = Vector2.One;
            _sprite.TextureRect = new IntRect(0,0, (int)render.Size.X, (int)render.Size.Y);
            _sprite.Color = color;
            _sprite.Origin = Vector2.Zero;
            _sprite.Rotation = 0;
            target.Draw(_sprite, RenderStates.Default);
        }

        /// <summary>
        /// Desenha o texto
        /// </summary>
        /// <param name="target"></param>
        /// <param name="text"></param>
        /// <param name="position"></param>
        /// <param name="color"></param>
        /// <param name="outlineThickness"></param>
        /// <param name="outlineColor"></param>
        public static void DrawText(string text, int charactersize, Vector2 position, Color color, bool shadow = false, bool round = true)
        {
            if (_text == null)
                throw new Exception("A font não foi carregada!");

            if (text == null || text.Trim().Length == 0)
                return;

            _text.DisplayedString = text;
            _text.CharacterSize = (uint)charactersize;            
            _text.OutlineThickness = 0;

            if (shadow)
            {
                _text.FillColor = new Color(0, 0, 0, Math.Min((byte)200, color.A));
                _text.Position = (round ? position.ToInt() : position) + new Vector2(1);
                target.Draw(_text);
            }

            _text.Position = round ? position.ToInt() : position;
            _text.FillColor = color;
            target.Draw(_text);
        }

        /// <summary>
        /// Desenha o texto colorido
        /// </summary>
        /// <param name="target"></param>
        /// <param name="text"></param>
        /// <param name="charactersize"></param>
        /// <param name="position"></param>
        /// <param name="outlineThickness"></param>
        /// <param name="outlineColor"></param>
        public static void DrawTextColor(string text, int charactersize, Vector2 position)
        {
            var color = Color.White;

            if (text.Contains("clr("))
            {
                var lines = new List<string>();
                var colorList = new List<string>();
                float off = 0;
                while (text.Length > 0 && text.Contains("clr(") && text.Contains(")"))
                {
                    var find = text.IndexOf("clr(");
                    var findEnd = text.Substring(find).IndexOf(")");
                    var colorKey = text.Substring(find + "clr(".Length, findEnd - 4).Split(",");

                    DrawText(text.Substring(0, find), charactersize,
                        position + new Vector2(off, 0), color);

                    if (colorKey.Length == 3)
                        color = new Color(byte.Parse(colorKey[0]), byte.Parse(colorKey[1]), byte.Parse(colorKey[2]));

                    if (colorKey.Length == 4)
                        color = new Color(byte.Parse(colorKey[0]), byte.Parse(colorKey[1]), byte.Parse(colorKey[2]), byte.Parse(colorKey[3]));

                    off += GetTextWidth(text.Substring(0, find), (uint)charactersize);

                    text = text.Remove(0, find + findEnd + 1);
                }

                if (text.Length > 0)
                    DrawText(text, charactersize, position + new Vector2(off, 0), color);
            }
            else
                DrawText(text, charactersize, position, color);
        }

        /// <summary>
        /// Desenha um degrade - QUADS vertex
        /// </summary>
        /// <param name="target"></param>
        /// <param name="pos"></param>
        /// <param name="color"></param>
        public static void DrawGradient(Vector2[] pos, Color[] color)
        {
            for (int i = 0; i < 4; i++)
                gradients[i] = new Vertex(pos[i], color[i]);

            target.Draw(gradients, PrimitiveType.Quads);
        }

        /// <summary>
        /// Desenha um degrade - QUADS vertex
        /// </summary>
        /// <param name="target"></param>
        /// <param name="pos1"></param>
        /// <param name="color1"></param>
        /// <param name="pos2"></param>
        /// <param name="color2"></param>
        /// <param name="pos3"></param>
        /// <param name="color3"></param>
        /// <param name="pos4"></param>
        /// <param name="color4"></param>
        public static void DrawGradient(Vector2 pos1, Color color1, Vector2 pos2, Color color2, Vector2 pos3, Color color3,
            Vector2 pos4, Color color4)
            => DrawGradient(new Vector2[] { pos1, pos2, pos3, pos4 }, new Color[] { color1, color2, color3, color4 });

        /// <summary>
        /// Desenha uma linha
        /// </summary>
        /// <param name="target"></param>
        /// <param name="pos1"></param>
        /// <param name="color1"></param>
        /// <param name="pos2"></param>
        /// <param name="color2"></param>
        public static void DrawLine(Vector2 pos1, Color color1, Vector2 pos2, Color color2)
        {
            lines[0] = new Vertex(pos1 + new Vector2(.5f), color1);
            lines[1] = new Vertex(pos2 + new Vector2(.5f), color2);

            target.Draw(lines, 0, 2, PrimitiveType.Lines);
        }

        /// <summary>
        /// Desenha uma linha
        /// </summary>
        /// <param name="target"></param>
        /// <param name="pos1"></param>
        /// <param name="pos2"></param>
        /// <param name="color"></param>
        public static void DrawLine(Vector2 pos1, Vector2 pos2, Color color)
            => DrawLine(pos1, color, pos2, color);

        public static void DrawLineShape(Vector2 pos1, Vector2 pos2, Color color, float thickness = 1f)
        {
            var angle = MathF.Atan2(pos2.y - pos1.y, pos2.x - pos1.x) * 180f / MathF.PI;
            var h = MathF.Sqrt(MathF.Pow(pos1.x - pos2.x, 2) +
                MathF.Pow(pos1.y - pos2.y, 2f));

            rec.FillColor = color;
            rec.Position = pos1;
            rec.Origin = new Vector2(0, thickness / 2f);
            rec.Size = new Vector2(h, thickness);
            rec.Rotation = angle;
            rec.OutlineThickness = 0;
            target.Draw(rec);
        }

        /// <summary>
        /// Desenha um retângulo
        /// </summary>
        /// <param name="target"></param>
        /// <param name="position"></param>
        /// <param name="size"></param>
        /// <param name="fillColor"></param>
        /// <param name="outlineThickness"></param>
        /// <param name="outlineColor"></param>
        public static void DrawRectangle(Vector2 position, Vector2 size, Color fillColor, float outlineThickness, Color outlineColor)
        {
            rec.Position = position.Floor();
            rec.Size = size;
            rec.FillColor = fillColor;
            rec.OutlineThickness = outlineThickness;
            rec.OutlineColor = outlineColor;
            rec.Rotation = 0;
            target.Draw(rec);
        }

        /// <summary>
        /// Desenha um retângulo
        /// </summary>
        /// <param name="target"></param>
        /// <param name="position"></param>
        /// <param name="size"></param>
        /// <param name="fillColor"></param>
        public static void DrawRectangle(Vector2 position, Vector2 size, Color fillColor)
            => DrawRectangle(position, size, fillColor, 0, Color.Transparent);

        /// <summary>
        /// Desenha um retângulo arredondado
        /// </summary>
        /// <param name="target"></param>
        /// <param name="position"></param>
        /// <param name="size"></param>
        /// <param name="fillColor"></param>
        /// <param name="outlineThickness"></param>
        /// <param name="outlineColor"></param>
        public static void DrawRoundedRectangle(Vector2 position, Vector2 size, Color fillColor, float radius, uint pointcount, float outlineThickness, Color outlineColor)
        {
            if (radius == 0)
                DrawRectangle(position, size, fillColor, outlineThickness, outlineColor);

            roundrec.Position = position.Floor();
            roundrec.Size = size;
            roundrec.FillColor = fillColor;
            roundrec.RadiusTop = radius;
            roundrec.RadiusBottom = radius;
            roundrec.cornerPointCount = pointcount;
            roundrec.OutlineThickness = outlineThickness;
            roundrec.OutlineColor = outlineColor;
            target.Draw(roundrec);
        }

        /// <summary>
        /// Desenha um retângulo arredondado
        /// </summary>
        /// <param name="target"></param>
        /// <param name="position"></param>
        /// <param name="size"></param>
        /// <param name="fillColor"></param>
        /// <param name="radius"></param>
        /// <param name="pointcount"></param>
        public static void DrawRoundedRectangle(Vector2 position, Vector2 size, Color fillColor, float radius, uint pointcount)
            => DrawRoundedRectangle(position, size, fillColor, radius, pointcount, 0, Color.Transparent);

        /// <summary>
        /// Desenha um retângulo arredondado
        /// </summary>
        /// <param name="target"></param>
        /// <param name="position"></param>
        /// <param name="size"></param>
        /// <param name="fillColor"></param>
        /// <param name="outlineThickness"></param>
        /// <param name="outlineColor"></param>
        public static void DrawRoundedRectangle(Vector2 position, Vector2 size, Color fillColor, float radius_top, float radius_bottom,
            uint pointcount, float outlineThickness, Color outlineColor)
        {
            roundrec.Position = position.Floor();
            roundrec.Size = size;
            roundrec.FillColor = fillColor;
            roundrec.RadiusTop = radius_top;
            roundrec.RadiusBottom = radius_bottom;
            roundrec.cornerPointCount = pointcount;
            roundrec.OutlineThickness = outlineThickness;
            roundrec.OutlineColor = outlineColor;
            target.Draw(roundrec);
        }

        public static void DrawRoundedRectangle(Vector2 position, Vector2 size, Color fillColor, float radius_top, float radius_bottom, uint pointcount)
            => DrawRoundedRectangle(position, size, fillColor, radius_top, radius_bottom, pointcount, 0, Color.Transparent);

        /// <summary>
        /// Desenha um circulo
        /// </summary>
        /// <param name=""></param>
        /// <param name="position"></param>
        /// <param name="radius"></param>
        /// <param name="fillColor"></param>
        /// <param name="outlineThickness"></param>
        /// <param name="outlineColor"></param>
        public static void DrawCircle(Vector2 position, float radius, Color fillColor, float outlineThickness, Color outlineColor)
        {
            cir.SetPointCount(360);
            cir.Position = position.Floor();
            cir.Radius = radius;
            cir.Origin = new Vector2(radius);
            cir.FillColor = fillColor;
            cir.OutlineThickness = outlineThickness;
            cir.OutlineColor = outlineColor;
            target.Draw(cir);
        }

        public static void DrawCircle(Vector2 position, float radius, Color fillColor)
            => DrawCircle(position, radius, fillColor, 0, Color.Transparent);

        /// <summary>
        /// Comprimento do Texto
        /// </summary>
        /// <param name="text"></param>
        /// <param name="characterSize"></param>
        /// <returns></returns>
        public static float GetTextWidth(string text, uint characterSize = 11)
        {
            if (text.Trim().Length == 0)
                return 0;

            _text.CharacterSize = characterSize;
            _text.DisplayedString = text;

            return _text.FindCharacterPos((uint)text.Length).X.Floor();
        }

        /// <summary>
        /// Comprimento do Texto
        /// </summary>
        /// <param name="text"></param>
        /// <param name="characterSize"></param>
        /// <returns></returns>
        public static float GetTextHeight(string text, uint characterSize = 11)
        {
            if (text.Trim().Length == 0)
                return 0;

            _text.CharacterSize = characterSize;
            _text.DisplayedString = text;

            return _text.GetLocalBounds().Height;
        }

        /// <summary>
        /// Word Wrap
        /// </summary>
        /// <param name="text"></param>
        /// <param name="width"></param>
        /// <param name="characterSize"></param>
        /// <returns></returns>
        public static string[] GetWordWrap(string text, int width, uint characterSize = 11)
        {
            var collection = new List<string>();
            if (text.Length > 0)
            {
                while (text.Contains("clr(") && text.Contains(")"))
                {
                    var find = text.IndexOf("clr(");
                    var findEnd = text.Substring(find).IndexOf(")");

                    text = text.Remove(find, findEnd + 1);
                }

                var words = text.Split();
                string line = "";
                foreach (var i in words)
                {
                    if (i == "/n")
                    {
                        collection.Add(line.Trim());
                        line = "";
                        continue;
                    }

                    if (GetTextWidth(line + i, characterSize) > width)
                    {
                        collection.Add(line.Trim());
                        line = i + " ";
                    }
                    else
                        line += i + " ";
                }
                if (line.Length > 0) collection.Add(line.Trim());
            }
            return collection.ToArray();
        }

        /// <summary>
        /// Word Wrap
        /// </summary>
        /// <param name="text"></param>
        /// <param name="width"></param>
        /// <param name="characterSize"></param>
        /// <returns></returns>
        public static string[] GetWordWrapColor(string text, int width, uint characterSize = 11)
        {
            var collection = new List<string>();
            if (text.Length > 0)
            {
                var words = text.Split();
                string line = "";
                string linetrue = "";
                var copyColor = "";
                foreach (var i in words)
                {
                    //float off = 0;
                    var wordTrue = i;

                    if (i == "/n")
                    {
                        collection.Add(linetrue.Trim());
                        linetrue = "";
                        line = "";
                        continue;
                    }

                    while (wordTrue.Contains("clr(") && wordTrue.Contains(")"))
                    {
                        var find = wordTrue.IndexOf("clr(");
                        var findEnd = wordTrue.Substring(find).IndexOf(")");
                        copyColor = wordTrue.Substring(find, findEnd + 1);
                        wordTrue = wordTrue.Remove(find, findEnd + 1);
                    }

                    if (GetTextWidth(line + wordTrue, characterSize) > width)
                    {
                        collection.Add(linetrue.Trim());
                        line = wordTrue + " ";
                        linetrue = copyColor + i + " ";
                    }
                    else
                    {
                        line += wordTrue + " ";
                        linetrue += i + " ";
                    }
                }
                if (linetrue.Length > 0) collection.Add(linetrue.Trim());
            }
            return collection.ToArray();
        }

        /// <summary>
        /// Word Wrap
        /// </summary>
        /// <param name="text"></param>
        /// <param name="width"></param>
        /// <param name="characterSize"></param>
        /// <returns></returns>
        public static string[] GetTextWrap(string text, int width, uint characterSize = 11)
        {
            var collection = new List<string>();
            if (text.Length > 0)
            {
                string line = "";
                foreach (var i in text)
                {

                    if (GetTextWidth(line + i, characterSize) > width)
                    {
                        collection.Add(line.Trim());
                        line = i.ToString();
                    }
                    else
                        line += i;
                }
                if (line.Length > 0) collection.Add(line.Trim());
            }
            return collection.ToArray();
        }

        static Random random = new Random();
        public static int Rand(int min, int max)
        {
            int min_real = Math.Min(min, max);
            int max_real = Math.Max(min, max);
            return random.Next(min_real, max_real + 1);
        }

        public static long Rand(long min, long max)
        {
            long min_real = Math.Min(min, max);
            long max_real = Math.Max(min, max);
            return (long)(min_real + (max_real + 1 - min_real) * random.NextDouble());
        }

        /// <summary>
        /// Valor máximo
        /// </summary>
        /// <param name="value"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static int Max(int value, int max) => (value > max ? max : value);

        /// <summary>
        /// Valor minimo
        /// </summary>
        /// <param name="value"></param>
        /// <param name="min"></param>
        /// <returns></returns>
        public static int Min(int value, int min) => (value < min ? min : value);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="assembly"></param>
        /// <param name="nameSpace"></param>
        /// <returns></returns>
        public static Type[] GetTypesInNamespace<T>(Assembly assembly, string nameSpace)
        {
            var types = assembly.GetTypes();
            return types.Where(i => i.IsClass && i.Namespace == nameSpace && typeof(T).IsAssignableFrom(i)).ToArray();
        }

        /// <summary>
        /// Corrige o valor flutuante
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static float Floor(this float obj)
            => (float)Math.Floor(obj);

        public static float Round(this float obj)
            => (float)Math.Round(obj, 2);

        /// <summary>
        /// Valor numérico ou não
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static bool IsNumeric(this string s)
        {
            float output;
            return float.TryParse(s, out output);
        }

        /// <summary>
        /// String to Int
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static int ToInt32(this string s)
            => int.Parse(s);

        /// <summary>
        /// Escreve um vetor 2D
        /// </summary>
        /// <param name="s"></param>
        /// <param name="value"></param>
        public static void Write(this System.IO.BinaryWriter s, Vector2 value)
        {
            s.Write(value.x);
            s.Write(value.y);
        }

        /// <summary>
        /// Le um vetor 2D
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static Vector2 ReadVector2(this System.IO.BinaryReader s)
            => new Vector2(s.ReadSingle(), s.ReadSingle());

        /// <summary>
        /// Escreve um retângulo
        /// </summary>
        /// <param name="s"></param>
        /// <param name="value"></param>
        public static void Write(this System.IO.BinaryWriter s, Rectangle value)
        {
            s.Write(value.position);
            s.Write(value.size);
        }

        /// <summary>
        /// Le um retângulo
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static Rectangle ReadRectangle(this System.IO.BinaryReader s)
            => new Rectangle(s.ReadVector2(), s.ReadVector2());

        /// <summary>
        /// Escreve uma cor
        /// </summary>
        /// <param name="s"></param>
        /// <param name="value"></param>
        public static void Write(this System.IO.BinaryWriter s, Color value)
            => s.Write(value.ToInteger());

        /// <summary>
        /// Le uma cor
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static Color ReadColor(this System.IO.BinaryReader s)
            => new Color(s.ReadUInt32());

        /// <summary>
        /// Escreve um array de string
        /// </summary>
        /// <param name="s"></param>
        /// <param name="value"></param>
        public static void Write(this System.IO.BinaryWriter s, string[] value)
        {
            for (int i = 0; i < value.Length; i++)
                s.Write(value[i]);
        }

        /// <summary>
        /// Le um array de string
        /// </summary>
        /// <param name="s"></param>
        /// <param name="lenght"></param>
        /// <returns></returns>
        public static string[] ReadStringArray(this System.IO.BinaryReader s, int lenght)
        {
            string[] value = new string[lenght];
            for (int i = 0; i < lenght; i++)
                value[i] = s.ReadString();
            return value;
        }

        /// <summary>
        /// Carrega a font
        /// </summary>
        /// <param name="filename"></param>
        public static void LoadFont(string filename)
        {
            var f = new Font(filename);            
            gameFont = f;
            _text = new Text("", gameFont);
            //_text.LetterSpacing = 0;
            
        }

        public static RenderTexture CreateRender2D(int Width, int Height)
        {
            return new RenderTexture((uint)Width, (uint)Height, new ContextSettings(32,8, Game.AntiAliasing));
        }

        public static void ClearColor(Color color)
        {
            target?.Clear(color);
        }

        public static void BeginRender(RenderTarget target)
        {
            renders.Add(LunEngine.target);

            LunEngine.target = target;
        }

        public static void EndRender()
        {
            if (renders.Count == 0)
                return;

            if (target is RenderTexture)
                (target as RenderTexture).Display();

            if (target is RenderWindow)
                (target as RenderWindow).Display();

            target = renders.Last();
            renders.Remove(target);
        }

        public static void BeginCamera(Camera2D camera)
        {
            cams.Add(currentCamera);
            currentCamera = camera;
            target.SetView(camera.view);
        }

        public static void EndCamera()
        {
            if (cams.Count == 0)
                return;
                  
            var lastCam = cams.Last();
            if (lastCam != null)
                target.SetView(lastCam.view);

            currentCamera = lastCam;
            cams.Remove(lastCam);
        }

        public static int TickCount
            => Environment.TickCount < 0 ? Environment.TickCount & int.MaxValue : Environment.TickCount;
    }
}
