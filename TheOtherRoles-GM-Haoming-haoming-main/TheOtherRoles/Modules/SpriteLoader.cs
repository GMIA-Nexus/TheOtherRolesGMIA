using System;
using System.IO;
using System.Reflection;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using UnityEngine;

namespace TheOtherRoles.Modules
{
    public interface Image
    {
        internal Sprite GetSprite();
    }
    public interface ISpriteLoader : Image
    {
    }

    public class WrapSpriteLoader : ISpriteLoader
    {
        Func<Sprite> supplier;

        public WrapSpriteLoader(Func<Sprite> supplier)
        {
            this.supplier = supplier;
        }

        public Sprite GetSprite() => supplier.Invoke();
    }
    public class SpriteLoader : Image
    {
        Sprite sprite = null!;
        float pixelsPerUnit;
        ITextureLoader textureLoader;
        public SpriteLoader(ITextureLoader textureLoader, float pixelsPerUnit)
        {
            this.textureLoader = textureLoader;
            this.pixelsPerUnit = pixelsPerUnit;
        }

        public Sprite GetSprite()
        {
            if (!sprite) sprite = textureLoader.GetTexture().ToSprite(pixelsPerUnit);
            sprite.hideFlags = textureLoader.GetTexture().hideFlags;
            return sprite;
        }

        static public SpriteLoader FromResource(string address, float pixelsPerUnit) => new(new ResourceTextureLoader(address), pixelsPerUnit);
    }

    public interface ITextureLoader
    {
        Texture2D GetTexture();
    }

    public interface IDividedSpriteLoader
    {
        Sprite GetSprite(int index);
        Image AsLoader(int index) => new WrapSpriteLoader(() => GetSprite(index));
        int Length { get; }
    }

    public static class GraphicsHelper
    {
        public static Sprite ToSprite(this Texture2D texture, float pixelsPerUnit) => ToSprite(texture, new Rect(0, 0, texture.width, texture.height), pixelsPerUnit);

        public static Sprite ToSprite(this Texture2D texture, Rect rect, float pixelsPerUnit)
        {
            return Sprite.Create(texture, rect, new Vector2(0.5f, 0.5f), pixelsPerUnit);
        }

        public static Sprite ToSprite(this Texture2D texture, Rect rect, Vector2 pivot, float pixelsPerUnit)
        {
            return Sprite.Create(texture, rect, pivot, pixelsPerUnit);
        }

        public static Texture2D LoadTextureFromDisk(string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    Texture2D texture = new(2, 2, TextureFormat.ARGB32, true);
                    byte[] byteTexture = File.ReadAllBytes(path);
                    LoadImage(texture, byteTexture, false);
                    return texture;
                }
            }
            catch
            {
                //System.Console.WriteLine("Error loading texture from disk: " + path);
            }
            return null!;
        }

        public static Texture2D LoadTextureFromResources(string path)
        {
            Texture2D texture = new(2, 2, TextureFormat.ARGB32, true);
            Assembly assembly = Assembly.GetExecutingAssembly();
            Stream stream = assembly.GetManifestResourceStream(path);
            if (stream == null) return null!;
            var byteTexture = new byte[stream.Length];
            stream.Read(byteTexture, 0, (int)stream.Length);
            LoadImage(texture, byteTexture, false);
            return texture;
        }

        internal delegate bool d_LoadImage(IntPtr tex, IntPtr data, bool markNonReadable);
        internal static d_LoadImage iCall_LoadImage = null!;
        public static bool LoadImage(Texture2D tex, byte[] data, bool markNonReadable)
        {
            if (iCall_LoadImage == null)
                iCall_LoadImage = IL2CPP.ResolveICall<d_LoadImage>("UnityEngine.ImageConversion::LoadImage");
            var il2cppArray = (Il2CppStructArray<byte>)data;
            return iCall_LoadImage.Invoke(tex.Pointer, il2cppArray.Pointer, markNonReadable);
        }
    }

    public class ResourceTextureLoader : ITextureLoader
    {
        string address;
        Texture2D texture = null;

        public ResourceTextureLoader(string address)
        {
            this.address = address;
        }

        public Texture2D GetTexture()
        {
            if (!texture) texture = GraphicsHelper.LoadTextureFromResources(address);
            return texture!;
        }
    }

    public class DiskTextureLoader : ITextureLoader
    {
        string address;
        Texture2D texture = null!;
        bool isUnloadAsset = false;

        public DiskTextureLoader MarkAsUnloadAsset() { isUnloadAsset = true; return this; }
        public DiskTextureLoader(string address)
        {
            this.address = address;
        }

        public Texture2D GetTexture()
        {
            if (!texture)
            {
                texture = GraphicsHelper.LoadTextureFromDisk(address);
                if (isUnloadAsset) texture.hideFlags |= HideFlags.DontUnloadUnusedAsset | HideFlags.HideAndDontSave;
            }
            return texture;
        }
    }


    public class DividedSpriteLoader : Image, IDividedSpriteLoader
    {
        float pixelsPerUnit;
        Sprite[] sprites;
        ITextureLoader texture;
        Tuple<int, int> division, size;
        public Vector2 Pivot = new(0.5f, 0.5f);

        public DividedSpriteLoader(ITextureLoader textureLoader, float pixelsPerUnit, int x, int y, bool isSize = false)
        {
            this.pixelsPerUnit = pixelsPerUnit;
            if (isSize)
            {
                this.size = new(x, y);
                this.division = null;
            }
            else
            {
                this.division = new(x, y);
                this.size = null;
            }
            sprites = null!;
            texture = textureLoader;
        }

        public Sprite GetSprite(int index)
        {
            if (size == null || division == null || sprites == null)
            {
                var texture2D = texture.GetTexture();
                if (size == null)
                    size = new(texture2D.width / division!.Item1, texture2D.height / division!.Item2);
                else if (division == null)
                    division = new(texture2D.width / size!.Item1, texture2D.height / size!.Item2);
                sprites = new Sprite[division!.Item1 * division!.Item2];
            }

            if (!sprites[index])
            {
                var texture2D = texture.GetTexture();
                int _x = index % division!.Item1;
                int _y = index / division!.Item1;
                sprites[index] = texture2D.ToSprite(new Rect(_x * size.Item1, (division.Item2 - _y - 1) * size.Item2, size.Item1, size.Item2), Pivot, pixelsPerUnit);
            }
            return sprites[index];
        }

        public Sprite GetSprite() => GetSprite(0);

        public ISpriteLoader WrapLoader(int index) => new WrapSpriteLoader(() => GetSprite(index));

        public Image AsLoader(int index) => new WrapSpriteLoader(() => GetSprite(index));

        public int Length
        {
            get
            {
                if (division == null) GetSprite(0);
                return division!.Item1 * division!.Item2;
            }
        }

        static public DividedSpriteLoader FromResource(string address, float pixelsPerUnit, int x, int y, bool isSize = false)
             => new(new ResourceTextureLoader(address), pixelsPerUnit, x, y, isSize);
        static public DividedSpriteLoader FromDisk(string address, float pixelsPerUnit, int x, int y, bool isSize = false)
             => new(new DiskTextureLoader(address), pixelsPerUnit, x, y, isSize);
    }
}
