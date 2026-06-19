using System;
using UnityEngine;

namespace MCI.Embedded.ReactorImGui;

public class Window
{
    private static int _lastWindowId;
    public static int NextWindowId()
    {
        return _lastWindowId++;
    }
    public int Id { get; set; } = NextWindowId();
    public bool Enabled { get; set; } = true;
    public Rect Rect { get; set; }
    public Action<int> Func { get; set; }
    public string Title { get; set; }
    public Window(Rect rect, string title, Action<int> func)
    {
        Rect = rect;
        Title = title;
        Func = func;
    }
    public Window(Rect rect, string title, Action func) : this(rect, title, _ => func())
    {
    }
    public virtual void OnGUI()
    {
        if (Enabled)
        {
            if (Event.current.type == EventType.Layout)
            {
                Rect = Rect.ResetSize();
            }

            GUI.skin.label.wordWrap = false;
            Rect = GUILayout.Window(Id, Rect, Func, Title, GUILayout.MinWidth(GUI.skin.label.CalcSize(new GUIContent(Title)).x * 2));

            if ((Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1)) && Rect.Contains(new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y)))
            {
                Input.ResetInputAxes();
            }
        }
    }
}