namespace Lun.Controls
{
    public class ListBox : Bond
    {
        public List<string> Items;

        public int SelectIndex
        {
            get => _selectIndex;
            set
            {
                _selectIndex = value;
            }
        }
        int _selectIndex = -1;


        //Design 
        public Color FillColor = Color.White;
        public Color TextColor = Color.Black;
        public Color OutlineColor = new Color(56, 61, 87);
        public int OutlineThickness = 0;
        public float Radius = 4f;
        public uint CornerPoints = 8;


        RenderTexture render;
        ScrollVertical scrlTop;
        int _hover;

        public ListBox(Bond bond) : base(bond)
        {
            Items = new List<string>();

            scrlTop = new ScrollVertical(this)
            {
                Anchor = Anchors.TopRight,
                Position = new Vector2(2, 1),
            };

            OnMouseMove += ListBox_OnMouseMove;
            OnMouseReleased += ListBox_OnMouseReleased;
        }

        private void ListBox_OnMouseReleased(ControlBase sender, MouseButtonEventArgs e)
        {
            if (_hover > -1)
                SelectIndex = _hover;
        }

        private void ListBox_OnMouseMove(ControlBase sender, Vector2 e)
        {
            var gp = GlobalPosition();
            _hover = -1;
            if (Items.Count * 14 > Size.y - 2)
            {
                var area = new Rectangle(gp + new Vector2(1), Size - new Vector2(2));
                if (area.Contains(e))
                    for (int i = 0; i < Items.Count; i++)
                    {
                        var rec = new Rectangle(gp + new Vector2(1, 1 + 14 * i - scrlTop.Value), new Vector2(Size.x - 12, 14));
                        if (rec.Contains(e))
                        {
                            _hover = i;
                            return;
                        }
                    }
            }
            else
            {
                for (int i = 0; i < Items.Count; i++)
                {
                    var rec = new Rectangle(gp + new Vector2(1, 1 + 14 * i), new Vector2(Size.x - 2, 14));
                    if (rec.Contains(e))
                    {
                        _hover = i;
                        return;
                    }
                }
            }
        }

        public new void AddControl(ControlBase child)
        {
            if (!(child is ScrollVertical))
                throw new Exception("Error: Lun::Control::ListBox::AddControl()\nSomente controle do tipo Scroll pode ser vinculado!");

            base.AddControl(child);
        }

        public override void Draw()
        {
            var gp = GlobalPosition();

            DrawRoundedRectangle(gp, Size, FillColor, Radius, CornerPoints, OutlineThickness, OutlineColor);

            if (SelectIndex >= Items.Count)
                SelectIndex = -1;

            if (Items.Count * 14 > Size.y - 2)
            {
                render = render ?? CreateRender2D((int)Size.x - 12, (int)Size.y - 2);
                scrlTop.Maximum = Items.Count * 14 - ((int)Size.y - 2);
                scrlTop.Velocity = Math.Max(5, scrlTop.Maximum / 100);
                scrlTop.Size = new Vector2(8, Size.y - 2);
                scrlTop.Visible = true;

                BeginRender(render);
                ClearColor(Color.Transparent);
                for (int i = 0; i < Items.Count; i++)
                {
                    var y = 14 * i - scrlTop.Value;
                    if (y + 14 >= 0 && y <= render.Size.Y)
                    {
                        if (i == _hover)
                            DrawRoundedRectangle(new Vector2(1, 14 * i - scrlTop.Value), new Vector2(Size.x - 13, 14), new Color(0, 0, 0, 80), Radius, CornerPoints);
                        if (i == SelectIndex)
                            DrawRoundedRectangle(new Vector2(1, 14 * i - scrlTop.Value), new Vector2(Size.x - 13, 14), new Color(96, 104, 146, 240), Radius, CornerPoints);

                        DrawText(Items[i], 11, new Vector2(2, 14 * i - scrlTop.Value), TextColor);
                    }
                }
                EndRender();

                DrawRenderTexture(render, gp + new Vector2(1, 1), Color.White);
            }
            else
            {
                scrlTop.Visible = false;
                for (int i = 0; i < Items.Count; i++)
                {
                    if (i == _hover)
                        DrawRoundedRectangle(gp + new Vector2(1, 1 + 14 * i), new Vector2(Size.x - 2, 14), new Color(0, 0, 0, 80), Radius, CornerPoints);

                    if (i == SelectIndex)
                        DrawRoundedRectangle(gp + new Vector2(1, 1 + 14 * i), new Vector2(Size.x - 2, 14), new Color(96, 104, 146, 240), Radius, CornerPoints);

                    DrawText(Items[i], 11, gp + new Vector2(4, 2 + 14 * i), TextColor);
                }
            }

            base.Draw();
        }
    }
}
