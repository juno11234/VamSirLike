using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class EnemyController : MonoBehaviour, IFighter
{
    [Header("Attack Settings")]
    [SerializeField] private float attackRange = 0.8f;
    [SerializeField] private float attackCooldown = 1f;
    [SerializeField] private float flashDuration = 0.1f;

    private float _attackTimer;
    public Collider2D MainCollider => _collider2D;
    private Collider2D _collider2D;

    private SpriteRenderer _spriteRenderer;
    private CombatSystem _combatSystem;
    private PlayerController _targetPlayer;

    private EnemyStat _stat;
    private float _currentHp;
    private Action<EnemyController> _onDeathCallback;

    private bool _isDead = false;

    private void Awake()
    {
        _collider2D = GetComponent<Collider2D>();
        _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    public void Setup(PlayerController target, EnemyStat stat, Action<EnemyController> onDeathCallback,
        CombatSystem combatSystem)
    {
        _targetPlayer = target;
        _stat = stat;
        _onDeathCallback = onDeathCallback;
        _currentHp = _stat.hp;
        _combatSystem = combatSystem;
        _attackTimer = attackCooldown;

        _isDead = false;

        ResetFlashEffect();
    }

    private void Update()
    {
        if (_targetPlayer == null) return;
        _attackTimer += Time.deltaTime;
        Vector3 offset = _targetPlayer.transform.position - transform.position;
        float distanceSqr = offset.sqrMagnitude;

        if (distanceSqr <= attackRange * attackRange)
        {
            if (_attackTimer >= attackCooldown)
            {
                AttackPlayer();
                _attackTimer = 0f;
            }
        }
        else
        {
            Vector3 direction = offset.normalized;
            transform.position += direction * (_stat.speed * Time.deltaTime);
        }
    }

    private void AttackPlayer()
    {
        InGameEvent evt = new InGameEvent
        {
            Type = InGameEvent.EventType.Combat,
            Sender = this,
            Receiver = _targetPlayer,
            Amount = _stat.atk
        };

        _combatSystem.AddInGameEvent(evt);
    }

    private void Die()
    {
        _onDeathCallback?.Invoke(this);
    }

    public void TakeDamage(InGameEvent combatEvent)
    {
        if (_isDead) return;

        _currentHp -= combatEvent.Amount;
        if (_currentHp <= 0)
        {
            _isDead = true;
            Die();
        }
        else
        {
            HitRoutineAsync().Forget();
        }
    }

    private async UniTaskVoid HitRoutineAsync()
    {
        _spriteRenderer.material.SetFloat("_Amount", 0.4f);

        await UniTask.Delay(TimeSpan.FromSeconds(flashDuration));

        if (this == null || gameObject.activeInHierarchy == false) return;
        ResetFlashEffect();
    }

    private void ResetFlashEffect()
    {
        _spriteRenderer.material.SetFloat("_Amount", 0f);
    }

    public void Heal(InGameEvent healthEvent)
    {
    }
}
