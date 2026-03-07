using UnityEngine;
using UnityEngine.Pool;

public class ProjectileTargetScanner : MonoBehaviour,ISkill
{
    [Header("Weapon Settings")] [SerializeField]
    private ProjectileAttack projectilePrefab;

    [SerializeField] private float scanRadius = 10f; // 적 탐색 범위
    [SerializeField] private LayerMask enemyLayer;

    private ObjectPool<ProjectileAttack> _pool;
    private Transform _projectileContainer;
    private CombatSystem _combatSystem;
    private SkillData _skillData;
    private IFighter _sender;

    private Collider2D[] _results = new Collider2D[100];
    private ContactFilter2D _filter;


    private float _timer;
    private float _cooldown;
    
    public void Init(CombatSystem combatSystem, IFighter sender, SkillData skillData)
    {
        _sender = sender;
        _combatSystem = combatSystem;
        _skillData = skillData;

        _cooldown = _skillData.cooldown;
        _filter = new ContactFilter2D();
        _filter.SetLayerMask(enemyLayer);
        _filter.useLayerMask = true;
        _filter.useTriggers = true;

        // 1. 하이어라키 최상단(Root)에 투사체들을 모아둘 빈 게임오브젝트 생성
        _projectileContainer = new GameObject("ProjectileContainer").transform;

        _pool = new ObjectPool<ProjectileAttack>(
            createFunc: () => Instantiate(projectilePrefab, _projectileContainer), // 자식으로 생성하여 하이어라키 정리
            actionOnGet: (obj) => obj.gameObject.SetActive(true),
            actionOnRelease: (obj) => obj.gameObject.SetActive(false),
            actionOnDestroy: (obj) => Destroy(obj.gameObject),
            defaultCapacity: 20,
            maxSize: 100 // 최대 100개 제한
        );

        ProjectileAttack[] prefab = new ProjectileAttack[20];
        for (int i = 0; i < 20; i++)
        {
            prefab[i] = _pool.Get(); // 1. 강제로 20개를 생성해서 꺼냄
        }

        for (int i = 0; i < 20; i++)
        {
            _pool.Release(prefab[i]); // 2. 즉시 풀로 반납하여 비활성화 대기 상태로 만듦
        }
    }

  

    private void Update()
    {
        _timer += Time.deltaTime;
        if (_timer >= _cooldown)
        {
            Fire();
            _timer -= _cooldown;
        }
    }

    private void Fire()
    {
        Transform target = FindClosestEnemy();
        if (target == null) return; // 범위 내에 적이 없으면 발사 안 함

        // 방향 벡터 계산
        Vector2 direction = (target.position - transform.position).normalized;

        // 풀에서 꺼내기
        ProjectileAttack projectile = _pool.Get();

        // 꺼낸 후 반드시 발사대 위치로 초기화
        projectile.transform.position = transform.position;

        // 투사체 초기화
        projectile.Init(_combatSystem, _sender, direction, _skillData.baseAtk, ReleaseProjectile);
    }

    private void ReleaseProjectile(ProjectileAttack projectile)
    {
        _pool.Release(projectile);
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
    public void LevelUp(SkillData skillData)
    {
        
    }
}