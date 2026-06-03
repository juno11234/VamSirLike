using UnityEngine;

public class SpiralProjectile : MonoBehaviour
{
    [SerializeField] private LayerMask enemyLayer;

    private float _angle;
    private float _radius;
    private Transform _center;
    private float _angleSpeed;
    private float _expandSpeed;
    private float _damage;

    private CombatSystem _combatSystem;
    private IFighter _sender;

    private float _hitRadius = 0.5f;

    public void Init(CombatSystem combatSystem, IFighter sender, Transform center, float startAngle, float angleSpeed,
        float expandSpeed, float damage)
    {
        _combatSystem = combatSystem;
        _sender = sender;
        _center = center;

        _angle = startAngle;
        _angleSpeed = angleSpeed;
        _expandSpeed = expandSpeed;
        _damage = damage;

        _radius = 0f;
    }

    private void Update()
    {
        _angle += _angleSpeed * Time.deltaTime;
        _radius += _expandSpeed * Time.deltaTime;

        float x = Mathf.Cos(_angle) * _radius;
        float y = Mathf.Sin(_angle) * _radius;

        transform.position = _center.position + new Vector3(x, y, 0);

        float angleDeg = _angle * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angleDeg - 90f);

        Collider2D[] results = Physics2D.OverlapCircleAll(transform.position, _hitRadius, enemyLayer);
        for (int i = 0; i < results.Length; i++)
        {
            IFighter targetMonster = _combatSystem.GetMonster(results[i]);
            if (targetMonster != null)
            {
                InGameEvent evt = new InGameEvent
                {
                    Type = InGameEvent.EventType.Combat,
                    Sender = _sender,
                    Receiver = targetMonster,
                    Amount = _damage
                };
                _combatSystem.AddInGameEvent(evt);
            }
        }

        if (_radius > 15f)
        {
            Destroy(gameObject);
        }
    }
}
