using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Rendering;
using UnityEngine;
using System.Collections;

namespace TheOtherRoles.MetaContext
{
    public interface Hint
    {
        public GUIContextSupplier GUI { get; }
    }

    public class HintWithImage : Hint
    {
        private Image Image { get; init; }
        private TextComponent Title { get; init; }
        private TextComponent Detail { get; init; }

        GUIContextSupplier Hint.GUI => () =>
        {
            return TORGUIContextEngine.API.VerticalHolder(GUIAlignment.Center,
                TORGUIContextEngine.API.Margin(new(null, 0.5f)),
                TORGUIContextEngine.API.Image(GUIAlignment.Center, Image, new(4f, 2.1f)),
                TORGUIContextEngine.API.Margin(new(null, 0.6f)),
                TORGUIContextEngine.API.Text(GUIAlignment.Left, TORGUIContextEngine.API.GetAttribute(AttributeAsset.DocumentTitle), Title),
                TORGUIContextEngine.API.Margin(new(null, 0.1f)),
                TORGUIContextEngine.API.HorizontalHolder(GUIAlignment.Center, TORGUIContextEngine.API.HorizontalMargin(0.16f), TORGUIContextEngine.API.Text(GUIAlignment.Left, TORGUIContextEngine.API.GetAttribute(AttributeAsset.DocumentStandard), Detail))
                );
        };

        public HintWithImage(Image image, TextComponent title, TextComponent detail)
        {
            Image = image;
            Title = title;
            Detail = detail;
        }
    }

    public class HintManager
    {
        internal static List<Hint> AllHints = new();

        static private Hint WithImage(string id) => new HintWithImage(SpriteLoader.FromResource("TheOtherRoles.Resources.Hints." + id.HeadUpper() + ".png", 100f), new TranslateTextComponent("hint" + id.HeadUpper() + "Title"), new TranslateTextComponent("hint" + id.HeadUpper() + "Detail"));
        static HintManager()
        {
            RegisterHint(WithImage("HelpInGame"));
            RegisterHint(WithImage("Busker"));
            RegisterHint(WithImage("Sherlock"));
            RegisterHint(WithImage("Achievement"));
            RegisterHint(WithImage("FreePlay"));
            RegisterHint(WithImage("AboutSection"));
        }

        public static void RegisterHint(Hint hint) => AllHints.Add(hint);

        public static IEnumerator CoShowHint(float delay = 0.5f)
        {
            yield return Effects.Wait(delay);

            var overlay = GameObject.Instantiate(TransitionFade.Instance.overlay, (Transform)null);
            overlay.transform.position = TransitionFade.Instance.overlay.transform.position + new Vector3(0, 0, -100f);
            overlay.color = Color.black;
            overlay.gameObject.layer = LayerMask.NameToLayer("UI");
            overlay.gameObject.AddComponent<SortingGroup>().sortingOrder = 150;

            var mask = Helpers.CreateObject<SpriteMask>("Mask", overlay.transform, new Vector3(0, 0, 5f));
            mask.sprite = overlay.sprite;
            mask.transform.localScale = overlay.size;

            var hint = AllHints[System.Random.Shared.Next(AllHints.Count)].GUI.Invoke().Instantiate(new(new(0.5f, 0.5f), new(0f, 0f, 0f)), new(6f, 4f), out _);
            hint?.transform.SetParent(overlay.transform);
            if (hint) hint!.transform.localPosition = new(0f, 0f, 10f);

            yield return Effects.ColorFade(overlay, Color.black, Color.clear, 0.5f);
        }

    }
}
