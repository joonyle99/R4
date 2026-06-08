using System;
using UnityEngine;

public abstract class CombatEntity : MonoBehaviour
{
    [SerializeField] protected int maxHp;
    [SerializeField] protected int attack;

    public int MaxHp => maxHp;
    public int Attack => attack;

    protected int currHp;
    public int CurrHp => currHp;
    public bool IsDead => currHp <= 0;

    protected Action onDead;

    protected virtual void Awake()
    {
        currHp = maxHp;
    }

    public virtual void TakeDamage(int damage)
    {
        if (IsDead) return;
        currHp -= damage;
        if (IsDead) OnDied();
    }

    protected virtual void OnDied()
    {
        onDead?.Invoke();
        HandleDead();
    }

    protected virtual void HandleDead()
    {
        Destroy(gameObject);
    }
}
