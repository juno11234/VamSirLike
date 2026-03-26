using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour, IFighter
{
    [Header("Map Boundaries")]
    public Vector2 minBounds = new Vector2(-10f, -10f); // 좌하단 끝 좌표
    public Vector2 maxBounds = new Vector2(10f, 10f); // 우상단 끝 좌표
    
    [Header("Magnet Settings")] [SerializeField]
    private float magnetRadius = 4f; // 보석 획득 반경

    [SerializeField] private float invincibilityDuration = 0.5f; // 무적 시간 (0.5초)
    [SerializeField] private float flashDuration = 0.1f; // 번쩍이는 시간 (0.1초)
    
    [SerializeField] private LayerMask itemLayer; // 보석 전용 레이어 (인스펙터 세팅 필수)
    [SerializeField] private int[] startingSkillIds;

    public Collider2D MainCollider => _collider;
    public float MaxHp { get; private set; }
    public float CurrentHp { get; private set; }
    
    public event Action<float, float> OnHpChanged;
    public event Action OnDeath;
    
    private SpriteRenderer _spriteRenderer;
    private CombatSystem _combatSystem;
    private SkillManager _skillManager;

    private PlayerActions _playerInput;
    private PlayerActions.PlayerMovementActions _player;
    private Vector2 _moveInput;

    private Collider2D _collider;
    
    private MaterialPropertyBlock _mpb;
    private static readonly int FlashAmountProp = Shader.PropertyToID("_Amount");
    
    private Collider2D[] _itemResults = new Collider2D[20];
    private ContactFilter2D _itemFilter;
    
    private float _moveSpeed;
    private bool _isInitialized = false;
    private bool _isInvincible = false;
    
    public void Initialize(PlayerStat stat, CombatSystem combatSystem, DataManager dataManager)
    {
        MaxHp = stat.baseHp;
        CurrentHp = MaxHp;
        _moveSpeed = stat.baseSpeed;
        _isInitialized = true;
        _combatSystem = combatSystem;
        _mpb = new MaterialPropertyBlock();

        _collider = GetComponent<Collider2D>();
        _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        _skillManager = GetComponentInChildren<SkillManager>();

        _skillManager.Init(_combatSystem, this, dataManager, startingSkillIds);

        _itemFilter = new ContactFilter2D();
        _itemFilter.SetLayerMask(itemLayer);
        _itemFilter.useLayerMask = true;
        _itemFilter.useTriggers = true;
        
        OnHpChanged?.Invoke(CurrentHp, MaxHp);

        if (_spriteRenderer != null)
        {
            _spriteRenderer.GetPropertyBlock(_mpb);
            _mpb.SetFloat(FlashAmountProp, 0f);
            _spriteRenderer.SetPropertyBlock(_mpb);
        }
    }

    private void Awake()
    {
        _playerInput = new PlayerActions();
        _player = _playerInput.PlayerMovement;

        _player.Move.performed += ctx => _moveInput = ctx.ReadValue<Vector2>();
        _player.Move.canceled += ctx => _moveInput = Vector2.zero;
    }

    private void OnEnable() => _playerInput.Enable();
    private void OnDisable() => _playerInput.Disable();

    void Update()
    {
        if (_isInitialized == false) return;
        Vector3 moveDir = new Vector3(_moveInput.x, _moveInput.y, 0f).normalized;

        if (moveDir.x != 0)
        {
            // x가 0보다 작으면(왼쪽) flipX를 true로, 크면(오른쪽) false로!
            _spriteRenderer.flipX = moveDir.x < 0;
        }

        Vector3 nextPosition = transform.position + moveDir * (_moveSpeed * Time.deltaTime);
        nextPosition.x = Mathf.Clamp(nextPosition.x, minBounds.x, maxBounds.x);
        nextPosition.y = Mathf.Clamp(nextPosition.y, minBounds.y, maxBounds.y);
        transform.position = nextPosition;
        CollectItems();
    }

    private void CollectItems()
    {
        int hitCount = Physics2D.OverlapCircle(transform.position, magnetRadius, _itemFilter, _itemResults);
        for (int i = 0; i < hitCount; i++)
        {
            if (_itemResults[i].TryGetComponent(out ExpItem item))
            {
                item.SetTarget(transform); 
            }
        }
    }

    public void TakeDamage(InGameEvent combatEvent)
    {
        if (_isInvincible) return;

        CurrentHp -= combatEvent.Amount;
        OnHpChanged?.Invoke(CurrentHp, MaxHp);
        if (CurrentHp <= 0)
        {
            Die();
        }
        else
        {
            HitRoutineAsync().Forget();
        }
    }

    private async UniTaskVoid HitRoutineAsync()
    {
        _isInvincible = true;

        if (_spriteRenderer != null)
        {
            _spriteRenderer.GetPropertyBlock(_mpb);
            _mpb.SetFloat(FlashAmountProp, 0.4f);
            _spriteRenderer.SetPropertyBlock(_mpb);
        }

        await UniTask.Delay(TimeSpan.FromSeconds(flashDuration));

        if (_spriteRenderer != null)
        {
            _spriteRenderer.GetPropertyBlock(_mpb);
            _mpb.SetFloat(FlashAmountProp, 0f);
            _spriteRenderer.SetPropertyBlock(_mpb);
        }

        float remainingIFrame = invincibilityDuration - flashDuration;
        if (remainingIFrame > 0)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(remainingIFrame));
        }

        _isInvincible = false;
    }

    public void Heal(InGameEvent healthEvent)
    {
        CurrentHp += healthEvent.Amount;
        if (CurrentHp > MaxHp) CurrentHp = MaxHp;

        OnHpChanged?.Invoke(CurrentHp, MaxHp);
    }

    private void Die()
    {
        OnDeath?.Invoke();
    }
}