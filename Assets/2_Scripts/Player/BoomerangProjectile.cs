using System;
using UnityEngine;

public class BoomerangProjectile : MonoBehaviour
{
  private enum State { FlyingOut, Returning }
    private State _currentState;

    [Header("Boomerang Settings")]
    [SerializeField] private float flySpeed = 10f;    // 날아가는 속도
    [SerializeField] private float returnSpeed = 15f; // 돌아올 때 속도 (보통 더 빠름)
    [SerializeField] private float maxDistance = 8f;  // 날아가는 최대 거리
    [SerializeField] private float spinSpeed = 1000f; // 빙글빙글 도는 속도

    private Vector3 _startPos;
    private Vector3 _flyDirection;
    
    private Transform _playerTransform;
    private CombatSystem _combatSystem;
    private IFighter _sender;
    private float _damage;
    private Action<BoomerangProjectile> _onRelease;

    public void Init(CombatSystem combatSystem, IFighter sender, Transform player, Vector3 direction, float damage, Action<BoomerangProjectile> onRelease)
    {
        _combatSystem = combatSystem;
        _sender = sender;
        _playerTransform = player;
        
        _flyDirection = direction;
        _damage = damage;
        _onRelease = onRelease;

        _startPos = transform.position;
        _currentState = State.FlyingOut; // 처음엔 날아가는 상태로 시작
    }

    private void Update()
    {
        // 1. 빙글빙글 회전 효과 (Z축 회전)
        transform.Rotate(0, 0, spinSpeed * Time.deltaTime);

        // 2. 상태에 따른 이동 로직
        if (_currentState == State.FlyingOut)
        {
            // 타겟 방향으로 직진
            transform.position += _flyDirection * (flySpeed * Time.deltaTime);

            // 시작 지점에서 최대 거리 이상 멀어졌다면 돌아오는 상태로 전환!
            if (Vector3.Distance(_startPos, transform.position) >= maxDistance)
            {
                _currentState = State.Returning;
            }
        }
        else if (_currentState == State.Returning)
        {
            // 플레이어가 죽었거나 없어졌다면 즉시 증발
            if (_playerTransform == null) 
            {
                _onRelease?.Invoke(this);
                return;
            }

            // 플레이어의 현재 위치를 향해 방향을 계속 틀면서 이동 (유도탄)
            Vector3 returnDir = (_playerTransform.position - transform.position).normalized;
            transform.position += returnDir * (returnSpeed * Time.deltaTime);

            // 플레이어와 충분히 가까워지면(잡으면) 풀로 반납
            if (Vector3.Distance(transform.position, _playerTransform.position) < 0.5f)
            {
                _onRelease?.Invoke(this);
            }
        }
    }
}
