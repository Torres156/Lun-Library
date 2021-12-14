namespace Lun.Controls
{
    public class ComboBox : Bond
    {
        internal const int HEIGHT = 20;
        const int ITEM_DISPLAY = 6;

        // Items
        public List<string> Items;


        // Design
        public Color FillColor = Color.White;
        public Color FillColor_Hover = new Color(200, 200, 200);
        public Color OutlineColor = new Color(120, 120, 120);
        public float OutlineThickness = 0;
        public Color TextColor = Color.Black;
        public float Radius = 4f;
        public uint CornerPoints = 8;
        public int TextSize = 11;

        public new Vector2 Size
        {
            get => base.Size;
            set
               => base.Size = new Vector2(value.x, isOpen ? HEIGHT + 4 + 14 * Math.Min(Items.Count, ITEM_DISPLAY) : HEIGHT);
        }

        public int SelectIndex
        {
            get => _selectIndex;
            set
            {
                _selectIndex = value;
                OnSelectIndexChanged?.Invoke(this);
            }
        }
        int _selectIndex = -1;


        // Internals
        internal bool isOpen = false;
        internal int hover = 0;


        // Privates
        RenderTexture render;
        ScrollVertical scrlTop;

        // Events
        public event HandleCommon OnSelectIndexChanged;


        public ComboBox(Bond bond) : base(bond)
        {
            Items = new List<string>();

            scrlTop = new ScrollVertical(this)
            {
                Anchor = Anchors.TopRight,
                Position = new Vector2(2, HEIGHT + 2),
                Size = new Vector2(8, ITEM_DISPLAY * 14),
                Visible = false,
            };

            OnMouseReleased += ComboBox_OnMouseReleased;
            OnMouseMove += ComboBox_OnMouseMove;
        }

        private void ComboBox_OnMouseMove(ControlBase sender, Vector2 e)
        {
            var gp = GlobalPosition();

            if (isOpen)
            {
                if (Items.Count > ITEM_DISPLAY)
                {
                    var area = new Rectangle(gp + new Vector2(2, HEIGHT + 2),
                        new Vector2(Size.x - 12, 14 * ITEM_DISPLAY));
                    if (area.Contains(e))
                        for (int i = 0; i < Items.Count; i++)
                        {
                            var rec = new Rectangle(gp + new Vector2(2, HEIGHT + 2 + 14 * i - scrlTop.Value), new Vector2(Size.x - 12, 14));
                            if (rec.Contains(e))
                            {
                                hover = i;
                                return;
                            }
                        }
                }
                else
                {
                    for (int i = 0; i < Items.Count; i++)
                    {
                        var rec = new Rectangle(gp + new Vector2(2, HEIGHT + 2 + 14 * i), new Vector2(Size.x - 4, 14));
                        if (rec.Contains(e))
                        {
                            hover = i;
                            return;
                        }
                    }
                }
            }
        }

        private void ComboBox_OnMouseReleased(ControlBase sender, MouseButtonEventArgs e)
        {
            if (Items.Count == 0)
                return;

            if (!isOpen)
            {
                isOpen = true;
                Size = new Vector2(Size.x, 0);
                Game.Scene.priority = this;
            }
            else
            {
                var gp = GlobalPosition();
                var area = new Rectangle(gp + new Vector2(2, HEIGHT + 2),
                       new Vector2(Size.x - 12, 14 * ITEM_DISPLAY));
                if (area.Contains(Game.MousePosition))
                {
                    SelectIndex = hover;
                    isOpen = false;
                    Size = new Vector2(Size.x, 0);
                    Game.Scene.priority = null;
                }
            }
        }

        public new void AddControl(ControlBase child)
        {
            if (!(child is ScrollVertical))
                throw new Exception("Error: Lun::Control::ComboBox::AddChild()\nSomente controle do tipo Scroll pode ser vinculado!");

            base.AddControl(child);
        }

        public override void Draw()
        {
            if (SelectIndex >= Items.Count) SelectIndex = 0;
           
            var gp = GlobalPosition();
            DrawRoundedRectangle(gp, Size, FillColor, Radius, CornerPoints);

            if (Items.Count > 0)
            {
                if (SelectIndex >= 0)
                {
                    var currentText = Items[SelectIndex];
                    DrawText(currentText, TextSize, gp + new Vector2(4, 2), TextColor);
                }
                if (isOpen)
                {
                    DrawLineShape(gp + new Vector2(4, HEIGHT), gp + new Vector2(Size.x - 4, HEIGHT), OutlineColor);

                    if (Items.Count > ITEM_DISPLAY)
                    {
                        render = render ?? CreateRender2D((int)Size.x - 12, 14 * ITEM_DISPLAY);

                        scrlTop.Visible = true;
                        scrlTop.Maximum = 14 * (Items.Count - ITEM_DISPLAY);
                        scrlTop.Velocity = Math.Max(5, scrlTop.Maximum / 100);
                        BeginRender(render);
                        ClearColor(Color.Transparent);

                        for (int i = 0; i < Items.Count; i++)
                        {
                            var y = 14 * i - scrlTop.Value;
                            if (y + 14 >= 0 && y <= render.Size.Y)
                            {
                                if (i == hover)
                                    DrawRoundedRectangle(new Vector2(0, 14 * i - scrlTop.Value), new Vector2(Size.x - 12, 14), FillColor_Hover,
                                        Radius, CornerPoints);

                                DrawText(Items[i], TextSize, new Vector2(2, 14 * i - scrlTop.Value), TextColor);
                            }
                        }
                        EndRender();

                        DrawRenderTexture(render, gp + new Vector2(2, HEIGHT + 2), Color.White);
                    }
                    else
                    {
                        scrlTop.Visible = false;
                        for (int i = 0; i < Items.Count; i++)
                        {
                            if (i == hover)
                                DrawRoundedRectangle(gp + new Vector2(2, HEIGHT + 2 + 14 * i), new Vector2(Size.x - 4, 14), FillColor_Hover,
                                    Radius, CornerPoints);

                            DrawText(Items[i], TextSize, gp + new Vector2(4, HEIGHT + 2 + 14 * i), TextColor);
                        }
                    }
                }
                else
                {
                    scrlTop.Visible = false;
                    hover = 0;
                }
            }
            base.Draw();
        }
    }
}
