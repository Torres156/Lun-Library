# Lun-Library
Version `<1.1.4>`
Lun is a library for Game 2D and UI development, its structure uses SFML 2.5 for graphical rendering.

## Screenshots
<p align="center">
  Sample - Hello<br>
  <img width="60%" src="https://github.com/Torres156/Lun-Library/blob/main/prints/01%20Hello.jpg">
  <br>Sample - Shapes<br>
  <img width="60%" src="https://github.com/Torres156/Lun-Library/blob/main/prints/02%20Shapes.jpg">
  <br>Sample - Texture<br>
  <img width="60%" src="https://github.com/Torres156/Lun-Library/blob/main/prints/03%20Texture.jpg">  
  <br>Sample - UI<br>
  <img width="60%" src="https://github.com/Torres156/Lun-Library/blob/main/prints/04%20UI.jpg">  
</p>

## 💻 Requirements
Before using, it is necessary to verify the requeriments:
* You have installed `<.NET Standard 2.1 / Visual Studio 2019 / Visual Studio 2022>`
* You are using `<C# 10.0>`
* Operating System using `<Window / Linux / Mac>`

## Using Lun-Library
### Nuget package
1. Add Lun-Library to your project with Nuget.
```
Install-Package LunLibrary -Version 1.1.4
```
### Clone this repository
1. Download/Clone this repository.

2. Add Lun-Library/Lun.csproj to your project as an existing project.

3. Add a [CSFML](https://github.com/sfml/csfml) DLL package to your project.
```
dotnet add package CSFML.Redist --version 2.5.1
```
### Start coding!
```csharp
global using Lun;
global using static Lun.LunEngine;
using Lun.Controls;

namespace HelloWorld
{
    static class Program
    {
        public static void Main()
        { 
            // Load a default font
            LoadFont("consola.ttf");
            
            // Window Settings for Start
            Game.WindowTitle = "Lun Engine";              // Window Title
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
        }
    }
}
```

## License
See [License](LICENSE) for details.
