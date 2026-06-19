using System;
using System.Linq;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using UnityEngine;

namespace TheOtherRoles
{
    public class SpawnCandidate
    {
#nullable enable
        static private Il2CppArrayBase<UnityEngine.Object>? audioClips = null;

        public Vector2 SpawnLocation;
        public Texture2D? Texture;
        public Sprite[] Sprites;
        public string TextureAddress;
        public StringNames LocationKey;
        public AudioClip? AudioClip;
        public string? AudioClipName;

        public Texture2D GetTexture()
        {
            if (Texture) return Texture!;
            Texture = Helpers.loadTextureFromResources(TextureAddress);
            ReloadSprites();
            return Texture;
        }

        public void ReloadTexture()
        {
            if (!Texture) Texture = Helpers.loadTextureFromResources(TextureAddress);
            ReloadSprites();
        }

        private void ReloadSprites()
        {
            foreach (Sprite sprite in Sprites)
            {
                if (sprite) UnityEngine.Object.Destroy(sprite);
            }
            Sprites = new Sprite[Texture!.width / 200];

            for (int i = 0; i < Sprites.Length; i++)
            {
                Sprites[i] = Helpers.loadSpriteFromResources(Texture, 100f, new Rect((float)(i * 200), 0f, 200f, -200f), new Vector2(0.5f, 1f));
            }
        }

        public Sprite? GetSprite()
        {
            GetTexture();
            if (Sprites.Length > 0) return Sprites[0];
            return null;
        }

        [Obsolete]
        public AudioClip? GetAudioClip()
        {
            if (AudioClipName == null) return null;

            if (audioClips == null) audioClips = UnityEngine.Object.FindObjectsOfTypeAll(AudioClip?.GetIl2CppType());

            if (AudioClip != null) return AudioClip;

            AudioClip = audioClips?.FirstOrDefault<UnityEngine.Object>((audio) => audio && audio.name == AudioClipName)?.TryCast<AudioClip>();
            return AudioClip;
        }

        public Il2CppSystem.Collections.IEnumerator GetEnumerator(SpriteRenderer renderer)
        {
            GetTexture();
            return Effects.Lerp(Sprites.Length * 0.06f, new Action<float>((t) =>
            {
                if (!renderer) return;
                int num = (int)(t * Sprites.Length);
                if (num < Sprites.Length) renderer.sprite = Sprites[num];
            }));
        }

        public SpawnCandidate(StringNames locationKey, Vector2 location, string textureAddress, string? audioClip)
        {
            SpawnLocation = location;
            LocationKey = locationKey;
            TextureAddress = textureAddress;
            Sprites = new Sprite[0];

            AudioClip = null;
            AudioClipName = audioClip;
        }
    }
}
