global using Lun;
global using static Lun.LunEngine;
using Lun.Controls;
using System;

namespace Lun.Samples._01_Hello
{
    class Program
    {
        static void Main(string[] args)
        {
            // Load a default font
            LoadFont("consola.ttf");

            // Window Settings for Start
            Game.WindowTitle = "Lun Library";             // Window Title
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
        private Batcher2D batch;
        private string[] word;
        public override void LoadContent()
        {
            batch = new Batcher2D();
            
        }

        public override void Draw()
        {
            batch.Begin();

            var text = "Causa [color=#FFC602]0[/color]([color=#477BE5]+0[/color]) de Dano Mágico nos alvos, e acrescenta [color=red]Gelidez[/color] nos alvos. /n /n [color=red]Gelidez:[/color] Causa [color=#FFC602]0[/color]([color=#477BE5]+0[/color]) de Dano Mágico em alvos afetados por [color=red]Gelidez[/color] próximos.";
            batch.AddBBCode(text, 12, Game.WindowSize / 2, 400, TextAligns.Left, 20);
            batch.End();

            DrawText("FPS: " + Game.FPS, 12, new Vector2(10, 10), Color.White, true);
            // DrawText("TICK: " + Game.Clock.ElapsedTime.AsMilliseconds(), 12, new Vector2(10, 24), Color.White, true);
            //
            // DrawText("Delta: " + Game.DeltaTime, 12, new Vector2(10, 38), Color.White, true);
        }
    }
}
