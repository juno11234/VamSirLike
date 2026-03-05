using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour, IFighter
{
    public Collider2D MainCollider => _collider;
    private Collider2D _collider;
    private PlayerActions _playerInput;
    private PlayerActions.PlayerMovementActions _player;
    private Vector2 _moveInput;
    private float _moveSpeed;
    private bool _isInitialized = false;
    private SpriteRenderer _spriteRenderer;
    private CombatSystem _combatSystem;

    [Header("Map Boundaries")] public Vector2 minBounds = new Vector2(-10f, -10f); // 좌하단 끝 좌표
    public Vector2 maxBounds = new Vector2(10f, 10f); // 우상단 끝 좌표

    public void Initialize(PlayerStat stat, CombatSystem combatSystem)
    {
        _collider = GetComponent<Collider2D>();
        _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        _moveSpeed = stat.baseSpeed;
        _isInitialized = true;
        _combatSystem = combatSystem;
        CircleAttack attack = GetComponentInChildren<CircleAttack>();
        attack.Init(_combatSystem);
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
    }


    public void TakeDamage(InGameEvent combatEvent)
    {
    }

    public void Heal(InGameEvent healthEvent)
    {
    }
}