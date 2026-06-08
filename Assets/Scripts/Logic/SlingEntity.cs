using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SlingBehaviour))]
public abstract class SlingEntity : CombatEntity
{
    public Rigidbody2D Rigid { get; protected set; }
    public SlingBehaviour SlingBehaviour { get; protected set; }
    public Peg OccupyingPeg { get; protected set; } // 점유 중인 말뚝

    private bool _isSnapping;

    protected Action<Peg> onOccupy;

    protected abstract bool isAttached { get; }
    protected abstract bool isFlying { get; }

    protected void InitializeSingleEntity(Action<Peg> onOccupy, Action onDead)
    {
        Rigid = GetComponent<Rigidbody2D>();

        SlingBehaviour = GetComponent<SlingBehaviour>();
        SlingBehaviour.Initialize(Rigid);

        this.onOccupy = onOccupy;
        this.onDead = onDead;

    }

    // ========= ... =========

    private void Occupy(Peg peg)
    {
        if (!isFlying || peg == OccupyingPeg) return;

        _isSnapping = true;

        OccupyingPeg = peg;
        OnOccupy(peg);
    }

    private void Occupying(Peg peg)
    {
        if (isFlying || peg != OccupyingPeg || !_isSnapping) return;

        var sqrDist = ((Vector2)(transform.position - peg.transform.position)).sqrMagnitude;
        if (sqrDist < 0.01f) _isSnapping = false;

        SlingBehaviour.Magnet(peg.transform.position);
    }

    // ========= ... =========

    protected override void HandleDead()
    {
        OccupyingPeg?.TryVacate(this);
        base.HandleDead();
    }

    // ========= ... =========

    protected abstract void OnOccupy(Peg peg);
    protected virtual void OnKill() { }

    // ============ 충돌 디스패치 ============

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.TryGetComponent<Peg>(out var peg))
        {
            HandlePegCollision(peg);
            return;
        }

        if (collider.TryGetComponent<EnemyBehaviour>(out var enemy))
        {
            HandleEnemyCollision(enemy);
        }
    }

    private void HandlePegCollision(Peg peg)
    {
        if (peg == OccupyingPeg) return;
        if (!isFlying) return;

        if (peg.TryOccupy(this))
        {
            Occupy(peg);
        }
        else
        {
            var occupant = peg.CurrSlingEntity;
            occupant.TakeDamage(attack);

            if (occupant.IsDead && peg.TryOccupy(this))
            {
                OnKill();
                Occupy(peg);
            }
        }
    }

    private void HandleEnemyCollision(EnemyBehaviour enemy)
    {
        if (!isFlying) return;

        enemy.TakeDamage(attack);
        if (enemy.IsDead)
            OnKill();
    }

    private void OnTriggerStay2D(Collider2D collider)
    {
        if (!collider.TryGetComponent<Peg>(out var peg)) return;

        Occupying(peg);
    }

    private void OnTriggerExit2D(Collider2D collider)
    {
        if (!collider.TryGetComponent<Peg>(out var peg)) return;

        peg.TryVacate(this);
    }
}
