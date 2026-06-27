using Lun.Controls;
using SFML.Graphics;
using System;
using System.Collections.Generic;
using System.Text;
using sfTexture = SFML.Graphics.Texture;

namespace Lun;

class Batcher2DValue
{
    private Vertex[] _vertices;
    private uint _count;

    public sfTexture Texture { get; set; } = null;
    public BlendMode BlendMode { get; set; } = BlendMode.Alpha;

    public Batcher2DValue(int capacity = 40960)
    {
        _vertices = new Vertex[capacity];
        _count = 0;
    }

    public void Add(Vertex vertex)
    {
        if (_count >= _vertices.Length)
        {
            Array.Resize(ref _vertices, _vertices.Length * 2);
        }
        _vertices[_count++] = vertex;
    }

    public void AddQuad(Vertex topLeft, Vertex topRight, Vertex bottomRight, Vertex bottomLeft)
    {
        Add(topLeft);
        Add(topRight);
        Add(bottomRight);
        Add(topLeft);
        Add(bottomRight);
        Add(bottomLeft);
    }

    public uint Count => _count;

    public void Clear()
    {
        _count = 0;
        Texture = null;
    }

    public Vertex[] Vertices => _vertices;
}

public class Batcher2D
{
    private RenderStates _states = RenderStates.Default;
    private List<Batcher2DValue> values = [];
    private int _count = 0;

    public Batcher2D()
    {

    }

    Batcher2DValue Create()
    {
        if (_count < values.Count)
        {
            return values[_count++];
        }

        var value = new Batcher2DValue();
        values.Add(value);
        _count++;
        return value;
    }

    Batcher2DValue GetOrCreate(sfTexture texture)
    {
        if (_count == 0 || _count == values.Count)
        {
            var value = Create();
            value.Texture = texture;
            return value;
        }

        var lastValue = values[_count - 1];
        if (lastValue.Texture != texture)
        {
            var value = Create();
            value.Texture = texture;
            return value;

        }
        
        return lastValue;        
    }

    public void Clear()
    {
        _count = 0;
        foreach (var value in values)
        {
            value.Clear();
        }
    }

    public int Count => _count;

    public void Begin(RenderStates? states = null)
    {
        if (states != null)
        {
            _states = states.Value;
        }

        Clear();
    }

    public void End()
    {
        if (_count == 0) return;

        for (int i = 0; i < _count; i++)
        {
            var value = values[i];
            if (value.Count > 0)
            {
                _states.Texture = value.Texture;
                _states.BlendMode = value.BlendMode;
                currentTarget.Draw(value.Vertices, 0, value.Count, PrimitiveType.Triangles, _states);                
            }
        }
    }

    sfTexture getFontTexture(int characterSize)
    {
        FontCache _font = null;
        if (!fontCache.TryGetValue(characterSize, out _font))
        {
            var cache = new FontCache(characterSize);
            fontCache.Add(characterSize, cache);
            _font = cache;
        }

        return _font._texture;
    }

    Glyph[] getFontGlyphs(int characterSize)
    {
        FontCache _font = null;
        if (!fontCache.TryGetValue(characterSize, out _font))
        {
            var cache = new FontCache(characterSize);
            fontCache.Add(characterSize, cache);
            _font = cache;
        }
        return _font._glyphs;
    }

    public void DrawString(string text, int characterSize, Vector2 position, Color color, bool shadow = false, bool rounded = true)
    {
        var texture = getFontTexture(characterSize);
        var glyphs = getFontGlyphs(characterSize);

        if (rounded)        
            position = position.ToInt();

        if (shadow)
            DrawString(text, characterSize, position + new Vector2(1, 1), new Color(0, 0, 0, color.A), false, rounded);

        var batcher = GetOrCreate(texture);

        float startX = position.x;
        float x = startX;
        float y = position.y;

        float lineSpacing = gameFont.GetLineSpacing((uint)characterSize);
        float baselineOffset = lineSpacing - gameFont.GetUnderlinePosition((uint)characterSize);

        for (int i = 0; i < text.Length; i++)
        {
            char c = text[i];
            if (c == '\n')
            {
                x = startX;
                y += lineSpacing;
                continue;
            }

            var glyph = glyphs[c]; 
            var bounds = glyph.Bounds;
            var texRect = glyph.TextureRect;

            float baselineY = y + baselineOffset;

            float left = x + bounds.Left;
            float top = baselineY + bounds.Top;
            float right = left + bounds.Width;
            float bottom = top + bounds.Height;

            float tx = texRect.Left;
            float ty = texRect.Top;
            float tw = texRect.Width;
            float th = texRect.Height;

            // Add the vertices for the glyph quad to six
            batcher.AddQuad(new Vertex(new Vector2(left, top), color, new Vector2(tx, ty)),
                            new Vertex(new Vector2(left + bounds.Width, top), color, new Vector2(tx + tw, ty)),
                            new Vertex(new Vector2(left + bounds.Width, top + bounds.Height), color, new Vector2(tx + tw, ty + th)),
                            new Vertex(new Vector2(left, top + bounds.Height), color, new Vector2(tx, ty + th)));

            x += glyph.Advance;
        }
    }

    public int AddBBCode(string text, int characterSize, Vector2 position, float width, TextAligns align = TextAligns.Left, float lineSpacing = 16, bool shadow = false)
    {
        var word = GetWordWrapBBColor(text, (int)width);
        for (int i = 0; i < word.Length; i++)
        {
            var x = align switch
            {                
                TextAligns.Left => position.x,
                TextAligns.Center => position.x - GetTextWidth(word[i], (uint)characterSize) / 2,
                TextAligns.Right => position.x - GetTextWidth(word[i], (uint)characterSize),
                _=> position.x,
            };
            DrawBBCode(word[i], characterSize, new Vector2(x, position.y + lineSpacing * i), shadow);
        }
        return word.Length;
    }

    public void DrawBBCode(string text, int characterSize, Vector2 position, bool shadow = false)
    {
        var colorStack = new Stack<Color>();
        colorStack.Push(Color.White);

        // Translate colors
        text = TranslateColors(text);

        float off = 0;
        int i = 0;
        int len = text.Length;

        Span<char> buffer = stackalloc char[16]; // Temp buffer de leitura

        while (i < len)
        {
            if (text[i] == '[')
            {
                // Tenta detectar tag
                if (i + 7 < len && text[i..].StartsWith("[color="))
                {
                    int startTag = i;
                    int tagClose = text.IndexOf(']', i);
                    if (tagClose > i + 7)
                    {
                        string tagContent = text.Substring(i + 7, tagClose - (i + 7));

                        // Desenha antes da tag
                        if (startTag > 0)
                        {
                            string before = text.Substring(0, startTag);
                            DrawString(before, characterSize, position + new Vector2(off, 0), colorStack.Peek(),
                                shadow: shadow);
                            off += GetTextWidth(before, (uint)characterSize);
                            text = text.Substring(startTag + 0); // corta já processado
                            i = 0;
                            len = text.Length;
                            continue;
                        }

                        // Parse cor
                        if (tagContent.StartsWith("#") && (tagContent.Length == 7 || tagContent.Length == 9))
                        {
                            ReadOnlySpan<char> hex = tagContent.AsSpan(1);
                            Span<byte> rgba = stackalloc byte[4];
                            for (int j = 0; j < hex.Length / 2; j++)
                                rgba[j] = Convert.ToByte(hex.Slice(j * 2, 2).ToString(), 16);

                            if (hex.Length == 6)
                                colorStack.Push(new Color(rgba[0], rgba[1], rgba[2]));
                            else
                                colorStack.Push(new Color(rgba[0], rgba[1], rgba[2], rgba[3]));
                        }

                        text = text.Substring(tagClose + 1);
                        i = 0;
                        len = text.Length;
                        continue;
                    }
                }
                else if (i + 7 < len && text[i..].StartsWith("[/color]"))
                {
                    // Desenha texto antes do fechamento
                    string before = text.Substring(0, i);
                    if (before.Length > 0)
                    {
                        DrawString(before, characterSize, position + new Vector2(off, 0), colorStack.Peek(),
                            shadow: shadow);
                        off += GetTextWidth(before, (uint)characterSize);
                    }

                    if (colorStack.Count > 1)
                        colorStack.Pop();

                    text = text.Substring(i + 8);
                    i = 0;
                    len = text.Length;
                    continue;
                }
            }

            i++;
        }

        // Resto do texto
        if (text.Length > 0)
            DrawString(text, characterSize, position + new Vector2(off, 0), colorStack.Peek(), shadow: shadow);
    }

    Vector2 RotatePoint(Vector2 point, Vector2 center, float angle)
    {
        float cos = (float)Math.Cos(angle);
        float sin = (float)Math.Sin(angle);
        // Translate point back to origin
        point -= center;
        // Rotate point
        float xnew = point.x * cos - point.y * sin;
        float ynew = point.x * sin + point.y * cos;
        // Translate point back
        return new Vector2(xnew, ynew) + center;
    }

    void DrawNativeTexture(sfTexture texture, Rectangle dest, Rectangle src, Vector2 origin, Color color, BlendMode blendMode, int rotation = 0)
    {
        dest.position -= origin;
        var batcher = GetOrCreate(texture);
        if (rotation != 0)
        {
            var center = dest.position + new Vector2(dest.width / 2, dest.height / 2);
            var rad = rotation * (float)Math.PI / 180f;
            // Calculate the four corners of the rectangle
            var topLeft = new Vector2(dest.x, dest.y);
            var topRight = new Vector2(dest.x + dest.width, dest.y);
            var bottomRight = new Vector2(dest.x + dest.width, dest.y + dest.height);
            var bottomLeft = new Vector2(dest.x, dest.y + dest.height);
            // Rotate each corner around the center
            topLeft = RotatePoint(topLeft, center, rad);
            topRight = RotatePoint(topRight, center, rad);
            bottomRight = RotatePoint(bottomRight, center, rad);
            bottomLeft = RotatePoint(bottomLeft, center, rad);
            
            batcher.AddQuad(
                new Vertex(topLeft, color, new Vector2(src.x, src.y)),
                new Vertex(topRight, color, new Vector2(src.x + src.width, src.y)),
                new Vertex(bottomRight, color, new Vector2(src.x + src.width, src.y + src.height)),
                new Vertex(bottomLeft, color, new Vector2(src.x, src.y + src.height))
            );
            return;
        }
        
        batcher.AddQuad(
            new Vertex(new Vector2(dest.x, dest.y), color, new Vector2(src.x, src.y)),
            new Vertex(new Vector2(dest.x + dest.width, dest.y), color, new Vector2(src.x + src.width, src.y)),
            new Vertex(new Vector2(dest.x + dest.width, dest.y + dest.height), color, new Vector2(src.x + src.width, src.y + src.height)),
            new Vertex(new Vector2(dest.x, dest.y + dest.height), color, new Vector2(src.x, src.y + src.height))
        );
    }

    void DrawLargeTexture(LargeTexture texture, Rectangle dest, Color color)
    {
        int maxWidth = (int)sfTexture.MaximumSize;
        int count = texture.TextureList.Length;

        var scale = new Vector2(dest.width / texture.Size.X, dest.height / texture.Size.Y);        
        for (int i = 0; i < count; i++) {
            var tex = texture.TextureList[i];
            // Calculate the position and size of the sub-texture in the destination rectangle
            var pos = dest.position + (Vector2)texture.PositionList[i] * scale;
            var size = (Vector2)tex.Size * scale;
            // Chunk size
            var src = new Rectangle(0,0, tex.Size.X,tex.Size.Y);
            DrawNativeTexture(tex, new Rectangle(pos,size), src, Vector2.Zero, color, BlendMode.Alpha);
        }    
    }

    public void DrawTexture(Texture texture, Rectangle dest, Rectangle src, Vector2 origin, Color color, BlendMode blendMode, int rotation = 0)
    {
        if (texture == null) throw new ArgumentNullException(nameof(texture));

        if (texture.type == TextureTypes.Normal)
        {
            DrawNativeTexture(texture.GetTexture(), dest, src, origin, color, blendMode, rotation);
        }
        else if (texture.type == TextureTypes.Large)
        {
            DrawLargeTexture(texture.GetLargeTexture(), dest, color);
        }
    }

    public void DrawTexture(Texture texture, Rectangle dest, Rectangle src, Vector2 origin, Color color, int rotation = 0)
    {
        if (texture == null) throw new ArgumentNullException(nameof(texture));
        
        if (texture.type == TextureTypes.Normal)
        {
            DrawNativeTexture(texture.GetTexture(), dest, src, origin, color, BlendMode.Alpha, rotation);
        }
        else if (texture.type == TextureTypes.Large)
        {
            DrawLargeTexture(texture.GetLargeTexture(), dest, color);
        }
    }

    public void DrawTexture(Texture texture, Rectangle dest, Color color)
    {
        DrawTexture(texture, dest, new Rectangle(0, 0, texture.size.x, texture.size.y), Vector2.Zero, color);
    }

}
