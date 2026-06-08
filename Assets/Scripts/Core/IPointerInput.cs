using System;
using UnityEngine;

public interface IPointerInput
{
    bool JustPressed { get; }
    bool JustReleased { get; }
    bool JustTapped { get; }
    bool IsDragging { get; }

    Vector2 GetScreenPos { get; }
    Vector2 GetScreenPosDelta { get; }
    Vector2 GetScreenDragDelta { get; }
    Vector2 GetWorldPos { get; }
    Vector2 GetWorldPosDelta { get; }

    event Action<Vector2> OnPress;
    event Action<Vector2> OnRelease;
    event Action<Vector2> OnTap;
    event Action<Vector2> OnDragStart;
    event Action<Vector2> OnDrag;
    event Action<Vector2> OnDragEnd;

    void Tick(float deltaTime);
}
