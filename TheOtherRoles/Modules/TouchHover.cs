using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TheOtherRoles.Modules;

public class TouchHover : MonoBehaviour
{
    static TouchHover() => ClassInjector.RegisterTypeInIl2Cpp<TouchHover>();

    protected Camera mainCam;
    public Controller controller = new();

    private Collider2D _collider2D;
    private bool _isHovered;

    /// <summary>
    /// Whether the mouse or finger is on the object. Replacement of OnMouseOver
    /// </summary>
    public UnityEngine.Events.UnityEvent OnHoverOver = new();
    /// <summary>
    /// Whether the mouse or finger is out of the object. Replacement of OnMouseOut
    /// </summary>
    public UnityEngine.Events.UnityEvent OnHoverOut = new();

    public virtual void Awake()
    {
        if (!mainCam)
            mainCam = Camera.main;
        controller.mainCam = mainCam;
        _collider2D = GetComponent<Collider2D>();
    }

    protected virtual void Update()
    {
        controller.Update();

        if (controller.mainCam != mainCam)
            controller.mainCam = mainCam;

        bool currentlyHovered = controller.CheckHover(_collider2D);
        if (currentlyHovered && !_isHovered)
        {
            _isHovered = true;
            OnHoverOver?.Invoke();
        }
        else if (!currentlyHovered && _isHovered)
        {
            _isHovered = false;
            OnHoverOut?.Invoke();
        }
    }
}
