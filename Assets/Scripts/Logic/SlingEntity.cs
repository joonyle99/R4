using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SlingBehaviour))]
public abstract class SlingEntity : CombatEntity
{
    public Rigidbody2D Rigid { get; protected set; }
    public Animator Animator { get; protected set; }
    public SlingBehaviour SlingBehaviour { get; protected set; }

    public Peg OccupyingPeg { get; protected set; }

    private bool _isSnapping;

    private Action<Peg> _onOccupy;
    private Action<EnemyBehaviour> _onHit;
    private Action<EnemyBehaviour> _onKill;

    protected void InitSingleEntity(Action<int> onDamaged, Action onDead, Action<Peg> onOccupy)
    {
        InitCombatEntity(onDamaged, onDead);

        _onOccupy = onOccupy;

        Rigid = GetComponentInChildren<Rigidbody2D>();
        Animator = GetComponentInChildren<Animator>();
        SlingBehaviour = GetComponentInChildren<SlingBehaviour>();
        SlingBehaviour.Initialize(Rigid);
    }

    // ========= ... =========

    private void Occupy(Peg peg)
    {
        if (peg == OccupyingPeg) return;

        _isSnapping = true;

        OccupyingPeg = peg;
        OnOccupy(peg);
    }

    protected virtual void OnOccupy(Peg peg)
    {
        _onOccupy?.Invoke(peg);
    }

    private void Hit(EnemyBehaviour enemy)
    {
        OnHit(enemy);
    }

    protected virtual void OnHit(EnemyBehaviour enemy)
    {
        _onHit?.Invoke(enemy);
    }

    private void Kill(EnemyBehaviour enemy)
    {
        OnKill(enemy);
    }

    protected virtual void OnKill(EnemyBehaviour enemy)
    {
        _onKill?.Invoke(enemy);
    }

    // ========= ... =========

    protected override void OnDamaged(int damage)
    {
        base.OnDamaged(damage);
    }

    protected override void OnDead()
    {
        base.OnDead();

        OccupyingPeg?.TryVacate(this);
    }

    // ============ ... ============

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.TryGetComponent<Peg>(out var peg))
        {
            HandlePegTriggerEnter(peg);
        }
        else if (collider.TryGetComponent<EnemyBehaviour>(out var enemy))
        {
            HandleEnemyTriggerEnter(enemy);
        }
    }

    private void OnTriggerStay2D(Collider2D collider)
    {
        if (collider.TryGetComponent<Peg>(out var peg))
        {
            HandlePegTriggerStay(peg);
        }
    }

    private void OnTriggerExit2D(Collider2D collider)
    {
        if (collider.TryGetComponent<Peg>(out var peg))
        {
            HandlePegTriggerExit(peg);
        }
    }

    // ========= ... =========

    private void HandlePegTriggerEnter(Peg peg)
    {
        if (peg.TryOccupy(this) && peg != OccupyingPeg)
            Occupy(peg);
    }

    private void HandleEnemyTriggerEnter(EnemyBehaviour enemy)
    {
        if (!enemy.IsDead)
        {
            enemy.TakeDamage(attack);
            Hit(enemy);
            if (enemy.IsDead)
                Kill(enemy);
        }
    }

    // ========= ... =========

    private void HandlePegTriggerStay(Peg peg)
    {
        if (peg == OccupyingPeg && _isSnapping)
        {
            var dist = SlingBehaviour.GetPosition() - (Vector2)peg.transform.position;
            var sqrDist = dist.sqrMagnitude;
            if (sqrDist < 0.01f)
            {
                _isSnapping = false;
                return;
            }

            SlingBehaviour.Magnet(peg.transform.position);
        }
    }

    // ========= ... =========

    private void HandlePegTriggerExit(Peg peg)
    {
        if (peg.TryVacate(this) && peg == OccupyingPeg)
            OccupyingPeg = null;
    }
}
