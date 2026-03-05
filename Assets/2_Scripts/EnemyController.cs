using System;
using UnityEngine;
using UnityEngine.Pool;

public class EnemyController : MonoBehaviour, IFighter
{
    public Collider2D MainCollider => _collider2D;
    private Collider2D _collider2D;
    private Transform _targetPlayer;
    private EnemyStat _stat;
    private float _currentHp;
    private Action<EnemyController> _onDeathCallback;

    // 스폰 매니저가 나를 소환할 때, 필요한 정보(타겟, 스탯, 풀)를 꽂아줍니다 (의존성 주입)
    public void Setup(Transform target, EnemyStat stat, Action<EnemyController> onDeathCallback)
    {
        _targetPlayer = target;
        _stat = stat;
        _onDeathCallback = onDeathCallback;
        _collider2D = GetComponent<Collider2D>();
        _currentHp = _stat.hp;
    }

    private void Update()
    {
        if (_targetPlayer == null) return;

        Vector3 direction = (_targetPlayer.position - transform.position).normalized;
        transform.position += direction * (_stat.speed * Time.deltaTime);
    }

    private void Die()
    {
        _onDeathCallback?.Invoke(this);
    }


    public void TakeDamage(InGameEvent combatEvent)
    {
        _currentHp -= combatEvent.Amount;
        if (_currentHp <= 0)
        {
            Die();
        }
    }

    public void Heal(InGameEvent healthEvent)
    {
        
    }
}