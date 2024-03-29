﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lun
{
    using SFML.Graphics;
    using System.IO;
    using NativeTexture = SFML.Graphics.Texture;
    public class Texture
    {
        NativeTexture texture;
        LargeTexture largeTexture;
        internal TextureTypes type = TextureTypes.Normal;

        string FileName = "";

        /// <summary>
        /// Construtor
        /// </summary>
        /// <param name="texture"></param>
        public Texture(NativeTexture texture)
        {
            this.texture = texture;
            type = TextureTypes.Normal;
        }

        /// <summary>
        /// Construtor
        /// </summary>
        /// <param name="name"></param>
        public Texture(string filename, bool large = false)
        {
            if (!File.Exists(filename))
                throw new Exception($"Arquivo não encontrado!\n{filename}");

            this.FileName = filename;

            if (large)
            {
                type = TextureTypes.Large;
                largeTexture = Loader.LoadLargeTexture(filename);
            }
            else
            {
                type = TextureTypes.Normal;
                texture = Loader.LoadNativeTexture(filename);
            }

        }

        /// <summary>
        /// Construtor
        /// </summary>
        /// <param name="data"></param>
        public Texture(byte[] data, bool large = false)
        {
            if (large)
            {
                type = TextureTypes.Large;
                largeTexture = new LargeTexture(new Image(data));
            }
            else
            {
                type = TextureTypes.Normal;
                texture = new NativeTexture(data);
            }
        }

        public NativeTexture GetTexture() => texture;
        public LargeTexture GetLargeTexture() => largeTexture;

        /// <summary>
        /// Tamanho da textura
        /// </summary>
        public Vector2 size
        {
            get
            {
                if (type == TextureTypes.Normal)
                    return texture != null ? (Vector2)texture.Size : Vector2.Zero;
                else
                    return largeTexture != null ? (Vector2)largeTexture.Size : Vector2.Zero;
            }
        }

        /// <summary>
        /// Redimensionamento suavel
        /// </summary>
        public bool Smooth
        {
            get
            {

                if (type == TextureTypes.Normal)
                    return texture != null ? texture.Smooth : false;
                else
                    return largeTexture != null ? largeTexture.Smooth : false;
            }

            set
            {
                if (type == TextureTypes.Normal)
                {
                    if (texture != null)
                        texture.Smooth = value;
                }
                else
                {
                    if (largeTexture != null)
                        largeTexture.Smooth = value;
                }
            }
        }

        /// <summary>
        /// Destruir textura
        /// </summary>
        public void Destroy()
        {
            if (type == TextureTypes.Normal)
                texture?.Dispose();
            else
                largeTexture?.Destroy();

            GC.SuppressFinalize(this);
        }

    }

    public enum TextureTypes
    {
        Normal,

        Large,
    }
}
