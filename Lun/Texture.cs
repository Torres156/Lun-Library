using System;
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
        const int TIMER_UNLOAD = 30000; // 30s

        NativeTexture texture;
        LargeTexture largeTexture;
        internal TextureTypes type = TextureTypes.Normal;
        internal long TimerUnload;

        string FileName = "";
        Vector2 textureSize;
        bool _smooth = false;
        private bool _render = false;

        /// <summary>
        /// Construtor
        /// </summary>
        /// <param name="texture"></param>
        public Texture(NativeTexture texture,bool isRender = false)
        {
            this.texture = texture;
            type = TextureTypes.Normal;
            textureSize = (Vector2)texture.Size;
            _render = isRender;
        }

        /// <summary>
        /// Construtor
        /// </summary>
        /// <param name="name"></param>
        public Texture(string filename, bool large = false, bool cache = true)
        {
            if (!File.Exists(filename))
                throw new Exception($"Arquivo não encontrado!\n{filename}");

            this.FileName = filename;

            if (large)
            {
                type = TextureTypes.Large;
                largeTexture = Loader.LoadLargeTexture(filename);
                textureSize = (Vector2)largeTexture.Size;
            }
            else
            {
                type = TextureTypes.Normal;
                texture = Loader.LoadNativeTexture(filename);
                textureSize = (Vector2)texture.Size;
            }

            if (cache)
                cacheTextures.Add(this);
        }

        /// <summary>
        /// Construtor
        /// </summary>
        /// <param name="data"></param>
        public Texture(byte[] data, bool large = false, bool cache = true)
        {
            if (large)
            {
                type = TextureTypes.Large;
                largeTexture = new LargeTexture(new Image(data));
                textureSize = (Vector2)largeTexture.Size;
            }
            else
            {
                type = TextureTypes.Normal;
                texture = new NativeTexture(data);
                textureSize = (Vector2)texture.Size;
            }

            if (cache)
                cacheTextures.Add(this);
        }

        public NativeTexture GetTexture() => texture;
        public LargeTexture GetLargeTexture() => largeTexture;

        /// <summary>
        /// Tamanho da textura
        /// </summary>
        public Vector2 size => textureSize;

        internal void Load()
        {
            if (!HasLoaded)
                if (type == TextureTypes.Large)
                {
                    largeTexture = Loader.LoadLargeTexture(FileName);
                    largeTexture.Smooth = _smooth;
                    textureSize = (Vector2)largeTexture.Size;
                }
                else
                {
                    texture = Loader.LoadNativeTexture(FileName);
                    texture.Smooth = _smooth;
                    textureSize = (Vector2)texture.Size;
                }

            TimerUnload = Environment.TickCount64 + 30000;
        }

        internal void Unload()
        {
            if (TimerUnload == 0 || Environment.TickCount64 < TimerUnload) return;
            if (type == TextureTypes.Large)
            {
                largeTexture?.Destroy();
                largeTexture = null;
            }
            else
            {
                texture?.Dispose();
                texture = null;
            }
        }

        public bool HasLoaded => type == TextureTypes.Normal ? texture != null : largeTexture != null;

        /// <summary>
        /// Redimensionamento suavel
        /// </summary>
        public bool Smooth
        {
            get
            {
                if (_render) return false;
                if (type == TextureTypes.Normal)
                    return texture?.Smooth ?? false;
                else
                    return largeTexture?.Smooth ?? false;
            }

            set
            {
                if (_render) return;
                if (type == TextureTypes.Normal)
                {
                    _smooth = value;
                    texture?.Smooth = value;
                }
                else
                {
                    _smooth = value;
                    largeTexture?.Smooth = value;
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
            cacheTextures.Remove(this);
        }

    }

    public enum TextureTypes
    {
        Normal,

        Large,
    }
}
