using JoonyleGameDevKit;

public sealed class PlayerBoostingState : StateBase<PlayerBehaviour>
{
    public override void Enter(PlayerBehaviour owner)
    {
        owner.SlingBehaviour.SetFlyingPhysics();
    }

    public override void Exit(PlayerBehaviour owner) { }

    public override void Update(PlayerBehaviour owner)
    {
        if (owner.Rigid.linearVelocity.y <= 0f)
            owner.ChangeState<PlayerFallingState>();
    }
}
