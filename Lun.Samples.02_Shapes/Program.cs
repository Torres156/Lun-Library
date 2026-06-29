global using Lun;
global using static Lun.LunEngine;
using Lun.Controls;
using System;
using SFML.Graphics;

namespace Lun.Samples._02_Shapes
{
    class Program
    {
        static void Main(string[] args)
        {
            // Load a default font
            LoadFont("consola.ttf");

            // Window Settings for Start
            Game.WindowTitle = "Lun Library";              // Window Title
            Game.WindowSizeMin = new Vector2(1024, 600);  // Minimun Size for Window resize
            Game.WindowSize = new Vector2(1024, 600);     // Start Size for Window
            Game.WindowCanResize = true;                  // Window can resized
            Game.BackgroundColor = Color.Black;           // Clear Background Color
            Game.FixedPhysicTime = 0;

            // Opening Scene
            Game.SetScene<StartScene>();

            // Running a engine
            Game.Run();
        }
    }

    class StartScene : SceneBase
    {
        private (Vector2, Color, Vector2)[] points = [];
        private VertexArray _vertex;
        private TextureAtlas _atlas;
        private RenderStates currentRenderStates = RenderStates.Default;
        public override void LoadContent()
        {
            var count = 10000;
            points = new (Vector2, Color, Vector2)[count];
            for (int i = 0; i < count; i++)
                points[i] = (new Vector2(Rand(0, (int)Game.WindowSize.x), Rand(0, (int)Game.WindowSize.y)),
                    new Color((byte)Rand(0, 255), (byte)Rand(0, 255), (byte)Rand(0, 255)),
                    //Color.White,
                    new Vector2(Rand(0, 32), Rand(0, 10)) * 32);
            
            _vertex = new VertexArray(PrimitiveType.Triangles, (uint)count * 6);
            for (int i = 0; i < count; i++)
            {
                var index = (uint)i * 4;
                
                _vertex[index] = new Vertex(points[i].Item1, points[i].Item2, points[i].Item3);
                _vertex[index + 1] = new Vertex(points[i].Item1 + new Vector2(0, 32), points[i].Item2, points[i].Item3 + new Vector2(0, 32));                
                _vertex[index + 2] = new Vertex(points[i].Item1 + new Vector2(32), points[i].Item2, points[i].Item3 + new Vector2(32));

                _vertex[index + 3] = _vertex[index];
                _vertex[index + 4] = _vertex[index + 2];
                _vertex[index + 5] = new Vertex(points[i].Item1 + new Vector2(32, 0), points[i].Item2, points[i].Item3 + new Vector2(32, 0));

            }
            
            var tile1 = new Texture("tile1.png");
            var tile2 = new Texture("tile2.png");
            _atlas = new TextureAtlas();
            _atlas.Append(tile1);
            _atlas.Append(tile2);
            _atlas.CreateAtlas();
            
            tile1.Destroy();
            tile2.Destroy();
          //  currentRenderStates.Texture = _atlas.GetRenderTexture().Texture;
        }

        public override void Draw(Batcher2D batcher)
        {
             // foreach (var point in points)
             //      DrawRectangle(point.Item1, new Vector2(25), point.Item2);
            
            _vertex.Draw(currentTarget, currentRenderStates);
            
            
            DrawText("FPS:" + Game.FPS, 12, new Vector2(10, 10), Color.White, true);
            DrawText("Texture limit:" + SFML.Graphics.Texture.MaximumSize, 12, new Vector2(10, 30), Color.White, true);
        }
    }
}
