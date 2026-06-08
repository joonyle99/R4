using System;
using UnityEngine;

public abstract class CombatEntity : MonoBehaviour
{
    [SerializeField] protected int maxHp;
    public int MaxHp => maxHp;
    [SerializeField] protected int attack;
    public int Attack => attack;

    protected int currHp;
    public int CurrHp => currHp;

    public bool IsDead => currHp <= 0;

    private Action<int> _onDamaged;
    private Action _onDead;

    protected void InitCombatEntity(Action<int> onDamaged, Action onDead)
    {
        _onDamaged = onDamaged;
        _onDead = onDead;

        currHp = maxHp;
    }

    // ========= ... =========

    public virtual void TakeDamage(int damage)
    {
        if (IsDead) return;
        currHp -= damage;
        OnDamaged(damage);
        if (IsDead) OnDead();
    }

    // ========= ... =========

    protected virtual void OnDamaged(int damage)
    {
        _onDamaged?.Invoke(damage);
    }

    protected virtual void OnDead()
    {
        _onDead?.Invoke();

        Destroy(gameObject);
    }
}
