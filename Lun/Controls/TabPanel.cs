using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lun.Controls
{
    public class TabPanel : Bond
    {
        /// <summary>
        /// Construtor
        /// </summary>
        /// <param name="bond"></param>
        public TabPanel(Bond bond) : base(bond)
        {

        }

        public override void Draw()
        {
            var tc = Bond as TabControl;
            var gp = GlobalPosition();
            DrawRoundedRectangle(gp,
                Size,
                tc.FillColor_Bar, 0, 4f, 8);

            base.Draw();
        }
    }
}
