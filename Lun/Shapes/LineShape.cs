using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lun.Shapes
{
    public class LineShape : SFML.Graphics.Shape
    {
        public LineShape()
        {

        }

        public LineShape(Vector2 startPoint, Vector2 endPoint)
        {
            SetPosition(startPoint, endPoint);
            Thickness = 1;
        }

        public float Thickness
        {
            get => _thickness;
            set
            {
                _thickness = value;
                Update();
            }
        }
        float _thickness = 1f;
        Vector2 direction;

        public override Vector2 GetPoint(uint index)
        {
            var unitDirection = direction / Length;
            Vector2 unitPer = new Vector2(-unitDirection.x, unitDirection.x);
            var offset = unitPer * (_thickness / 2f);


            switch (index)
            {
                default:
                case 0: return offset;
                case 1: return (direction + offset);
                case 2: return (direction - offset);
                case 3: return Vector2.Zero - offset;
            }
        }

        public void SetPosition(Vector2 start, Vector2 end)
        {
            direction = end - start;
            Position = start;
        }

        public override uint GetPointCount()
        {
            return 4;
        }

        public float Length
            => MathF.Sqrt(MathF.Pow(direction.x, 2) + MathF.Pow(direction.y, 2));


    }
}
