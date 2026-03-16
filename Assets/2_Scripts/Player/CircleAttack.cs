using System;
using UnityEngine;
using VContainer;

public class CircleAttack : SkillBase
{
    [SerializeField] private float radius = 3f; // 타격 범위 (반지름)
    [SerializeField] private ParticleSystem attackEffect;
    
    private float _timer;
    // 가비지(GC)를 절대 생성하지 않는 고정 배열 (최대 50마리 동시 타격)
    private Collider2D[] _results = new Collider2D[100];
    private ContactFilter2D _filter;

    private void Start()
    {
        EffectScale();

        _filter = new ContactFilter2D();
        _filter.SetLayerMask(enemyLayer);
        _filter.useLayerMask = true;
        _filter.useTriggers = true;
    }

    private void EffectScale()
    {
        float diameter = (radius * 2f) + (radius * 0.5f);
        attackEffect.transform.localScale = new Vector3(diameter, diameter, 1f);
    }

    private void Update()
    {
        _timer += Time.deltaTime;

        // 쿨타임이 찰 때마다 광역 타격 발동!
        if (_timer >= Cooldown)
        {
            PulseAttack();
            attackEffect.Stop();
            attackEffect.Play();
            _timer -= Cooldown;
        }
    }

    private void PulseAttack()
    {
        int hitCount = Physics2D.OverlapCircle(transform.position, radius, _filter, _results);

        for (int i = 0; i < hitCount; i++)
        {
            IFighter targetMonster = CombatSystem.GetMonster(_results[i]);
            if (targetMonster == null) continue;
            var evt = new InGameEvent
            {
                Type = InGameEvent.EventType.Combat,
                Sender = Sender,
                Receiver = targetMonster,
                Amount = Damage,
            };
            CombatSystem.AddInGameEvent(evt);
        }
    }

    public override void LevelUp(SkillData skillData)
    {
        base.LevelUp(skillData);
        radius += SkillData.enhancePerLevel * 0.25f;
        EffectScale();
    }
}