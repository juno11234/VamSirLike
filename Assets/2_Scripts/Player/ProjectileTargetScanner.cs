using System.Collections;
using UnityEngine;

public class ProjectileTargetScanner : SkillBase
{
    [Header("Weapon Settings")]
    [SerializeField] private ProjectileAttack projectilePrefab;
    [SerializeField] private float scanRadius = 10f;
    [SerializeField] private float fireDelay = 0.1f;

    private float _timer;
    private int _projectileCount = 1;

    public override void Init(CombatSystem combatSystem, IFighter sender, SkillData skillData)
    {
        base.Init(combatSystem, sender, skillData);
        _projectileCount = 1;
    }

    private void Update()
    {
        _timer += Time.deltaTime;
        if (_timer >= Cooldown)
        {
            StartCoroutine(FireCoroutine());
            _timer -= Cooldown;
        }
    }

    private IEnumerator FireCoroutine()
    {
        for (int i = 0; i < _projectileCount; i++)
        {
            Transform target = FindClosestEnemy();

            if (target == null) yield break;

            Vector2 direction = (target.position - transform.position).normalized;
            ProjectileAttack projectile = Instantiate(projectilePrefab);
            projectile.transform.position = transform.position;
            projectile.Init(CombatSystem, Sender, direction, Damage, OnProjectileFinished);

            if (i < _projectileCount - 1)
            {
                yield return new WaitForSeconds(fireDelay);
            }
        }
    }

    private void OnProjectileFinished(ProjectileAttack projectile)
    {
        Destroy(projectile.gameObject);
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

    public override void LevelUp(SkillData skillData)
    {
        base.LevelUp(skillData);
        _projectileCount++;
    }
}
