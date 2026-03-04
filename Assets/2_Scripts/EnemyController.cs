using UnityEngine;
using UnityEngine.Pool;

public class EnemyController : MonoBehaviour
{
    private Transform _targetPlayer;
    private IObjectPool<GameObject> _managedPool;

    private EnemyStat _stat;

    private float _currentHp;

    // 스폰 매니저가 나를 소환할 때, 필요한 정보(타겟, 스탯, 풀)를 꽂아줍니다 (의존성 주입)
    public void Setup(Transform target, EnemyStat stat, IObjectPool<GameObject> pool)
    {
        _targetPlayer = target;
        _stat = stat;
        _managedPool = pool;

        _currentHp = _stat.hp;
    }

    private void Update()
    {
        if (_targetPlayer == null) return;

        Vector3 direction = (_targetPlayer.position - transform.position).normalized;
        transform.position += direction * (_stat.speed * Time.deltaTime);
    }

    public void TakeDamage(float damage)
    {
        _currentHp -= damage;
        if (_currentHp <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        if (_managedPool != null)
        {
            _managedPool.Release(gameObject);
        }
    }
}