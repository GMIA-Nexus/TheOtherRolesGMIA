using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using System.IO.Compression;
using Cpp2IL.Core.Extensions;
using System.Collections;
using HarmonyLib;

namespace TheOtherRoles.MetaContext
{
    public interface ITextureLoader : IManageableAsset
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
        public static Texture2D LoadTextureFromResources(string path)
        {
            Texture2D texture = new(2, 2, TextureFormat.ARGB32, false);
            Assembly assembly = Assembly.GetExecutingAssembly();
            Stream stream = assembly.GetManifestResourceStream(path);
            if (stream == null) return null!;
            var byteTexture = new byte[stream.Length];
            stream.Read(byteTexture, 0, (int)stream.Length);
            LoadImage(texture, byteTexture, true);
            return texture;
        }

        public static System.Collections.IEnumerator LoadTextureFromResourcesAsync(string path, Action<Texture2D> onLoad)
        {
            Texture2D texture = new(2, 2, TextureFormat.ARGB32, false);
            Assembly assembly = Assembly.GetExecutingAssembly();
            Stream stream = assembly.GetManifestResourceStream(path);
            if (stream == null) yield break;
            var byteTexture = new byte[stream.Length];
            yield return stream.ReadAsync(byteTexture, 0, (int)stream.Length).WaitAsCoroutine();
            LoadImage(texture, byteTexture, true);
            onLoad.Invoke(texture);
        }

        public static Texture2D LoadTextureFromStream(Stream stream) => LoadTextureFromByteArray(stream.ReadBytes());

        public static Texture2D LoadTextureFromByteArray(byte[] data)
        {
            Texture2D texture = new(2, 2, TextureFormat.ARGB32, true);
            LoadImage(texture, data, true);
            return texture;
        }

        public static Texture2D LoadTextureFromDisk(string path, bool isReadable = false)
        {
            try
            {
                if (File.Exists(path))
                {
                    Texture2D texture = new(2, 2, TextureFormat.ARGB32, true);
                    byte[] byteTexture = File.ReadAllBytes(path);
                    LoadImage(texture, byteTexture, !isReadable);
                    return texture;
                }
            }
            catch
            {
                //System.Console.WriteLine("Error loading texture from disk: " + path);
            }
            return null!;
        }

        public static System.Collections.IEnumerator LoadTextureFromDiskAsync(string path, Action<Texture2D> onLoad)
        {
            if (File.Exists(path))
            {
                Texture2D texture = new(2, 2, TextureFormat.ARGB32, true);
                var task = File.ReadAllBytesAsync(path);
                yield return task.WaitAsCoroutine();
                LoadImage(texture, task.Result, true);
                onLoad?.Invoke(texture);
            }
        }

        public static Texture2D LoadTextureFromZip(ZipArchive zip, string path)
        {
            if (zip == null) return null!;
            try
            {
                var entry = zip.GetEntry(path);
                if (entry != null)
                {
                    Texture2D texture = new(2, 2, TextureFormat.ARGB32, true);
                    Stream stream = entry.Open();
                    byte[] byteTexture = new byte[entry.Length];
                    stream.Read(byteTexture, 0, byteTexture.Length);
                    stream.Close();
                    LoadImage(texture, byteTexture, true);
                    return texture;
                }
            }
            catch
            {
                System.Console.WriteLine("Error loading texture from disk: " + path);
            }
            return null!;
        }

        public static System.Collections.IEnumerator LoadTextureFromZipAsync(ZipArchive zip, string path, Action<Texture2D> onLoad)
        {
            if (zip == null) yield break;

            var entry = zip.GetEntry(path);
            if (entry != null)
            {
                Texture2D texture = new(2, 2, TextureFormat.ARGB32, true);
                Stream stream = entry.Open();
                byte[] byteTexture = new byte[entry.Length];
                yield return stream.ReadAsync(byteTexture, 0, byteTexture.Length).WaitAsCoroutine();
                stream.Close();
                LoadImage(texture, byteTexture, true);
                onLoad.Invoke(texture);
            }
        }

        public static Sprite ToSprite(this Texture2D texture, float pixelsPerUnit) => ToSprite(texture, new Rect(0, 0, texture.width, texture.height), pixelsPerUnit);

        public static Sprite ToSprite(this Texture2D texture, Vector2 pivot, float pixelsPerUnit)
        {
            return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), pivot, pixelsPerUnit);
        }

        public static Sprite ToSprite(this Texture2D texture, Rect rect, float pixelsPerUnit)
        {
            return Sprite.Create(texture, rect, new Vector2(0.5f, 0.5f), pixelsPerUnit);
        }

        public static Sprite ToSprite(this Texture2D texture, Rect rect, Vector2 pivot, float pixelsPerUnit)
        {
            return Sprite.Create(texture, rect, pivot, pixelsPerUnit);
        }

        public static Sprite ToExpandableSprite(this Texture2D texture, float pixelsPerUnit, int x, int y)
        {
            return Sprite.CreateSprite(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), pixelsPerUnit, 0, SpriteMeshType.FullRect, new Vector4(x, y, x, y), false, new(0L));
        }

        public static Sprite ToExpandableSprite(this Texture2D texture, Rect rect, float pixelsPerUnit, int x, int y)
        {
            return Sprite.CreateSprite(texture, rect, new Vector2(0.5f, 0.5f), pixelsPerUnit, 0, SpriteMeshType.FullRect, new Vector4(x, y, x, y), false, new(0L));
        }

        internal delegate bool d_LoadImage(IntPtr tex, IntPtr data, bool markNonReadable);
        internal static d_LoadImage iCall_LoadImage = null!;
        public static bool LoadImage(Texture2D tex, byte[] data, bool markNonReadable)
        {
            if (iCall_LoadImage == null) iCall_LoadImage = IL2CPP.ResolveICall<d_LoadImage>("UnityEngine.ImageConversion::LoadImage");
            var il2cppArray = (Il2CppStructArray<byte>)data;
            return iCall_LoadImage.Invoke(tex.Pointer, il2cppArray.Pointer, markNonReadable);
        }
    }

    internal abstract class AssetBundleResource<T> where T : UnityEngine.Object
    {
        abstract protected AssetBundle AssetBundle { get; }

        private string name;
        private T asset = null;
        public T Asset
        {
            get
            {
                if (!asset) asset = AssetBundle.LoadAsset(name, Il2CppInterop.Runtime.Il2CppType.Of<T>())?.Cast<T>()!;
                return asset;
            }
        }

        public AssetBundleResource(string name)
        {
            this.name = name;
        }
    }

    internal class TORSpriteLoader : AssetBundleResource<Sprite>, Image
    {
        protected override AssetBundle AssetBundle => Modules.AssetLoader.AssetBundle;

        Sprite Image.GetSprite() => Asset;

        public TORSpriteLoader(string name) : base(name) { }

        public void UnloadAsset() { }
        public System.Collections.IEnumerator LoadAsset() { yield break; }
        public void MarkAsUnloadAsset() { }
    }

    internal class TORSpriteLoaderWithDefault : AssetBundleResource<Sprite>, Image
    {
        protected override AssetBundle AssetBundle => Modules.AssetLoader.AssetBundle;
        private Image defaultImage;
        private bool failed = false;
        Sprite Image.GetSprite()
        {
            if (!failed)
            {
                var asset = Asset;
                if (Asset != null) return Asset;
                failed = true;
            }
            return defaultImage?.GetSprite();
        }

        public TORSpriteLoaderWithDefault(string name, Image defaultImage) : base(name)
        {
            this.defaultImage = defaultImage;
        }

        public void UnloadAsset() { }
        public System.Collections.IEnumerator LoadAsset() { yield break; }
        public void MarkAsUnloadAsset() { }
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

        public System.Collections.IEnumerator LoadAsset()
        {
            if (!texture)
            {
                yield return GraphicsHelper.LoadTextureFromResourcesAsync(address, t =>
                {
                    if (!texture)
                    {
                        texture = t;
                    }
                    else
                    {
                        GameObject.Destroy(t);
                    }
                });
            }
        }

        public void UnloadAsset()
        {
            if (texture) GameObject.Destroy(texture);
        }

        public void MarkAsUnloadAsset() { }
    }

    public class DiskTextureLoader : ITextureLoader
    {
        string address;
        Texture2D texture = null!;
        bool isUnloadAsset = false;

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

        public void UnloadAsset()
        {
            if (texture && !isUnloadAsset) GameObject.Destroy(texture);
        }

        public System.Collections.IEnumerator LoadAsset()
        {
            if (!texture)
            {
                yield return GraphicsHelper.LoadTextureFromDiskAsync(address, t =>
                {
                    if (!texture)
                    {
                        texture = t;
                        if (isUnloadAsset) texture.hideFlags |= HideFlags.DontUnloadUnusedAsset | HideFlags.HideAndDontSave;
                    }
                    else
                    {
                        GameObject.Destroy(t);
                    }
                });
            }
        }

        public void MarkAsUnloadAsset() => isUnloadAsset = true;
    }

    public class ZipTextureLoader : ITextureLoader
    {
        ZipArchive archive;
        string address;
        Texture2D texture = null!;
        public bool IsUnloadAsset = true;

        public ZipTextureLoader(ZipArchive zip, string address)
        {
            this.archive = zip;
            this.address = address;
        }

        public Texture2D GetTexture()
        {
            if (!texture)
            {
                texture = GraphicsHelper.LoadTextureFromZip(archive, address);
                if (texture != null && IsUnloadAsset) texture.hideFlags |= HideFlags.DontUnloadUnusedAsset | HideFlags.HideAndDontSave;
                if (texture == null) TheOtherRolesPlugin.Logger.LogError($"Image is not found. (path:{address})");
            }
            return texture!;
        }

        public void UnloadAsset()
        {
            if (texture && !IsUnloadAsset) GameObject.Destroy(texture);
        }

        public System.Collections.IEnumerator LoadAsset()
        {
            yield return GraphicsHelper.LoadTextureFromZipAsync(archive, address, t =>
            {
                if (!texture)
                {
                    texture = t;
                    if (IsUnloadAsset) texture.hideFlags |= HideFlags.DontUnloadUnusedAsset | HideFlags.HideAndDontSave;
                }
                else
                {
                    GameObject.Destroy(t);
                }
            });
        }

        public void MarkAsUnloadAsset() => IsUnloadAsset = true;
    }

    public class StreamTextureLoader : ITextureLoader
    {
        Texture2D texture = null!;
        Func<Stream> stream;
        bool isUnloadTexture = false;
        public StreamTextureLoader(Func<Stream> stream)
        {
            this.stream = stream;
        }

        public Texture2D GetTexture()
        {
            if (!texture)
            {
                var bytes = stream.Invoke()?.ReadBytes();

                if (bytes != null)
                {
                    texture = new Texture2D(2, 2, TextureFormat.ARGB32, true);
                    GraphicsHelper.LoadImage(texture, bytes, true);
                    if (isUnloadTexture) texture.hideFlags |= HideFlags.DontUnloadUnusedAsset | HideFlags.HideAndDontSave;
                }
                else
                {
                    texture = null!;
                }
            }
            return this.texture;
        }

        public void UnloadAsset()
        {
            if (texture && !isUnloadTexture)
            {
                GameObject.Destroy(texture);
                texture = null!;
            }
        }

        public System.Collections.IEnumerator LoadAsset()
        {
            GetTexture();
            yield break;
        }
        public void MarkAsUnloadAsset() => isUnloadTexture = true;
    }

    public class UnloadTextureLoader : ITextureLoader
    {
        Texture2D texture = null!;

        public UnloadTextureLoader(byte[] byteTexture)
        {
            texture = new Texture2D(2, 2, TextureFormat.ARGB32, true);
            texture.MarkDontUnload();
            GraphicsHelper.LoadImage(texture, byteTexture, true);
        }

        public class AsyncLoader
        {
            public UnloadTextureLoader Result { get; private set; } = null;
            Func<Stream> stream;

            public AsyncLoader(Func<Stream> stream)
            {
                this.stream = stream;
            }

            private async Task<byte[]> ReadStreamAsync(Action<Exception> exceptionHandler = null)
            {
                try
                {
                    var myStream = stream.Invoke();
                    if (myStream == null) return [];

                    List<byte> bytes = [];

                    MemoryStream dest = new();
                    await myStream.CopyToAsync(dest);
                    return dest.ToArray();
                }
                catch (Exception ex)
                {
                    exceptionHandler?.Invoke(ex);

                }
                return [];
            }

            public IEnumerator LoadAsync(Action<Exception> exceptionHandler = null)
            {
                if (stream == null) yield break;


                var task = ReadStreamAsync(exceptionHandler);
                while (!task.IsCompleted) yield return new WaitForSeconds(0.15f);

                Result = new UnloadTextureLoader(task.Result);
            }
        }

        public Texture2D GetTexture() => texture;

        public void UnloadAsset() { }
        public System.Collections.IEnumerator LoadAsset() { yield break; }
        public void MarkAsUnloadAsset() { }
    }

    public class DirectTextureLoader : ITextureLoader
    {
        Texture2D texture = null!;

        public DirectTextureLoader(Texture2D texture)
        {
            this.texture = texture;
        }

        public Texture2D GetTexture() => texture;

        public void UnloadAsset()
        {
        }

        public IEnumerator LoadAsset()
        {
            yield break;
        }

        public void MarkAsUnloadAsset()
        {
        }
    }

    public class SpriteLoader : Image
    {
        Sprite sprite = null!;
        float pixelsPerUnit;
        ITextureLoader textureLoader;
        Vector2 Pivot = new(0.5f, 0.5f);
        public SpriteLoader(ITextureLoader textureLoader, float pixelsPerUnit)
        {
            this.textureLoader = textureLoader;
            this.pixelsPerUnit = pixelsPerUnit;
        }

        public Sprite GetSprite()
        {
            if (!sprite) sprite = textureLoader.GetTexture().ToSprite(Pivot, pixelsPerUnit);
            sprite.hideFlags = textureLoader.GetTexture().hideFlags;
            return sprite;
        }

        static public SpriteLoader FromResource(string address, float pixelsPerUnit) => new (new ResourceTextureLoader(address), pixelsPerUnit);

        public SpriteLoader SetPivot(Vector2 pivot)
        {
            this.Pivot = pivot;
            return this;
        }

        public void UnloadAsset()
        {
            if (sprite)
            {
                GameObject.Destroy(sprite);
                sprite = null!;
                textureLoader.UnloadAsset();
            }
        }

        public System.Collections.IEnumerator LoadAsset()
        {
            yield return textureLoader.LoadAsset();
        }

        public void MarkAsUnloadAsset() => textureLoader.MarkAsUnloadAsset();
    }


    public class ResourceExpandableSpriteLoader : Image
    {
        Sprite sprite = null!;
        Texture2D texture = null!;
        string address;
        float pixelsPerUnit;
        //端のピクセル数
        int x, y;
        public ResourceExpandableSpriteLoader(string address, float pixelsPerUnit, int xSidePixels, int ySidePixels)
        {
            this.address = address;
            this.pixelsPerUnit = pixelsPerUnit;
            this.x = xSidePixels;
            this.y = ySidePixels;
        }

        public Sprite GetSprite()
        {
            if (!sprite)
            {
                if (!texture) texture = GraphicsHelper.LoadTextureFromResources(address);
                sprite = texture.ToExpandableSprite(pixelsPerUnit, x, y);
            }
            return sprite;
        }

        public void UnloadAsset()
        {
            if (sprite) GameObject.Destroy(sprite);
            if (texture) GameObject.Destroy(texture);
        }

        public System.Collections.IEnumerator LoadAsset()
        {
            if (!sprite)
            {
                if (!texture)
                {
                    yield return GraphicsHelper.LoadTextureFromResourcesAsync(address, t =>
                    {
                        if (!texture)
                            texture = t;
                        else
                            GameObject.Destroy(t);

                        if (!sprite)
                        {
                            sprite = texture.ToExpandableSprite(pixelsPerUnit, x, y);
                        }
                    });
                }
            }
        }
        public void MarkAsUnloadAsset() { }
    }


    public class DividedSpriteLoader : MultiImage, Image, IDividedSpriteLoader
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

        public DividedSpriteLoader SetPivot(Vector2 pivot)
        {
            Pivot = pivot;
            return this;
        }

        public Sprite GetSprite(int index)
        {
            if (size == null || division == null || sprites == null)
            {
                var texture2D = texture?.GetTexture();

                if (texture2D)
                {
                    if (size == null)
                        size = new(texture2D!.width / division!.Item1, texture2D.height / division!.Item2);
                    else if (division == null)
                        division = new(texture2D!.width / size!.Item1, texture2D.height / size!.Item2);
                    sprites = new Sprite[division!.Item1 * division!.Item2];
                }
                else
                {
                    sprites = null!;
                }
            }

            if (sprites == null) return null!;

            index %= sprites.Length;

            if (!sprites[index])
            {
                var texture2D = texture!.GetTexture();
                int _x = index % division!.Item1;
                int _y = index / division!.Item1;
                sprites[index] = texture2D.ToSprite(new Rect(_x * size.Item1, (division.Item2 - _y - 1) * size.Item2, size.Item1, size.Item2), Pivot, pixelsPerUnit);
            }
            return sprites[index];
        }

        public Sprite GetSprite() => GetSprite(0);

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

        public void UnloadAsset()
        {
            if (sprites != null)
            {
                sprites.Do(s => { if (s) GameObject.Destroy(s); });
                sprites = null!;
                texture.UnloadAsset();
            }
        }

        public System.Collections.IEnumerator LoadAsset()
        {
            yield return texture.LoadAsset();
        }

        public void MarkAsUnloadAsset() => texture.MarkAsUnloadAsset();

    }

    public class XOnlyDividedSpriteLoader : Image, IDividedSpriteLoader
    {
        float pixelsPerUnit;
        Sprite[] sprites;
        ITextureLoader texture;
        int? division, size;
        public Vector2 Pivot = new(0.5f, 0.5f);

        public XOnlyDividedSpriteLoader(ITextureLoader textureLoader, float pixelsPerUnit, int x, bool isSize = false)
        {
            this.pixelsPerUnit = pixelsPerUnit;
            if (isSize)
            {
                this.size = x;
                this.division = null;
            }
            else
            {
                this.division = x;
                this.size = null;
            }
            sprites = null!;
            texture = textureLoader;
        }

        public Sprite GetSprite(int index)
        {
            if (!size.HasValue || !division.HasValue || sprites == null)
            {
                var texture2D = texture.GetTexture();
                if (size == null)
                    size = texture2D.width / division;
                else if (division == null)
                    division = texture2D.width / size!;
                sprites = new Sprite[division!.Value];
            }

            if (!sprites[index])
            {
                var texture2D = texture.GetTexture();
                sprites[index] = texture2D.ToSprite(new Rect(index * size!.Value, 0, size!.Value, texture2D.height), Pivot, pixelsPerUnit);
            }
            return sprites[index];
        }

        public Sprite GetSprite() => GetSprite(0);

        public int Length
        {
            get
            {
                if (!division.HasValue) GetSprite(0);
                return division!.Value;
            }
        }

        public Image WrapLoader(int index) => new WrapSpriteLoader(() => GetSprite(index));

        static public XOnlyDividedSpriteLoader FromResource(string address, float pixelsPerUnit, int x, bool isSize = false)
             => new(new ResourceTextureLoader(address), pixelsPerUnit, x, isSize);
        static public XOnlyDividedSpriteLoader FromDisk(string address, float pixelsPerUnit, int x, bool isSize = false)
             => new(new DiskTextureLoader(address), pixelsPerUnit, x, isSize);

        public void UnloadAsset()
        {
            if (sprites != null)
            {
                sprites.Do(s => { if (s) GameObject.Destroy(s); });
                sprites = null!;
                texture.UnloadAsset();
            }
        }

        public System.Collections.IEnumerator LoadAsset()
        {
            yield return texture.LoadAsset();
        }

        public void MarkAsUnloadAsset() => texture.MarkAsUnloadAsset();
    }

    public class DividedExpandableSpriteLoader : MultiImage, Image, IDividedSpriteLoader
    {
        float pixelsPerUnit;
        Sprite[] sprites;
        ITextureLoader texture;
        Tuple<int, int> division, size;
        private int edgeX, edgeY;

        public DividedExpandableSpriteLoader(ITextureLoader textureLoader, float pixelsPerUnit, int edgeX, int edgeY, int x, int y, bool isSize = false)
        {
            this.pixelsPerUnit = pixelsPerUnit;
            this.edgeX = edgeX;
            this.edgeY = edgeY;
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

            index %= sprites.Length;

            if (!sprites[index])
            {
                var texture2D = texture.GetTexture();
                int _x = index % division!.Item1;
                int _y = index / division!.Item1;
                sprites[index] = texture2D.ToExpandableSprite(new Rect(_x * size.Item1, (division.Item2 - _y - 1) * size.Item2, size.Item1, size.Item2), pixelsPerUnit, edgeX, edgeY);
            }
            return sprites[index];
        }

        public Sprite GetSprite() => GetSprite(0);

        public Image AsLoader(int index) => new WrapSpriteLoader(() => GetSprite(index));

        public int Length
        {
            get
            {
                if (division == null) GetSprite(0);
                return division!.Item1 * division!.Item2;
            }
        }

        static public DividedExpandableSpriteLoader FromResource(string address, float pixelsPerUnit, int edgeX, int edgeY, int x, int y, bool isSize = false)
             => new(new ResourceTextureLoader(address), pixelsPerUnit, edgeX, edgeY, x, y, isSize);
        static public DividedExpandableSpriteLoader FromDisk(string address, float pixelsPerUnit, int edgeX, int edgeY, int x, int y, bool isSize = false)
             => new(new DiskTextureLoader(address), pixelsPerUnit, edgeX, edgeY, x, y, isSize);

        public void UnloadAsset()
        {
            if (sprites != null)
            {
                sprites.Do(s => { if (s) GameObject.Destroy(s); });
                sprites = null!;
                texture.UnloadAsset();
            }
        }

        public System.Collections.IEnumerator LoadAsset()
        {
            yield return texture.LoadAsset();
        }

        public void MarkAsUnloadAsset() => texture.MarkAsUnloadAsset();
    }
}
