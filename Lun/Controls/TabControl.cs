using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lun.Controls
{
    public class TabControl : Bond
    {
        Dictionary<string, TabPanel> Items;
        int _hoverIndex = -1;

        /// <summary>
        /// Cor de fundo
        /// </summary>        
        public Color FillColor = new Color(96, 104, 146, 240);

        /// <summary>
        /// Cor da barra
        /// </summary>
        public Color FillColor_Bar = new Color(56, 61, 87, 240);

        public int SelectIndex
        {
            get => _selectIndex;
            set
            {
                if (Items.Count == 0)
                {
                    _selectIndex = -1;
                    return;
                }

                if (_selectIndex != value)
                {
                    _selectIndex = value;

                    var select = Items.Values.ElementAt(value);
                    foreach (var i in Items.Values)
                        if (i != select)
                            i.Hide();
                    select.Show();

                    OnSelectIndex?.Invoke(this);
                }
            }
        }
        int _selectIndex = 0;

        public event HandleCommon OnSelectIndex;

        /// <summary>
        /// Construtor
        /// </summary>
        /// <param name="bond"></param>
        public TabControl(Bond bond) : base(bond)
        {
            Items = new Dictionary<string, TabPanel>();
        }

        /// <summary>
        /// Indexador
        /// </summary>
        /// <param name="Index"></param>
        /// <returns></returns>
        public TabPanel this[int Index]
            => Items.Values.ElementAt(Index);

        /// <summary>
        /// Indexador
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public TabPanel this[string name]
            => Items[name];

        /// <summary>
        /// Desenha os painel
        /// </summary>
        public override void Draw()
        {
            var count = Items.Count;
            if (count > 0)
            {
                var gp = GlobalPosition();

                float xOff = 0;
                for (int i = 0; i < count; i++)
                {
                    var pos = gp + new Vector2(xOff, SelectIndex == i ? 0 : 5);
                    DrawRoundedRectangle(pos,
                        new Vector2(10 + GetTextWidth(Items.Keys.ElementAt(i), 12), SelectIndex == i ? 25 : 20),
                        SelectIndex == i ? FillColor_Bar : (_hoverIndex == i ? FillColor_Bar + new Color(100, 100, 100) : Color.White), 4f, 0, 8);

                    DrawText(Items.Keys.ElementAt(i), 12,
                        pos + new Vector2(5, (SelectIndex == i ? 25 : 20) / 2 - 7),
                        SelectIndex == i ? Color.White : FillColor_Bar);

                    xOff += 10 + GetTextWidth(Items.Keys.ElementAt(i), 12) + 1;
                }
            }

            base.Draw();
        }

        public override bool MouseMoved(Vector2 e)
        {
            _hoverIndex = -1;
            if (Hover())
            {
                var gp = GlobalPosition();
                var count = Items.Count;
                if (count > 0)
                {
                    float xOff = 0;
                    for (int i = 0; i < count; i++)
                    {
                        var pos = gp + new Vector2(xOff, SelectIndex == i ? 0 : 5);

                        if (new Rectangle(pos,
                            new Vector2(10 + GetTextWidth(Items.Keys.ElementAt(i), 12),
                            SelectIndex == i ? 25 : 20)).Contains(e))
                        {
                            _hoverIndex = i;
                            return true;
                        }

                        xOff += 10 + GetTextWidth(Items.Keys.ElementAt(i), 12) + 1;
                    }
                }
            }

            return base.MouseMoved(e);
        }

        public override bool MouseReleased(MouseButtonEventArgs e)
        {
            if (Hover() && _hoverIndex > -1)
                SelectIndex = _hoverIndex;
            return base.MouseReleased(e);
        }

        /// <summary>
        /// Adiciona um novo tab
        /// </summary>
        /// <param name="Name"></param>
        public void AddTab(string Name)
        {
            var p = new TabPanel(this)
            {
                Position = new Vector2(0, 25),
                Size = new Vector2(Size.x, Size.y - 25),
                Visible = false,
            };

            if (Items.Count == 0)
                p.Show();

            Items.Add(Name, p);
        }

        public new void Resize()
        {
            foreach (var i in Items.Values)
                i.Size = new Vector2(Size.x, Size.y - 25);
        }
    }
}
