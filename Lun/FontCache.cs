using System.Text;
using SFML.Graphics;

namespace Lun;

public class FontCache : IDisposable
{
    internal readonly SFML.Graphics.Texture _texture;
    internal readonly Glyph[] _glyphs = [];
    internal readonly float _lineSpacing, _underlinePosition;
    private readonly RenderStates _states;
    public int CharacterSize { get; private set; }    

    public FontCache(int characterSize = 12)
    {
        _glyphs = new Glyph[256];
        CharacterSize = characterSize;

        var builder = new StringBuilder();
        for (int i = 0; i < 256; i++)
            builder.Append((char)i);

        string allChars = builder.ToString();
        foreach (var c in allChars)
            _glyphs[c] = gameFont.GetGlyph(c, (uint)characterSize, false, 0);

        _texture = gameFont.GetTexture((uint)characterSize);
        _lineSpacing = gameFont.GetLineSpacing((uint)characterSize);
        _underlinePosition = gameFont.GetUnderlinePosition((uint)characterSize);
        
        _states = new RenderStates(_texture);
    }

    /// <summary>
    /// Comprimento do Texto
    /// </summary>
    /// <param name="text"></param>
    /// <param name="characterSize"></param>
    /// <returns></returns>
    public float GetTextWidth(string text, bool ignoreBB = false)
    {
        if (text.Trim().Length == 0)
            return 0;

        if (!ignoreBB)
            text = FilterTextBBColor(text);

        float x = 0;
        foreach (var c in text)
        {
            if (c == '\n')
            {
                x = 0;
                continue;
            }

            var glyph = _glyphs[c];
            x += glyph.Advance;
        }

        return x.Round();
    }
    
    public void DrawText(string str, Vector2 pos, Color color, bool shadow = false)
    {
        if (shadow)
        {
            var shadowColor = new Color(0, 0, 0, color.A);
            DrawText(str, pos + Vector2.One, shadowColor, false);
        }

        pos = pos.ToInt();
        float startX = pos.x;
        float x = startX;
        float y = pos.y;

        float lineSpacing = _lineSpacing;
        float baselineOffset = lineSpacing - _underlinePosition;
        
        var _vertices = new Vertex[6 * str.Length];
        var index = 0;

        for (int i = 0; i < str.Length; i++)
        {
            char c = str[i];
            if (c == '\n')
            {
                x = startX;
                y += lineSpacing;
                continue;
            }

            var glyph = _glyphs[c]; //font.GetGlyph(c, fontSize, false, 0);
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

            //batcher.AddQuad(new Vertex(new Vector2(left, top), color, new Vector2(tx, ty)),
            //                new Vertex(new Vector2(left + bounds.Width, top), color, new Vector2(tx + tw, ty)),
            //                new Vertex(new Vector2(left + bounds.Width, top + bounds.Height), color, new Vector2(tx + tw, ty + th)),
            //                new Vertex(new Vector2(left, top + bounds.Height), color, new Vector2(tx, ty + th)));


            _vertices[index] = new Vertex(new Vector2(left, top), color, new Vector2(tx, ty));
            _vertices[index + 1] = new Vertex(new Vector2(right, top), color, new Vector2(tx + tw, ty));
            _vertices[index + 2] = new Vertex(new Vector2(right, bottom), color, new Vector2(tx + tw, ty + th));
            _vertices[index + 3] = _vertices[index];
            _vertices[index + 4] = _vertices[index + 2];
            _vertices[index + 5] = new Vertex(new Vector2(left, bottom), color, new Vector2(tx, ty + th));
            index += 6;
            x += glyph.Advance;
        }
        
        currentTarget.Draw(_vertices, 0, (uint)index, PrimitiveType.Triangles, _states);
    }

    public void Dispose()
    {        
        _texture.Dispose();
    }
}