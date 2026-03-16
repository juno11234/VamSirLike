using UnityEngine;
using UnityEngine.Pool;

public class BoomerangManager : MonoBehaviour, ISkill
{
    [Header("Skill Settings")]
    [SerializeField] private BoomerangProjectile boomerangPrefab;
    [SerializeField] private float scanRadius = 12f;
    [SerializeField] private LayerMask enemyLayer;

    private ObjectPool<BoomerangProjectile> _pool;
    private Transform _projectileContainer;
    private CombatSystem _combatSystem;
    private SkillData _skillData;
    private IFighter _sender;

    private Collider2D[] _results = new Collider2D[100];
    private ContactFilter2D _filter;

    private float _timer;
    private float _cooldown;

    public int CurrentLevel { get; private set; }

    public void Init(CombatSystem combatSystem, IFighter sender, SkillData skillData)
    {
        _sender = sender;
        _combatSystem = combatSystem;
        _skillData = skillData;

        CurrentLevel = 1;
        _cooldown = _skillData.cooldown;

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
            maxSize: 30
        );
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
        // 적이 없으면 랜덤한 방향으로 던지도록 처리해도 좋습니다.
        if (target == null) return;

        Vector3 direction = (target.position - transform.position).normalized;

        BoomerangProjectile proj = _pool.Get();
        proj.transform.position = transform.position;

        // [중요] 플레이어의 Transform을 같이 넘겨줍니다!
        proj.Init(_combatSystem, _sender, transform, direction, _skillData.baseAtk, ReleaseProjectile);
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

    public void LevelUp(SkillData skillData)
    {
        _skillData = skillData;
        _cooldown = _skillData.cooldown;
        CurrentLevel++;
    }
}