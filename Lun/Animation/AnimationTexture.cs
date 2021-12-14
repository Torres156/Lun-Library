using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lun.Animation
{
    public class AnimationTexture
    {
        /// <summary>
        /// Texturas das frames
        /// </summary>
        public Texture[] Texture = { };

        /// <summary>
        /// Quantia de frames
        /// </summary>
        public int FrameCount = 1;

        /// <summary>
        /// Quantia de Frames na Horizontal
        /// </summary>
        public int FrameHCount = 1;

        /// <summary>
        /// Quantia de Frames na Vertical
        /// </summary>
        public int FrameVCount = 1;

        /// <summary>
        /// Construtor
        /// </summary>
        /// <param name="animData"></param>
        public AnimationTexture(string filePath)
        {
            var animData = new AnimationSpriteSheet(filePath);
            if (animData.Data.Count > 0)
            {
                FrameCount = animData.Data.Count;
                FrameHCount = animData.X;
                FrameVCount = animData.Y;

                Texture = new Texture[FrameCount];
                for (int i = 0; i < FrameCount; i++)
                    Texture[i] = new Texture(new SFML.Graphics.Texture(animData.Data[i].Data)) { Smooth = true };
            }

            GC.SuppressFinalize(animData);
            animData = null;
        }

        public AnimationTexture(byte[] data)
        {
            var animData = new AnimationSpriteSheet(data);
            if (animData.Data.Count > 0)
            {
                FrameCount = animData.Data.Count;
                FrameHCount = animData.X;
                FrameVCount = animData.Y;

                Texture = new Texture[FrameCount];
                for (int i = 0; i < FrameCount; i++)
                    Texture[i] = new Texture(new SFML.Graphics.Texture(animData.Data[i].Data)) { Smooth = true };
            }

            GC.SuppressFinalize(animData);
            animData = null;
        }
    }
}
