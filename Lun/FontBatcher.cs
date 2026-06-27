using System.Text;
using Lun.Controls;
using SFML.Graphics;

namespace Lun;

public class FontBatcher
{
    private static readonly Dictionary<int, SFML.Graphics.Texture> cacheFonts = [];
    private static readonly Dictionary<int, Dictionary<char, Glyph>> cacheGlyphs = [];
    
    private readonly uint fontSize = 12;
    private readonly Vertex[] _vertices;
    private int _countVertices = 0;
    private readonly RenderStates states;
    private readonly Dictionary<char, Glyph> _glyphs;

    public FontBatcher(int fontSize = 12,int max_vertices = 40960)
    {
        this.fontSize = (uint)fontSize;
        _vertices = new Vertex[max_vertices];

        if (!cacheFonts.ContainsKey(fontSize))
        {
            _glyphs = new Dictionary<char, Glyph>();

            var builder = new StringBuilder();
            for (int i = 0; i < 256; i++)
                builder.Append((char)i);

            string allChars = builder.ToString();
            foreach (var c in allChars)
                _glyphs.Add(c, gameFont.GetGlyph(c, this.fontSize, false, 0));
            
            cacheFonts.TryAdd(fontSize, gameFont.GetTexture((uint)fontSize));
            cacheGlyphs.TryAdd(fontSize, _glyphs);
        }
        
        _glyphs = cacheGlyphs[fontSize];
        states = new RenderStates(cacheFonts[fontSize]);
    }
    
    public void Begin()
    {
        _countVertices = 0;
    }

    public void End()
    {
        if (_countVertices == 0) return;
        currentTarget.Draw(_vertices, 0, (uint)_countVertices, PrimitiveType.Quads, states);
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

        float lineSpacing = gameFont.GetLineSpacing(fontSize);
        float baselineOffset = lineSpacing - gameFont.GetUnderlinePosition(fontSize);

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

            _vertices[_countVertices] = new Vertex(new Vector2(left, top), color, new Vector2(tx, ty));
            _vertices[_countVertices + 1] = new Vertex(new Vector2(right, top), color, new Vector2(tx + tw, ty));
            _vertices[_countVertices + 2] = new Vertex(new Vector2(right, bottom), color, new Vector2(tx + tw, ty + th));
            _vertices[_countVertices + 3] = new Vertex(new Vector2(left, bottom), color, new Vector2(tx, ty + th));
            _countVertices += 4;

            x += glyph.Advance;
        }
    }

    public void AddBBCode(string text, Vector2 position, float width, TextAligns align = TextAligns.Left, float lineSpacing = 16, bool shadow = false)
    {
        var word = GetWordWrapBBColor(text, (int)width);
        for(int i = 0; i < word.Length; i++)
        {
            var x = align switch
            {
                TextAligns.Left => position.x,
                TextAligns.Center => position.x - GetTextWidth(word[i]) / 2,
                TextAligns.Right => position.x - GetTextWidth(word[i])
            };
            DrawBBCode(word[i], new Vector2(x, position.y + lineSpacing * i), shadow);
        }
    }
    
    public void DrawBBCode(string text, Vector2 position, bool shadow = false)
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
                            DrawText(before, position + new Vector2(off, 0), colorStack.Peek(),
                                shadow: shadow);
                            off += GetTextWidth(before);
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
                        DrawText(before, position + new Vector2(off, 0), colorStack.Peek(),
                            shadow: shadow);
                        off += GetTextWidth(before);
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
            DrawText(text, position + new Vector2(off, 0), colorStack.Peek(), shadow: shadow);
    }

    public string[] GetWordWrapBBColor(string text, int width)
        {
            var result = new List<string>();
            if (string.IsNullOrWhiteSpace(text))
                return result.ToArray();

            // Traduz nomes de cor para hex
            text = TranslateColors(text);

            var colorStack = new Stack<string>();
            var trueLineBuilder = new StringBuilder();

            int i = 0;
            int len = text.Length;
            string currentWord = "";
            string currentVisible = "";

            while (i < len)
            {
                char c = text[i];

                // Espaço
                if (char.IsWhiteSpace(c))
                {
                    ProcessWord(currentWord);
                    currentWord = "";
                    i++;
                    continue;
                }

                // Quebra de linha "/n"
                if (c == '/' && i + 1 < len && text[i + 1] == 'n')
                {
                    ProcessWord(currentWord);
                    AddLine();
                    currentWord = "";
                    i += 2;
                    continue;
                }

                currentWord += c;
                i++;
            }

            if (trueLineBuilder.Length > 0)
            {
                var lastColor = colorStack.Count > 0 ? colorStack.Pop() : null;
                if (lastColor != null) currentWord = lastColor.ToString() + currentWord;

                // Última palavra e linha
                if (currentWord.Length > 0)
                    ProcessWord(currentWord);

                string closingTags = string.Concat(Enumerable.Repeat("[/color]", colorStack.Count));
                string finalLine = trueLineBuilder.ToString().TrimEnd() + closingTags;

                result.Add(finalLine.TrimEnd());
            }

            return result.ToArray();

            // ===== Subfunções internas =====

            void ProcessWord(string word)
            {
                if (string.IsNullOrEmpty(word)) return;

                string rawWord = word;

                // Detecta e trata [color=...] e [/color]
                if (word.Contains("[color="))
                {
                    int start = word.IndexOf("[color=");
                    int end = word.IndexOf(']', start);
                    if (start >= 0 && end > start)
                    {
                        string tag = word.Substring(start, end - start + 1);
                        colorStack.Push(tag);
                        rawWord = word.Remove(start, end - start + 1);
                    }
                }

                if (word.Contains("[/color]"))
                {
                    int end = word.IndexOf("[/color]");
                    rawWord = word.Remove(end, "[/color]".Length);
                    if (colorStack.Count > 0)
                        colorStack.Pop();
                }

                // Mede linha visível (sem tags)
                float testWidth = GetTextWidth(currentVisible + rawWord);

                if (testWidth > width && currentVisible.Length > 0)
                {
                    AddLine();
                    currentVisible = rawWord + " ";
                    var colorString = GetAllCurrentColorTags();
                    if (!string.IsNullOrWhiteSpace(colorString) && word.Contains(colorString))
                        word = word.Replace(colorString, "");
                    trueLineBuilder.Append(GetAllCurrentColorTags()).Append(word).Append(' ');
                }
                else
                {
                    currentVisible += rawWord + " ";
                    trueLineBuilder.Append(word).Append(' ');
                }
            }

            void AddLine()
            {
                result.Add(trueLineBuilder.ToString().TrimEnd());
                trueLineBuilder.Clear();
                currentVisible = "";
                // NÃO reabre tags aqui. Elas são reabertas por `ProcessWord`
            }

            string GetAllCurrentColorTags()
            {
                return colorStack.Count > 0 ? string.Join("", colorStack.Reverse()) : "";
            }
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
}