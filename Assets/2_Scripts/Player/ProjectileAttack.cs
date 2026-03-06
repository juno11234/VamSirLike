using System;
using UnityEngine;

public class ProjectileAttack : MonoBehaviour
{
    [Header("Projectile Settings")] [SerializeField]
    private float speed = 15f;

    [SerializeField] private float lifeTime = 3f; // 화면 밖으로 나가면 삭제할 시간
    [SerializeField] private LayerMask enemyLayer;

    private CombatSystem _combatSystem;
    private IFighter _sender;
    private Vector2 _direction;
    private float _damage;
    private float _timer;

    private  Action<ProjectileAttack> _onReturnPool;

    public void Init(CombatSystem combatSystem,IFighter sender, Vector2 direction, float damage, Action<ProjectileAttack> returnPool)
    {
        _combatSystem = combatSystem;
        _sender = sender;
        _direction = direction;
        _damage = damage;
        _timer = 0f;
        _onReturnPool = returnPool;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    private void Update()
    {
        float distanceThisFrame = speed * Time.deltaTime;

        // 1. 이동하기 전에, 내가 이번 프레임에 이동할 궤적(선) 위에 적이 있는지 레이저(Raycast)로 확인
        RaycastHit2D hit = Physics2D.Raycast(transform.position, _direction, distanceThisFrame, enemyLayer);

        if (hit.collider != null)
        {
            // 적이 감지됨!
            IFighter target = _combatSystem.GetMonster(hit.collider);
            if (target != null)
            {
                InGameEvent evt = new InGameEvent
                {
                    Type = InGameEvent.EventType.Combat,
                    Sender = _sender,
                    Receiver = target,
                    Amount = _damage
                };
                _combatSystem.AddInGameEvent(evt);
            }

            ReturnToPool();
            return; // 파괴되었으므로 더 이상 이동하지 않음
        }

        transform.Translate(_direction * distanceThisFrame, Space.World);
        // 수명 체크
        _timer += Time.deltaTime;
        if (_timer >= lifeTime)
        {
            ReturnToPool();
            return;
        }
    }

    private void ReturnToPool()
    {
        _onReturnPool?.Invoke(this);
    }
    
    /*private void OnTriggerEnter2D(Collider2D other)
    {
        IFighter target = _combatSystem.GetMonster(other);

        if (target != null)
        {
            // 타격 이벤트 생성
            InGameEvent evt = new InGameEvent
            {
                Type = InGameEvent.EventType.Combat,
                Sender = _sender,
                Receiver = target,
                Amount = _damage
            };

            _combatSystem.AddInGameEvent(evt);

            // 타격 후 투사체 파괴
            Destroy(gameObject); // 추후 Pool.Release()로 교체
        }
    }*/
}