using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour, IFighter
{
    [Header("Magnet Settings")] [SerializeField]
    private float magnetRadius = 4f; // 보석 획득 반경

    [SerializeField] private LayerMask itemLayer; // 보석 전용 레이어 (인스펙터 세팅 필수)
    [SerializeField] private int[] startingSkillIds;
    public Collider2D MainCollider => _collider;
    private Collider2D _collider;
    private PlayerActions _playerInput;
    private PlayerActions.PlayerMovementActions _player;
    private Vector2 _moveInput;
    private float _moveSpeed;
    private bool _isInitialized = false;
    private SpriteRenderer _spriteRenderer;
    private CombatSystem _combatSystem;
    private SkillManager _skillManager;

    [Header("Map Boundaries")] public Vector2 minBounds = new Vector2(-10f, -10f); // 좌하단 끝 좌표
    public Vector2 maxBounds = new Vector2(10f, 10f); // 우상단 끝 좌표

    public float MaxHp { get; private set; }
    public float CurrentHp { get; private set; }
    public event Action<float, float> OnHpChanged;

    private Collider2D[] _itemResults = new Collider2D[20];
    private ContactFilter2D _itemFilter;

    public void Initialize(PlayerStat stat, CombatSystem combatSystem, DataManager dataManager)
    {
        MaxHp = stat.baseHp;
        CurrentHp = MaxHp;
        _moveSpeed = stat.baseSpeed;
        _isInitialized = true;
        _combatSystem = combatSystem;

        _collider = GetComponent<Collider2D>();
        _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        _skillManager = GetComponentInChildren<SkillManager>();

        _skillManager.Init(_combatSystem, this, dataManager, startingSkillIds);

        _itemFilter = new ContactFilter2D();
        _itemFilter.SetLayerMask(itemLayer);
        _itemFilter.useLayerMask = true;
        _itemFilter.useTriggers = true;
        OnHpChanged?.Invoke(CurrentHp, MaxHp);
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
            // TryGetComponent가 GetComponent보다 미세하게 더 빠르고 안전합니다
            if (_itemResults[i].TryGetComponent(out ExpItem item))
            {
                item.SetTarget(transform); // 획득!
            }
        }
    }

    public void TakeDamage(InGameEvent combatEvent)
    {
        CurrentHp -= combatEvent.Amount;
        OnHpChanged?.Invoke(CurrentHp, MaxHp);
        if (CurrentHp <= 0)
        {
            Die();
        }
    }

    public void Heal(InGameEvent healthEvent)
    {
        CurrentHp += healthEvent.Amount;
        if (CurrentHp > MaxHp) CurrentHp = MaxHp;

        OnHpChanged?.Invoke(CurrentHp, MaxHp);
    }

    private void Die()
    {
        Debug.Log("플레이어 사망!");
    }
}