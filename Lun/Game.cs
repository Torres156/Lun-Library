using Lun.Controls;
using Lun.SFML.Graphics;
using Lun.SFML.Window;
using System;
using System.Runtime;
using System.Runtime.InteropServices;

namespace Lun
{
    using static LunEngine;
    public static class Game
    {
        private const int SW_MAXIMIZE = 3;
        private const int SW_MINIMIZE = 6;
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetWindowPlacement(IntPtr hWnd, ref WINDOWPLACEMENT lpwndpl);

        struct WINDOWPLACEMENT
        {
            public int Lenght;
            public int flags;
            public int showCmd;
            public Point ptMinPosition;
            public Point ptMaxPosition;
            public IntRect rcNormalPosition;
        }

        public static bool Running = false;                                                 // Estado do jogo
        public static int FPS { get; private set; }                                         // FPS atual
        public static Vector2 MousePosition { get; private set; }                           // Posição do mouse
        public static readonly string Path = AppDomain.CurrentDomain.BaseDirectory + "/";   // Diretório do jogo
        public static RenderWindow Window { get; private set; }                             // Dispositivo gráfico para janela
        public static float FixedPhysicTime = 1f / 60f;                                     // Tempo fixo para processamento físico
        public static Color BackgroundColor = Color.CornflowerBlue;                         // Cor de fundo
        public static SceneBase Scene { get; private set; }                                 // Cena atual
        public static uint AntiAliasing = 8;


        // Configurações de Janela
        public static Vector2 WindowSizeMin = new Vector2(640, 360);
        public static Vector2 WindowSize = new Vector2(1024, 600);
        public static string WindowTitle = "Lun";
        public static bool WindowMaximize = false;
        public static bool WindowCanResize = false;
        public static bool WindowFullscreen = false;
        public static Camera2D DefaultCamera { get; private set; }


        // Events
        public static event Action OnUpdate;
        public static event Action OnClosed;
        public static event Action OnResize;


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
            long timerDelay = 0, timerNew = 0, timerOld = 0;
            long timerFps = 0, timerAnimation = 0;
            int countFPS = 0;
            float accumulate = 0;

            GCSettings.LatencyMode = GCLatencyMode.LowLatency;            

            timerNew = Environment.TickCount64;            
            while (Running)
            {
                if (Environment.TickCount64 > timerDelay)
                {
                    timerOld = timerNew;
                    timerNew = Environment.TickCount64;

                    if (Environment.TickCount64 > timerAnimation)
                    {
                        TextBox.s_animation = !TextBox.s_animation;
                        timerAnimation = Environment.TickCount64 + 250;
                    }

                    Sound.ProcessSounds();

                    var delta = (timerNew - timerOld) / 1000f;
                    accumulate += delta;
                    while(accumulate >= FixedPhysicTime)
                    {
                        accumulate -= FixedPhysicTime;
                        Scene?.FixedUpdate();
                    }
                    Scene?.Update();
                    OnUpdate?.Invoke();

                    Window.DispatchEvents();

                    BeginRender(Window);

                    ClearColor(BackgroundColor);

                    BeginCamera(DefaultCamera);
                    Scene?.Draw();
                    EndCamera();

                    EndRender();

                    timerDelay = Environment.TickCount64 + 1;
                }

                countFPS++;
                if (Environment.TickCount64 > timerFps)
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
            var video = new VideoMode((uint)WindowSize.x, (uint)WindowSize.y);
            var context = new ContextSettings(32, 8, AntiAliasing, 4, 6, ContextSettings.Attribute.Debug, false);
            if (!WindowFullscreen)
                Window = new RenderWindow(video, WindowTitle, WindowCanResize ? Styles.Close | Styles.Resize : Styles.Close, context);
            else
                Window = new RenderWindow(video, WindowTitle, Styles.Fullscreen, context);

            if (WindowMaximize)
                ShowWindow(Window.SystemHandle, SW_MAXIMIZE);

            Window.SetActive(false);
            Window.SetFramerateLimit(0);
            Window.SetVerticalSyncEnabled(false);            
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
            MousePosition = new Vector2(e.X, e.Y);
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
            bool isUpdate = false;
            if (e.Width < WindowSizeMin.x || e.Height < WindowSizeMin.y)
                isUpdate = true;

            if (Window != null)
            {
                WINDOWPLACEMENT cmd = new WINDOWPLACEMENT();
                cmd.Lenght = Marshal.SizeOf(cmd);
                GetWindowPlacement(Window.SystemHandle, ref cmd);
                WindowMaximize = cmd.showCmd == 3;
            }

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
                Window = new RenderWindow(VideoMode.DesktopMode, WindowTitle, Styles.Fullscreen,
                    new ContextSettings(32,0, AntiAliasing));
                Window.SetVerticalSyncEnabled(false);
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

        public static void SetScene<T>(params object[] args) where T : SceneBase
        {
            Scene?.UnloadContent();
            Scene = (T)Activator.CreateInstance(typeof(T), args) ;
            Scene.LoadContent();
        }

        public static T GetScene<T>() where T : SceneBase
            => (T)Scene;
    }
}
