global using Lun;
global using static Lun.LunEngine;
using Lun.Controls;
using System;

namespace Lun.Samples._03_Textures
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
            Game.AntiAliasing = 4;
            Game.FixedPhysicTime = 0;

            // Opening Scene
            Game.SetScene<StartScene>();

            // Running a engine
            Game.Run();
        }
    }

    class StartScene : SceneBase
    {
        Texture Background;
        Batcher2D batcher;

        public override void LoadContent()
        {
            // Active smooth to resize
            Background = new Texture("background.jpg", true) { Smooth = true };
            batcher = new Batcher2D();
        }

        public override void UnloadContent()
        {
            Background.Destroy();
            base.UnloadContent();
        }

        public override void Draw()
        {
            //DrawTexture(Background, new Rectangle(Vector2.Zero, Game.WindowSize));

            batcher.Begin();
            batcher.DrawTexture(Background, new Rectangle(Vector2.Zero, Game.WindowSize), Color.White);
            batcher.DrawString("Lun Library", 12, new Vector2(20, 20), Color.White);
            batcher.DrawString("FPS: " + Game.FPS, 12, new Vector2(20, 60), Color.White);            
            batcher.End();

            DrawText("Batch Count: " + batcher.Count, 12, new Vector2(20, 40), Color.White);
        }
    }
}
