using System;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    private int _aliveCount;
    private Action _onAllEnemiesDead;

    public void Initialize(PlayerBehaviour player, Action onAllEnemiesDead)
    {
        _onAllEnemiesDead = onAllEnemiesDead;

        var enemies = FindObjectsByType<EnemyBehaviour>(FindObjectsSortMode.None);
        _aliveCount = enemies.Length;

        foreach (var enemy in enemies)
            enemy.Initialize(player.transform, OnEnemyDead);
    }

    public void Tick(float deltaTime) { }

    private void OnEnemyDead()
    {
        _aliveCount--;
        if (_aliveCount <= 0)
            _onAllEnemiesDead?.Invoke();
    }
}
