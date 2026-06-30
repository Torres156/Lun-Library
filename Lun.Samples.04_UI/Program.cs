global using Lun;
global using static Lun.LunEngine;
using Lun.Controls;
using System;

namespace Lun.Samples._04_UI
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
            Game.AntiAliasing = 8;

            // Opening Scene
            Game.SetScene<StartScene>();

            // Running a engine
            Game.Run();
        }
    }

    class StartScene : SceneBase
    {
        Form formSample;
        Button buttonSample;
        ListBox listSample;
        ComboBox comboSample;

        Form formSample2;
        CheckButton checkSample;
        TabControl tabSample;
        CheckBox checkBox;

        TextBox txtSample;

        public override void LoadContent()
        {
            formSample = new Form(this)
            {
                Size = new Vector2(150,220),
                Title = "Fixed Size",
                Position = new Vector2(250, 10),
            };

            buttonSample = new Button(formSample)
            {
                Text = "Sample Button",
                Size = new Vector2(100,20),
                Anchor = Anchors.TopCenter,
                Position = new Vector2(0,10),
            };

            listSample = new ListBox(formSample)
            {
                Size = new Vector2(130,100),
                Anchor = Anchors.TopCenter,
                Position = new Vector2(0,40),
            };
            for (int i = 0; i < 20; i++)
                listSample.Items.Add(i + " Sample");

            comboSample = new ComboBox(formSample)
            {
                Size = new Vector2(130,20),
                Anchor = Anchors.TopCenter,
                Position = new Vector2(0, 150)
            };
            comboSample.Items.AddRange(new[] { "Hey sample", "The sample", "Hello", "NTC", "Mobile", "Phone", "Computer" });
            comboSample.SelectIndex = 0;

            formSample2 = new Form(this)
            {
                Size = new Vector2(150, 220),
                SizeMinimum = new Vector2(150, 220),
                Title = "Resize Sample",
                Position = new Vector2(10),
                CanResize = true,
                Anchor = Anchors.Center,                
            };

            checkSample = new CheckButton(formSample2)
            {
                Size = new Vector2(130,20),
                Text = "CheckButton Sample",
                Anchor= Anchors.TopCenter,
                Position = new Vector2(0,10),
            };

            tabSample = new TabControl(formSample2)
            {
                Size = new Vector2(130,100),
                Anchor = Anchors.TopCenter,
                Position = new Vector2(0,40),
            };
            tabSample.AddTab("Tab 01");
            tabSample.AddTab("Tab 02");

            txtSample = new TextBox(this)
            {
                Size = new Vector2(100, 20),
                Position = new Vector2(200, 20),
                MaxLength = 10,
            };
            txtSample.AddSuggestion("Numbers", "Numeric", "Only numbers");
            txtSample.AddSuggestion("Number", "Numeric", "Only numbers");

            checkBox = new CheckBox(this)
            {
                Size = new Vector2(100, 20),
                Position = new Vector2(200, 50),                
            };
        }

        public override void Draw(Batcher2D batcher)
        {
            var text = "Causa [color=#FFC602]75[/color]([color=#477BE5]+133[/color])([color=#CA29E1]+20[/color]) de Dano Mágico nos alvos, e nos [color=red]Gelidez[/color] nos alvos. /n /n Causa [color=#FFC602]50[/color]([color=#477BE5]+98[/color])([color=#CA29E1]+14[/color]) de Dano Mágico em outros alvos próximos afetados por [color=red]Gelidez[/color].";
            
            batcher.Begin();
            //batcher.AddBBCode(text, 12, new Vector2(10, 14), 190);
            batcher.DrawString("FPS:" + Game.FPS, 12, new Vector2(10, 300), Color.White);
                        
            batcher.End();

            base.Draw(batcher);
        }
    }
}
