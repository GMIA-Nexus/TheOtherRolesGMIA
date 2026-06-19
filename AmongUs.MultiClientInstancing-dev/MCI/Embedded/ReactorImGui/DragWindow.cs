using System;
using UnityEngine;

namespace MCI.Embedded.ReactorImGui;

public class DragWindow : Window
{
    public DragWindow(Rect rect, string title, Action<int> func) : base(rect, title, func)
    {
        Func = id =>
        {
            func(id);

            GUI.DragWindow(new Rect(0, 0, 10000, 20));
            Rect = Rect.ClampScreen();
        };
    }
    public DragWindow(Rect rect, string title, Action func) : this(rect, title, _ => func())
    {
    }
}
