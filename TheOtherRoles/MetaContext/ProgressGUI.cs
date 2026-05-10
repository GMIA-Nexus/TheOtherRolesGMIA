using System;
using System.Collections.Generic;
using System.Linq;
using GUIContextEngine = TheOtherRoles.MetaContext.TORGUIContextEngine;

namespace TheOtherRoles.MetaContext;

static public class ProgressGUI
{
    public record OneLineTextElement(float? Margin, string Text, (Func<string> generator, int length)? Generator)
    {
        public static implicit operator OneLineTextElement(float margin) => new(margin, null, null);
        public static implicit operator OneLineTextElement(string text) => new(null, text, null);
        public static implicit operator OneLineTextElement((Func<string> generator, int length) text) => new(null, null, text);
    }

    static private GUIContext UpdatableText(Func<string> text, int length) => GUIContextEngine.API.RealtimeText(GUIAlignment.Left, GUIContextEngine.API.GetAttribute(AttributeAsset.OverlayContent), text, length);
    static public GUIContext RawText(string text) => GUIContextEngine.API.RawText(GUIAlignment.Left, GUIContextEngine.API.GetAttribute(AttributeAsset.OverlayContent), text);
    static public GUIContext RawText(string text, GUIAlignment alignment) => GUIContextEngine.API.RawText(alignment, GUIContextEngine.API.GetAttribute(AttributeAsset.OverlayContent), text);
    static public GUIContext OneLineText(params IEnumerable<OneLineTextElement> elements) => GUIContextEngine.API.HorizontalHolder(GUIAlignment.Left, elements.Select(e => e.Margin.HasValue ? GUIContextEngine.API.HorizontalMargin(e.Margin.Value) : e.Text != null ? RawText(e.Text) : UpdatableText(e.Generator!.Value.generator, e.Generator.Value.length)));
    static public GUIContext AssignableNameText(RoleInfo assignable) => GUIContextEngine.API.RawText(GUIAlignment.Left, GUIContextEngine.API.GetAttribute(AttributeAsset.OverlayTitle), Helpers.cs(assignable.color, assignable.name));
    static public GUIContext SmallAssignableNameText(RoleInfo assignable, string prefix = null) => GUIContextEngine.API.RawText(GUIAlignment.Left, GUIContextEngine.API.GetAttribute(AttributeAsset.OverlayContent), (prefix ?? "") + "<b>" + Helpers.cs(assignable.color, assignable.name) + "</b>");
    static public GUIContext Holder(params IEnumerable<GUIContext> widgets) => GUIContextEngine.API.VerticalHolder(GUIAlignment.Left, widgets.Where(w => w != null));
}
