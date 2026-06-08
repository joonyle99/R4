using System;
using UnityEngine;
using JoonyleGameDevKit;
using System.Collections;
using System.Collections.Generic;

public sealed class PlayerBehaviour : SlingEntity, IBoundaryHitHandler
{
    public CameraController CameraController { get; private set; }
    public IPointerInput PointerInput { get; private set; }

    [SerializeField] private Transform _centerPoint;
    public Transform CenterPoint => _centerPoint;
    [SerializeField] private Transform _handPoint;
    public Transform HandPoint => _handPoint;
    [SerializeField] private Transform _footPoint;
    public Transform FootPoint => _footPoint;

    [Space]

    [SerializeField] private float _hitStopDuration = 0.1f;
    [SerializeField] private float _fallingMoveSpeed = 3f;
    public float FallingMoveSpeed => _fallingMoveSpeed;
    [SerializeField] private float _fallingJoystickRange = 150f;
    public float FallingJoystickRange => _fallingJoystickRange;

    private Dictionary<Type, Transform> _stateAnchorMap = new();
    private StateMachine<PlayerBehaviour> _fsm;

    public Transform CurrAnchorPoint => _stateAnchorMap.TryGetValue(_fsm.CurrState.GetType(), out var point) ? point : transform;

    public void Initialize(CameraController cameraController, IPointerInput pointerInput, Action<int> onDamaged, Action onDead, Action<Peg> onOccupy)
    {
        InitSingleEntity(onDamaged, onDead, onOccupy);

        CameraController = cameraController;
        PointerInput = pointerInput;

        _stateAnchorMap = new Dictionary<Type, Transform>
        {
            { typeof(PlayerAttachedState), _handPoint },
            { typeof(PlayerAimingState), _handPoint },
            { typeof(PlayerBoostingState), _centerPoint },
            { typeof(PlayerFallingState), _centerPoint },
        };

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

    public Vector2 GetPosition()
    {
        return transform.position;
    }
    
    public Vector2 GetAnchorPosition()
    {
        var anchor = CurrAnchorPoint;
        return anchor.position;
    }

    public void SetPosition(Vector2 targetPos)
    {
        var anchor = CurrAnchorPoint;
        var offset = (Vector2)(anchor.position - transform.position);
        Rigid.position = targetPos - offset;
    }

    // ============ ... ============

    protected override void OnOccupy(Peg peg)
    {
        base.OnOccupy(peg);

        _fsm.ChangeState<PlayerAttachedState>();
    }

    protected override void OnKill(EnemyBehaviour enemy)
    {
        base.OnKill(enemy);

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
        if (OccupyingPeg == null) return;

        Rigid.linearVelocity = Vector2.zero;
        SetPosition(OccupyingPeg.transform.position);

        OccupyingPeg.TryOccupy(this);

        _fsm.ChangeState<PlayerAttachedState>();

        // _onOccupy?.Invoke(OccupyingPeg);
    }
}
