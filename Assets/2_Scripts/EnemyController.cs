using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Pool;

public class EnemyController : MonoBehaviour, IFighter
{
    [Header("Attack Settings")] [SerializeField]
    private float attackRange = 0.8f; // 공격 사거리 (플레이어와 닿는 거리)

    [SerializeField] private float attackCooldown = 1f; // 1초마다 공격
    [SerializeField] private float flashDuration = 0.1f; // 번쩍이는 시간
    private float _attackTimer;
    public Collider2D MainCollider => _collider2D;
    private Collider2D _collider2D;
    private SpriteRenderer _spriteRenderer;
    private CombatSystem _combatSystem;
    private PlayerController _targetPlayer;
    private EnemyStat _stat;
    private float _currentHp;
    private Action<EnemyController> _onDeathCallback;

    // 쉐이더 최적화를 위한 프로퍼티 블록
    private MaterialPropertyBlock _mpb;
    private static readonly int FlashAmountProp = Shader.PropertyToID("_Amount"); 

    private void Awake()
    {
        _collider2D = GetComponent<Collider2D>();
        _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        _mpb = new MaterialPropertyBlock();
    }

    // 스폰 매니저가 나를 소환할 때, 필요한 정보(타겟, 스탯, 풀)를 꽂아줍니다 (의존성 주입)
    public void Setup(PlayerController target, EnemyStat stat, Action<EnemyController> onDeathCallback,
        CombatSystem combatSystem)
    {
        _targetPlayer = target;
        _stat = stat;
        _onDeathCallback = onDeathCallback;
        _currentHp = _stat.hp;
        _combatSystem = combatSystem;
        _attackTimer = attackCooldown;

        ResetFlashEffect();
    }

    private void Update()
    {
        if (_targetPlayer == null) return;
        _attackTimer += Time.deltaTime;
        Vector3 offset = _targetPlayer.transform.position - transform.position;
        float distanceSqr = offset.sqrMagnitude;

        // if (offset.x != 0)
        // {
        //     _spriteRenderer.flipX = offset.x > 0; 
        // }

        // 2. 사거리 안으로 들어왔다면?
        if (distanceSqr <= attackRange * attackRange)
        {
            // 쿨타임이 찼을 때만 때림
            if (_attackTimer >= attackCooldown)
            {
                AttackPlayer();
                _attackTimer = 0f;
            }
        }
        else
        {
            // 3. 사거리 밖이라면 플레이어 쪽으로 이동
            Vector3 direction = offset.normalized;
            transform.position += direction * (_stat.speed * Time.deltaTime);
        }
    }

    private void AttackPlayer()
    {
        // CombatSystem을 통해 공격 이벤트 전송 (GC 발생 없음)
        InGameEvent evt = new InGameEvent
        {
            Type = InGameEvent.EventType.Combat,
            Sender = this,
            Receiver = _targetPlayer, // _targetPlayer는 IFighter를 상속받았으므로 바로 전달 가능
            Amount = _stat.atk
        };

        _combatSystem.AddInGameEvent(evt);
    }

    private void Die()
    {
        _onDeathCallback?.Invoke(this);
    }


    public void TakeDamage(InGameEvent combatEvent)
    {
        _currentHp -= combatEvent.Amount;
        if (_currentHp <= 0)
        {
            Die();
        }
        else
        {
            // 체력이 남았다면 피격 이펙트 재생!
            HitRoutineAsync().Forget();
        }
    }

    private async UniTaskVoid HitRoutineAsync()
    {
        // 1. 하얗게 번쩍!
        _spriteRenderer.GetPropertyBlock(_mpb);
        _mpb.SetFloat(FlashAmountProp, 0.4f);
        _spriteRenderer.SetPropertyBlock(_mpb);

        // 2. 대기
        await UniTask.Delay(TimeSpan.FromSeconds(flashDuration));

        // 3. 대기하는 동안 적이 죽어서 풀로 돌아갔거나 파괴되었다면 중단
        if (this == null || !gameObject.activeInHierarchy) return;

        // 4. 원래 색으로 복구
        ResetFlashEffect();
    }

    private void ResetFlashEffect()
    {
        _spriteRenderer.GetPropertyBlock(_mpb);
        _mpb.SetFloat(FlashAmountProp, 0f);
        _spriteRenderer.SetPropertyBlock(_mpb);
    }

    public void Heal(InGameEvent healthEvent)
    {
    }
}