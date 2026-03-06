using System;
using UnityEngine;
using VContainer;

public class CircleAttack : MonoBehaviour, ISkill
{
    
    private float _damage = 5f; // 한 번에 입힐 데미지

    [SerializeField] private float radius = 3f; // 타격 범위 (반지름)
    private float _cooldown = 1f; // 몇 초마다 타격할지 (틱)
    [SerializeField] private LayerMask enemyLayer; // 몬스터만 골라내기 위한 레이어 마스크

    [SerializeField] private ParticleSystem attackEffect;

    private float _timer;

    private CombatSystem _combatSystem;
    private IFighter _sender;
    private SkillData _data;

    // 가비지(GC)를 절대 생성하지 않는 고정 배열 (최대 50마리 동시 타격)
    private Collider2D[] _results = new Collider2D[100];
    private ContactFilter2D _filter;

    // VContainer가 시작할 때 이 함수를 부르면서 CombatSystem을 던져줍니다.
    public void Init(CombatSystem combatSystem, IFighter sender, SkillData skillData)
    {
        _combatSystem = combatSystem;
        _sender = sender;
        _data = skillData;
        _cooldown = _data.cooldown;
        _damage = _data.baseAtk;
    }

    private void Start()
    {
        float diameter = (radius * 2f) + (radius * 0.5f);
        attackEffect.transform.localScale = new Vector3(diameter, diameter, 1f);

        _filter = new ContactFilter2D();
        _filter.SetLayerMask(enemyLayer);
        _filter.useLayerMask = true;
        _filter.useTriggers = true;
    }

    private void Update()
    {
        _timer += Time.deltaTime;

        // 쿨타임이 찰 때마다 광역 타격 발동!
        if (_timer >= _cooldown)
        {
            PulseAttack();
            attackEffect.Stop();
            attackEffect.Play();
            _timer -= _cooldown;
        }
    }

    private void PulseAttack()
    {
        int hitCount = Physics2D.OverlapCircle(transform.position, radius, _filter, _results);

        for (int i = 0; i < hitCount; i++)
        {
            IFighter targetMonster = _combatSystem.GetMonster(_results[i]);
            if (targetMonster == null) continue;
            var evt = new InGameEvent
            {
                Type = InGameEvent.EventType.Combat,
                Receiver = targetMonster,
                Amount = _damage,
            };
            _combatSystem.AddInGameEvent(evt);
        }
    }
}