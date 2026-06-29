using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SFML.Window;

namespace Lun.Controls
{
    public class CheckBox : ControlBase
    {
        public bool Checked
        {
            get => _check;
            set
            {
                _check = value;
                OnCheckChanged?.Invoke(this);
            }
        }
        bool _check = false;

        public Color FillColor = new Color(56, 61, 87, 240);

        public Color OutlineColor = Color.White;

        public float Radius = 0f;

        public int PointCount = 8;

        public event HandleCommon OnCheckChanged;               

        /// <summary>
        /// Construtor
        /// </summary>
        /// <param name="bond"></param>
        public CheckBox(Bond bond) : base(bond)
        {
            
        }

        /// <summary>
        /// Desenha a caixa
        /// </summary>
        public override void Draw(Batcher2D batcher)
        {
            var gp = GlobalPosition();

            if (Radius > 0)
                batcher.DrawRoundedRectangle(gp, Size, FillColor, Radius, PointCount, -1, OutlineColor);
            else
                batcher.DrawRectangle(gp, Size, FillColor, -1, OutlineColor);

            if (Checked)
                if (Radius > 0)
                    batcher.DrawRoundedRectangle(gp + 2, Size - 4, OutlineColor, Radius, PointCount);
                else
                    batcher.DrawRectangle(gp + 2, Size - 4, OutlineColor);
            base.Draw(batcher);
        }

        public override bool MouseReleased(MouseButtonEventArgs e)
        {
            if (Hover())
            {
                Checked = !Checked;
            }
            return base.MouseReleased(e);
        }
    }
}
