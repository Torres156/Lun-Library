using Lun.SFML.Graphics;

namespace Lun
{
    public class Camera2D
    {
        internal View view;

        public float Scale
        {
            get => _scale;
            set
            {
                _scale = value;
                view.Size = Size;
            }
        }
        float _scale = 1f;

        public Vector2 Size
        {
            get => _size * _scale;
            set
            {
                _size = value;
                view.Size = _size * Scale;
            }
        }
        Vector2 _size;

        public Vector2 RealSize
        {
            get => _size;
        }

        public Vector2 Center
        {
            get => (Vector2)view.Center;
            set
            {
                view.Center = value;
            }
        }

        public Camera2D(Vector2 Size)
        {
            view = new View(Size / 2f, Size);
            _size = Size;
        }

        public void Move(Vector2 value)
            => view.Move(value);

        public Point GetMousePosition()
            => (Point)Game.Window.MapPixelToCoords(Game.MousePosition, view);

    }
}
