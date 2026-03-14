using System;
using UnityEngine;

public class ExpItem : MonoBehaviour
{
    public float expAmount { get; private set; }
    private Action<ExpItem> _onCollectCallback;

    private Transform _target;
    private bool _isFlying = false;
    private float _flySpeed = 15f; 

    public void Init(float amount, Action<ExpItem> onCollectCallback)
    {
        expAmount = amount;
        _onCollectCallback = onCollectCallback;
        
        // 풀에서 꺼낼 때 초기화
        _isFlying = false; 
        _target = null;
    }
    public void SetTarget(Transform target)
    {
        _target = target;
        _isFlying = true;
    }

    private void Update()
    {
        if (!_isFlying || _target == null) return;

        // 플레이어를 향해 이동
        transform.position = Vector3.MoveTowards(transform.position, _target.position, _flySpeed * Time.deltaTime);

        // 플레이어 몸(0.5f 거리)에 닿으면 그제서야 획득!
        if (Vector3.Distance(transform.position, _target.position) < 0.5f)
        {
            _isFlying = false; // 중복 획득 방지
            _onCollectCallback?.Invoke(this);
        }
    }
}
