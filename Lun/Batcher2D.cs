using Lun.Controls;
using SFML.Graphics;
using SFML.System;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Text;
using sfTexture = SFML.Graphics.Texture;

namespace Lun;

class Batcher2DValue
{
    private Vertex[] _vertices;
    private uint _count;

    public sfTexture Texture { get; set; } = null;
    public BlendMode BlendMode { get; set; } = BlendMode.Alpha;
    public PrimitiveType PrimitiveType { get; set; } = PrimitiveType.Triangles;

    public Batcher2DValue(int capacity = 40960)
    {
        _vertices = new Vertex[capacity];
        _count = 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(Vertex vertex)
    {
        if (_count >= _vertices.Length)
        {
            Array.Resize(ref _vertices, _vertices.Length * 2);
        }
        _vertices[_count++] = vertex;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

    Batcher2DValue GetOrCreate(sfTexture texture, PrimitiveType primitiveType = PrimitiveType.Triangles)
    {
        if (_count == 0 || _count == values.Count)
        {
            var value = Create();
            value.Texture = texture;
            value.PrimitiveType = primitiveType;
            return value;
        }

        var lastValue = values[_count - 1];
        if (lastValue.Texture != texture || lastValue.PrimitiveType != primitiveType)
        {
            var value = Create();
            value.Texture = texture;
            value.PrimitiveType = primitiveType;
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

        sfTexture currentTexture = null;
        BlendMode currentBlend = default;

        for (int i = 0; i < _count; i++)
        {
            var value = values[i];

            if (value.Count == 0)
                continue;

            if (!ReferenceEquals(currentTexture, value.Texture))
            {
                currentTexture = value.Texture;
                _states.Texture = currentTexture;
            }

            if (!currentBlend.Equals(value.BlendMode))
            {
                currentBlend = value.BlendMode;
                _states.BlendMode = currentBlend;
            }

            currentTarget.Draw(
                value.Vertices,
                0,
                value.Count,
                value.PrimitiveType,
                _states);
        }
    }

    FontCache getFontCache(int characterSize)
    {
        FontCache _font = null;
        if (!fontCache.TryGetValue(characterSize, out _font))
        {
            var cache = new FontCache(characterSize);
            fontCache.Add(characterSize, cache);
            _font = cache;
        }
        return _font;
    }

    sfTexture getFontTexture(int characterSize)
    {
        return getFontCache(characterSize)._texture;
    }

    Glyph[] getFontGlyphs(int characterSize)
    {
        return getFontCache(characterSize)._glyphs;
    }

    public void DrawString(string text, int characterSize, Vector2 position, Color color, bool shadow = false, bool rounded = true)
    {
        var texture = getFontTexture(characterSize);
        var glyphs = getFontGlyphs(characterSize);
        var cache = getFontCache(characterSize);

        if (rounded)
            position = position.ToInt();

        if (shadow)
            DrawString(text, characterSize, position + new Vector2(1, 1), new Color(0, 0, 0, color.A), false, rounded);

        var batcher = GetOrCreate(texture);

        float startX = position.x;
        float x = startX;
        float y = position.y;

        float lineSpacing = cache._lineSpacing;
        float baselineOffset = lineSpacing - cache._underlinePosition;

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
        var batcher = GetOrCreate(texture);
        float rad = rotation * (float)Math.PI / 180f;

        // --- CORREÇÃO DA ESCALA NA ORIGEM ---
        // Se a 'origin' passada veio do tamanho original da textura, 
        // precisamos escalá-la para bater com o tamanho de 'dest'.
        float scaleX = dest.width / src.width;
        float scaleY = dest.height / src.height;

        var scaledOrigin = new Vector2(origin.x * scaleX, origin.y * scaleY);
        // -------------------------------------

        // 1. Agora usamos o scaledOrigin para os cantos locais
        var topLeft = new Vector2(0 - scaledOrigin.x, 0 - scaledOrigin.y);
        var topRight = new Vector2(dest.width - scaledOrigin.x, 0 - scaledOrigin.y);
        var bottomRight = new Vector2(dest.width - scaledOrigin.x, dest.height - scaledOrigin.y);
        var bottomLeft = new Vector2(0 - scaledOrigin.x, dest.height - scaledOrigin.y);

        // 2. Rotação ao redor do pivô local correto (0, 0)
        if (rotation != 0)
        {
            Vector2 zero = Vector2.Zero;
            topLeft = RotatePoint(topLeft, zero, rad);
            topRight = RotatePoint(topRight, zero, rad);
            bottomRight = RotatePoint(bottomRight, zero, rad);
            bottomLeft = RotatePoint(bottomLeft, zero, rad);
        }

        // 3. Transladar para a posição final do mundo
        var worldPos = new Vector2(dest.x, dest.y);
        topLeft += worldPos;
        topRight += worldPos;
        bottomRight += worldPos;
        bottomLeft += worldPos;

        // 4. Enviar para o lote
        batcher.BlendMode = blendMode;
        batcher.AddQuad(
            new Vertex(topLeft, color, new Vector2(src.x, src.y)),
            new Vertex(topRight, color, new Vector2(src.x + src.width, src.y)),
            new Vertex(bottomRight, color, new Vector2(src.x + src.width, src.y + src.height)),
            new Vertex(bottomLeft, color, new Vector2(src.x, src.y + src.height))
        );
    }

    public void DrawRenderTexture(RenderTexture render, Vector2 position, Color color)
    {
        var texture = render.Texture;
        var size = new Vector2(texture.Size.X, texture.Size.Y);
        DrawNativeTexture(texture, new Rectangle(position, size), new Rectangle(0, 0, size.x, size.y), Vector2.Zero, color, BlendMode.Alpha);
    }

    void DrawLargeTexture(LargeTexture texture, Rectangle dest, Color color)
    {
        int maxWidth = (int)sfTexture.MaximumSize;
        int count = texture.TextureList.Length;

        var scale = new Vector2(dest.width / texture.Size.X, dest.height / texture.Size.Y);
        for (int i = 0; i < count; i++)
        {
            var tex = texture.TextureList[i];
            // Calculate the position and size of the sub-texture in the destination rectangle
            var pos = dest.position + (Vector2)texture.PositionList[i] * scale;
            var size = (Vector2)tex.Size * scale;
            // Chunk size
            var src = new Rectangle(0,0, tex.Size.X,tex.Size.Y);
            DrawNativeTexture(tex, new Rectangle(pos, size), src, Vector2.Zero, color, BlendMode.Alpha);
        }
    }

    public void DrawTexture(Texture texture, Rectangle dest, Rectangle src, Vector2 origin, Color color, BlendMode blendMode, int rotation = 0)
    {
        if (texture == null) throw new ArgumentNullException(nameof(texture));
        texture.Load();

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
        DrawTexture(texture, dest, src, origin, color, BlendMode.Alpha, rotation);
    }

    public void DrawTexture(Texture texture, Rectangle dest, Rectangle src, Color color)
    {
        DrawTexture(texture, dest, src, Vector2.Zero, color, BlendMode.Alpha);
    }

    public void DrawTexture(Texture texture, Rectangle dest, Color color)
    {
        DrawTexture(texture, dest, new Rectangle(0, 0, texture.size.x, texture.size.y), Vector2.Zero, color);
    }

    public void DrawTexture(Texture texture, Rectangle dest)
    {
        DrawTexture(texture, dest, new Rectangle(0, 0, texture.size.x, texture.size.y), Vector2.Zero, Color.White);
    }

    public void DrawTexture(Texture texture, Vector2 position, Color color)
    {
        DrawTexture(texture, new Rectangle(position, texture.size), new Rectangle(0, 0, texture.size.x, texture.size.y), Vector2.Zero, color);
    }

    private List<Vector2> BuildRoundedRectPoints(
    Vector2 position,
    Vector2 size,
    float tl,
    float tr,
    float bl,
    float br,
    int segments)
    {
        List<Vector2> pts = new();

        float delta = (MathF.PI * 0.5f) / (segments - 1);

        void AddArc(Vector2 c, float startAngle, float radius)
        {
            if (radius <= 0f)
            {
                pts.Add(c);
                return;
            }

            for (int i = 0; i < segments; i++)
            {
                float a = startAngle + delta * i;

                pts.Add(new Vector2(
                    c.x + MathF.Cos(a) * radius,
                    c.y + MathF.Sin(a) * radius));
            }
        }

        // TR
        AddArc(
            new(position.x + size.x - tr, position.y + tr),
            -MathF.PI / 2,
            tr);

        // BR
        AddArc(
            new(position.x + size.x - br, position.y + size.y - br),
            0,
            br);

        // BL
        AddArc(
            new(position.x + bl, position.y + size.y - bl),
            MathF.PI / 2,
            bl);

        // TL
        AddArc(
            new(position.x + tl, position.y + tl),
            MathF.PI,
            tl);

        return pts;
    }

    public void DrawRoundedRectangleOutlineAllCorner(Vector2 position, Vector2 size, Color outlineColor, float topLeftRadius, float topRightRadius, float bottomLeftRadius, float bottomRightRadius, int cornerPoint = 8, int outlineThickness = 1)
    {
        position = position.Floor();
        size = size.Floor();

        var batcher = GetOrCreate(null, PrimitiveType.Triangles);

        // Usando Radianos diretamente para evitar conversões repetidas
        float deltaAngle = (MathF.PI * 0.5f) / (cornerPoint - 1);

        List<Vector2> inner = new();
        List<Vector2> outer = new();

        void AddCorner(Vector2 center, float startAngleRad, float radius)
        {
            // Mesmo se o raio for 0, rodamos o loop para gerar os pontos de quina necessários
            float r = MathF.Max(0f, radius);

            for (int i = 0; i < cornerPoint; i++)
            {
                float angle = startAngleRad + (deltaAngle * i);

                // No SFML, Y para baixo: Cos é X, Sin é Y.
                Vector2 dir = new(MathF.Cos(angle), MathF.Sin(angle));

                // Se o raio for zero, o 'inner' será exatamente o 'center' (a quina viva).
                // O 'outer' vai expandir perfeitamente na direção do ângulo criando o chanfro/quina externa.
                inner.Add(center + (dir * r));
                outer.Add(center + (dir * (r + outlineThickness)));
            }
        }

        // SEGUIDO A ORDEM HORÁRIA (Igual ao preenchimento para os loops casarem perfeitamente)

        // 1. TR (Top-Right): Ângulo de -90° a 0° (-PI/2 a 0)
        AddCorner(
            new(position.x + size.x - topRightRadius, position.y + topRightRadius),
            -MathF.PI * 0.5f,
            topRightRadius);

        // 2. BR (Bottom-Right): Ângulo de 0° a 90° (0 a PI/2)
        AddCorner(
            new(position.x + size.x - bottomRightRadius, position.y + size.y - bottomRightRadius),
            0f,
            bottomRightRadius);

        // 3. BL (Bottom-Left): Ângulo de 90° a 180° (PI/2 a PI)
        AddCorner(
            new(position.x + bottomLeftRadius, position.y + size.y - bottomLeftRadius),
            MathF.PI * 0.5f,
            bottomLeftRadius);

        // 4. TL (Top-Left): Ângulo de 180° a 270° (PI a 3PI/2)
        AddCorner(
            new(position.x + topLeftRadius, position.y + topLeftRadius),
            MathF.PI,
            topLeftRadius);

        // Desenha o contorno conectando as duas tiras de pontos (Inner e Outer)
        for (int i = 0; i < inner.Count; i++)
        {
            int next = (i + 1) % inner.Count;

            // Garanta que a ordem dos vértices no AddQuad respeite o Backface Culling do seu motor/SFML
            batcher.AddQuad(
                new Vertex(outer[i], outlineColor),
                new Vertex(outer[next], outlineColor),
                new Vertex(inner[next], outlineColor),
                new Vertex(inner[i], outlineColor)
            );
        }
    }

    public void DrawRoundedRectangleOutline(Vector2 position, Vector2 size, Color outlineColor, float radius, int cornerPoint = 8, int outlineThickness = 0)
    {
        DrawRoundedRectangleOutlineAllCorner(position, size, outlineColor, radius, radius, radius, radius, cornerPoint, outlineThickness);
    }

    public void DrawRoundedRectangleAllCorner(Vector2 position, Vector2 size, Color fillColor, float topLeftRadius, float topRightRadius, float bottomLeftRadius, float bottomRightRadius, int cornerPoint = 8, int outlineThickness = 0, Color outlineColor = default)
    {
        position = position.Floor();
        size = size.Floor();

        // O batcher espera triângulos isolados (3 vértices por triângulo)
        var batcher = GetOrCreate(null, PrimitiveType.Triangles);

        var points = BuildRoundedRectPoints(
        position, size,
        topLeftRadius, topRightRadius, bottomLeftRadius, bottomRightRadius,
        cornerPoint);

        if (points.Count < 3)
            return;

        // Criamos um ponto central para servir de âncora.
        // Isso distribui os triângulos perfeitamente como fatias de pizza,
        // eliminando artefatos e problemas de arredondamento nos cantos.
        Vector2 center = new(position.x + (size.x * 0.5f), position.y + (size.y * 0.5f));

        for (int i = 0; i < points.Count; i++)
        {
            Vector2 p1 = points[i];
            // O operador % garante que o último ponto feche o circuito com o primeiro
            Vector2 p2 = points[(i + 1) % points.Count];

            // Adiciona os 3 vértices que formam UM triângulo isolado
            batcher.Add(new Vertex(center, fillColor));
            batcher.Add(new Vertex(p1, fillColor));
            batcher.Add(new Vertex(p2, fillColor));
        }

        if (outlineThickness > 0)
        {
            DrawRoundedRectangleOutlineAllCorner(position, size, outlineColor, topLeftRadius, topRightRadius, bottomLeftRadius, bottomRightRadius, cornerPoint, outlineThickness);
        }
    }

    public void DrawRoundedRectangle(Vector2 position, Vector2 size, Color fillColor, float radius, int cornerPoint = 8, int outlineThickness = 0, Color outlineColor = default)
    {
        DrawRoundedRectangleAllCorner(position, size, fillColor, radius, radius, radius, radius, cornerPoint, outlineThickness, outlineColor);
    }

    public void DrawLine(Vector2 start, Vector2 end, Color color, float thickness = 1f)
    {
        var batcher = GetOrCreate(null, PrimitiveType.Triangles);

        Vector2 direction = end - start;
        if (direction.LengthSquared() <= 0.0001f)
            return;

        direction.Normalize();

        // --- TRUQUE DO PIXEL SNAPPING ---
        // Se a linha for puramente horizontal ou vertical, alinhar os pontos de início/fim
        // evita que o AA do SFML tente "adivinhar" e engrossele a linha.
        if (start.x == end.x || start.y == end.y)
        {
            start = new Vector2(MathF.Floor(start.x) + 0.5f, MathF.Floor(start.y) + 0.5f);
            end = new Vector2(MathF.Floor(end.x) + 0.5f, MathF.Floor(end.y) + 0.5f);
        }

        Vector2 normal = new Vector2(-direction.y, direction.x);
        normal *= thickness * 0.5f;

        Vector2 v0 = start + normal;
        Vector2 v1 = start - normal;
        Vector2 v2 = end + normal;
        Vector2 v3 = end - normal;

        batcher.AddQuad(
            new Vertex(v0, color),
            new Vertex(v1, color),
            new Vertex(v3, color),
            new Vertex(v2, color)
        );
    }

    public void DrawRectangleOutline(Vector2 position, Vector2 size, Color outlineColor, int outlineThickness = 1)
    {
        DrawRectangle(new Vector2(position.x - outlineThickness, position.y - outlineThickness), new Vector2(size.x + outlineThickness * 2, outlineThickness), outlineColor);
        DrawRectangle(new Vector2(position.x - outlineThickness, position.y + size.y), new Vector2(size.x + outlineThickness * 2, outlineThickness), outlineColor);
        DrawRectangle(new Vector2(position.x - outlineThickness, position.y), new Vector2(outlineThickness, size.y), outlineColor);
        DrawRectangle(new Vector2(position.x + size.x, position.y), new Vector2(outlineThickness, size.y), outlineColor);
    }

    public void DrawRectangle(Vector2 position, Vector2 size, Color fillColor, int outlineThickness = 0, Color outlineColor = default)
    {
        var batcher = GetOrCreate(null, PrimitiveType.Triangles);
        Vector2 topLeft = position;
        Vector2 topRight = new(position.x + size.x, position.y);
        Vector2 bottomLeft = new(position.x, position.y + size.y);
        Vector2 bottomRight = new(position.x + size.x, position.y + size.y);
        batcher.AddQuad(
            new Vertex(topLeft, fillColor),
            new Vertex(topRight, fillColor),
            new Vertex(bottomRight, fillColor),
            new Vertex(bottomLeft, fillColor)
        );

        if (outlineThickness > 0)
        {
            DrawRectangleOutline(position, size, outlineColor, outlineThickness);
        }
    }

    public void DrawGradientRectangle(Vector2 position, Vector2 size, Color topLeftColor, Color topRightColor, Color bottomLeftColor, Color bottomRightColor)
    {
        var batcher = GetOrCreate(null, PrimitiveType.Triangles);
        Vector2 topLeft = position;
        Vector2 topRight = new(position.x + size.x, position.y);
        Vector2 bottomLeft = new(position.x, position.y + size.y);
        Vector2 bottomRight = new(position.x + size.x, position.y + size.y);
        batcher.AddQuad(
            new Vertex(topLeft, topLeftColor),
            new Vertex(topRight, topRightColor),
            new Vertex(bottomRight, bottomRightColor),
            new Vertex(bottomLeft, bottomLeftColor)
        );
    }

    public void DrawCircleOutline(Vector2 position, float radius, Color outlineColor, int segments = 32, int outlineThickness = 1)
    {
        var batcher = GetOrCreate(null, PrimitiveType.Triangles);
        float deltaAngle = (MathF.PI * 2f) / segments;
        List<Vector2> innerPoints = new();
        List<Vector2> outerPoints = new();
        for (int i = 0; i < segments; i++)
        {
            float angle = deltaAngle * i;
            Vector2 innerPoint = new(
                position.x + MathF.Cos(angle) * radius,
                position.y + MathF.Sin(angle) * radius);
            Vector2 outerPoint = new(
                position.x + MathF.Cos(angle) * (radius + outlineThickness),
                position.y + MathF.Sin(angle) * (radius + outlineThickness));
            innerPoints.Add(innerPoint);
            outerPoints.Add(outerPoint);
        }
        for (int i = 0; i < segments; i++)
        {
            int next = (i + 1) % segments;
            batcher.AddQuad(
                new Vertex(outerPoints[i], outlineColor),
                new Vertex(outerPoints[next], outlineColor),
                new Vertex(innerPoints[next], outlineColor),
                new Vertex(innerPoints[i], outlineColor)
            );
        }
    }

    public void DrawCircle(Vector2 position, float radius, Color fillColor, int segments = 32, int outlineThickness = 0, Color outlineColor = default)
    {
        var batcher = GetOrCreate(null, PrimitiveType.Triangles);
        float deltaAngle = (MathF.PI * 2f) / segments;
        List<Vector2> points = new();
        for (int i = 0; i < segments; i++)
        {
            float angle = deltaAngle * i;
            Vector2 point = new(
                position.x + MathF.Cos(angle) * radius,
                position.y + MathF.Sin(angle) * radius);
            points.Add(point);
        }
        for (int i = 0; i < segments; i++)
        {
            int next = (i + 1) % segments;
            batcher.Add(new Vertex(position, fillColor));
            batcher.Add(new Vertex(points[i], fillColor));
            batcher.Add(new Vertex(points[next], fillColor));
        }
        if (outlineThickness > 0)
        {
            DrawCircleOutline(position, radius, outlineColor, segments, outlineThickness);
        }
    }

    public void DrawGradientVertical(Vector2 position, Vector2 size, Color topColor, Color bottomColor)
    {
        var batcher = GetOrCreate(null, PrimitiveType.Triangles);
        Vector2 topLeft = position;
        Vector2 topRight = new(position.x + size.x, position.y);
        Vector2 bottomLeft = new(position.x, position.y + size.y);
        Vector2 bottomRight = new(position.x + size.x, position.y + size.y);
        batcher.AddQuad(
            new Vertex(topLeft, topColor),
            new Vertex(topRight, topColor),
            new Vertex(bottomRight, bottomColor),
            new Vertex(bottomLeft, bottomColor)
        );
    }

    public void DrawGradientHorizontal(Vector2 position, Vector2 size, Color leftColor, Color rightColor)
    {
        var batcher = GetOrCreate(null, PrimitiveType.Triangles);
        Vector2 topLeft = position;
        Vector2 topRight = new(position.x + size.x, position.y);
        Vector2 bottomLeft = new(position.x, position.y + size.y);
        Vector2 bottomRight = new(position.x + size.x, position.y + size.y);
        batcher.AddQuad(
            new Vertex(topLeft, leftColor),
            new Vertex(topRight, rightColor),
            new Vertex(bottomRight, rightColor),
            new Vertex(bottomLeft, leftColor)
        );
    }
}
