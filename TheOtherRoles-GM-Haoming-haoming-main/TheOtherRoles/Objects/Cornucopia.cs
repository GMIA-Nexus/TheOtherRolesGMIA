using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TheOtherRoles.Objects
{
    class Cornucopia
    {
        private static Sprite banner;
        private static bool isInGame(string name)
        {
            return PlayerControl.AllPlayerControls.ToArray().Count(player => player.Data.PlayerName == name) != 0;
        }
        private static Sprite getBannerSprite()
        {
            if (banner == null) banner = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.cornucopia.png", 100f);
            return banner;
        }
        public static void showBanner()
        {
            if (isInGame("ナカムラ") || isInGame("リタ") || isInGame("おふろ"))
            {
                GameObject obj = new GameObject("Cornucopia");
                obj.transform.position = new Vector3(-4.14f, -8.12f, 0);
                obj.transform.localScale = new Vector3(0.19f, 0.19f, 1f);
                SpriteRenderer bannerImage = obj.AddComponent<SpriteRenderer>();
                bannerImage.sprite = getBannerSprite();
                obj.SetActive(true);
            }
        }

    }
}
