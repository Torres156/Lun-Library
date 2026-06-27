using System;
using System.Collections.Generic;
using SFML.Graphics;

namespace Lun.Samples._02_Shapes;

using NativeTexture = SFML.Graphics.Texture;

public class TextureAtlas
{
    private readonly int MAX_WIDTH =  (int)SFML.Graphics.Texture.MaximumSize;
    
    private RenderTexture _render;
    private List<Rectangle> _sources = [];
    private Vector2 _currentSize;
    private Queue<Texture> _textures = [];
    
    public void Append(Texture texture)
    {
        checkRender(texture);
        _textures.Enqueue(texture);
    }

    void checkRender(Texture texture)
    {
        if (texture?.GetTexture() == null)
            throw new Exception("Texture not found");
        
        _currentSize.x = texture.size.x > _currentSize.x ? texture.size.x : _currentSize.x;
        _currentSize.y = texture.size.y > _currentSize.y ? texture.size.y : _currentSize.y;
    }
    
    void createRender()
    {
        if (_currentSize.x * _textures.Count > MAX_WIDTH)
        {
            var countX = (int)Math.Floor(MAX_WIDTH / _currentSize.x);
            var countY = (int)Math.Ceiling((double)_textures.Count / countX);
            var width =  countX * _currentSize.x;
            var height = countY * _currentSize.y;

            _render = CreateRender2D((int)width, (int)height);
        }
        else
        {
            var width =  _textures.Count * _currentSize.x;
            var height =  _currentSize.y;
            _render = CreateRender2D((int)width, (int)height);
        }
        
    }

    public void CreateAtlas()
    {
        createRender();
        
        
        var pos = new Vector2(0, 0);
        BeginRender(_render);
        ClearColor(Color.Transparent);
        while (_textures.Count != 0)
        {
            var current = _textures.Dequeue();
            if (pos.x + _currentSize.x > MAX_WIDTH)
            {
                pos.x = 0;
                pos.y += _currentSize.y;
            }
            
            Console.WriteLine($"Add texture in position x: {pos.x}, y: {pos.y}");
            _sources.Add(new Rectangle(pos, current.size));
            DrawTexture(current, new Rectangle(pos, current.size));
            
            pos.x += _currentSize.x;
        }
        EndRender();
    }
    
    public RenderTexture GetRenderTexture() => _render;
    
    
    
}
