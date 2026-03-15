using System;
using UnityEngine;

public class SpiralProjectile : MonoBehaviour
{
 private float _angle;
    private float _radius;
    private Transform _center; // 회전의 중심 (플레이어)
    private float _angleSpeed;
    private float _expandSpeed;
    private float _damage;

    private CombatSystem _combatSystem;
    private IFighter _sender;
    private Action<SpiralProjectile> _onRelease;

    // 풀에서 꺼낼 때마다 호출될 초기화 함수
    public void Init(CombatSystem combatSystem, IFighter sender, Transform center, float startAngle, float angleSpeed, float expandSpeed, float damage, Action<SpiralProjectile> onRelease)
    {
        _combatSystem = combatSystem;
        _sender = sender;
        _center = center;
        
        _angle = startAngle;
        _angleSpeed = angleSpeed;
        _expandSpeed = expandSpeed;
        _damage = damage;
        
        _radius = 0f; // 중심(0)에서부터 출발
        _onRelease = onRelease;
    }

    private void Update()
    {
        // 1. 각도와 반지름을 시간 단위로 증가
        _angle += _angleSpeed * Time.deltaTime;
        _radius += _expandSpeed * Time.deltaTime;

        // 2. 삼각함수를 이용해 나선형(원형) 좌표 계산
        float x = Mathf.Cos(_angle) * _radius;
        float y = Mathf.Sin(_angle) * _radius;

        // 3. 위치 적용
        transform.position = _center.position + new Vector3(x, y, 0);

        // --- [추가된 회전 로직] ---
        // _angle은 라디안(Radian) 값이므로 우리가 아는 도(Degree) 단위로 변환해 줍니다.
        float angleDeg = _angle * Mathf.Rad2Deg;
        
        transform.rotation = Quaternion.Euler(0, 0, angleDeg - 90f); 
        // -----------------------

        // 4. 화면 밖으로 충분히 멀어지면 풀로 반납
        if (_radius > 15f)
        {
            _onRelease?.Invoke(this);
        }
    }

    // 적과 충돌 시 데미지 처리
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Tag가 "Enemy"인지 확인 (프로젝트 설정에 맞게 변경하세요)
        if (other.CompareTag("Enemy"))
        {
            IFighter receiver = other.GetComponent<IFighter>();
            if (receiver != null)
            {
                InGameEvent evt = new InGameEvent
                {
                    Type = InGameEvent.EventType.Combat,
                    Sender = _sender,
                    Receiver = receiver,
                    Amount = _damage
                };
                _combatSystem.AddInGameEvent(evt);
            }
        }
    }
}
