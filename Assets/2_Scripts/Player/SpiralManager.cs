using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class SpiralManager : SkillBase
{
    [Header("Skill Settings")]
    [SerializeField] private SpiralProjectile projectilePrefab;
    [SerializeField] private float angleSpeed = 5f;
    [SerializeField] private float expandSpeed = 2f;
    [SerializeField] private float fireDelay = 0.5f;

    private float _timer;
    private float _currentSpawnAngle;
    private int _projectileCount = 1;

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
            FireAsync().Forget();
            _timer -= Cooldown;
        }
    }

    private async UniTaskVoid FireAsync()
    {
        for (int i = 0; i < _projectileCount; i++)
        {
            SpiralProjectile proj = Instantiate(projectilePrefab);

            Transform centerTransform = ((MonoBehaviour)Sender).transform;

            proj.Init(CombatSystem, Sender, centerTransform, _currentSpawnAngle, angleSpeed, expandSpeed, Damage);

            _currentSpawnAngle += 1.5f;

            if (i < _projectileCount - 1)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(fireDelay));
            }
        }
    }

    public override void LevelUp(SkillData skillData)
    {
        base.LevelUp(skillData);
        _projectileCount++;
    }
}
