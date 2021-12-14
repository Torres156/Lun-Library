using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public uint PointCount = 8;

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
        public override void Draw()
        {
            var gp = GlobalPosition();

            if (Radius > 0)
                DrawRoundedRectangle(gp, Size, FillColor, Radius, PointCount, -1, OutlineColor);
            else
                DrawRectangle(gp, Size, FillColor, -1, OutlineColor);

            if (Checked)
                if (Radius > 0)
                    DrawRoundedRectangle(gp + 2, Size - 4, OutlineColor, Radius, PointCount);
                else
                    DrawRectangle(gp + 2, Size - 4, OutlineColor);
            base.Draw();
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
