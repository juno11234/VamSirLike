using UnityEngine;

public class ProjectileAttack : MonoBehaviour
{
    [Header("Projectile Settings")]
    [SerializeField] private float speed = 15f;
    [SerializeField] private float lifeTime = 3f; // 화면 밖으로 나가면 삭제할 시간
    
    private CombatSystem _combatSystem;
    private IFighter _sender;
    private Vector2 _direction;
    private float _damage;
    private float _timer;

    public void Init(CombatSystem combatSystem, Vector2 direction, float damage)
    {
        _combatSystem = combatSystem;
        _direction = direction;
        _damage = damage;
        _timer = 0f;

        // 투사체 이미지가 날아가는 방향을 바라보게 회전
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }
    private void Update()
    {
        // 매 프레임 지정된 방향으로 이동
        transform.Translate(_direction * (speed * Time.deltaTime), Space.World);

        // 수명 체크
        _timer += Time.deltaTime;
        if (_timer >= lifeTime)
        {
            Destroy(gameObject); // 추후 Pool.Release()로 교체
        }
    }
    
    // [핵심] 투사체는 Overlap이 아닌 OnTriggerEnter2D 사용
    private void OnTriggerEnter2D(Collider2D other)
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
    }
}
