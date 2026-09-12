using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using TheOtherRoles.MetaContext;
using TheOtherRoles.Modules;
using TMPro;
using UnityEngine;
using static TheOtherRoles.TheOtherRoles;

namespace TheOtherRoles.Roles
{
    [TORRPCHolder]
    public class Mafioso : RoleBase<Mafioso>
    {
        public static Color color = Palette.ImpostorRed;
        public static PlayerVoteArea skipMeeting = null;
        private static List<Sprite> chainSprites = [];
        private static int animIndex = 0;
        public static List<(SpriteRenderer renderer, float timer, int frameIndex)> ChainObjects = [];
        private const int CHAIN_SPRITE_COUNT = 15;
        private const int CHAIN_OBJECT_COUNT = 11;
        public static int numUses = 1;

        public Mafioso()
        {
            RoleId = roleId = RoleId.Mafioso;
        }

        public static IEnumerable<DocumentReplacement> GetReplacementPart() => [new("%NUM%", CustomOptionHolder.mafiosoNumberOfSkips.getFloat().ToString())];

        public static void UpdateButton(MeetingHud __instance)
        {
            var skip = __instance.SkipVoteButton;
            skipMeeting?.gameObject.SetActive(!PlayerControl.LocalPlayer.Data.IsDead && !skip.VoteComplete && numUses > 0);
            skipMeeting?.VoteComplete = skip.VoteComplete;
            skipMeeting?.GetComponent<SpriteRenderer>().enabled = skip.GetComponent<SpriteRenderer>().enabled;
            skipMeeting?.GetComponentsInChildren<TextMeshPro>()[0].text = ModTranslation.getString("mafiosoForceSkip");
        }

        public static void GenButton(MeetingHud __instance)
        {
            var skip = __instance.SkipVoteButton;
            skipMeeting = Object.Instantiate(skip, skip.transform.parent);
            skipMeeting.Parent = __instance;
            skipMeeting.SetPlayerId(251);
            skipMeeting.transform.localPosition = skip.transform.localPosition + new Vector3(0f, -0.17f, 0f);
            skip.transform.localPosition += new Vector3(0f, 0.20f, 0f);
            UpdateButton(__instance);


            if (ChainObjects != null)
            {
                foreach (var chainObj in ChainObjects)
                {
                    if (chainObj.renderer != null)
                        GameObject.Destroy(chainObj.renderer.gameObject);
                }
                ChainObjects?.Clear();
            }
        }

        public static void UpdateTimer(MeetingHud __instance)
        {
            switch (__instance.state)
            {
                case MeetingHud.MeetingStates.Discussion:
                    if (__instance.discussionTimer < GameOptionsManager.Instance.currentNormalGameOptions.DiscussionTime)
                    {
                        skipMeeting.SetDisabled();
                        break;
                    }

                    skipMeeting.SetEnabled();
                    break;
            }

            UpdateButton(__instance);
        }

        private static void CreateChainObjects()
        {
            ChainObjects = new();

            for (int i = 0; i < CHAIN_OBJECT_COUNT; i++)
            {
                ChainObjects.Add((CreateChainRenderer(UnityEngine.Random.Range(1.8f, -1.7f), UnityEngine.Random.Range(-15f, 15f)), 0f, 0));
            }
            ChainObjects.Add((CreateChainRenderer(0, 0, -12f), 0f, 0));
        }

        private static void InitializeChainSprites()
        {
            chainSprites.Clear();
            Sprite emptySprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 1, 1), Vector2.zero);

            for (int i = 0; i < CHAIN_SPRITE_COUNT; i++)
            {
                var sprite = (i is 0 or 1 or 2) ? null : Helpers.loadSpriteFromResources($"TheOtherRoles.Resources.MeetingAnimation.average_anim_chain_0{i + 17}.png", 115f);
                chainSprites.Add(sprite ?? emptySprite);
            }
        }

        private static SpriteRenderer CreateChainRenderer(float y, float rotation, float z = 7)
        {
            SpriteRenderer renderer = new GameObject("ChainObject").AddComponent<SpriteRenderer>();
            renderer.transform.parent = MeetingHud.Instance.transform;
            renderer.gameObject.layer = 5;
            renderer.transform.localPosition = new(0, y, z);
            renderer.transform.localScale = new(2f, 1.7f, 2f);
            renderer.transform.Rotate(new(0, 0, rotation));
            return renderer;
        }

        public static RemoteProcess SkipMeeting = new("MafiosoSkipMeeting", (_) =>
        {
            if (MeetingHud.Instance == null) return;
            SoundEffectsManager.play("mafiosoMeeting");
            onSkipMeeting(MeetingHud.Instance);
        });

        public static void onSkipMeeting(MeetingHud __instance)
        {
            __instance.playerStates.ToList().ForEach(x => x.gameObject.SetActive(false));

            CreateChainObjects();

            animIndex = 0;

            TORGUIManager.Instance.StartCoroutine(UpdateChain(__instance).WrapToIl2Cpp());
        }

        static IEnumerator UpdateChain(MeetingHud __instance)
        {
            bool allChainsComplete = false;

            while (!allChainsComplete)
            {
                allChainsComplete = true;

                for (int i = 0; i <= animIndex && i < ChainObjects.Count; i++)
                {
                    var chainObj = ChainObjects[i];
                    if (chainObj.frameIndex < chainSprites.Count)
                    {
                        chainObj.renderer.sprite = chainSprites[chainObj.frameIndex];
                        ChainObjects[i] = (chainObj.renderer, chainObj.timer, chainObj.frameIndex + 1);
                        allChainsComplete = false;
                    }
                }

                if (animIndex + 1 < ChainObjects.Count)
                    animIndex++;

                yield return Effects.Wait(1 / 60f);
            }

            yield return Effects.Wait(0.7f);
            foreach (var objs in ChainObjects)
                objs.renderer.gameObject.SetActive(false);

            __instance.playerStates.ToList().ForEach(x => x.gameObject.SetActive(true));

            if (AmongUsClient.Instance.AmHost)
                __instance.RpcVotingComplete(System.Array.Empty<MeetingHud.VoterState>(), null, false, false, 0);

            yield break;
        }

        public static void clearAndReload()
        {
            skipMeeting = null;
            InitializeChainSprites();
            numUses = Mathf.RoundToInt(CustomOptionHolder.mafiosoNumberOfSkips.getFloat());
            players = [];
        }
    }
}
