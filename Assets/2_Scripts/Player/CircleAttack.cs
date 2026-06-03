using UnityEngine;
using VContainer;

public class CircleAttack : SkillBase
{
    [SerializeField] private float radius = 3f;
    [SerializeField] private ParticleSystem attackEffect;

    private float _timer;

    private void Start()
    {
        EffectScale();
    }

    private void EffectScale()
    {
        float diameter = (radius * 2f) + (radius * 0.5f);
        attackEffect.transform.localScale = new Vector3(diameter, diameter, 1f);
    }

    private void Update()
    {
        _timer += Time.deltaTime;

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
        Collider2D[] results = Physics2D.OverlapCircleAll(transform.position, radius, enemyLayer);

        for (int i = 0; i < results.Length; i++)
        {
            IFighter targetMonster = CombatSystem.GetMonster(results[i]);
            if (targetMonster == null) continue;
            InGameEvent evt = new InGameEvent
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
