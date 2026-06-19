using System;
using System.Collections.Generic;
using UnityEngine;

namespace TheOtherRoles.Objects
{
    class AssassinTrace
    {
        public static List<AssassinTrace> traces = new();

        private GameObject trace;
        private float timeRemaining;

        private static Sprite TraceSprite;
        public static Sprite getTraceSprite()
        {
            if (TraceSprite) return TraceSprite;
            TraceSprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.AssassinTraceW.png", 225f);
            return TraceSprite;
        }

        public AssassinTrace(Vector2 p, float duration = 1f)
        {
            trace = new GameObject("AssassinTrace") { layer = 11 };
            trace.AddSubmergedComponent(SubmergedCompatibility.Classes.ElevatorMover);
            //Vector3 position = new Vector3(p.x, p.y, PlayerControl.LocalPlayer.transform.localPosition.z + 0.001f); // just behind player
            Vector3 position = new(p.x, p.y, p.y / 1000f + 0.01f);
            trace.transform.position = position;
            trace.transform.localPosition = position;

            var traceRenderer = trace.AddComponent<SpriteRenderer>();
            traceRenderer.sprite = getTraceSprite();

            timeRemaining = duration;

            // display the assassins color in the trace
            float colorDuration = CustomOptionHolder.assassinTraceColorTime.getFloat();
            FastDestroyableSingleton<HudManager>.Instance.StartCoroutine(Effects.Lerp(colorDuration, new Action<float>((p) =>
            {
                Color c = Palette.PlayerColors[(int)Assassin.assassin.Data.DefaultOutfit.ColorId];
                if (Helpers.isLighterColor(Assassin.assassin)) c = Color.white;
                else c = Palette.PlayerColors[6];
                //if (Camouflager.camouflageTimer > 0) {
                //    c = Palette.PlayerColors[6];
                //}

                Color g = Color.green; // Usual display color. could also be Palette.PlayerColors[6] for default grey like camo
                // if this stays black (0,0,0), it can ofc be removed.

                Color combinedColor = Mathf.Clamp01(p) * g + Mathf.Clamp01(1 - p) * c;

                if (traceRenderer) traceRenderer.color = combinedColor;
            })));

            float fadeOutDuration = 1f;
            if (fadeOutDuration > duration) fadeOutDuration = 0.5f * duration;
            FastDestroyableSingleton<HudManager>.Instance.StartCoroutine(Effects.Lerp(duration, new Action<float>((p) =>
            {
                float interP = 0f;
                if (p < (duration - fadeOutDuration) / duration)
                    interP = 0f;
                else interP = (p * duration + fadeOutDuration - duration) / fadeOutDuration;
                if (traceRenderer) traceRenderer.color = new Color(traceRenderer.color.r, traceRenderer.color.g, traceRenderer.color.b, Mathf.Clamp01(1 - interP));
            })));

            trace.SetActive(true);
            traces.Add(this);
        }

        public static void clearTraces()
        {
            traces = new List<AssassinTrace>();
        }

        public static void UpdateAll()
        {
            foreach (AssassinTrace traceCurrent in new List<AssassinTrace>(traces))
            {
                traceCurrent.timeRemaining -= Time.fixedDeltaTime;
                if (traceCurrent.timeRemaining < 0)
                {
                    traceCurrent.trace.SetActive(false);
                    UnityEngine.Object.Destroy(traceCurrent.trace);
                    traces.Remove(traceCurrent);
                }
            }
        }
    }
}
