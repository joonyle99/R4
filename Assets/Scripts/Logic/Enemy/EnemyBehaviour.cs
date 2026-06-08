using System;
using UnityEngine;

public abstract class EnemyBehaviour : CombatEntity
{
    public void InitEnemyBehaviour(Action<int> onDamaged, Action onDead)
    {
        InitCombatEntity(onDamaged, onDead);
    }
}
