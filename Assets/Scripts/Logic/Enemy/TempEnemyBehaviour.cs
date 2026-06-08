using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class TempEnemyBehaviour : EnemyBehaviour
{
    [SerializeField] private float _chaseRange = 5f;
    [SerializeField] private float _chaseSpeed = 2f;
    [SerializeField] private float _contactDamageCooldown = 1f;

    private Rigidbody2D _rigid;
    private Transform _playerTransform;
    private float _lastDamageTime = float.MinValue;

    protected override void Awake()
    {
        base.Awake();
        _rigid = GetComponent<Rigidbody2D>();
        _rigid.gravityScale = 0f;
    }

    protected override void OnInitialize(Transform playerTransform)
    {
        _playerTransform = playerTransform;
    }

    private void FixedUpdate()
    {
        if (_playerTransform == null) return;

        var toPlayer = (Vector2)(_playerTransform.position - transform.position);
        _rigid.linearVelocity = toPlayer.sqrMagnitude <= _chaseRange * _chaseRange
            ? toPlayer.normalized * _chaseSpeed
            : Vector2.zero;
    }

    private void OnTriggerStay2D(Collider2D collider)
    {
        if (Time.time < _lastDamageTime + _contactDamageCooldown) return;
        if (!collider.TryGetComponent<SlingEntity>(out var entity)) return;

        entity.TakeDamage(attack);
        _lastDamageTime = Time.time;
    }
}
