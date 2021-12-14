
namespace Lun.Controls
{
    using SFML.Window;
    public abstract class ControlBase
    { 
        #region Properties
        public string Name = "";

        /// <summary>
        /// Posição
        /// </summary>
        public Vector2 Position { get; set; }

        /// <summary>
        /// Tamanho
        /// </summary>
        public Vector2 Size;

        /// <summary>
        /// Ancora
        /// </summary>
        public Anchors Anchor = Anchors.TopLeft;

        public Bond Bond;

        /// <summary>
        /// Visibilidade do controle
        /// </summary>
        public bool Visible
        {
            get => _visible;
            set
            {
                if (_visible != value)
                {
                    _visible = value;
                    OnVisibleChanged?.Invoke(this);
                }
            }
        }

        bool _visible = true;
        #endregion

        #region Events
        // Events
        public event HandleDraw OnDraw;
        public event HandleMouseButton OnMousePressed, OnMouseReleased;
        public event HandleCommon OnVisibleChanged;
        public event HandleMouseMove OnMouseMove;
        public event HandleMouseScrolled OnMouseScrolled;

        // Delegates
        public delegate void HandleCommon(ControlBase sender);
        public delegate void HandleDraw(ControlBase sender);
        public delegate void HandleMouseButton(ControlBase sender, MouseButtonEventArgs e);
        public delegate void HandleMouseScrolled(ControlBase sender, MouseWheelScrollEventArgs e);
        public delegate void HandleMouseMove(ControlBase sender, Vector2 e);
        #endregion

        #region Methods

        /// <summary>
        /// Construtor
        /// </summary>
        public ControlBase()
        {

        }

        /// <summary>
        /// Construtor
        /// </summary>
        /// <param name="Bond"></param>
        public ControlBase(Bond Bond)
        {
            this.Bond = Bond;
            Bond?.AddControl(this);
        }

        /// <summary>
        /// Posição global
        /// </summary>
        /// <returns></returns>
        public Vector2 GlobalPosition()
        {
            var pos = Position;
            var bondpos = Bond != null ? Bond.GlobalPosition() : new Vector2();
            var bondsize = Bond != null ? Bond.Size : (Vector2)Game.WindowSize;

            if (Bond != null && Bond is Form)
            {
                bondpos += new Vector2(0, 2 + Form.BAR_HEIGHT);
                bondsize -= new Vector2(0,4 + Form.BAR_HEIGHT);
            }

            switch (Anchor)
            {
                case Anchors.TopLeft:
                    pos = bondpos + pos;
                    break;

                case Anchors.TopCenter:
                    pos.x = bondpos.x + pos.x + (bondsize.x - Size.x) / 2;
                    pos.y = bondpos.y + pos.y;
                    break;

                case Anchors.TopRight:
                    pos.x = bondpos.x + bondsize.x - Size.x - pos.x;
                    pos.y = bondpos.y + pos.y;
                    break;

                case Anchors.Left:
                    pos.x = bondpos.x + pos.x;
                    pos.y = bondpos.y + pos.y + (bondsize.y - Size.y) / 2;
                    break;

                case Anchors.Center:
                    pos = bondpos + pos + (bondsize - Size) / 2;
                    break;

                case Anchors.Right:
                    pos.x = bondpos.x - pos.x + bondsize.x - Size.x;
                    pos.y = bondpos.y + pos.y + (bondsize.y - Size.y) / 2;
                    break;

                case Anchors.BottomLeft:
                    pos.x = bondpos.x + pos.x;
                    pos.y = bondpos.y + bondsize.y - pos.y - Size.y;
                    break;

                case Anchors.BottomCenter:
                    pos.x = bondpos.x + pos.x + (bondsize.x - Size.x) / 2;
                    pos.y = bondpos.y + bondsize.y - pos.y - Size.y;
                    break;

                case Anchors.BottomRight:
                    pos = bondpos + bondsize - pos - Size;
                    break;
            }

            if (this is Form)
            {
                if (pos.x < bondpos.x) pos.x = bondpos.x;
                if (pos.y < bondpos.y) pos.y = bondpos.y;
                if (pos.x + Size.x > bondpos.x + bondsize.x) pos.x = bondpos.x + bondsize.x - Size.x;
                if (pos.y + Size.y > bondpos.y + bondsize.y) pos.y = bondpos.y + bondsize.y - Size.y;
            }

            if (this is ComboBox)
            {
                if (Anchor > Anchors.TopRight && Size.y > ComboBox.HEIGHT)
                    pos.y += Size.y - ComboBox.HEIGHT;
            }

            return pos;
        }

        /// <summary>
        /// Desenha o controle
        /// </summary>
        /// <param name="target"></param>
        /// <param name="states"></param>
        public virtual void Draw()
        {
            OnDraw?.Invoke(this);
        }

        /// <summary>
        /// Mouse pressionado
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public virtual bool MousePressed(MouseButtonEventArgs e)
        {
            if (Hover())
            {
                OnMousePressed?.Invoke(this, e);
                return this.GetType().BaseType.FullName.Contains("SceneBase") ? false : true;
            }
            return false;
        }

        /// <summary>
        /// Mouse quando solta
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public virtual bool MouseReleased(MouseButtonEventArgs e)
        {
            if (Hover())
            {
                OnMouseReleased?.Invoke(this, e);
                var result = this.GetType().BaseType.Name.Contains("SceneBase") ? false : true;
                return result;
            }
            return false;
        }

        /// <summary>
        /// Movimento do mouse
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public virtual bool MouseMoved(Vector2 e)
        {
            if (Hover())
            {
                OnMouseMove?.Invoke(this, e);
                return this.GetType().FullName.Contains("SceneBase") ? false : true;
            }
            return false;
        }

        /// <summary>
        /// Verifica se o mouse está sob o controle
        /// </summary>
        /// <returns></returns>
        public bool Hover()
        {
            var mp = Game.MousePosition;
            var p = GlobalPosition();

            return (mp.x >= p.x && mp.x <= p.x + Size.x)
                && (mp.y >= p.y && mp.y <= p.y + Size.y);
        }

        /// <summary>
        /// Scroll do mouse
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public virtual bool MouseScrolled(MouseWheelScrollEventArgs e)
        {
            if (Hover())
            {
                OnMouseScrolled?.Invoke(this, e);
                bool result = this.GetType().FullName.Contains("SceneBase");
                return result;
            }
            return false;
        }

        /// <summary>
        /// Deixa o controle visível
        /// </summary>
        public void Show()
            => Visible = true;

        /// <summary>
        /// Esconde o controle
        /// </summary>
        public void Hide()
            => Visible = false;

        /// <summary>
        /// Altera a visibilidade do controle
        /// </summary>
        public void Toggle()
            => Visible = !_visible;

        public virtual void Resize()
        { }

        public virtual void Destroy()
        {

        }

        #endregion
    }
}
