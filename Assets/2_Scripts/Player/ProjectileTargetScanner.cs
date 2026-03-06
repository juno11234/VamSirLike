using UnityEngine;

public class ProjectileTargetScanner : MonoBehaviour
{
    [Header("Weapon Settings")]
    [SerializeField] private ProjectileAttack projectilePrefab;
    [SerializeField] private float scanRadius = 10f; // 적 탐색 범위
    [SerializeField] private float cooldown = 1f;    // 발사 쿨타임
    [SerializeField] private float damage = 10f;     // 임시 데미지
    [SerializeField] private LayerMask enemyLayer;
    
    private CombatSystem _combatSystem;
    private float _timer;

    private Collider2D[] _results = new Collider2D[50];
    private ContactFilter2D _filter;
    
    public void Init(CombatSystem combatSystem, IFighter sender)
    {
        _combatSystem = combatSystem;
        _filter = new ContactFilter2D();
        _filter.SetLayerMask(enemyLayer);
        _filter.useLayerMask = true;
        _filter.useTriggers = true;
    
    }
    
    private void Update()
    {
        _timer += Time.deltaTime;
        if (_timer >= cooldown)
        {
            Fire();
            _timer = 0f;
        }
    }
    private void Fire()
    {
        Transform target = FindClosestEnemy();
        if (target == null) return; // 범위 내에 적이 없으면 발사 안 함

        // 방향 벡터 계산
        Vector2 direction = (target.position - transform.position).normalized;

        // 투사체 생성 (우선 Instantiate 사용. 추후 ObjectPool 연동 권장)
        ProjectileAttack projectile = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
        
        // 투사체 초기화
        projectile.Init(_combatSystem, direction, damage);
    }
    // [핵심] 가장 가까운 적 찾기
    private Transform FindClosestEnemy()
    {
        int hitCount = Physics2D.OverlapCircle(transform.position, scanRadius, _filter, _results);
        if (hitCount == 0) return null;

        Transform closestTarget = null;
        float minDistanceSqr = Mathf.Infinity;

        for (int i = 0; i < hitCount; i++)
        {
            // 거리 계산 최적화: 루트 연산 없이 제곱(sqrMagnitude)으로만 비교
            float distSqr = (transform.position - _results[i].transform.position).sqrMagnitude;
            if (distSqr < minDistanceSqr)
            {
                minDistanceSqr = distSqr;
                closestTarget = _results[i].transform;
            }
        }

        return closestTarget;
    }
}
