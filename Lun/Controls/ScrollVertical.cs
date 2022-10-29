
namespace Lun.Controls
{
    public class ScrollVertical : ControlBase
    {
        public int Maximum = 100;
        public int Value = 0;
        public int Velocity = 5;
        public bool CanScrollWithBond = true;


        // Design
        public Color BackgroundColor = new Color(50, 54, 76, 240);
        public Color FillColor = Color.White;
        public float Radius = 4f;
        public uint CornerPoints = 8;

        internal bool _press = false;
        internal static ScrollVertical Current;


        public ScrollVertical(Bond bond) : base(bond)
        {
            OnMousePressed += ScrollVertical_OnMousePressed;
            OnMouseReleased += ScrollVertical_OnMouseReleased;
        }

        private void ScrollVertical_OnMouseReleased(ControlBase sender, MouseButtonEventArgs e)
        {
            _press = false;
            Current = null;
        }

        private void ScrollVertical_OnMousePressed(ControlBase sender, MouseButtonEventArgs e)
        {
            var gp = GlobalPosition();
            float realHeight = Size.y - 2;
            float percent = 1f / (float)(Maximum + 1);
            float barHeight = MathF.Max(16f, realHeight * percent);
            percent = Value / (float)(Maximum + 1);
            float y = (realHeight - barHeight) * percent;

            var pos = gp + new Vector2(1, 1 + y);
            if (new Rectangle(pos, new Vector2(Size.x - 2, barHeight)).Contains(Game.MousePosition))
            {
                _press = true;
                Current = this;
            }
        }

        public override void Draw()
        {
            var gp = GlobalPosition();

            // Background
            DrawRoundedRectangle(gp, Size, BackgroundColor, Radius, CornerPoints);

            float realHeight = Size.y - 2;
            float percent = 1f / (float)(Maximum);
            float barHeight = MathF.Max(16f, realHeight * percent);
            percent = Value / (float)(Maximum + 1);
            float y = (realHeight - barHeight) * percent;
            DrawRoundedRectangle(gp + new Vector2(1, 1 + y), new Vector2(Size.x - 2, barHeight), FillColor, Radius, CornerPoints);

            // Change value
            if (_press)
            {
                var v = Math.Min(Math.Max(0, Game.MousePosition.y - (gp.y + 1)), Size.y - 2);
                var percent2 = v / (float)(Size.y - 2);
                Value = (int)(Maximum * percent2);
            }

            base.Draw();
        }

        internal void InternalMouseScrollWheel(int delta)
        {
            if (delta < 0)
            {
                Value += Velocity;
                if (Value > Maximum)
                    Value = Maximum;
            }
            else if (delta > 0)
            {
                Value -= Velocity;
                if (Value < 0)
                    Value = 0;
            }
        }

        public override bool MouseScrolled(MouseWheelScrollEventArgs e)
        {
            if (Hover())
            {
                InternalMouseScrollWheel((int)e.Delta);
                return true;
            }

            return base.MouseScrolled(e);
        }
    }
}
