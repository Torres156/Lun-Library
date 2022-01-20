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

        public override void LoadContent()
        {
            formSample = new Form(this)
            {
                Size = new Vector2(150,220),
                Title = "Fixed Size",
                Position = new Vector2(10),
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
        }


    }
}
