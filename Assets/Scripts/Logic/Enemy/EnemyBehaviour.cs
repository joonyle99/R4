using System;
using UnityEngine;

public abstract class EnemyBehaviour : CombatEntity
{
    public void Initialize(Transform playerTransform, Action onDead)
    {
        this.onDead = onDead;
        OnInitialize(playerTransform);
    }

    protected virtual void OnInitialize(Transform playerTransform) { }
}
