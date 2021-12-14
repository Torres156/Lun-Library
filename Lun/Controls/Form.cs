
namespace Lun.Controls
{
    using static LunEngine;
    public class Form : Bond
    {
        public const int BAR_HEIGHT = 30;

        #region Properties
        /// <summary>
        /// Titulo do formulário
        /// </summary>
        public string Title = "";

        /// <summary>
        /// Cor de fundo
        /// </summary>        
        public Color FillColor = new Color(96, 104, 146, 240);

        /// <summary>
        /// Cor da barra
        /// </summary>
        public Color FillColor_Bar = new Color(56, 61, 87, 240);

        /// <summary>
        /// Botão de fechar
        /// </summary>
        public bool UseButtonExit = true;
        bool hover_exit = false;

        /// <summary>
        /// Arredondamento
        /// </summary>
        public int Radius = 6;

        /// <summary>
        /// Cor da borda
        /// </summary>
        public Color OutlineColor = new Color(200, 200, 200);

        /// <summary>
        /// Espessura da borda
        /// </summary>
        public int OutlineThickness = 1;

        /// <summary>
        /// Pode ser arrastado
        /// </summary>
        public bool canDragged = true;

        private bool isModal = false;

        /// <summary>
        /// Pode redimensionar
        /// </summary>
        public bool CanResize = false;

        public bool ResizeRelative = false;

        /// <summary>
        /// Tamanho mínimo
        /// </summary>
        public Vector2 SizeMinimum;

        /// <summary>
        /// Tamanho máximo
        /// </summary>
        public Vector2 SizeMaximum;


        bool modeResize = false;
        Vector2 pointOriginResize;

        public event HandleCommon OnResizeChanged;

        #endregion

        #region Methods
        /// <summary>
        /// Construtor
        /// </summary>
        /// <param name="bond"></param>
        public Form(Bond bond) : base(bond)
        {

        }

        /// <summary>
        /// Desenha o formulario
        /// </summary>
        /// <param name="target"></param>
        /// <param name="states"></param>
        public override void Draw()
        {
            var gp = GlobalPosition();

            // Borda
            if (OutlineThickness != 0)
                DrawRoundedRectangle(gp, Size, Color.Transparent, Radius, 8, OutlineThickness, OutlineColor);

            // Barra
            DrawRoundedRectangle(gp, new Vector2(Size.x, BAR_HEIGHT), FillColor_Bar, Radius, 0, 8);

            // Fundo
            DrawRoundedRectangle(gp + new Vector2(0, BAR_HEIGHT), new Vector2(Size.x, Size.y - BAR_HEIGHT), FillColor, 0, Radius, 8);

            // Resize
            if (CanResize)
            {
                var off = Radius != 0 ? Radius : 5;
                DrawLine(gp + Size - new Vector2(off, 1), gp + Size - new Vector2(1, off), OutlineColor);
                DrawLine(gp + Size - new Vector2(off + 4, 1), gp + Size - new Vector2(1, off + 4), OutlineColor);
            }

            var currentTitle = Title;
            string realDisplay = currentTitle;
            float maxWidth = Size.x - 20 - (UseButtonExit ? 30 : 4);
            if (GetTextWidth(currentTitle, 12) > maxWidth)
            {
                realDisplay = "";
                for (int i = currentTitle.Length - 1; i > 0; i--)
                    if (GetTextWidth(currentTitle.Substring(0, i), 12) < maxWidth)
                    {
                        realDisplay = currentTitle.Substring(0, i);
                        break;
                    }
            }

            DrawText(realDisplay, 12, gp + new Vector2(10, 7), Color.White);

            // Botão de fechar
            if (UseButtonExit)
            {
                if (hover_exit)
                    DrawRoundedRectangle(gp + new Vector2(Size.x - 24, 4), new Vector2(20), new Color(255, 255, 255, 40), 4, 4);
                DrawText("X", 11, gp + new Vector2(Size.x - 24 + (20 - GetTextWidth("X", 11)) / 2, 7), Color.White);
            }
            base.Draw();
        }

        /// <summary>
        /// Movimento do mouse
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public override bool MouseMoved(Vector2 e)
        {

            var result = base.MouseMoved(e);
            var gp = GlobalPosition();

            hover_exit = false;

            if (modeResize)
            {
                var off = Radius != 0 ? Radius : 5;
                var resize_value = e - pointOriginResize;

                if (ResizeRelative)
                    Size += MathF.Max(resize_value.x, resize_value.y);
                else
                    Size += resize_value;

                pointOriginResize = gp + Size - new Vector2(off + 4, off + 4);

                if (SizeMinimum.x > 0)
                    Size.x = Math.Max(SizeMinimum.x, Size.x);
                else
                    Size.x = Math.Max(BAR_HEIGHT, Size.x);

                if (SizeMinimum.y > 0)
                    Size.y = Math.Max(SizeMinimum.y, Size.y);
                else
                    Size.y = Math.Max(BAR_HEIGHT + 1, Size.y);

                if (SizeMaximum.x > 0)
                    Size.x = Math.Min(Size.x, SizeMaximum.x);
                else
                    Size.x = Math.Min(Size.x, Game.WindowSize.x);

                if (SizeMaximum.y > 0)
                    Size.y = Math.Min(Size.y, SizeMaximum.y);
                else
                    Size.y = Math.Min(Size.y, Game.WindowSize.y);

                return true;
            }

            if (Hover())
                if (UseButtonExit && e.x >= gp.x + (Size.x - 24) && e.x <= gp.x + Size.x - 4
                    && e.y >= gp.y + 4 && e.y <= gp.y + 28)
                {
                    hover_exit = true;
                }

            return result;
        }

        /// <summary>
        /// Solta o clique
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public override bool MouseReleased(MouseButtonEventArgs e)
        {
            var result = base.MouseReleased(e);

            if (modeResize)
            {
                modeResize = false;
                Game.Scene.priority = null;
                OnResizeChanged.Invoke(this);
                return true;
            }

            if (Hover())
            {
                if (UseButtonExit && hover_exit)
                {
                    Hide();
                    Bond?.RemoveFocusForm(this);
                }
            }
            return result;
        }

        /// <summary>
        /// Clique pressionado
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public override bool MousePressed(MouseButtonEventArgs e)
        {
            var result = base.MousePressed(e);

            if (modeResize)
                return true;

            if (Hover())
            {
                Bond?.SetFocusForm(this);

                var gp = GlobalPosition();
                if (canDragged)
                    if (e.X >= gp.x && e.X <= gp.x + Size.x - 24)
                        if (e.Y >= gp.y && e.Y <= gp.y + BAR_HEIGHT)
                        {
                            var mousep = new Vector2(e.X, e.Y) - gp;
                            Bond?.SetDragForm(this, mousep);
                        }

                // Resize
                if (CanResize)
                {
                    var off = Radius != 0 ? Radius : 5;
                    var pos = gp + Size - new Vector2(off + 4, off + 4);
                    if (new Rectangle(pos, new Vector2(off + 4)).Contains(Game.MousePosition))
                    {
                        Game.Scene.SetControlPriority(this);
                        modeResize = true;
                        pointOriginResize = Game.MousePosition;
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Deixa o formulário visivel
        /// </summary>
        public new void Show()
        {
            base.Show();
            Bond?.SetFocusForm(this);
        }

        public void ShowDialog()
        {
            isModal = true;
            Game.Scene.form_Priority = this;
            Show();
        }

        /// <summary>
        /// Esconde o formulário
        /// </summary>
        public new void Hide()
        {
            base.Hide();
            Bond?.RemoveFocusForm(this);

            if (isModal)
            {
                Game.Scene.RemovePriority();
            }
        }

        /// <summary>
        /// Altera a visibilidade do formulário
        /// </summary>
        public new void Toggle()
        {
            if (Visible)
                Hide();
            else
                Show();
        }

        public virtual void Form_Dragged()
        { }

        #endregion

    }
}
