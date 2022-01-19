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

            // Opening Scene
            Game.SetScene<StartScene>();

            // Running a engine
            Game.Run();
        }
    }

    class StartScene : SceneBase
    {
        Texture Background;

        public override void LoadContent()
        {
            // Active smooth to resize
            Background = new Texture("background.jpg", true) { Smooth = true };
        }

        public override void UnloadContent()
        {
            Background.Destroy();
            base.UnloadContent();
        }

        public override void Draw()
        {
            DrawTexture(Background, new Rectangle(Vector2.Zero, Game.WindowSize));
        }
    }
}
