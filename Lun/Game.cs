using Lun.Controls;
using System;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using SFML.Graphics;
using SFML.Window;

namespace Lun
{
    using static LunEngine;

    public static class Game
    {
        private const int SW_MAXIMIZE = 3;

        private const int SW_MINIMIZE = 6;
        // [DllImport("user32.dll")]
        // [return: MarshalAs(UnmanagedType.Bool)]
        // static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        //
        // [DllImport("user32.dll", SetLastError = true)]
        // [return: MarshalAs(UnmanagedType.Bool)]
        // static extern bool GetWindowPlacement(IntPtr hWnd, ref WINDOWPLACEMENT lpwndpl);
        //
        // struct WINDOWPLACEMENT
        // {
        //     public int Lenght;
        //     public int flags;
        //     public int showCmd;
        //     public Point ptMinPosition;
        //     public Point ptMaxPosition;
        //     public IntRect rcNormalPosition;
        // }

        public static bool Running = false; // Estado do jogo
        public static int FPS { get; private set; } // FPS atual
        public static Vector2 MousePosition { get; private set; } // Posição do mouse
        public static readonly string Path = AppDomain.CurrentDomain.BaseDirectory + "/"; // Diretório do jogo
        public static RenderWindow Window { get; private set; } // Dispositivo gráfico para janela
        public static uint FixedPhysicTime = 60; // Tempo fixo para processamento físico
        public static Color BackgroundColor = Color.CornflowerBlue; // Cor de fundo
        public static SceneBase Scene { get; private set; } // Cena atual
        public static uint AntiAliasing = 0;
        public static float DeltaTime { get; private set; } = 0.0f;
        public static bool PreventTextureMultiThread = false;


        // Configurações de Janela
        public static Vector2 WindowSizeMin = new Vector2(640, 360);
        public static Vector2 WindowSize = new Vector2(1024, 600);
        public static string WindowTitle = "Lun";
        public static bool WindowMaximize = false;
        public static bool WindowCanResize = false;
        public static bool WindowFullscreen = false;
        public static Camera2D DefaultCamera { get; private set; }
        public static bool MouseVisible { get; private set; } = true;
        public static bool VerticalSync { get; private set; } = false;


        // Events
        public static event Action OnUpdate;
        public static event Action OnClosed;
        public static event Action OnResize;
        public static event Action OnDraw;

        public static SFML.System.Clock Clock;


        public static void Run()
        {
            CreateWindow();
            HandleEvents();

            DefaultCamera = new Camera2D(WindowSize);
            Running = true;
            GameLoop();
        }

        static void GameLoop()
        {
            long timerFps = 0, timerAnimation = 0;
            int countFPS = 0;
            float physicTime = 1f / FixedPhysicTime;
            float delta = 0f;
            long timerTexture = 0;
            Clock = new SFML.System.Clock();
            var clock = new SFML.System.Clock();

            var batcher = new Batcher2D();
            while (Running)
            {
                delta = clock.Restart().AsSeconds();
                DeltaTime = delta;
                if (Environment.TickCount64 > timerAnimation)
                {
                    TextBox.s_animation = !TextBox.s_animation;
                    timerAnimation = Environment.TickCount64 + 250;
                }

                if (Environment.TickCount64 > timerTexture)
                {
                    foreach (var texture in cacheTextures)
                        texture?.Unload();
                    timerTexture = Environment.TickCount64 + 5000;
                }

                Sound.ProcessSounds();

                Scene?.Update();
                OnUpdate?.Invoke();

                BeginRender(Window);

                Window.DispatchEvents();

                ClearColor(BackgroundColor);

                BeginCamera(DefaultCamera);
                Scene?.Draw(batcher);
                EndCamera();

                OnDraw?.Invoke();

                EndRender();

                countFPS++;
                if (Environment.TickCount64 >= timerFps)
                {
                    FPS = countFPS;
                    countFPS = 0;
                    timerFps = Environment.TickCount64 + 1000;
                }
            }

            Sound.StopSounds();
            Sound.StopMusic();
            Window.Close();
        }

        static void CreateWindow()
        {
            var video = new VideoMode(WindowSize);
            var context = new ContextSettings(24, 8, AntiAliasing);
            if (!WindowFullscreen)
                Window = new RenderWindow(video, WindowTitle,
                    WindowCanResize ? Styles.Close | Styles.Resize : Styles.Close,State.Windowed, context);
            else
                Window = new RenderWindow(video, WindowTitle, Styles.Close,State.Fullscreen, context);

            Window.SetActive(false);
            Window.SetFramerateLimit(FixedPhysicTime);
            Window.SetVerticalSyncEnabled(VerticalSync);
        }

        static void HandleEvents()
        {
            Window.Resized += Window_Resized;
            Window.Closed += Window_Closed;
            Window.MouseButtonPressed += Window_MouseButtonPressed;
            Window.MouseButtonReleased += Window_MouseButtonReleased;
            Window.MouseMoved += Window_MouseMoved;
            Window.TextEntered += Window_TextEntered;
            Window.MouseWheelScrolled += Window_MouseWheelScrolled;
            Window.KeyPressed += Window_KeyPressed;
            Window.KeyReleased += Window_KeyReleased;
        }

        private static void Window_KeyReleased(object sender, KeyEventArgs e)
        {
            Scene?.KeyReleased(e);
        }

        private static void Window_KeyPressed(object sender, KeyEventArgs e)
        {
            Scene?.KeyPressed(e);
        }

        private static void Window_MouseWheelScrolled(object sender, MouseWheelScrollEventArgs e)
        {
            Scene?.MouseScrolled(e);
        }

        private static void Window_TextEntered(object sender, TextEventArgs e)
        {
            Scene?.TextEntered(e);
        }

        private static void Window_MouseMoved(object sender, MouseMoveEventArgs e)
        {
            MousePosition = new Vector2(e.Position.X, e.Position.Y);
            Scene?.MouseMoved(MousePosition);
        }

        private static void Window_MouseButtonReleased(object sender, MouseButtonEventArgs e)
        {
            Scene?.MouseReleased(e);
        }

        private static void Window_MouseButtonPressed(object sender, MouseButtonEventArgs e)
        {
            Scene?.MousePressed(e);
        }

        private static void Window_Closed(object sender, EventArgs e)
        {
            OnClosed?.Invoke();
            Running = false;
        }

        private static void Window_Resized(object sender, SizeEventArgs e)
        {
            bool isUpdate = e.Size.X < WindowSizeMin.x || e.Size.Y < WindowSizeMin.y;

            if (isUpdate)
            {
                Window.Close();
                Window = null;
                WindowSize = WindowSizeMin;
                CreateWindow();
                HandleEvents();
            }

            WindowSize = (Vector2)Window.Size;
            Scene?.Resize();
            OnResize?.Invoke();
            DefaultCamera.Size = WindowSize;
            DefaultCamera.Center = (WindowSize / 2);
        }

        public static void ToggleFullscreen()
        {
            if (!WindowFullscreen)
            {
                Window.Close();
                Window = new RenderWindow(VideoMode.DesktopMode, WindowTitle, Styles.Close,State.Fullscreen,
                    new ContextSettings(24, 8, AntiAliasing));
                Window.SetVerticalSyncEnabled(VerticalSync);
                HandleEvents();
                WindowFullscreen = true;
                WindowSize = (Vector2)Window.Size;
            }
            else
            {
                Window.Close();
                CreateWindow();
                HandleEvents();
                WindowFullscreen = false;
            }

            //if (scene != null)
            //    scene.Resize();
        }

        public static void WindowReload()
        {
            Window.Close();
            Window = null;
            CreateWindow();
            HandleEvents();
            Scene?.Resize();
            OnResize?.Invoke();
            DefaultCamera.Size = WindowSize;
            DefaultCamera.Center = (WindowSize / 2);
        }

        public static void SetScene<T>(params object[] args) where T : SceneBase
        {
            Scene?.UnloadContent();
            Scene = (T)Activator.CreateInstance(typeof(T), args);            
            Scene.LoadContent();
            Scene.UpdatePosition();
        }

        public static T GetScene<T>() where T : SceneBase => (T)Scene;

        public static void SetMouseVisible(bool value)
        {
            Window.SetMouseCursorVisible(value);
            MouseVisible = value;
        }
    }
}