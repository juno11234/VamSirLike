using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private PlayerActions _playerInput;
    private PlayerActions.PlayerMovementActions _player;
    private Vector2 _moveInput;
    private float _moveSpeed;
    private bool _isInitialized = false;

    [Header("Map Boundaries")]
    public Vector2 minBounds = new Vector2(-10f, -10f); // 좌하단 끝 좌표
    public Vector2 maxBounds = new Vector2(10f, 10f);   // 우상단 끝 좌표
    public void Initialize(PlayerStat stat)
    {
        _moveSpeed = stat.baseSpeed;
        _isInitialized = true;
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
        Vector3 nextPosition = transform.position + moveDir * (_moveSpeed * Time.deltaTime);
        nextPosition.x = Mathf.Clamp(nextPosition.x, minBounds.x, maxBounds.x);
        nextPosition.y = Mathf.Clamp(nextPosition.y, minBounds.y, maxBounds.y);
        transform.position = nextPosition;
    }
}