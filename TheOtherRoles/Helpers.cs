using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using AmongUs.Data;
using AmongUs.GameOptions;
using BepInEx.Unity.IL2CPP.Utils;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using HarmonyLib;
using Hazel;
using Reactor.Utilities.Extensions;
using TheOtherRoles.CustomGameModes;
using TheOtherRoles.MetaContext;
using TheOtherRoles.Modules;
using TheOtherRoles.Objects;
using TheOtherRoles.Patches;
using TheOtherRoles.Roles;
using TheOtherRoles.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using static TheOtherRoles.TheOtherRoles;

namespace TheOtherRoles
{

    public enum MurderAttemptResult {
        PerformKill,
        SuppressKill,
        BlankKill,
        ReverseKill,
        DelayVampireKill
    }

    public enum CustomGamemodes {
        Classic,
        Guesser,
        HideNSeek,
        FreePlay
    }

    public static class Direction
    {
        public static Vector2 up = Vector2.up;
        public static Vector2 down = Vector2.down;
        public static Vector2 left = Vector2.left;
        public static Vector2 right = Vector2.right;
        public static Vector2 upleft = new(-0.70710677f, 0.70710677f);
        public static Vector2 upright = new(0.70710677f, 0.70710677f);
        public static Vector2 downleft = new(-0.70710677f, -0.70710677f);
        public static Vector2 downright = new(0.70710677f, -0.70710677f);
    }

    public class StackfullCoroutine
    {
        private List<IEnumerator> stack = new();

        public StackfullCoroutine(IEnumerator enumerator)
        {
            stack.Add(enumerator);
        }

        public bool MoveNext()
        {
            if (stack.Count == 0) return false;

            var current = stack[stack.Count - 1];
            if (!current.MoveNext())
                stack.RemoveAt(stack.Count - 1);
            else if (current.Current != null)
            {
                if (current.Current is IEnumerator child)
                    stack.Add(child);
                else if (current.Current is Il2CppSystem.Collections.IEnumerator il2CppChild)
                    stack.Add(il2CppChild.WrapToManaged());
            }

            return stack.Count > 0;
        }

        public void Wait()
        {
            while (MoveNext()) { }
        }

        //状態を共有している点に注意
        public IEnumerator AsEnumerator()
        {
            while (MoveNext()) yield return null;
        }
    }

    public class ParallelCoroutine
    {
        StackfullCoroutine[] coroutines;
        bool someoneFinished = false;
        public ParallelCoroutine(params StackfullCoroutine[] coroutines)
        {
            this.coroutines = coroutines;
        }

        public bool MoveNext()
        {
            bool allFinished = true;
            foreach (var c in coroutines)
            {
                bool result = c.MoveNext();
                someoneFinished |= !result;
                allFinished &= !result;
            }

            return !allFinished;
        }

        //状態を共有している点に注意
        public IEnumerator AsEnumerator()
        {
            while (MoveNext()) yield return null;
        }

        //ただ待機するだけのコルーチン。処理自体は別の誰かが担う必要がある。
        public IEnumerator JustWaitSomeoneFinished()
        {
            while (!someoneFinished) yield return null;
        }

        public IEnumerator WaitAndProcessTillSomeoneFinished()
        {
            while (!someoneFinished)
            {
                MoveNext();
                yield return null;
            }
        }

        public bool SomeoneFinished => someoneFinished;
    }

    public static class ManagedCoroutineHelper
    {
        static public StackfullCoroutine AsStackfullCoroutine(this IEnumerator enumerator) => new(enumerator);
        static public StackfullCoroutine Continue(Func<bool> func)
        {
            IEnumerator CoWait()
            {
                while (func()) yield return null;
            }
            return CoWait().AsStackfullCoroutine();
        }
    }

    public static class Helpers
    {

        public static string previousEndGameSummary = "";
        public static Dictionary<string, Sprite> CachedSprites = new();

        public static Sprite loadSpriteFromResources(string path, float pixelsPerUnit) {
            try
            {
                if (CachedSprites.TryGetValue(path + pixelsPerUnit, out var sprite)) return sprite;
                Texture2D texture = loadTextureFromResources(path);
                sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), pixelsPerUnit);
                sprite.hideFlags |= HideFlags.HideAndDontSave | HideFlags.DontSaveInEditor;
                return CachedSprites[path + pixelsPerUnit] = sprite;
            } catch {
                System.Console.WriteLine("Error loading sprite from path: " + path);
            }
            return null;
        }

        public static Sprite loadSpriteFromResource(Texture2D texture, float pixelsPerUnit, Rect textureRect, Vector2 pivot)
        {
            return Sprite.Create(texture, textureRect, pivot, pixelsPerUnit);
        }

        public static unsafe Texture2D loadTextureFromResources(string path) {
            try {
                Texture2D texture = new(2, 2, TextureFormat.ARGB32, true);
                Assembly assembly = Assembly.GetExecutingAssembly();
                Stream stream = assembly.GetManifestResourceStream(path);
                var length = stream.Length;
                var byteTexture = new Il2CppStructArray<byte>(length);
                stream.Read(new Span<byte>(IntPtr.Add(byteTexture.Pointer, IntPtr.Size * 4).ToPointer(), (int)length));
                if (path.Contains("HorseHats")) {
                    byteTexture = new Il2CppStructArray<byte>(byteTexture.Reverse().ToArray());
                }
                ImageConversion.LoadImage(texture, byteTexture, false);
                return texture;
            } catch {
                System.Console.WriteLine("Error loading texture from resources: " + path);
            }
            return null;
        }

        public static Texture2D loadTextureFromDisk(string path) {
            try {
                if (File.Exists(path)) {
                    Texture2D texture = new(2, 2, TextureFormat.ARGB32, true);
                    var byteTexture = Il2CppSystem.IO.File.ReadAllBytes(path);
                    ImageConversion.LoadImage(texture, byteTexture, false);
                    return texture;
                }
            } catch {
                TheOtherRolesPlugin.Logger.LogError("Error loading texture from disk: " + path);
            }
            return null;
        }

        /* This function has been removed from TOR because we switched to assetbundles for compressed audio. leaving it here for reference - Gendelo
        public static AudioClip loadAudioClipFromResources(string path, string clipName = "UNNAMED_TOR_AUDIO_CLIP") {
            // must be "raw (headerless) 2-channel signed 32 bit pcm (le)" (can e.g. use Audacity?to export)
            try {
                Assembly assembly = Assembly.GetExecutingAssembly();
                Stream stream = assembly.GetManifestResourceStream(path);
                var byteAudio = new byte[stream.Length];
                _ = stream.Read(byteAudio, 0, (int)stream.Length);
                float[] samples = new float[byteAudio.Length / 4]; // 4 bytes per sample
                int offset;
                for (int i = 0; i < samples.Length; i++) {
                    offset = i * 4;
                    samples[i] = (float)BitConverter.ToInt32(byteAudio, offset) / Int32.MaxValue;
                }
                int channels = 2;
                int sampleRate = 48000;
                AudioClip audioClip = AudioClip.Create(clipName, samples.Length / 2, channels, sampleRate, false);
                audioClip.SetData(samples, 0);
                return audioClip;
            } catch {
                System.Console.WriteLine("Error loading AudioClip from resources: " + path);
            }
            return null;

            // Usage example:
            //AudioClip exampleClip = Helpers.loadAudioClipFromResources("TheOtherRoles.Resources.exampleClip.raw");
            //if (Constants.ShouldPlaySfx()) SoundManager.Instance.PlaySound(exampleClip, false, 0.8f);
            
        }*/
        public static PlayerControl playerById(byte id)
        {
            foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                if (player.PlayerId == id)
                    return player;
            return null;
        }

        public static Dictionary<byte, PlayerControl> allPlayersById()
        {
            Dictionary<byte, PlayerControl> res = new();
            foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                res.Add(player.PlayerId, player);
            return res;
        }

        // Intersteing Color Gradient Feature :)
        public static string GradientColorText(string startColorHex, string endColorHex, string text)
        {
            if (startColorHex.Length != 6 || endColorHex.Length != 6)
            {
                TheOtherRolesPlugin.Logger.LogError("GradientColorText : Invalid Color Hex Code, Hex code should be 6 characters long (without #) (e.g., FFFFFF).");
                return text;
            }

            Color startColor = HexToColor(startColorHex);
            Color endColor = HexToColor(endColorHex);

            int textLength = text.Length;
            float stepR = (endColor.r - startColor.r) / (float)textLength;
            float stepG = (endColor.g - startColor.g) / (float)textLength;
            float stepB = (endColor.b - startColor.b) / (float)textLength;
            float stepA = (endColor.a - startColor.a) / (float)textLength;

            string gradientText = "";

            for (int i = 0; i < textLength; i++)
            {
                float r = startColor.r + (stepR * i);
                float g = startColor.g + (stepG * i);
                float b = startColor.b + (stepB * i);
                float a = startColor.a + (stepA * i);


                string colorhex = ColorToHex(new Color(r, g, b, a));
                gradientText += $"<color=#{colorhex}>{text[i]}</color>";

            }

            return gradientText;

        }

        public static Color HexToColor(string hex)
        {
            Color color = new();
            _ = ColorUtility.TryParseHtmlString("#" + hex, out color);
            return color;
        }

        private static string ColorToHex(Color color)
        {
            Color32 color32 = (Color32)color;
            return $"{color32.r:X2}{color32.g:X2}{color32.b:X2}{color32.a:X2}";
        }

        public static void handleVampireBiteOnBodyReport() {
            // Murder the bitten player and reset bitten (regardless whether the kill was successful or not)
            foreach (var vampire in Vampire.players)
            {
                checkMurderAttemptAndKill(vampire.player, vampire.bitten, true, false);
                Vampire.SetBitten.Invoke((byte.MaxValue, byte.MaxValue, vampire.player.PlayerId));
            }
        }

        public static void handleTrapperTrapOnBodyReport()
        {
            Trapper.MeetingFlag.Invoke();
        }

        public static IEnumerator BlackmailShhh()
        {
            yield return HudManager.Instance.CoFadeFullScreen(Color.clear, new Color(0f, 0f, 0f, 0.98f));
            var TempPosition = HudManager.Instance.shhhEmblem.transform.localPosition;
            var TempDuration = HudManager.Instance.shhhEmblem.HoldDuration;
            HudManager.Instance.shhhEmblem.transform.localPosition = new Vector3(
                HudManager.Instance.shhhEmblem.transform.localPosition.x,
                HudManager.Instance.shhhEmblem.transform.localPosition.y,
                HudManager.Instance.FullScreen.transform.position.z + 1f);
            var jailCell = new GameObject("jailCell");
            if (Jailor.isJailed(PlayerControl.LocalPlayer.PlayerId))
            {
                jailCell.transform.SetParent(HudManager.Instance.shhhEmblem!.transform);
                jailCell.transform.localPosition =
                    new Vector3(0, 0, HudManager.Instance.shhhEmblem.Hand.transform.localPosition.z);
                jailCell.transform.localScale = new Vector3(0.83f, 0.83f, 1f);
                jailCell.gameObject.layer = HudManager.Instance.shhhEmblem!.gameObject.layer;

                var render = jailCell.AddComponent<SpriteRenderer>();
                render.sprite = Jailor.JailCell.GetSprite();
                jailCell.gameObject.SetActive(true);
                jailCell.GetComponent<SpriteRenderer>().enabled = true;
                HudManager.Instance.shhhEmblem.TextImage.text = Blackmailer.players.Any(x => x.player && x.blackmailed == PlayerControl.LocalPlayer)
                ? ModTranslation.getString("jailAndBlackmailText")
                : ModTranslation.getString("jailorJailText");
                HudManager.Instance.shhhEmblem.Hand.gameObject.SetActive(false);
            }
            else
            {
                HudManager.Instance.shhhEmblem.TextImage.text = ModTranslation.getString("blackmailerBlackmailText");
            }
            HudManager.Instance.shhhEmblem.HoldDuration = 2.5f;
            yield return HudManager.Instance.ShowEmblem(true);
            HudManager.Instance.shhhEmblem.transform.localPosition = TempPosition;
            HudManager.Instance.shhhEmblem.HoldDuration = TempDuration;
            yield return HudManager.Instance.CoFadeFullScreen(new Color(0f, 0f, 0f, 0.98f), Color.clear);
            jailCell.Destroy();
            yield return null;
        }

#if WINDOWS
        public static void enableCursor(bool initalSetCursor)
        {
            if (initalSetCursor)
            {
                Sprite sprite = loadSpriteFromResources("TheOtherRoles.Resources.Cursor.png", 115f);
                Cursor.SetCursor(sprite.texture, Vector2.zero, CursorMode.Auto);
                return;
            }
            else
            {
                Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
            }
        }
#endif

        public static void refreshRoleDescription(PlayerControl player) {
            List<RoleInfo> infos = RoleInfo.getRoleInfoForPlayer(player);
            List<string> taskTexts = new(infos.Count);

            foreach (var roleInfo in infos)
            {
                taskTexts.Add(getRoleString(roleInfo));
            }

            var toRemove = new List<PlayerTask>();
            foreach (PlayerTask t in player.myTasks.GetFastEnumerator())
            {
                var textTask = t.TryCast<ImportantTextTask>();
                if (textTask == null) continue;

                var currentText = textTask.Text;

                if (taskTexts.Contains(currentText)) taskTexts.Remove(currentText); // TextTask for this RoleInfo does not have to be added, as it already exists
                else toRemove.Add(t); // TextTask does not have a corresponding RoleInfo and will hence be deleted
            }

            foreach (PlayerTask t in toRemove) {
                t.OnRemove();
                player.myTasks.Remove(t);
                UnityEngine.Object.Destroy(t.gameObject);
            }

            // Add TextTask for remaining RoleInfos
            foreach (string title in taskTexts) {
                var task = new GameObject("RoleTask").AddComponent<ImportantTextTask>();
                task.transform.SetParent(player.transform, false);
                task.Text = title;
                player.myTasks.Insert(0, task);
            }

            if (Madmate.madmate.Any(x => x.PlayerId == player.PlayerId) || CreatedMadmate.createdMadmate.Any(x => x.PlayerId == player.PlayerId))
            {
                var task = new GameObject("RoleTask").AddComponent<ImportantTextTask>();
                task.transform.SetParent(player.transform, false);
                task.Text = cs(Madmate.color, $"{Madmate.fullName}: " + ModTranslation.getString("madmateShortDesc"));
                player.myTasks.Insert(0, task);
            }
        }

        internal static string getRoleString(RoleInfo roleInfo)
        {
            if (roleInfo.roleId == RoleId.Jackal)
            {
                var getSidekickText = Jackal.local.canCreateSidekick ? ModTranslation.getString("jackalWithSidekick") : ModTranslation.getString("jackalShortDesc");
                return cs(roleInfo.color, $"{roleInfo.name}: {getSidekickText}");
            }

            if (roleInfo.roleId == RoleId.Invert)
            {
                return cs(roleInfo.color, $"{roleInfo.name}: {roleInfo.shortDescription} ({Invert.meetings})");
            }

            return cs(roleInfo.color, $"{roleInfo.name}: {roleInfo.shortDescription}");
        }

        public static bool isLovers(this PlayerControl player)
        {
            return player != null && Lovers.isLovers(player);
        }

        public static PlayerControl getPartner(this PlayerControl player)
        {
            return Lovers.getPartner(player);
        }

        public static void ForEach<T>(this IList<T> self, Action<T> todo)
        {
            for (int i = 0; i < self.Count; i++)
            {
                todo(self[i]);
            }
        }

        public static void SetMissing(this VitalsPanel panel)
        {
            panel.IsDead = true;
            panel.IsDiscon = false;
            panel.Background.sprite = VitalsStatePatch.MissBackground.GetSprite();
            panel.Cardio.gameObject.SetActive(false);
        }

        public static bool isRole(this PlayerControl player, RoleId role)
        {
            foreach (var t in RoleData.allRoleIds)
            {
                if (role == t.Key)
                {
                    return (bool)t.Value.GetMethod("isRole", BindingFlags.Public | BindingFlags.Static)?.Invoke(null, [player]);
                }
            }
            return false;
        }

        public static void setRole(this PlayerControl player, RoleId role)
        {
            foreach (var t in RoleData.allRoleIds)
            {
                if (role == t.Key)
                {
                    t.Value.GetMethod("setRole", BindingFlags.Public | BindingFlags.Static)?.Invoke(null, [player]);
                    return;
                }
            }
        }

        public static void eraseAllRoles(this PlayerControl player)
        {
            foreach (var t in RoleData.allRoleIds)
            {
                t.Value.GetMethod("eraseRole", BindingFlags.Public | BindingFlags.Static)?.Invoke(null, [player]);
            }
        }

        public static void swapRoles(this PlayerControl player, PlayerControl target)
        {
            foreach (var t in RoleData.allRoleIds)
            {
                if (player.isRole(t.Key))
                {
                    t.Value.GetMethod("swapRole", BindingFlags.Public | BindingFlags.Static)?.Invoke(null, [player, target]);
                }
            }
        }

        public static bool isLighterColor(int colorId) {
            return CustomColors.lighterColors.Contains(colorId);
        }

        public static bool isCustomServer() {
            if (FastDestroyableSingleton<ServerManager>.Instance == null) return false;
            StringNames n = FastDestroyableSingleton<ServerManager>.Instance.CurrentRegion.TranslateName;
            return n is not StringNames.ServerNA and not StringNames.ServerEU and not StringNames.ServerAS;
        }

        public static SpriteLoader ConvertSpriteToSpriteLoader(Sprite sprite)
        {
            if (sprite == null)
                throw new ArgumentNullException(nameof(sprite));

            Texture2D texture = sprite.texture;
            var textureLoader = new DirectTextureLoader(texture);
            var spriteLoader = new SpriteLoader(textureLoader, sprite.pixelsPerUnit);
            spriteLoader.SetPivot(sprite.pivot / sprite.rect.size);

            return spriteLoader;
        }

        public static string Bold(this string original)
        {
            return "<b>" + original + "</b>";
        }

        public static bool IsPiled(this PassiveUiElement uiElem)
        {
            var currentOver = PassiveButtonManager.Instance.currentOver;
            if (!currentOver || !uiElem) return false;
            return currentOver.GetInstanceID() == uiElem.GetInstanceID();
        }

        public static int[] MinPlayers = [
            4, 4, 7, 9, 13, 15, 18
            ];

        public static int[] RecommendedKillCooldown = [
            0,
            0, 0, 0, 45, 30,
            15, 35, 30, 25, 20,
            20, 20, 20, 20, 20,
            20, 20, 20, 20, 20,
            20, 20, 20, 20
            ];

        public static int[] RecommendedImpostors = [
            0,
            0, 0, 0, 1, 1,
            1, 2, 2, 2, 2,
            2, 3, 3, 3, 3,
            3, 4, 4, 4, 4,
            5, 5, 5, 5
            ];

        public static int[] MaxImpostors = [
            0,
            0, 0, 0, 1, 1,
            1, 2, 2, 3, 3,
            3, 3, 4, 4, 5,
            5, 5, 6, 6, 6,
            6, 6, 6, 6
            ];

        static public int[] Sequential(int length)
        {
            var array = new int[length];
            for (int i = 0; i < length; i++) array[i] = i;
            return array;
        }

        public static Vector3 AsWorldPos(this Vector3 vec, bool isBack)
        {
            Vector3 result = vec;
            result.z = result.y / 1000f;
            if (isBack) result.z += 0.001f;
            return result;
        }

        static public IEnumerator Smooth(this Transform transform, Vector3 goalLocalPosition, float duration)
        {
            float p = 0f;
            var origin = transform.localPosition;
            while (p < 1f)
            {
                p += Time.deltaTime / duration;

                float pp = (1f - p) * (1f - p);
                pp *= pp * pp;
                transform.localPosition = origin * pp + goalLocalPosition * (1f - pp);
                yield return null;
            }

            transform.localPosition = goalLocalPosition;
        }

        private static readonly Image LightMask = SpriteLoader.FromResource("TheOtherRoles.Resources.LighterLightMask.png", 100f);
        public static SpriteRenderer CreateCustomLight(Vector2 pos, float range, bool enabled = true, Sprite maskSprite = null)
        {
            var trueRange = range;
            var light = new GameObject("Light");
            light.transform.position = (Vector3)pos + new Vector3(0f, 0f, -50f);
            light.transform.localScale = new(trueRange, trueRange, 1f);
            light.layer = LayerMask.NameToLayer("Shadow");

            var lightRenderer = light.AddComponent<SpriteRenderer>();
            lightRenderer.sprite = maskSprite ?? LightMask.GetSprite();
            lightRenderer.material.shader = PlayerControl.LocalPlayer.LightPrefab.LightCutawayMaterial.shader;
            lightRenderer.enabled = enabled;
            return lightRenderer;
        }

        public static Vector3 convertPos(int index, int arrangeType, (int x, int y)[] arrangement, Vector3 origin, Vector3[] originOffset, Vector3 contentsOffset, float[] scale, (float x, float y)[] contentAreaMultiplier)
        {
            int num1 = index % arrangement[arrangeType].x;
            int num2 = index / arrangement[arrangeType].x;
            return origin + originOffset[arrangeType] + new Vector3(contentsOffset.x * scale[arrangeType] * contentAreaMultiplier[arrangeType].x * num1, contentsOffset.y * scale[arrangeType] * contentAreaMultiplier[arrangeType].y * num2, (float)(-(double)num2 * 0.0099999997764825821));
        }

        public static int GetDisplayType(int players)
        {
            if (players <= 15)
                return 0;
            return players > 18 ? 2 : 1;
        }

        public static bool hasFakeTasks(this PlayerControl player) {
            return player.isRole(RoleId.Jester) || player.isRole(RoleId.Jackal) || player.isRole(RoleId.Sidekick) || player.isRole(RoleId.Arsonist) || player.isRole(RoleId.Vulture) || player.isRole(RoleId.Opportunist) || player.isRole(RoleId.Moriarty)
                || (Madmate.madmate.Any(x => x.PlayerId == player.PlayerId) && !Madmate.hasTasks) ||
                (CreatedMadmate.createdMadmate.Any(x => x.PlayerId == player.PlayerId) && !CreatedMadmate.hasTasks) || player.isRole(RoleId.Akujo) || player.isRole(RoleId.Kataomoi) || player.isRole(RoleId.PlagueDoctor) || player.isRole(RoleId.Cupid) || (player.isRole(RoleId.SchrodingersCat) && !SchrodingersCat.hideRole)
                || player.isRole(RoleId.Immoralist) || player.isRole(RoleId.Doomsayer) || player.isRole(RoleId.Pelican) || player.isRole(RoleId.Yandere);
        }

        public static bool canBeErased(this PlayerControl player) {
            return !player.isRole(RoleId.Jackal) && !player.isRole(RoleId.Sidekick);
        }

        public static bool shouldShowGhostInfo() {
            return (PlayerControl.LocalPlayer != null && PlayerControl.LocalPlayer.Data.IsDead && (RoleManager.IsGhostRole(PlayerControl.LocalPlayer.Data.RoleType) || FreePlayGM.isFreePlayGM)
                && ClientOption.GetValue(ClientOption.ClientOptionType.SpoilerAfterDeath) == 1) || AmongUsClient.Instance.GameState == InnerNet.InnerNetClient.GameStates.Ended;
        }

        public static bool shouldClearTask(this PlayerControl target) => (target.hasFakeTasks() || target.isRole(RoleId.Thief) || (target.isRole(RoleId.Shifter) && Shifter.isNeutral) || (target.isRole(RoleId.TaskMaster) && target.PlayerId == PlayerControl.LocalPlayer.PlayerId && TaskMaster.isTaskComplete)
                || Madmate.madmate.Any(x => x.PlayerId == target.PlayerId) || CreatedMadmate.createdMadmate.Any(x => x.PlayerId == target.PlayerId) || target.isRole(RoleId.JekyllAndHyde) || target.isRole(RoleId.Fox)) && !FreePlayGM.isFreePlayGM;

        public static void clearAllTasks(this PlayerControl player) {
            if (player == null) return;
            if (TimeMaster.hasAlivePlayers && TimeMaster.reviveDuringReweind && AssignRolePatch.blockAssignRole) return;
            foreach (var playerTask in player.myTasks.GetFastEnumerator())
            {
                playerTask.OnRemove();
                UnityEngine.Object.Destroy(playerTask.gameObject);
            }
            player.myTasks.Clear();

            if (player.Data != null && player.Data.Tasks != null)
                player.Data.Tasks.Clear();
        }

        public static bool isMira()
        {
            return GameOptionsManager.Instance.CurrentGameOptions.MapId == 1;
        }

        public static bool isAirship()
        {
            return GameOptionsManager.Instance.CurrentGameOptions.MapId == 4;
        }
        public static bool isSkeld()
        {
            return GameOptionsManager.Instance.CurrentGameOptions.MapId == 0;
        }
        public static bool isPolus()
        {
            return GameOptionsManager.Instance.CurrentGameOptions.MapId == 2;
        }

        public static bool isFungle()
        {
            return GameOptionsManager.Instance.CurrentGameOptions.MapId == 5;
        }

        public static bool MushroomSabotageActive()
        {
            return PlayerControl.LocalPlayer.myTasks.ToArray().Any((x) => x.TaskType == TaskTypes.MushroomMixupSabotage);
        }

        public static bool ShowButtons
        {
            get
            {
                return !(MapBehaviour.Instance && MapBehaviour.Instance.IsOpen) &&
                !MeetingHud.Instance &&
                !ExileController.Instance;
            }
        }

        public static bool ShowKillAnimation
        {
            get
            {
                return MeetingHud.Instance?.state is not
                    MeetingHud.VoteStates.Animating and not MeetingHud.VoteStates.Results
                    and not MeetingHud.VoteStates.Proceeding;
            }
        }

        static public IDividedSpriteLoader NextButtonSprite = XOnlyDividedSpriteLoader.FromResource("TheOtherRoles.Resources.HatsNextButton.png", 220f, 2);

        public static List<string> removeLineFeed(string line)
        {
            var lines = line.Split("\n", StringSplitOptions.None);
            return lines.ToList();
        }

        static public TitleShower GetTitleShower(this PlayerControl player)
        {
            if (player.TryGetComponent<TitleShower>(out var result))
                return result;
            else
            {
                return player?.gameObject.AddComponent<TitleShower>();
            }
        }

        public static string HeadLower(this string text) => char.ToLower(text[0]) + text[1..];
        public static string HeadUpper(this string text) => char.ToUpper(text[0]) + text[1..];

        public static IEnumerator CoGush(this PlayerControl player)
        {

            player.MyPhysics.body.velocity = Vector2.zero;
            if (player.AmOwner) player.MyPhysics.inputHandler.enabled = true;
            player.moveable = false;
            player.MyPhysics.myPlayer.Visible = true;
            player.cosmetics.AnimateSkinExitVent();

            yield return player.MyPhysics.Animations.CoPlayExitVentAnimation();

            player.cosmetics.AnimateSkinIdle();
            player.MyPhysics.Animations.PlayIdleAnimation();
            player.moveable = true;
            player.currentRoleAnimations.ForEach((Action<RoleEffectAnimation>)((an) => an.ToggleRenderer(true)));
            if (player.AmOwner) player.MyPhysics.inputHandler.enabled = false;
        }

        public static void SetRawOverlay(this PassiveButton button, string rawText)
        {
            button.OnMouseOver.AddListener((Action)(() => TORGUIManager.Instance.SetHelpContext(button, rawText)));
            button.OnMouseOut.AddListener((Action)(() => TORGUIManager.Instance.HideHelpContextIf(button)));
        }

        private static List<SupportedLangs> _languagesToBold =
        [
            SupportedLangs.Russian,
            SupportedLangs.Japanese,
            SupportedLangs.SChinese,
            SupportedLangs.TChinese,
            SupportedLangs.Korean
        ];

        public static void AdjustNotification(this LobbyNotificationMessage notification)
        {
            if (!_languagesToBold.Contains(DataManager.Settings.Language.CurrentLanguage))
            {
                notification.Text.fontStyle = FontStyles.Bold;
                notification.Text.SetOutlineThickness(0.35f);
            }
        }

        public static GUIContext GetProgressContext(PlayerControl player)
        {
            if (player == null) return null;
            var role = Role.allRoles.FirstOrDefault(r => r.player == player);
            var roleInfo = RoleInfo.getRoleInfoForPlayer(player, false, true).FirstOrDefault();
            string stateText = RoleInfo.getStatesString(player, false).Replace(" - ", "");
            try { stateText = stateText.HeadUpper(); } catch { }
            if (!player.Data.IsDead) stateText = ModTranslation.getString("roleSummaryAlive");
            GUIContext stateContext = stateText != null ? TORGUIContextEngine.API.RawText(GUIAlignment.Left,
                TORGUIContextEngine.API.GetAttribute(AttributeAsset.DocumentBold), player.Data.PlayerName + ": " + stateText) : null;
            GUIContext[] progContext = (role?.ProgressContext != null) ? [TORGUIContextEngine.API.VerticalHolder(GUIAlignment.Left, TORGUIContextEngine.API.RawText(GUIAlignment.Left,
                    TORGUIContextEngine.API.GetAttribute(AttributeAsset.OverlayTitle), cs(roleInfo.color, roleInfo.name)), role.ProgressContext.Move(new(0.14f, 0f)))] : [];
            var context = stateContext != null ? [.. progContext.Prepend(stateContext)] : progContext;
            return context.Length == 0 ? null : TORGUIContextEngine.API.VerticalHolder(GUIAlignment.Left, context);
        }

        public static GUIContext Move(this GUIContext inner, Vector2 diff)
        {
            if (inner == null) return null;
            if (diff.x > 0f)
                inner = TORGUIContextEngine.API.HorizontalHolder(inner.Alignment, TORGUIContextEngine.API.HorizontalMargin(diff.x), inner);
            if (diff.x < 0f)
                inner = TORGUIContextEngine.API.HorizontalHolder(inner.Alignment, inner, TORGUIContextEngine.API.HorizontalMargin(-diff.x));

            if (diff.y > 0f)
                inner = TORGUIContextEngine.API.VerticalHolder(inner.Alignment, TORGUIContextEngine.API.VerticalMargin(diff.y), inner);
            if (diff.y < 0f)
                inner = TORGUIContextEngine.API.VerticalHolder(inner.Alignment, inner, TORGUIContextEngine.API.HorizontalMargin(-diff.y));

            return inner;
        }

        public static void SetOverlay(this PassiveButton button, Func<GUIContext> widget)
        {
            var hover = button.GetComponent<TouchHover>();
            if (hover == null) return;
            hover.OnHoverOver.AddListener((Action)(() =>
            {
                var val = widget.Invoke();
                if (val != null)
                {
                    TORGUIManager.Instance.SetHelpContext(button, val);
                }
            }));
            hover.OnHoverOut.AddListener((Action)(() => TORGUIManager.Instance.HideHelpContextIf(button)));
        }

        public static void generateNormalTasks(this PlayerControl player)
        {
            if (player == null) return;
            var tasks = new Il2CppSystem.Collections.Generic.List<byte>();
            var hashSet = new Il2CppSystem.Collections.Generic.HashSet<TaskTypes>();

            List<byte> taskTypeIds = new();
            var commonTasks = new Il2CppSystem.Collections.Generic.List<NormalPlayerTask>();
            foreach (var task in ShipStatus.Instance.CommonTasks) {
                if (ShipStatusPatch.commonTasks.Any(x => x == task.TaskType)) {
                    commonTasks.Add(task);
                    TheOtherRolesPlugin.Logger.LogMessage($"Generated {task.TaskType.ToString()} for player {player.Data.PlayerName}");
                }
            }

            int start = 0;
            MapUtilities.CachedShipStatus.AddTasksFromList(ref start, ShipStatusPatch.commonTasks.Count, tasks, hashSet, commonTasks);

            taskTypeIds.AddRange(tasks.ToArray().ToList());
            var option = GameOptionsManager.Instance.currentNormalGameOptions;
            taskTypeIds.AddRange(generateTasks(0, option.NumShortTasks, option.NumLongTasks));

            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.UncheckedSetTasks, Hazel.SendOption.Reliable, -1);
            writer.Write(player.PlayerId);
            writer.WriteBytesAndSize(taskTypeIds.ToArray());
            AmongUsClient.Instance.FinishRpcImmediately(writer);
            RPCProcedure.uncheckedSetTasks(player.PlayerId, taskTypeIds.ToArray());
        }

        public static void HandleRoleFlashOnDeath(PlayerControl target)
        {
            if (Noisemaker.players.Any(x => x.player && x.target == target))
            {
                if ((Noisemaker.soundTarget == Noisemaker.SoundTarget.Noisemaker && PlayerControl.LocalPlayer.isRole(RoleId.Noisemaker)) ||
                    (Noisemaker.soundTarget == Noisemaker.SoundTarget.Crewmates && !isNeutral(PlayerControl.LocalPlayer) && !PlayerControl.LocalPlayer.Data.Role.IsImpostor) ||
                    (Noisemaker.soundTarget == Noisemaker.SoundTarget.Everyone))
                { InstantiateNoisemakerArrow(target.transform.localPosition, true).arrow.SetDuration(Noisemaker.duration); }
                if (Noisemaker.players.Any(x => x.player == PlayerControl.LocalPlayer && x.target == target))
                {
                    _ = new StaticAchievementToken("noisemaker.common1");
                    _ = new StaticAchievementToken("noisemaker.challenge");
                    Noisemaker.local.target = null;
                }
            }
            if (PlayerControl.LocalPlayer.isRole(RoleId.Immoralist) && !PlayerControl.LocalPlayer.Data.IsDead)
                showFlash(new Color(42f / 255f, 187f / 255f, 245f / 255f));

            // Seer show flash and add dead player position
            if (Seer.exists && ((PlayerControl.LocalPlayer.isRole(RoleId.Seer) && PlayerControl.LocalPlayer != target) || shouldShowGhostInfo()) && Seer.livingPlayers.Count > 0 && Seer.mode <= 1)
            {
                showFlash(new Color(42f / 255f, 187f / 255f, 245f / 255f), message: ModTranslation.getString("seerInfo"), textColor: Color.white, textSprite: RoleHelpers.GetRoleIcon(RoleId.Seer).GetSprite());
                if (PlayerControl.LocalPlayer.isRole(RoleId.Seer))
                {
                    _ = new StaticAchievementToken("seer.common1");
                    Seer.local.acTokenChallenge.Value++;
                }
            }
            if (Seer.deadBodyPositions != null) Seer.deadBodyPositions.Add(target.transform.position);

            // Tracker store body positions
            if (Tracker.deadBodyPositions != null) Tracker.deadBodyPositions.Add(target.transform.position);

            // VIP Modifier
            if (Vip.vip.FindAll(x => x.PlayerId == target.PlayerId).Count > 0)
            {
                Color color = Color.yellow;
                if (Vip.showColor)
                {
                    color = Color.white;
                    if (target.Data.Role.IsImpostor) color = Color.red;
                    else if (isNeutral(target)) color = Color.blue;
                }
                showFlash(color, 1.5f);
            }
        }

        public static Camera FindCamera(int cameraLayer) => Camera.allCameras.FirstOrDefault(c => (c.cullingMask & (1 << cameraLayer)) != 0);

        public static Vector3 WorldToScreenPoint(Vector3 worldPos, int cameraLayer)
        {
            return FindCamera(cameraLayer)?.WorldToScreenPoint(worldPos) ?? Vector3.zero;
        }

        public static Vector3 ScreenToWorldPoint(Vector3 screenPos, int cameraLayer)
        {
            return FindCamera(cameraLayer)?.ScreenToWorldPoint(screenPos) ?? Vector3.zero;
        }

#if WINDOWS
        public static string GetClipboardString()
        {
            uint type = 0;
            if (ClipboardHelper.IsClipboardFormatAvailable(1U)) { type = 1U; Debug.Log("ASCII"); }
            if (ClipboardHelper.IsClipboardFormatAvailable(13U)) { type = 13U; Debug.Log("UNICODE"); }
            if (type == 0) return "";

            string result;
            try
            {
                if (!ClipboardHelper.OpenClipboard(IntPtr.Zero))
                {
                    result = "";
                }
                else
                {

                    IntPtr clipboardData = ClipboardHelper.GetClipboardData(type);
                    if (clipboardData == IntPtr.Zero)
                        result = "";
                    else
                    {
                        IntPtr intPtr = IntPtr.Zero;
                        try
                        {
                            intPtr = ClipboardHelper.GlobalLock(clipboardData);
                            int len = ClipboardHelper.GlobalSize(clipboardData);

                            if (type == 1U)
                                result = Marshal.PtrToStringAnsi(clipboardData, len);
                            else
                            {
                                result = Marshal.PtrToStringUni(clipboardData) ?? "";
                            }
                        }
                        finally
                        {
                            if (intPtr != IntPtr.Zero) ClipboardHelper.GlobalUnlock(intPtr);
                        }
                    }
                }
            }
            finally
            {
                ClipboardHelper.CloseClipboard();
            }
            return result;
        }
#endif

        static public SpriteRenderer CreateSharpBackground(Vector2 size, Color color, Transform transform)
        {
            var renderer = CreateObject<SpriteRenderer>("Background", transform, new Vector3(0, 0, 0.25f));
            return CreateSharpBackground(renderer, color, size);
        }

        static public ResourceExpandableSpriteLoader SharpWindowBackgroundSprite = new("TheOtherRoles.Resources.StatisticsBackground.png", 100f, 5, 5);

        static public SpriteRenderer CreateSharpBackground(SpriteRenderer renderer, Color color, Vector2 size)
        {
            renderer.sprite = SharpWindowBackgroundSprite.GetSprite();
            renderer.drawMode = SpriteDrawMode.Sliced;
            renderer.tileMode = SpriteTileMode.Continuous;
            renderer.color = color;
            renderer.size = size;
            return renderer;
        }

        static public float GetKillCooldown(this PlayerControl player)
        {
            if (player.isRole(RoleId.SerialKiller)) return SerialKiller.killCooldown;
            if (player.isRole(RoleId.SchrodingersCat)) return SchrodingersCat.killCooldown;
            return GameOptionsManager.Instance.currentNormalGameOptions.KillCooldown;
        }

        static public IEnumerator Sequence(params IEnumerator[] enumerator)
        {
            foreach (var e in enumerator) yield return e;
        }

        public static int ComputeConstantHash(this string str)
        {
            const long MulPrime = 467;
            const int SurPrime = 9670057;

            long val = 0;
            foreach (char c in str)
            {
                val *= MulPrime;
                val += c;
                val %= SurPrime;
            }
            return (int)(val % SurPrime);
        }

        public static void SetTargetWithLight(this FollowerCamera camera, MonoBehaviour target)
        {
            camera.Target = target;
            PlayerControl.LocalPlayer.lightSource?.transform?.SetParent(target.transform, false);
            if (target != PlayerControl.LocalPlayer) PlayerControl.LocalPlayer.NetTransform.Halt();
        }

        static public bool Find<T>(this IEnumerable<T> enumerable, Func<T, bool> predicate, [MaybeNullWhen(false)] out T found)
        {
            found = enumerable.FirstOrDefault(predicate);
            return found != null;
        }

        public static string ComputeConstantHashAsString(this string str)
        {
            var val = str.ComputeConstantHash();
            StringBuilder builder = new();
            while (val > 0)
            {
                builder.Append((char)('a' + (val % 26)));
                val /= 26;
            }
            return builder.ToString();
        }

        public static void ModShow(this HideAndSeekDeathPopup __instance, PlayerControl player, int deathIndex)
        {
            __instance.nameplate.SetPlayer(player);
            Vector3 localPosition = __instance.transform.localPosition;
            localPosition.z -= deathIndex;
            __instance.transform.localPosition = localPosition;
            __instance.StartCoroutine(AnimateCoroutine(__instance));
        }

        static public IEnumerator WaitAsCoroutine(this Task task)
        {
            while (!task.IsCompleted) yield return null;
            yield break;
        }

        static public IEnumerable<T> Delimit<T>(this IEnumerable<T> enumerable, T delimiter)
        {
            bool isFirst = true;
            foreach (T item in enumerable)
            {
                if (!isFirst) yield return delimiter;
                yield return item;
                isFirst = false;
            }
        }

        private static IEnumerator AnimateCoroutine(HideAndSeekDeathPopup __instance)
        {
            HideAndSeekDeathPopup andSeekDeathPopup = __instance;
            DateTime startTime = DateTime.UtcNow;
            bool doNeedReduceSpeed = true;
            bool doNeedIncrease = true;
            while (true)
            {
                AnimatorStateInfo animatorStateInfo = andSeekDeathPopup.animator.GetCurrentAnimatorStateInfo(0);
                if (animatorStateInfo.IsName("Show"))
                {
                    if (doNeedReduceSpeed && DateTime.UtcNow.Subtract(startTime).TotalSeconds >= 0.5)
                    {
                        __instance.animator.speed *= 0.15f;
                        doNeedReduceSpeed = false;
                    }
                    if (doNeedIncrease && DateTime.UtcNow.Subtract(startTime).TotalSeconds >= 6.8f)
                    {
                        __instance.animator.speed *= 6.66f;
                        doNeedIncrease = false;
                    }
                    yield return null;
                }
                else
                    break;
            }
            UnityEngine.Object.Destroy(andSeekDeathPopup.gameObject);
        }

        static public IEnumerator Action(Action action)
        {
            action.Invoke();
            yield break;
        }

        public static PassiveButton SetUpButton(this GameObject gameObject, bool withSound = false, SpriteRenderer buttonRenderer = null, Color? defaultColor = null, Color? selectedColor = null)
        {
            var button = gameObject.AddComponent<PassiveButton>();
            button.OnClick = new UnityEngine.UI.Button.ButtonClickedEvent();
            button.OnMouseOut = new UnityEngine.Events.UnityEvent();
            button.OnMouseOver = new UnityEngine.Events.UnityEvent();

            if (withSound)
            {
                button.OnClick.AddListener((Action)(() => VanillaAsset.PlaySelectSE()));
                button.OnMouseOver.AddListener((Action)(() => VanillaAsset.PlayHoverSE()));
            }
            if (buttonRenderer != null)
            {
                button.OnMouseOut.AddListener((Action)(() => buttonRenderer!.color = defaultColor ?? Color.white));
                button.OnMouseOver.AddListener((Action)(() => buttonRenderer!.color = selectedColor ?? Color.green));
            }

            if (buttonRenderer != null) buttonRenderer.color = defaultColor ?? Color.white;

            return button;
        }

        static public bool AnyNonTriggersBetween(Vector2 pos1, Vector2 pos2, out Vector2 vector, int? layerMask = null)
        {
            layerMask ??= Constants.ShipAndAllObjectsMask;
            vector = pos2 - pos1;
            return PhysicsHelpers.AnyNonTriggersBetween(pos1, vector.normalized, vector.magnitude, layerMask!.Value);
        }

        public static bool destroyGameObjects(GameObject root, DestroyInfo[] excludeInfos = null, GameObject obj = null)
        {
            if (obj == null)
                obj = root;

            DestroyInfo findInfo = null;
            if (excludeInfos != null)
                findInfo = Array.Find(excludeInfos, (info) => info.searchName == obj.name);
            if (obj != root && findInfo == null)
            {
                UnityEngine.Object.DestroyImmediate(obj);
                return true;
            }

            if (findInfo == null || findInfo.isDestroyChild)
            {
                for (int i = 0; i < obj.transform.GetChildCount(); ++i)
                {
                    if (destroyGameObjects(root, excludeInfos, obj.transform.GetChild(i).gameObject))
                        --i;
                }
            }
            return false;
        }

        public static Shader achievementMaterialShader;
        public static Shader achievementProgressShader;

        static public bool IsEmpty<T>(this IEnumerable<T> enumerable) => !enumerable.Any(_ => true);

        public static int CurrentMonth => DateTime.Now.Month;

        public class DestroyInfo
        {
            public DestroyInfo(string searchName, bool isFindChild = true)
            {
                this.searchName = searchName;
                this.isDestroyChild = isFindChild;
            }

            public string searchName = "";
            public bool isDestroyChild = true;
        }

        public static bool hideGameObjects(GameObject root, DestroyInfo[] excludeInfos = null, GameObject obj = null)
        {
            if (obj == null)
                obj = root;

            DestroyInfo findInfo = null;
            if (excludeInfos != null)
                findInfo = Array.Find(excludeInfos, (info) => info.searchName == obj.name);
            if (obj != root && findInfo == null)
            {
                obj.SetActive(false);
                return true;
            }

            if (findInfo == null || findInfo.isDestroyChild)
            {
                for (int i = 0; i < obj.transform.GetChildCount(); ++i)
                {
                    if (destroyGameObjects(root, excludeInfos, obj.transform.GetChild(i).gameObject))
                        --i;
                }
            }
            return false;
        }

        public static PlayerControl getPlayerById(byte playerId)
        {
            return PlayerControl.AllPlayerControls.GetFastEnumerator().ToArray().Where(p => p.PlayerId == playerId).FirstOrDefault();
        }

        public static MeshFilter CreateRectMesh(this MeshFilter filter, Vector2 size, Vector3? center = null)
        {
            center ??= Vector3.zero;

            var mesh = filter.mesh;

            float x = size.x * 0.5f;
            float y = size.y * 0.5f;
            mesh.SetVertices(new Vector3[] {
                new Vector3(-x, -y) + center.Value,
                new Vector3(x, -y) + center.Value,
                new Vector3(-x, y) + center.Value,
                new Vector3(x, y) + center.Value});
            mesh.SetTriangles(new int[] { 0, 2, 1, 2, 3, 1 }, 0);
            mesh.SetUVs(0, new Vector2[] { new(0, 0), new(1, 0), new(0, 1), new(1, 1) });
            var color = new Color32(255, 255, 255, 255);
            mesh.SetColors(new Color32[] { color, color, color, color });

            return filter;
        }

        static public void CloseInternal(this Minigame minigame)
        {
            if (minigame.amClosing != Minigame.CloseState.Closing)
            {
                if (minigame.CloseSound && Constants.ShouldPlaySfx()) SoundManager.Instance.PlaySound(minigame.CloseSound, false, 1f, null);
                if (PlayerControl.LocalPlayer) PlayerControl.HideCursorTemporarily();

                minigame.amClosing = Minigame.CloseState.Closing;
                minigame.StartCoroutine(minigame.CoDestroySelf());
            }
            else
            {
                GameObject.Destroy(minigame.gameObject);
            }
        }

        static public void BeginInternal(this Minigame minigame, PlayerTask task)
        {
            Minigame.Instance = minigame;
            minigame.MyTask = task;
            if (task && task.TryGetComponent<NormalPlayerTask>(out var normalTask)) minigame.MyNormTask = normalTask;
            minigame.timeOpened = Time.realtimeSinceStartup;
            if (PlayerControl.LocalPlayer)
            {
                if (MapBehaviour.Instance) MapBehaviour.Instance.Close();
                PlayerControl.LocalPlayer.MyPhysics.SetNormalizedVelocity(Vector2.zero);
            }
            minigame.TransType = TransitionType.None;
            minigame.StartCoroutine(minigame.CoAnimateOpen());
            minigame.CloseSound = ShipStatus.Instance.ShortTasks.First().MinigamePrefab.CloseSound;
        }

        public static RenderTexture SetCameraRenderTexture(this Camera camera, int textureX, int textureY)
        {
            if (camera.targetTexture) UnityEngine.Object.Destroy(camera.targetTexture);
            camera.targetTexture = new RenderTexture(textureX, textureY, 32, RenderTextureFormat.ARGB32);

            return camera.targetTexture;
        }

        public static (MeshRenderer renderer, MeshFilter filter) CreateMeshRenderer(string objName, Transform parent, Vector3 localPosition, int? layer, Color? color = null)
        {
            var meshFilter = CreateObject<MeshFilter>("mesh", parent, localPosition, layer);
            var meshRenderer = meshFilter.gameObject.AddComponent<MeshRenderer>();
            meshRenderer.material = new Material(Shader.Find(color.HasValue ? "Unlit/Color" : "Unlit/Texture"));
            if (color.HasValue) meshRenderer.sharedMaterial.color = color.Value;
            meshFilter.mesh = new Mesh();

            return (meshRenderer, meshFilter);
        }

        /// <summary>
        /// Determines whether the source can see the target
        /// </summary>
        /// <param name="source">Source player</param>
        /// <param name="target">Target player</param>
        /// <returns></returns>
        public static bool isVisible(PlayerControl source, PlayerControl target)
        {
            return !AnyNonTriggersBetween(source.GetTruePosition(), target.GetTruePosition(), out var vec)
                        && vec.magnitude < ShipStatus.Instance.CalculateLightRadius(GameData.Instance.GetPlayerById(source.PlayerId)) * 0.75f;
        }

        public static void setSemiTransparent(this PoolablePlayer player, bool value) {
            float alpha = value ? 0.25f : 1f;
            foreach (SpriteRenderer r in player.gameObject.GetComponentsInChildren<SpriteRenderer>())
                r.color = new Color(r.color.r, r.color.g, r.color.b, alpha);
            player.cosmetics.nameText.color = new Color(player.cosmetics.nameText.color.r, player.cosmetics.nameText.color.g, player.cosmetics.nameText.color.b, alpha);
        }

        public static string GetString(this TranslationController t, StringNames key, params Il2CppSystem.Object[] parts) {
            return t.GetString(key, parts);
        }

        public static string cs(Color c, string s) {
            return string.Format("<color=#{0:X2}{1:X2}{2:X2}{3:X2}>{4}</color>", ToByte(c.r), ToByte(c.g), ToByte(c.b), ToByte(c.a), s);
        }

        public static int lineCount(string text) {
            return text.Count(c => c == '\n');
        }

        private static byte ToByte(float f) {
            f = Mathf.Clamp01(f);
            return (byte)(f * 255);
        }

        public struct TextFeatures
        {
            public float fontSizeMultiplier;
            public float lineSpacingOffset;
            public float heightMultiplier;
            public float topMarginMultiplier;
            public float scrollSpeedMultiplier;
        }

        public static TextFeatures AnalyzeTextFeatures(string text)
        {
            TextFeatures features = new()
            {
                fontSizeMultiplier = 1f,
                lineSpacingOffset = 0f,
                heightMultiplier = 1f,
                topMarginMultiplier = 1f,
                scrollSpeedMultiplier = 0.08f
            };

            bool hasCJK = System.Text.RegularExpressions.Regex.IsMatch(
                text, @"[\p{IsHiragana}\p{IsKatakana}\p{IsCJKUnifiedIdeographs}]");

            bool hasComplexScript = System.Text.RegularExpressions.Regex.IsMatch(
                text, @"[\p{IsArabic}\p{IsThai}]");

            if (hasCJK)
            {
                features.fontSizeMultiplier = 0.85f;
                features.lineSpacingOffset = -20f;
                features.heightMultiplier = 1.15f;
                features.scrollSpeedMultiplier = 0.05f;

                bool hasKana = text.Any(c => c >= '\u3040' && c <= '\u30FF');
                if (hasKana)
                {
                    features.topMarginMultiplier = 0.8f;
                    features.heightMultiplier = 1.25f;
                }
            }
            else if (hasComplexScript)
            {
                features.fontSizeMultiplier = 0.9f;
                features.lineSpacingOffset = -10f;
                features.heightMultiplier = 1.2f;
                features.scrollSpeedMultiplier = 0.06f;
            }

            float avgLineLength = (float)text.Split('\n').Average(line => line.Length);
            if (avgLineLength > 60)
            {
                features.fontSizeMultiplier *= 0.95f;
                features.lineSpacingOffset -= 5f;
            }

            return features;
        }

        public static KeyValuePair<byte, float> MaxPair(this Dictionary<byte, float> self, out bool tie) {
            tie = true;
            KeyValuePair<byte, float> result = new(byte.MaxValue, int.MinValue);
            foreach (KeyValuePair<byte, float> keyValuePair in self)
            {
                if (keyValuePair.Value > result.Value)
                {
                    result = keyValuePair;
                    tie = false;
                }
                else if (keyValuePair.Value == result.Value)
                {
                    tie = true;
                }
            }
            return result;
        }

        public static T MarkDontUnload<T>(this T obj) where T : UnityEngine.Object
        {
            GameObject.DontDestroyOnLoad(obj);
            obj.hideFlags |= HideFlags.DontUnloadUnusedAsset | HideFlags.HideAndDontSave;

            return obj;
        }

        public static TMPro.TextMeshPro getFirst(this TMPro.TextMeshPro[] text)
        {
            if (text == null) return null;
            foreach (var self in text)
                if (self.text == "") return self;
            return text[0];
        }

        public static int totalCounts(this TMPro.TextMeshPro[] text)
        {
            if (text == null) return 0;
            int count = 0;
            foreach (var self in text)
                if (self.text != "") count++;
            return count;
        }

        public static bool sabotageActive()
        {
            var sabSystem = ShipStatus.Instance.Systems[SystemTypes.Sabotage].CastFast<SabotageSystemType>();
            return sabSystem.AnyActive;
        }

        public static float sabotageTimer()
        {
            var sabSystem = ShipStatus.Instance.Systems[SystemTypes.Sabotage].CastFast<SabotageSystemType>();
            return sabSystem.Timer;
        }
        public static bool canUseSabotage()
        {
            var sabSystem = ShipStatus.Instance.Systems[SystemTypes.Sabotage].CastFast<SabotageSystemType>();
            IActivatable doors = null;
            if (ShipStatus.Instance.Systems.TryGetValue(SystemTypes.Doors, out ISystemType systemType))
            {
                doors = systemType.CastFast<IActivatable>();
            }
            return GameManager.Instance.SabotagesEnabled() && sabSystem.Timer <= 0f && !sabSystem.AnyActive && !(doors != null && doors.IsActive);
        }

        public static List<byte> generateTasks(int numCommon, int numShort, int numLong)
        {
            if (numCommon + numShort + numLong <= 0)
            {
                numShort = 1;
            }

            var tasks = new Il2CppSystem.Collections.Generic.List<byte>();
            var hashSet = new Il2CppSystem.Collections.Generic.HashSet<TaskTypes>();

            var commonTasks = new Il2CppSystem.Collections.Generic.List<NormalPlayerTask>();
            foreach (var task in MapUtilities.CachedShipStatus.CommonTasks.OrderBy(x => rnd.Next())) commonTasks.Add(task);

            var shortTasks = new Il2CppSystem.Collections.Generic.List<NormalPlayerTask>();
            foreach (var task in MapUtilities.CachedShipStatus.ShortTasks.OrderBy(x => rnd.Next())) shortTasks.Add(task);

            var longTasks = new Il2CppSystem.Collections.Generic.List<NormalPlayerTask>();
            foreach (var task in MapUtilities.CachedShipStatus.LongTasks.OrderBy(x => rnd.Next())) longTasks.Add(task);

            int start = 0;
            MapUtilities.CachedShipStatus.AddTasksFromList(ref start, numCommon, tasks, hashSet, commonTasks);

            start = 0;
            MapUtilities.CachedShipStatus.AddTasksFromList(ref start, numShort, tasks, hashSet, shortTasks);

            start = 0;
            MapUtilities.CachedShipStatus.AddTasksFromList(ref start, numLong, tasks, hashSet, longTasks);

            return tasks.ToArray().ToList();
        }

        public static void generateAndAssignTasks(this PlayerControl player, int numCommon, int numShort, int numLong)
        {
            if (player == null) return;

            List<byte> taskTypeIds = generateTasks(numCommon, numShort, numLong);

            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.UncheckedSetTasks, Hazel.SendOption.Reliable, -1);
            writer.Write(player.PlayerId);
            writer.WriteBytesAndSize(taskTypeIds.ToArray());
            AmongUsClient.Instance.FinishRpcImmediately(writer);
            RPCProcedure.uncheckedSetTasks(player.PlayerId, taskTypeIds.ToArray());
        }

        public static bool isChinese()
        {
            try
            {
                var name = CultureInfo.CurrentUICulture.Name;
                if (name.StartsWith("zh")) return true;
                return false;
            }
            catch
            {
                return false;
            }
        }

        public static T FindAsset<T>(string name) where T : Il2CppObjectBase
        {
            foreach (var asset in UnityEngine.Object.FindObjectsOfTypeIncludingAssets(Il2CppType.Of<T>()))
            {
                if (asset.name == name) return asset.Cast<T>();
            }
            return null;
        }

        static public float Distance(this Vector3 myVec, Vector3 vector)
        {
            var vec = myVec - vector;
            vec.z = 0;
            return vec.magnitude;
        }

        public static LineRenderer SetUpLineRenderer(string objName, Transform parent, Vector3 localPosition, int? layer = null, float width = 0.2f)
        {
            var line = CreateObject<LineRenderer>(objName, parent, localPosition, layer);
            line.material.shader = Shader.Find("Sprites/Default");
            line.SetColors(Color.clear, Color.clear);
            line.positionCount = 2;
            line.SetPositions(new Vector3[] { Vector3.zero, Vector3.zero });
            line.useWorldSpace = false;
            line.SetWidth(width, width);
            return line;
        }

        static public float Distance(this Vector2 myVec, Vector2 vector) => (myVec - vector).magnitude;

        public static bool hidePlayerName(PlayerControl source, PlayerControl target) {
            if (Camouflager.camouflageTimer > 0f || MushroomSabotageActive()) return true; // No names are visible
            if (!source.Data.Role.IsImpostor && Ninja.isStealthed(target)) return true; // Hide Ninja nametags from non-impostors
            if (Sprinter.isSprinting(target) && source != target) return true; // Hide Sprinter nametags
            if (Fox.stealthed && target.isRole(RoleId.Fox) && source != target) return true; // Hide Fox nametags
            if (source != target && Kataomoi.isStalking(target)) return true; // Hide Kataomoi nametags
            if (Patches.SurveillanceMinigamePatch.nightVisionIsActive) return true;
            else if (Assassin.players.Any(x => x.player == target && x.isInvisble)) return true;
            else if (!TORMapOptions.hidePlayerNames) return false; // All names are visible
            else if (source == null || target == null) return true;
            else if (source == target) return false; // Player sees his own name
            else if (source.Data.Role.IsImpostor && (target.Data.Role.IsImpostor || target.isRole(RoleId.Spy) || Sidekick.players.Any(x => x.player == target && x.wasTeamRed) || Jackal.players.Any(x => x.player == target && x.wasTeamRed))) return false; // Members of team Impostors see the names of Impostors/Spies
            else if (source.getPartner() == target) return false; // Members of team Lovers see the names of each other
            else if (Jackal.players.Any(x => x.player == source && (target == x.fakeSidekick || (Jackal.getSidekick(source) != null && Jackal.getSidekick(source).player == target))) || Sidekick.players.Any(x => x.player == source && x.jackal.player == target)) return false; // Members of team Jackal see the names of each other
            else if (Deputy.knowsSheriff && (Sheriff.players.Any(x => x.player == source && Sheriff.getDeputy(source) != null && Sheriff.getDeputy(source).player == target) || Deputy.players.Any(x => x.player == source && x.sheriff.player == target))) return false; // Sheriff & Deputy see the names of each other
            else if ((source.isRole(RoleId.Fox) || source.isRole(RoleId.Immoralist)) && (target.isRole(RoleId.Fox) || target.isRole(RoleId.Immoralist))) return false; // Members of team Fox see the names of each other
            return true;
        }

        public static void setDefaultLook(this PlayerControl target, bool enforceNightVisionUpdate = true) {
            if (MushroomSabotageActive())
            {
                var instance = ShipStatus.Instance.CastFast<FungleShipStatus>().specialSabotage;
                MushroomMixupSabotageSystem.CondensedOutfit condensedOutfit = instance.currentMixups[target.PlayerId];
                NetworkedPlayerInfo.PlayerOutfit playerOutfit = instance.ConvertToPlayerOutfit(condensedOutfit);
                target.MixUpOutfit(playerOutfit);
            }
            else
                target.setLook(target.Data.PlayerName, target.Data.DefaultOutfit.ColorId, target.Data.DefaultOutfit.HatId, target.Data.DefaultOutfit.VisorId, target.Data.DefaultOutfit.SkinId, target.Data.DefaultOutfit.PetId, enforceNightVisionUpdate);
        }

        public static void setLook(this PlayerControl target, String playerName, int colorId, string hatId, string visorId, string skinId, string petId, bool enforceNightVisionUpdate = true) {
            target.RawSetColor(colorId);
            target.RawSetVisor(visorId, colorId);
            target.RawSetHat(hatId, colorId);
            target.RawSetName(hidePlayerName(PlayerControl.LocalPlayer, target) ? "" : playerName);


            SkinViewData nextSkin = null;
            try { nextSkin = ShipStatus.Instance.CosmeticsCache.GetSkin(skinId); } catch { return; };

            PlayerPhysics playerPhysics = target.MyPhysics;
            AnimationClip clip = null;
            var spriteAnim = playerPhysics.myPlayer.cosmetics.skin.animator;
            var currentPhysicsAnim = playerPhysics.Animations.Animator.GetCurrentAnimation();


            if (currentPhysicsAnim == playerPhysics.Animations.group.RunAnim) clip = nextSkin.RunAnim;
            else if (currentPhysicsAnim == playerPhysics.Animations.group.SpawnAnim) clip = nextSkin.SpawnAnim;
            else if (currentPhysicsAnim == playerPhysics.Animations.group.EnterVentAnim) clip = nextSkin.EnterVentAnim;
            else if (currentPhysicsAnim == playerPhysics.Animations.group.ExitVentAnim) clip = nextSkin.ExitVentAnim;
            else if (currentPhysicsAnim == playerPhysics.Animations.group.IdleAnim) clip = nextSkin.IdleAnim;
            else clip = nextSkin.IdleAnim;
            float progress = playerPhysics.Animations.Animator.m_animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
            playerPhysics.myPlayer.cosmetics.skin.skin = nextSkin;
            playerPhysics.myPlayer.cosmetics.skin.UpdateMaterial();

            spriteAnim.Play(clip, 1f);
            spriteAnim.m_animator.Play("a", 0, progress % 1);
            spriteAnim.m_animator.Update(0f);

            target.RawSetPet(petId, colorId);

            if (enforceNightVisionUpdate) Patches.SurveillanceMinigamePatch.enforceNightVision(target);
            Chameleon.update();  // so that morphling and camo wont make the chameleons visible
        }

        /// <summary>
        /// Show a flash on the screen with a fade in and fade out effect
        /// </summary>
        /// <param name="color">The color of the flash screen</param>
        /// <param name="duration">The duration of the flash effect</param>
        /// <param name="message">The message to display during the flash</param>
        /// <param name="textColor">The color of the text during the flash</param>
        /// <param name="textSprite">The sprite of the text to display during the flash</param>
        public static void showFlash(Color color, float duration = 1f, string message = "", Color textColor = default, Sprite textSprite = null) {
            if (FastDestroyableSingleton<HudManager>.Instance == null || FastDestroyableSingleton<HudManager>.Instance.FullScreen == null) return;
            var renderer = UnityEngine.Object.Instantiate(FastDestroyableSingleton<HudManager>.Instance.FullScreen, HudManager.Instance.transform);
            renderer.gameObject.SetActive(true);
            renderer.enabled = true;

            var messageText = CreateAndShowNotification(message, textColor == default ? HudManager.Instance.Notifier.settingsChangeColor : textColor, new Vector3(0f, 1f, -20f), spr : textSprite);
            messageText.alphaTimer = duration + 2f;
            messageText.AdjustNotification();

            FastDestroyableSingleton<HudManager>.Instance.StartCoroutine(Effects.Lerp(duration, new Action<float>((p) => {

                if (p < 0.5) {
                    if (renderer != null)
                        renderer.color = new Color(color.r, color.g, color.b, Mathf.Clamp01(p * 2 * 0.75f));
                } else {
                    if (renderer != null)
                        renderer.color = new Color(color.r, color.g, color.b, Mathf.Clamp01((1 - p) * 2 * 0.75f));
                }
                if (p == 1f && renderer != null) renderer.enabled = false;
            })));
        }

        /// <summary>
        /// Flashes the screen without a fade in or fade out duration
        /// </summary>
        /// <param name="color">The color of the flash</param>
        /// <param name="fadeIn">The duration of the fade-in effect</param>
        /// <param name="fadeOut">The duration of the fade-out effect</param>
        /// <param name="maxAlpha">The maximum alpha value of the flash</param>
        /// <param name="flashDuring">The duration to keep the flash at maximum alpha</param>
        /// <param name="message">The message to display during the flash</param>
        /// <param name="textColor">The color of the text during the flash</param>
        /// <param name="textSprite">The sprite of the text to display during the flash</param>
        public static void flashScreen(Color color, float fadeIn, float fadeOut, float maxAlpha = 0.5f, float flashDuring = 0f, string message = "", Color textColor = default, Sprite textSprite = null)
        {
            float duration = fadeIn + fadeOut + flashDuring;

            var flash = GameObject.Instantiate(FastDestroyableSingleton<HudManager>.Instance.FullScreen, FastDestroyableSingleton<HudManager>.Instance.transform);
            flash.color = color;
            flash.enabled = true;
            flash.gameObject.active = true;

            var messageText = CreateAndShowNotification(message, textColor == default ? HudManager.Instance.Notifier.settingsChangeColor : textColor, new Vector3(0f, 1f, -20f), spr : textSprite);
            messageText.alphaTimer = duration + 2f;
            messageText.AdjustNotification();

            FastDestroyableSingleton<HudManager>.Instance.StartCoroutine(Effects.Lerp(duration, new Action<float>((p) =>
            {
                if (p < (fadeIn / duration))
                {
                    if (flash != null)
                        flash.color = new Color(color.r, color.g, color.b, Mathf.Clamp01((PlayerControl.LocalPlayer.Data.IsDead ? Mathf.Min(maxAlpha, 0.2f) : maxAlpha) * p / (fadeIn / duration)));
                }
                else if (1 - p < (fadeOut / duration))
                {
                    if (flash != null)
                        flash.color = new Color(color.r, color.g, color.b, Mathf.Clamp01((PlayerControl.LocalPlayer.Data.IsDead ? Mathf.Min(maxAlpha, 0.2f) : maxAlpha) * (1 - p) / (fadeOut / duration)));
                }
                else
                {
                    if (flash != null)
                        flash.color = new Color(color.r, color.g, color.b, PlayerControl.LocalPlayer.Data.IsDead ? Mathf.Min(maxAlpha, 0.2f) : maxAlpha);
                }
                if ((p == 1f || MeetingHud.Instance) && flash != null)
                {
                    flash.enabled = false;
                    GameObject.Destroy(flash.gameObject);
                }
            })));
        }

        public static void ModRevive(this PlayerControl player, bool resetRoleIfGhost = true)
        {
            player.Data.IsDead = false;
            player.gameObject.layer = LayerMask.NameToLayer("Players");
            player.MyPhysics.ResetMoveState();
            player.cosmetics.SetPetSource(player);
            player.cosmetics.SetNameMask(true);
            if (player.AmOwner)
            {
                DestroyableSingleton<HudManager>.Instance.ShadowQuad.gameObject.SetActive(true);
                DestroyableSingleton<HudManager>.Instance.KillButton.ToggleVisible(player.Data.Role.IsImpostor);
                DestroyableSingleton<HudManager>.Instance.AdminButton.ToggleVisible(player.Data.Role.IsImpostor);
                DestroyableSingleton<HudManager>.Instance.SabotageButton.ToggleVisible(player.Data.Role.IsImpostor);
                DestroyableSingleton<HudManager>.Instance.ImpostorVentButton.ToggleVisible(player.Data.Role.IsImpostor);
                DestroyableSingleton<HudManager>.Instance.Chat.ForceClosed();
                DestroyableSingleton<HudManager>.Instance.Chat.SetVisible(false);
            }
            if (!resetRoleIfGhost || !AmongUsClient.Instance.AmHost || !RoleManager.IsGhostRole(player.Data.Role.Role))
                return;
            player.RpcSetRole(RoleTypes.Crewmate);
        }

        public static GameObject CreateObject(string objName, Transform parent, Vector3 localPosition, int? layer = null)
        {
            var obj = new GameObject(objName);
            obj.transform.SetParent(parent);
            obj.transform.localPosition = localPosition;
            obj.transform.localScale = new Vector3(1f, 1f, 1f);
            if (layer.HasValue) obj.layer = layer.Value;
            else if (parent != null) obj.layer = parent.gameObject.layer;
            return obj;
        }

        public static T CreateObject<T>(string objName, Transform parent, Vector3 localPosition, int? layer = null) where T : Component
        {
            return CreateObject(objName, parent, localPosition, layer).AddComponent<T>();
        }

        public static void SetModText(this TextTranslatorTMP text, string translationKey)
        {
            text.TargetText = (StringNames)short.MaxValue;
            text.defaultStr = translationKey;
        }

        public static bool CanSeeInvisible(this PlayerControl player) => (player.Data.IsDead && (RoleManager.IsGhostRole(player.Data.RoleType) || FreePlayGM.isFreePlayGM)) || Lighter.isLightActive(player);

        public static void SetAsUIAspectContent(this GameObject obj, AspectPosition.EdgeAlignments alignment, Vector3 distanceFromEdge)
        {
            var aspectPos = obj.AddComponent<AspectPosition>();
            aspectPos.Alignment = alignment;
            aspectPos.parentCam = HudManager.InstanceExists ? HudManager.Instance.UICamera : Camera.main;
            aspectPos.DistanceFromEdge = distanceFromEdge;
            aspectPos.AdjustPosition();
        }

        public static void AddScaledSprite(this AspectScaledAsset scaledAsset, SpriteRenderer renderer)
        {
            if (scaledAsset.initialized)
                scaledAsset.allSprites.Add(new(renderer.size.x, renderer));
            else
                scaledAsset.spritesToScale.Add(renderer);
        }


        public static void ForEachAllChildren(this GameObject gameObject, Action<GameObject> todo)
        {
            gameObject.ForEachChild((Il2CppSystem.Action<GameObject>)((obj) => {
                todo.Invoke(obj);
                obj.ForEachAllChildren(todo);
            }));
        }

        public static IEnumerator CoAnimIcon(PlayerControl target)
        {
            var holder = CreateObject("AnimHolder", HudManager.Instance.transform, new(0f, -1.85f, -10f));

            var display = VanillaAsset.GetPlayerDisplay();
            display.transform.SetParent(holder.transform);
            display.transform.localPosition = new(0.4f, -0.2f, -1f);
            display.transform.localScale = new(0.45f, 0.45f, 1f);
            display.gameObject.ForEachAllChildren(obj => obj.layer = LayerMask.NameToLayer("UI"));
            display.Cosmetics.SetColor(6);
            display.Animations.PlayRunAnimation();

            var currentBody = display.Cosmetics.currentBodySprite.BodySprite;

            var arrow = CreateObject<SpriteRenderer>("Arrow", holder.transform, new(0f, 0f, -0.5f));
            arrow.sprite = Detective.detectiveIcon.GetSprite();

            Color arrowNormalColor = new(0.25f, 0.4f, 0.7f);

            float p = 0f;
            while (p < 1f)
            {
                currentBody.color = Color.white.AlphaMultiplied(p);
                arrow.color = arrowNormalColor.AlphaMultiplied(p);
                p += Time.deltaTime / 0.35f;
                yield return null;
            }

            yield return Effects.Wait(0.12f);

            SpriteRenderer additionalRenderer = null;
            p = 0f;
            while (p < Detective.inspectDuration && !MeetingHud.Instance && !PlayerControl.LocalPlayer.Data.IsDead && PlayerControl.LocalPlayer.isRole(RoleId.Detective))
            {
                if (target == null) break;
                Vector2 del = target.transform.position - FindCamera(LayerMask.NameToLayer("UI")).transform.position;
                arrow.transform.localScale = Vector3.one;
                arrow.transform.localEulerAngles = new(0f, 0f, Mathf.Atan2(del.y, del.x) * 180f / Mathf.PI);
                p += Time.deltaTime;
                yield return null;
            }

            p = 0f;
            while (p < 1f)
            {
                currentBody.color = Color.white.AlphaMultiplied(1f - p);
                arrow.color = arrowNormalColor.AlphaMultiplied(1f - p);
                if (additionalRenderer != null) additionalRenderer.color = Color.white.AlphaMultiplied(1f - p);
                p += Time.deltaTime / 0.8f;
                yield return null;
            }

            GameObject.Destroy(holder.gameObject);
        }

        public static R GetRolePrefab<R>() where R : RoleBehaviour
        {
            foreach (RoleBehaviour role in RoleManager.Instance.AllRoles)
            {
                R r = role.TryCast<R>();
                if (r != null)
                {
                    return r;
                }
            }
            return null;
        }

        public static (GameObject obj, NoisemakerArrow arrow) InstantiateNoisemakerArrow(Vector2 targetPos, bool withSound = false)
        {
            var noisemaker = GetRolePrefab<NoisemakerRole>();
            if (noisemaker != null)
            {
                if (withSound && Constants.ShouldPlaySfx())
                {
                    SoundManager.Instance.PlayDynamicSound("NoisemakerAlert", noisemaker.deathSound, false, (DynamicSound.GetDynamicsFunction)((source, dt) =>
                    {
                        if (!PlayerControl.LocalPlayer)
                        {
                            source.volume = 0f;
                            return;
                        }
                        source.volume = 1f;
                        Vector2 truePosition = PlayerControl.LocalPlayer.GetTruePosition();
                        source.volume = SoundManager.GetSoundVolume(targetPos, truePosition, 7f, 50f, 0.5f);
                    }), SoundManager.Instance.SfxChannel);
                    VibrationManager.Vibrate(1f, PlayerControl.LocalPlayer.GetTruePosition(), 7f, 1.2f, VibrationManager.VibrationFalloff.None, null, false);
                }
                GameObject gameObject = GameObject.Instantiate<GameObject>(noisemaker.deathArrowPrefab, Vector3.zero, Quaternion.identity);
                var deathArrow = gameObject.GetComponent<NoisemakerArrow>();
                deathArrow.SetDuration(3f);
                deathArrow.gameObject.SetActive(true);
                deathArrow.target = targetPos;

                return (gameObject, deathArrow);
            }

            return (null, null)!;
        }

        public static void DoTransitionFade(this TransitionFade transitionFade, GameObject transitionFrom, GameObject transitionTo, Action onTransition, Action callback)
        {
            if (transitionTo) transitionTo!.SetActive(false);

            IEnumerator Coroutine()
            {
                yield return Effects.ColorFade(transitionFade.overlay, Color.clear, Color.black, 0.1f);
                if (transitionFrom && transitionFrom!.gameObject) transitionFrom.gameObject.SetActive(false);
                if (transitionTo && transitionTo!.gameObject) if (transitionTo != null) transitionTo.gameObject.SetActive(true);
                onTransition.Invoke();
                yield return null;
                yield return Effects.ColorFade(transitionFade.overlay, Color.black, Color.clear, 0.1f);
                callback.Invoke();
                yield break;
            }

            transitionFade.StartCoroutine(Coroutine().WrapToIl2Cpp());
        }

        public static void TryAdd<T>(this List<T> list, T param) where T : class
        {
            if (list.Contains(param) || param == null) return;
            list.Add(param);
        }

        public static void setInvisible(PlayerControl player, Color color, float alpha)
        {
            if (player.cosmetics.currentBodySprite.BodySprite != null)
                player.cosmetics.currentBodySprite.BodySprite.color = color;

            if (player.cosmetics.skin?.layer != null)
                player.cosmetics.skin.layer.color = color;

            if (player.cosmetics.hat != null)
            {
                player.cosmetics.hat.FrontLayer.color = color;
                player.cosmetics.hat.BackLayer.color = color;
            }

            if (player.cosmetics.currentPet != null)
                player.cosmetics.currentPet.SetAlpha(alpha);

            if (player.cosmetics.visor != null)
                player.cosmetics.visor.Image.color = color;

            if (player.cosmetics.colorBlindText != null)
                player.cosmetics.colorBlindText.color = color;

            if (player.cosmetics.PettingHand != null)
                player.cosmetics.PettingHand.SetAlpha(alpha);
        }

        // From Mira-API

        /// <summary>
        /// Creates and shows a notification.
        /// </summary>
        /// <param name="text">The text you want to display.</param>
        /// <param name="color">The color of the text and image.</param>
        /// <param name="clip">The sound you want to play with the notification.</param>
        /// <param name="spr">The sprite beside the notification.</param>
        /// <returns>The created notification.</returns>
        public static LobbyNotificationMessage CreateAndShowNotification(string text, Color color, AudioClip clip = null, Sprite spr = null)
        {
            return CreateAndShowNotification(text, color, new Vector3(0f, 0f, -2f), clip, spr);
        }

        /// <summary>
        /// Creates and shows a notification.
        /// </summary>
        /// <param name="text">The text you want to display.</param>
        /// <param name="color">The color of the text and image.</param>
        /// <param name="localPos">The position of the notification.</param>
        /// <param name="clip">The sound you want to play with the notification.</param>
        /// <param name="spr">The sprite beside the notification.</param>
        /// <returns>The created notification.</returns>
        public static LobbyNotificationMessage CreateAndShowNotification(string text, Color color, Vector3 localPos, AudioClip clip = null, Sprite spr = null)
        {
            var popper = HudManager.Instance.Notifier;
            var newMessage = UnityEngine.Object.Instantiate(popper.notificationMessageOrigin, Vector3.zero, Quaternion.identity, popper.transform);
            newMessage.transform.localPosition = localPos;
            newMessage.SetUp(text, spr ?? null, color, new System.Action(() => popper.OnMessageDestroy(newMessage)));
            popper.lastMessageKey = -1;
            popper.ShiftMessages();
            popper.AddMessageToQueue(newMessage);

            if (clip == null) return newMessage;
            SoundManager.Instance.StopSound(clip);
            SoundManager.Instance.PlaySound(clip, false, 2f);

            return newMessage;
        }

        public static PlainShipRoom getPlainShipRoom(PlayerControl p)
        {
            PlainShipRoom[] array = null;
            Il2CppReferenceArray<Collider2D> buffer = new Collider2D[10];
            ContactFilter2D filter = default;
            filter.layerMask = Constants.PlayersOnlyMask;
            filter.useLayerMask = true;
            filter.useTriggers = false;
            array = MapUtilities.CachedShipStatus?.AllRooms;
            if (array == null) return null;
            foreach (PlainShipRoom plainShipRoom in array)
            {
                if (plainShipRoom.roomArea)
                {
                    int hitCount = plainShipRoom.roomArea.OverlapCollider(filter, buffer);
                    if (hitCount == 0) continue;
                    for (int i = 0; i < hitCount; i++)
                    {
                        if (buffer[i]?.gameObject == p.gameObject)
                        {
                            return plainShipRoom;
                        }
                    }
                }
            }
            return null;
        }

        public static Vector3 AsVector3(this Vector2 vec, float z)
        {
            Vector3 result = vec;
            result.z = z;
            return result;
        }

        public static bool roleCanUseSabotage(this PlayerControl player) {
            bool roleCouldUse = false;
            if (Madmate.madmate.Any(x => x.PlayerId == player?.PlayerId) && Madmate.canSabotage)
                roleCouldUse = true;
            else if (CreatedMadmate.createdMadmate.Any(x => x.PlayerId == player?.PlayerId) && CreatedMadmate.canSabotage)
                roleCouldUse = true;
            else if (player.isRole(RoleId.Janitor) && !Janitor.canSabotage)
                roleCouldUse = false;
            else if (player.isRole(RoleId.Mafioso) && Godfather.exists && Godfather.allPlayers.Any(x => !x.Data.IsDead))
                roleCouldUse = false;
            else if (player?.Data?.Role?.IsImpostor == true)
                roleCouldUse = true;
            return roleCouldUse;
        }

        public static bool roleCanUseVents(this PlayerControl player) {
            bool roleCouldUse = false;
            if (player.isRole(RoleId.Engineer))
                roleCouldUse = true;
            else if (Jackal.canUseVents && player.isRole(RoleId.Jackal))
                roleCouldUse = true;
            else if (Sidekick.canUseVents && player.isRole(RoleId.Sidekick))
                roleCouldUse = true;
            else if (Spy.canEnterVents && player.isRole(RoleId.Spy))
                roleCouldUse = true;
            else if (Vulture.canUseVents && player.isRole(RoleId.Vulture))
                roleCouldUse = true;
            else if (Madmate.canVent && Madmate.madmate.Any(x => x.PlayerId == player.PlayerId))
                roleCouldUse = true;
            else if (CreatedMadmate.canEnterVents && CreatedMadmate.createdMadmate.Any(x => x.PlayerId == player.PlayerId))
                roleCouldUse = true;
            else if (player.isRole(RoleId.Moriarty))
                roleCouldUse = true;
            else if (JekyllAndHyde.players.Any(x => x.player == player && !x.isJekyll()))
                roleCouldUse = true;
            else if (Jester.canUseVents && player.isRole(RoleId.Jester))
                roleCouldUse = true;
            else if (Thief.canUseVents && player.isRole(RoleId.Thief))
                roleCouldUse = true;
            else if (player.isRole(RoleId.SchrodingersCat) && SchrodingersCat.hasTeam() && SchrodingersCat.team != SchrodingersCat.Team.Crewmate)
                roleCouldUse = true;
            else if (player.isRole(RoleId.Pelican) && Pelican.canUseVents)
                roleCouldUse = true;
            else if (player.isRole(RoleId.Yandere))
                roleCouldUse = true;
            else if (player.isRole(RoleId.TaskMaster) && TaskMaster.canVent)
                roleCouldUse = true;
            else if (player.Data?.Role != null && player.Data.Role.CanVent)
            {
                if (player.isRole(RoleId.Ninja) && !Ninja.canUseVents)
                    roleCouldUse = false;
                else if (player.isRole(RoleId.Undertaker) && Undertaker.DraggedBody != null && !Undertaker.canVentWhileDragging)
                    roleCouldUse = false;
                else
                    roleCouldUse = true;
            }
            return roleCouldUse;
        }
        public static bool checkArmored(PlayerControl target, bool breakShield, bool showShield, bool additionalCondition = true)
        {
            if (target != null && Armored.armored != null && Armored.armored == target && !Armored.isBrokenArmor && additionalCondition)
            {
                if (breakShield)
                {
                    RPCProcedure.BreakArmor.Invoke();
                }
                if (showShield)
                {
                    target.ShowFailedMurder();
                }
                return true;
            }
            return false;
        }

        public static MurderAttemptResult checkMuderAttempt(PlayerControl killer, PlayerControl target, bool blockRewind = false, bool ignoreBlank = false, bool ignoreIfKillerIsDead = false) {
            var targetRole = RoleInfo.getRoleInfoForPlayer(target, false).FirstOrDefault();

            // Modified vanilla checks
            if (AmongUsClient.Instance.IsGameOver) return MurderAttemptResult.SuppressKill;
            if (killer == null || killer.Data == null || (killer.Data.IsDead && !ignoreIfKillerIsDead) || killer.Data.Disconnected) return MurderAttemptResult.SuppressKill; // Allow non Impostor kills compared to vanilla code
            if (target == null || target.Data == null || target.Data.IsDead || target.Data.Disconnected) return MurderAttemptResult.SuppressKill; // Allow killing players in vents compared to vanilla code

            if (GameOptionsManager.Instance.currentGameOptions.GameMode == GameModes.HideNSeek) return MurderAttemptResult.PerformKill;

            // Handle first kill attempt
            if (TORMapOptions.shieldFirstKill && TORMapOptions.firstKillPlayer == target) return MurderAttemptResult.SuppressKill;

            // Handle blank shot
            if (!ignoreBlank && Pursuer.blankedList.Any(x => x.PlayerId == killer.PlayerId)) {
                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SetBlanked, Hazel.SendOption.Reliable, -1);
                writer.Write(killer.PlayerId);
                writer.Write((byte)0);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                RPCProcedure.setBlanked(killer.PlayerId, 0);

                return MurderAttemptResult.BlankKill;
            }

            // Block impostor shielded kill
            if (Medic.IsShielded(target)) {
                foreach (var medic in Medic.GetMedic(target)) {
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(killer.NetId, (byte)CustomRPC.ShieldedMurderAttempt, Hazel.SendOption.Reliable, -1);
                    writer.Write(medic.player.PlayerId);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.shieldedMurderAttempt(medic.player.PlayerId);

                    Medic.GainAchievement.Invoke((killer.PlayerId, medic.player.PlayerId));
                }
                SoundEffectsManager.play("fail");
                return MurderAttemptResult.SuppressKill;
            }

            // Block impostor not fully grown mini kill
            else if (Mini.mini != null && target == Mini.mini && !Mini.isGrownUp()) {
                return MurderAttemptResult.SuppressKill;
            }

            // Block Time Master with time shield kill
            else if (TimeMaster.players.Any(x => x.player == target && x.shieldActive)) {
                if (!blockRewind) { // Only rewind the attempt was not called because a meeting startet 
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(killer.NetId, (byte)CustomRPC.TimeMasterRewindTime, Hazel.SendOption.Reliable, -1);
                    writer.Write(target.PlayerId);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.timeMasterRewindTime(target.PlayerId);
                }
                return MurderAttemptResult.SuppressKill;
            }

            else if (Cupid.checkShieldActive(target))
            {
                Cupid.scapeGoat(target);
                return MurderAttemptResult.BlankKill;
            }

            // Kill the killer if Veteran is on alert
            else if (Veteran.players.Any(x => x.player == target && x.alertActive))
            {
                RPCProcedure.RpcClearAchievement.Invoke(("veteran.challenge", target.PlayerId));
                RPCProcedure.RpcClearAchievement.Invoke(("veteran.challenge2", target.PlayerId));
                return MurderAttemptResult.ReverseKill;
            }

            // Thief if hit crew only kill if setting says so, but also kill the thief.
            else if (Thief.isFailedThiefKill(target, killer, targetRole)) {
                var thief = Thief.getRole(killer);
                if (!checkArmored(killer, true, true))
                    thief.suicideFlag = true;
                return MurderAttemptResult.SuppressKill;
            }

            // Block Armored with armor kill
            else if (checkArmored(target, true, killer == PlayerControl.LocalPlayer, !Sheriff.exists || !killer.isRole(RoleId.Sheriff) || (isEvil(target) && Sheriff.canKillNeutrals) || isKiller(target)))
            {
                return MurderAttemptResult.BlankKill;
            }

            // Block hunted with time shield kill
            else if (Hunted.timeshieldActive.Contains(target.PlayerId)) {
                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(killer.NetId, (byte)CustomRPC.HuntedRewindTime, Hazel.SendOption.Reliable, -1);
                writer.Write(target.PlayerId);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                RPCProcedure.huntedRewindTime(target.PlayerId);

                return MurderAttemptResult.SuppressKill;
            }

            if (TransportationToolPatches.isUsingTransportation(target) && !blockRewind && killer.isRole(RoleId.Vampire))
            {
                return MurderAttemptResult.DelayVampireKill;
            }
            else if (TransportationToolPatches.isUsingTransportation(target))
                return MurderAttemptResult.SuppressKill;

            return MurderAttemptResult.PerformKill;
        }

        public static void MurderPlayer(PlayerControl killer, PlayerControl target, bool showAnimation)
        {
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.UncheckedMurderPlayer, Hazel.SendOption.Reliable, -1);
            writer.Write(killer.PlayerId);
            writer.Write(target.PlayerId);
            writer.Write(showAnimation ? Byte.MaxValue : 0);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
            RPCProcedure.uncheckedMurderPlayer(killer.PlayerId, target.PlayerId, showAnimation ? Byte.MaxValue : (byte)0);
        }

        public static MurderAttemptResult checkMurderAttemptAndKill(PlayerControl killer, PlayerControl target, bool isMeetingStart = false, bool showAnimation = true, bool ignoreBlank = false, bool ignoreIfKillerIsDead = false)  {
            // The local player checks for the validity of the kill and performs it afterwards (different to vanilla, where the host performs all the checks)
            // The kill attempt will be shared using a custom RPC, hence combining modded and unmodded versions is impossible

            MurderAttemptResult murder = checkMuderAttempt(killer, target, isMeetingStart, ignoreBlank, ignoreIfKillerIsDead);
            if (murder == MurderAttemptResult.PerformKill) {
                MurderPlayer(killer, target, showAnimation);
            }
            else if (murder == MurderAttemptResult.DelayVampireKill)
            {
                HudManager.Instance.StartCoroutine(Effects.Lerp(10f, new Action<float>((p) => {
                    if (!TransportationToolPatches.isUsingTransportation(target) && Vampire.players.Any(x => x.bitten == target))
                    {
                        Vampire.SetBitten.Invoke((byte.MaxValue, byte.MaxValue, killer.PlayerId));
                        MurderPlayer(killer, target, showAnimation);
                    }
                })));
            }

            if (murder == MurderAttemptResult.ReverseKill)
            {
                checkMurderAttemptAndKill(target, killer, isMeetingStart);
            }
            return murder;            
        }
    
        public static void shareGameVersion() {
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.VersionHandshake, Hazel.SendOption.Reliable, -1);
            writer.Write((byte)TheOtherRolesPlugin.Version.Major);
            writer.Write((byte)TheOtherRolesPlugin.Version.Minor);
            writer.Write((byte)TheOtherRolesPlugin.Version.Build);
            writer.Write(AmongUsClient.Instance.AmHost ? Patches.GameStartManagerPatch.timer : -1f);
            writer.WritePacked(AmongUsClient.Instance.ClientId);
            writer.Write((byte)(TheOtherRolesPlugin.Version.Revision < 0 ? 0xFF : TheOtherRolesPlugin.Version.Revision));
            writer.Write(Assembly.GetExecutingAssembly().ManifestModule.ModuleVersionId.ToByteArray());
            writer.Write(TheOtherRolesPlugin.SubVersionString);
            writer.Write(Constants.GetPlatformType() == Platforms.Android);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
            RPCProcedure.versionHandshake(TheOtherRolesPlugin.Version.Major, TheOtherRolesPlugin.Version.Minor, TheOtherRolesPlugin.Version.Build, TheOtherRolesPlugin.Version.Revision,
                Assembly.GetExecutingAssembly().ManifestModule.ModuleVersionId, AmongUsClient.Instance.ClientId, TheOtherRolesPlugin.SubVersionString, Constants.GetPlatformType() == Platforms.Android);
        }

        public static List<PlayerControl> getKillerTeamMembers(PlayerControl player) {
            List<PlayerControl> team = new();
            foreach(PlayerControl p in PlayerControl.AllPlayerControls) {
                if (player.Data.Role.IsImpostor && p.Data.Role.IsImpostor && player.PlayerId != p.PlayerId && team.All(x => x.PlayerId != p.PlayerId)) team.Add(p);
                else if (player.isRole(RoleId.Jackal) && p.isRole(RoleId.Sidekick)) team.Add(p); 
                else if (player.isRole(RoleId.Sidekick) && p.isRole(RoleId.Jackal)) team.Add(p);
            }
            
            return team;
        }

        /// <summary>
        /// Check whether the action should be done. (i.e. Veteran alert)
        /// </summary>
        /// <param name="player">The action player</param>
        /// <param name="target">The target of the action player</param>
        /// <returns></returns>
        public static bool checkSuspendAction(PlayerControl player, PlayerControl target)
        {
            if (player == null || target == null) return false;
            if (Veteran.players.Any(x => x.player == target && x.alertActive))
            {
                if (isEvil(player))
                {
                    _ = checkMuderAttempt(player, target);  // Gives the Veteran the achievement
                    checkMurderAttemptAndKill(target, player);
                    return true;
                }
            }
            return false;
        }

        public static bool isNeutral(PlayerControl player) {
            RoleInfo roleInfo = RoleInfo.getRoleInfoForPlayer(player, false, true).FirstOrDefault();
            if (roleInfo != null)
                return roleInfo.isNeutral;
            return false;
        }

        public static bool isKiller(PlayerControl player) {
            return player.Data.Role.IsImpostor || 
                (isNeutral(player) && 
                !player.isRole(RoleId.Jester) && 
                !player.isRole(RoleId.Arsonist) && 
                !player.isRole(RoleId.Vulture) && 
                !player.isRole(RoleId.Lawyer) && 
                !player.isRole(RoleId.Pursuer) &&
                !player.isRole(RoleId.Opportunist) &&
                !player.isRole(RoleId.Akujo) &&
                !player.isRole(RoleId.PlagueDoctor) &&
                !player.isRole(RoleId.Cupid) &&
                !(player.isRole(RoleId.SchrodingersCat) && !SchrodingersCat.hasTeam()));

        }

        public static bool isCrew(this PlayerControl player) {
            return player != null && !player.Data.Role.IsImpostor && !isNeutral(player);
        }

        public static bool isEvil(PlayerControl player) {
            return player.Data.Role.IsImpostor || isNeutral(player);
        }

        public static bool zoomOutStatus = false;
        public static void toggleZoom(bool reset=false) {
            float orthographicSize = reset || zoomOutStatus ? 3f : 12f;

            zoomOutStatus = !zoomOutStatus && !reset;
            Camera.main.orthographicSize = orthographicSize;
            foreach (var cam in Camera.allCameras) {
                if (cam != null && cam.gameObject.name == "UI Camera") cam.orthographicSize = orthographicSize;  // The UI is scaled too, else we cant click the buttons. Downside: map is super small.
            }

            var tzGO = GameObject.Find("TOGGLEZOOMBUTTON");
            if (tzGO != null)
            {
                var rend = tzGO.transform.Find("Inactive").GetComponent<SpriteRenderer>();
                var rendActive = tzGO.transform.Find("Active").GetComponent<SpriteRenderer>();
                rend.sprite = zoomOutStatus ? loadSpriteFromResources("TheOtherRoles.Resources.Plus_Button.png", 100f) : loadSpriteFromResources("TheOtherRoles.Resources.Minus_Button.png", 100f);
                rendActive.sprite = zoomOutStatus ? loadSpriteFromResources("TheOtherRoles.Resources.Plus_ButtonActive.png", 100f) : loadSpriteFromResources("TheOtherRoles.Resources.Minus_ButtonActive.png", 100f);
                tzGO.transform.localScale = new Vector3(1.2f, 1.2f, 1f) * (zoomOutStatus ? 4 : 1);
            }

            ResolutionManager.ResolutionChanged.Invoke((float)Screen.width / Screen.height, Screen.width, Screen.height, Screen.fullScreen); // This will move button positions to the correct position.
        }

        public static void AddModSettingsChangeMessage(this NotificationPopper popper, StringNames key, string value, string option, bool playSound = true)
        {
            string str = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.LobbyChangeSettingNotification, "<font=\"Barlow-Black SDF\" material=\"Barlow-Black Outline\">" + option + "</font>", "<font=\"Barlow-Black SDF\" material=\"Barlow-Black Outline\">" + value + "</font>");
            popper.SettingsChangeMessageLogic(key, str, playSound);
        }

        private static long GetBuiltInTicks()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var builtin = assembly.GetType("Builtin");
            if (builtin == null) return 0;
            var field = builtin.GetField("CompileTime");
            if (field == null) return 0;
            var value = field.GetValue(null);
            if (value == null) return 0;
            return (long)value;
        }

        public static async Task checkBeta() {
            if (TheOtherRolesPlugin.betaDays > 0) {
                TheOtherRolesPlugin.Logger.LogMessage($"Beta check");
                var ticks = GetBuiltInTicks();
                var compileTime = new DateTime(ticks, DateTimeKind.Utc);  // This may show as an error, but it is not, compilation will work!
                TheOtherRolesPlugin.Logger.LogMessage($"Compiled at {compileTime.ToString(CultureInfo.InvariantCulture)}");
                DateTime? now;
                // Get time from the internet, so no-one can cheat it (so easily).
                try {
                    var client = new System.Net.Http.HttpClient();
                    using var response = await client.GetAsync("http://www.google.com/");
                    if (response.IsSuccessStatusCode)
                        now = response.Headers.Date?.UtcDateTime;
                    else {
                        TheOtherRolesPlugin.Logger.LogMessage($"Could not get time from server: {response.StatusCode}");
                        now = DateTime.UtcNow; //In case something goes wrong. 
                    }
                } catch (System.Net.Http.HttpRequestException) {
                    now = DateTime.UtcNow;
                }
                if ((now - compileTime)?.TotalDays > TheOtherRolesPlugin.betaDays) {
                    TheOtherRolesPlugin.Logger.LogMessage($"Beta expired!");
#if WINDOWS
                    BepInExUpdater.MessageBoxTimeout(BepInExUpdater.GetForegroundWindow(), "BETA is expired. You cannot play this version anymore.", "The Other Roles Beta", 0,0, 10000);
#endif
                    Application.Quit();

                } else TheOtherRolesPlugin.Logger.LogMessage($"Beta will remain runnable for {TheOtherRolesPlugin.betaDays - (now - compileTime)?.TotalDays} days!");
            }
        }

        public static bool hasImpVision(NetworkedPlayerInfo player) {
            return player.Role.IsImpostor
                || (player.Object.isRole(RoleId.Jackal) && Jackal.hasImpostorVision)
                || (player.Object.isRole(RoleId.Sidekick) && Sidekick.hasImpostorVision)
                || (player.Object.isRole(RoleId.Spy) && Spy.hasImpostorVision)
                || (player.Object.isRole(RoleId.Jester) && Jester.hasImpostorVision)
                || (player.Object.isRole(RoleId.Thief) && Thief.hasImpostorVision)
                || (Madmate.madmate.Any(x => x.PlayerId == player.PlayerId) && Madmate.hasImpostorVision)
                || (CreatedMadmate.createdMadmate.Any(x => x.PlayerId == player.PlayerId) && CreatedMadmate.hasImpostorVision)
                || player.Object.isRole(RoleId.Moriarty)
                || JekyllAndHyde.players.Any(x => x.player && x.player.PlayerId == player.PlayerId && !x.isJekyll())
                || player.Object.isRole(RoleId.Fox)
                || (player.Object.isRole(RoleId.Pelican) && Pelican.hasImpVision)
                || (player.Object.isRole(RoleId.Yandere) && Yandere.hasImpVision)
                || (player.Object.isRole(RoleId.SchrodingersCat) && SchrodingersCat.hasTeam() && SchrodingersCat.team != SchrodingersCat.Team.Crewmate);
        }
        
        public static object TryCast(this Il2CppObjectBase self, Type type)
        {
            return AccessTools.Method(self.GetType(), nameof(Il2CppObjectBase.TryCast)).MakeGenericMethod(type).Invoke(self, Array.Empty<object>());
        }

        public static void SetDeadBodyOutline(DeadBody target, Color? color)
        {
            if (target == null || target.bodyRenderers[0] == null) return;
            target.bodyRenderers[0].material.SetFloat("_Outline", color == null ? 0f : 1f);
            if (color != null) target.bodyRenderers[0].material.SetColor("_OutlineColor", color.Value);
        }

        public static int GetClientId(PlayerControl control)
        {
            for (int i = 0; i < AmongUsClient.Instance.allClients.Count; i++)
            {
                InnerNet.ClientData data = AmongUsClient.Instance.allClients[i];
                if (data.Character == control)
                    return data.Id;
            }
            return -1;
        }

        public static DeadBody setDeadTarget(float maxDistance = 0f, PlayerControl targetingPlayer = null)
        {
            DeadBody result = null;
            float closestDistance = float.MaxValue;

            if (!MapUtilities.CachedShipStatus) return null;

            if (targetingPlayer == null) targetingPlayer = PlayerControl.LocalPlayer;
            if (targetingPlayer.Data.IsDead) return null;

            maxDistance = maxDistance == 0f ? 1f : maxDistance + 0.1f;

            Vector2 truePosition = targetingPlayer.GetTruePosition() - new Vector2(-0.2f, -0.22f);

            bool flag = GameOptionsManager.Instance.currentNormalGameOptions.GhostsDoTasks
                        && (!AmongUsClient.Instance || !AmongUsClient.Instance.IsGameOver);

            Collider2D[] allocs = Physics2D.OverlapCircleAll(truePosition, maxDistance,
                LayerMask.GetMask("Players", "Ghost"));


            foreach (Collider2D collider2D in allocs)
            {
                if (!flag || collider2D.tag != "DeadBody") continue;
                DeadBody component = collider2D.GetComponent<DeadBody>();

                if (!(Vector2.Distance(truePosition, component.TruePosition) <=
                      maxDistance)) continue;

                float distance = Vector2.Distance(truePosition, component.TruePosition);

                if (!(distance < closestDistance)) continue;

                result = component;
                closestDistance = distance;
            }

            if (result && targetingPlayer.isRole(RoleId.Undertaker))
                SetDeadBodyOutline(result, Undertaker.color);

            return result;
        }

        public static void HandleUndertakerDropOnBodyReport()
        {
            if (Undertaker.allPlayers.Count == 0) return;
            var position = Undertaker.DraggedBody != null
                ? Undertaker.DraggedBody.transform.position
                : Vector3.zero;
            Undertaker.DropBody.LocalInvoke(position);
        }

        public static byte[] GetUnstrippedData(this DownloadHandler dh)
        {
            var nativeData = dh.GetNativeData();
            if (nativeData.IsCreated)
                return nativeData.ToArray();
            return null;
        }
    }
}
