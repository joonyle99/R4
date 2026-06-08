using System;
using UnityEngine;
using JoonyleGameDevKit;
using System.Collections;

public class PlayerBehaviour : SlingEntity, IBoundaryHitHandler
{
    public CameraController CameraController { get; private set; }
    public IPointerInput PointerInput { get; private set; }
    private StateMachine<PlayerBehaviour> _fsm;

    [SerializeField] private float _hitStopDuration = 0.1f;
    [SerializeField] private float _fallingMoveSpeed = 3f;
    [SerializeField] private float _fallingJoystickRange = 150f;

    public float FallingMoveSpeed => _fallingMoveSpeed;
    public float FallingJoystickRange => _fallingJoystickRange;

    private bool _killedEnemy;

    protected override bool isAttached => _fsm.CurrState is PlayerAttachedState;
    protected override bool isFlying => _fsm.CurrState is PlayerBoostingState or PlayerFallingState;

    public void Initialize(CameraController cameraController, IPointerInput pointerInput, Action<Peg> onOccupy, Action onDead)
    {
        CameraController = cameraController;
        PointerInput = pointerInput;

        InitializeSingleEntity(onOccupy, onDead);

        _fsm = new StateMachine<PlayerBehaviour>(this);
        _fsm.AddState(new PlayerAttachedState());
        _fsm.AddState(new PlayerAimingState());
        _fsm.AddState(new PlayerBoostingState());
        _fsm.AddState(new PlayerFallingState());
        _fsm.ChangeState<PlayerFallingState>();

        SlingBehaviour.SetActiveSling(true);
    }

    public void Tick(float deltaTime) => _fsm.Update(deltaTime);
    public void ChangeState<TState>() where TState : StateBase<PlayerBehaviour> => _fsm.ChangeState<TState>();

    // ============ ... ============

    protected override void OnOccupy(Peg peg)
    {
        _fsm.ChangeState<PlayerAttachedState>();

        if (_killedEnemy)
        {
            _killedEnemy = false;
            SlingBehaviour.SetActiveSling(true); // 보너스 행동
        }
        else
        {
            onOccupy?.Invoke(peg); // 일반 착지 → 턴 종료
        }
    }

    protected override void OnKill()
    {
        _killedEnemy = true;

        if (_fsm.CurrState is PlayerBoostingState)
        {
            StartCoroutine(HitStop());
        }
    }

    // ========= ... =========

    private IEnumerator HitStop()
    {
        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(_hitStopDuration);
        Time.timeScale = 1f;
    }

    // ========= ... =========

    public void OnBoundaryHit()
    {
        if (!isFlying) return;
        if (OccupyingPeg == null) return;

        Rigid.linearVelocity = Vector2.zero;
        transform.position = OccupyingPeg.transform.position;

        OccupyingPeg.TryOccupy(this);

        _fsm.ChangeState<PlayerAttachedState>();

        onOccupy?.Invoke(OccupyingPeg);
    }
}
