using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lun.Shapes
{
    public class RoundedRectangleShape : SFML.Graphics.Shape
    {
        public Vector2 Size
        {
            get => size;
            set
            {
                if (size != value)
                {
                    size = value;
                    Update();
                }
            }
        }
        Vector2 size;

        public float RadiusTop
        {
            get => radiusTop;
            set
            {
                if (radiusTop != value)
                {
                    radiusTop = value;
                    Update();
                }
            }
        }
        float radiusTop = 0f;

        public float RadiusBottom
        {
            get => radiusBottom;
            set
            {
                if (radiusBottom != value)
                {
                    radiusBottom = value;
                    Update();
                }
            }
        }
        float radiusBottom = 0f;

        public uint cornerPointCount
        {
            get => cornerpoint;
            set
            {
                if (cornerpoint != value)
                {
                    cornerpoint = value;
                    Update();
                }
            }
        }
        uint cornerpoint = 6;

        public RoundedRectangleShape()
        { }

        public RoundedRectangleShape(Vector2 Size, float radius, uint cornerPointCount)
        {            
            this.Size = Size;
            this.radiusTop = radius;
            this.radiusBottom = radius;
            this.cornerPointCount = cornerPointCount;
            Update();
        }

        public override Vector2 GetPoint(uint index)
        {
            if (index >= GetPointCount())
                return new Vector2(0);

            float deltaAngle = 90f / (cornerpoint - 1);
            var center = new Vector2();
            uint centerIndex = index / cornerpoint;
            float radius = 0f;
            switch (centerIndex)
            {
                case 0:
                    center.x = size.x - radiusTop;
                    center.y = radiusTop;
                    radius = radiusTop;
                    break;                    
                case 1:
                    center.x = radiusTop;
                    center.y = radiusTop;
                    radius = radiusTop;
                    break;
                case 2:
                    center.x = radiusBottom;
                    center.y = size.y - radiusBottom;
                    radius = radiusBottom;
                    break;
                case 3:
                    center.x = size.x - radiusBottom;
                    center.y = size.y - radiusBottom;
                    radius = radiusBottom;
                    break;
            }

            return new Vector2(radius * MathF.Cos(deltaAngle * (index - centerIndex) * MathF.PI / 180) + center.x,
                -radius * MathF.Sin(deltaAngle * (index - centerIndex) * MathF.PI / 180) + center.y);
        }

        public override uint GetPointCount()
        {
            return cornerpoint * 4;
        }
    }
}
