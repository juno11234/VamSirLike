using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Pool;

public class ProjectileTargetScanner : SkillBase
{
    [Header("Weapon Settings")]
    [SerializeField] private ProjectileAttack projectilePrefab;
    [SerializeField] private float scanRadius = 10f; // 적 탐색 범위
    [SerializeField] private float fireDelay = 0.1f; // 연사 시 투사체 간의 발사 간격 (0.1초)

    private ObjectPool<ProjectileAttack> _pool;
    private Transform _projectileContainer;

    private Collider2D[] _results = new Collider2D[100];
    private ContactFilter2D _filter;

    private float _timer;
    private int _projectileCount = 1;

    public override void Init(CombatSystem combatSystem, IFighter sender, SkillData skillData)
    {
        base.Init(combatSystem, sender, skillData);
        _projectileCount = 1;

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
        if (_timer >= Cooldown)
        {
            FireAsync().Forget();
            _timer -= Cooldown;
        }
    }

    private async UniTaskVoid FireAsync()
    {
        for (int i = 0; i < _projectileCount; i++)
        {
            Transform target = FindClosestEnemy();

            if (target == null) break;

            Vector2 direction = (target.position - transform.position).normalized;
            ProjectileAttack projectile = _pool.Get();
            projectile.transform.position = transform.position;
            projectile.Init(CombatSystem, Sender, direction, Damage, ReleaseProjectile);

            if (i < _projectileCount - 1)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(fireDelay));
            }
        }
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

    public override void LevelUp(SkillData skillData)
    {
        base.LevelUp(skillData);
        _projectileCount++;
    }
}