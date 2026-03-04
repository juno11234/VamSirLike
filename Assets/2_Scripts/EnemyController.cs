using UnityEngine;
using UnityEngine.Pool;

public class EnemyController : MonoBehaviour
{
    private Transform _targetPlayer;
    // 나를 관리하는 오브젝트 풀의 주소표를 가지고 있어야 반납할 수 있습니다.
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
        // 타겟이 없으면 움직이지 않음
        if (_targetPlayer == null) return;

        // 1. 방향 벡터 수학 공식: (목적지 - 출발지)의 정규화(normalized)
        Vector3 direction = (_targetPlayer.position - transform.position).normalized;

        // 2. 무거운 Rigidbody 물리 연산 없이, 순수 수학(transform)으로 가볍게 이동!
        transform.position += direction * (_stat.speed * Time.deltaTime);
        
    }

    public void TakeDamage(float damage)
    {
        _currentHp-=damage;
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