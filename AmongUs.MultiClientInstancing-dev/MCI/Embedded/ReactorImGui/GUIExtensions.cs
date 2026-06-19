using UnityEngine;

namespace MCI.Embedded.ReactorImGui;

public static class GUIExtensions
{
    public static Rect ClampScreen(this Rect rect)
    {
        rect.x = Mathf.Clamp(rect.x, 0, Screen.width - rect.width);
        rect.y = Mathf.Clamp(rect.y, 0, Screen.height - rect.height);

        return rect;
    }
    public static Rect ResetSize(this Rect rect)
    {
        rect.width = rect.height = 0;

        return rect;
    }
    public static Sprite CreateSprite(this Texture2D texture, Vector2? pivot = null, float pixelsPerUnit = 100f)
    {
        return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), pivot ?? new Vector2(0.5f, 0.5f), pixelsPerUnit);
    }
    public static void SetSize(this RectTransform rectTransform, float width, float height)
    {
        rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
        rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
    }
}
