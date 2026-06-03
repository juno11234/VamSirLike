using UnityEngine;

public class BoomerangManager : SkillBase
{
    [Header("Skill Settings")]
    [SerializeField] private BoomerangProjectile boomerangPrefab;
    [SerializeField] private float scanRadius = 12f;

    private float _timer;

    public override void Init(CombatSystem combatSystem, IFighter sender, SkillData skillData)
    {
        base.Init(combatSystem, sender, skillData);
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
            float randomAngle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            direction = new Vector3(Mathf.Cos(randomAngle), Mathf.Sin(randomAngle), 0);
        }

        BoomerangProjectile proj = Instantiate(boomerangPrefab);
        proj.transform.position = transform.position;
        proj.Init(CombatSystem, Sender, transform, direction, Damage);
    }

    private Transform FindClosestEnemy()
    {
        Collider2D[] results = Physics2D.OverlapCircleAll(transform.position, scanRadius, enemyLayer);
        if (results.Length == 0) return null;

        Transform closestTarget = null;
        float minDistance = Mathf.Infinity;

        for (int i = 0; i < results.Length; i++)
        {
            float dist = Vector2.Distance(transform.position, results[i].transform.position);
            if (dist < minDistance)
            {
                minDistance = dist;
                closestTarget = results[i].transform;
            }
        }

        return closestTarget;
    }
}
