using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour, IFighter
{
    [Header("Map Boundaries")]
    public Vector2 minBounds = new Vector2(-10f, -10f);
    public Vector2 maxBounds = new Vector2(10f, 10f);

    [Header("Magnet Settings")]
    [SerializeField] private float magnetRadius = 4f;

    [SerializeField] private float invincibilityDuration = 0.5f;
    [SerializeField] private float flashDuration = 0.1f;

    [SerializeField] private LayerMask itemLayer;
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

        _collider = GetComponent<Collider2D>();
        _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        _skillManager = GetComponentInChildren<SkillManager>();

        _skillManager.Init(_combatSystem, this, dataManager, startingSkillIds);

        OnHpChanged?.Invoke(CurrentHp, MaxHp);

        if (_spriteRenderer != null)
        {
            _spriteRenderer.material.SetFloat("_Amount", 0f);
        }
    }

    private void Awake()
    {
        _playerInput = new PlayerActions();
        _player = _playerInput.PlayerMovement;

        // 람다식: 입력 이벤트 콜백을 간결하게 등록하기 위해 사용
        _player.Move.performed += ctx => _moveInput = ctx.ReadValue<Vector2>();
        _player.Move.canceled += ctx => _moveInput = Vector2.zero;
    }

    private void OnEnable() => _playerInput.Enable();
    private void OnDisable() => _playerInput.Disable();

    private void Update()
    {
        if (_isInitialized == false) return;
        Vector3 moveDir = new Vector3(_moveInput.x, _moveInput.y, 0f).normalized;

        if (moveDir.x != 0)
        {
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
        Collider2D[] itemResults = Physics2D.OverlapCircleAll(transform.position, magnetRadius, itemLayer);
        for (int i = 0; i < itemResults.Length; i++)
        {
            if (itemResults[i].TryGetComponent(out ExpItem item))
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
            _spriteRenderer.material.SetFloat("_Amount", 0.4f);
        }

        await UniTask.Delay(TimeSpan.FromSeconds(flashDuration));

        if (_spriteRenderer != null)
        {
            _spriteRenderer.material.SetFloat("_Amount", 0f);
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
