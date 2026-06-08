using UnityEngine;
using JoonyleGameDevKit;

public sealed class PlayerAttachedState : StateBase<PlayerBehaviour>
{
    public override void Enter(PlayerBehaviour owner)
    {
        owner.SlingBehaviour.SetAttachedPhysics();
    }

    public override void Exit(PlayerBehaviour owner) { }

    public override void Update(PlayerBehaviour owner)
    {
        var pointerInput = owner.PointerInput;
        var pointerWorldPos = pointerInput.GetWorldPos;
        if (pointerInput.IsDragging
            && owner.SlingBehaviour.IsActiveSling
            && owner.SlingBehaviour.IsValidAiming(pointerWorldPos))
        {
            owner.ChangeState<PlayerAimingState>();
        }
    }
}
