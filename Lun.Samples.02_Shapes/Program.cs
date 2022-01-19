global using Lun;
global using static Lun.LunEngine;
using Lun.Controls;
using System;

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

            // Opening Scene
            Game.SetScene<StartScene>();

            // Running a engine
            Game.Run();
        }
    }

    class StartScene : SceneBase
    {
        public override void LoadContent()
        {

        }

        public override void Draw()
        {
            var text = "Hello Lun!";
            DrawText(text, 12, new Vector2((Game.WindowSize.x - GetTextWidth(text, 12)) / 2, Game.WindowSize.y / 2 - 7), Color.White);

            var rec = new Rectangle(new Vector2(10), new Vector2(150, 100));
            DrawRectangle(rec.position, rec.size, Color.White, 2, Color.Red);

            var recRounded = new Rectangle(new Vector2(10,120), new Vector2(150, 100));
            var rounded = 4f;
            DrawRoundedRectangle(recRounded.position, recRounded.size, Color.White,
                rounded, 6, 2, Color.Blue);

            var circleRadius = 50f;
            DrawCircle(Game.MousePosition, circleRadius, Color.CornflowerBlue);
        }
    }
}
