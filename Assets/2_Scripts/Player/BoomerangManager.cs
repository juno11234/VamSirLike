using UnityEngine;
using UnityEngine.Pool;

public class BoomerangManager : SkillBase
{
    [Header("Skill Settings")]
    [SerializeField] private BoomerangProjectile boomerangPrefab;
    [SerializeField] private float scanRadius = 12f;

    private ObjectPool<BoomerangProjectile> _pool;
    private Transform _projectileContainer;

    private Collider2D[] _results = new Collider2D[100];
    private ContactFilter2D _filter;

    private float _timer;

    public override void Init(CombatSystem combatSystem, IFighter sender, SkillData skillData)
    {
        base.Init(combatSystem, sender, skillData);

        _filter = new ContactFilter2D();
        _filter.SetLayerMask(enemyLayer);
        _filter.useLayerMask = true;
        _filter.useTriggers = true;

        _projectileContainer = new GameObject("BoomerangContainer").transform;

        _pool = new ObjectPool<BoomerangProjectile>(
            createFunc: () => Instantiate(boomerangPrefab, _projectileContainer),
            actionOnGet: (obj) => obj.gameObject.SetActive(true),
            actionOnRelease: (obj) => obj.gameObject.SetActive(false),
            actionOnDestroy: (obj) => Destroy(obj.gameObject),
            defaultCapacity: 10,
            maxSize: 10
        );

        BoomerangProjectile[] prefab = new BoomerangProjectile[20];
        for (int i = 0; i < 10; i++)
        {
            prefab[i] = _pool.Get();
        }

        for (int i = 0; i < 10; i++)
        {
            _pool.Release(prefab[i]);
        }

        _timer += Cooldown;
    }

    private void Update()
    {
        _timer += Time.deltaTime;
        if (_timer >= Cooldown)
        {
            Fire();
            _timer -= Cooldown;
        }
    }

    private void Fire()
    {
        Transform target = FindClosestEnemy();
        Vector3 direction;
        if (target != null)
        {
            direction = (target.position - transform.position).normalized;
        }
        else
        {
            // 적이 없으면 허공에 랜덤으로 던짐! (증발 방지)
            float randomAngle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            direction = new Vector3(Mathf.Cos(randomAngle), Mathf.Sin(randomAngle), 0);
        }

        BoomerangProjectile proj = _pool.Get();
        proj.transform.position = transform.position;

        // [중요] 플레이어의 Transform을 같이 넘겨줍니다!
        proj.Init(CombatSystem, Sender, transform, direction, Damage, ReleaseProjectile);
    }

    private void ReleaseProjectile(BoomerangProjectile proj)
    {
        _pool.Release(proj);
    }

    private Transform FindClosestEnemy()
    {
        int hitCount = Physics2D.OverlapCircle(transform.position, scanRadius, _filter, _results);
        if (hitCount == 0) return null;

        Transform closestTarget = null;
        float minDistanceSqr = Mathf.Infinity;

        for (int i = 0; i < hitCount; i++)
        {
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