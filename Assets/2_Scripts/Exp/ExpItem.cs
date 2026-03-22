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
        if (_isFlying == false || _target == null) return;

        transform.position = Vector3.MoveTowards(transform.position, _target.position, _flySpeed * Time.deltaTime);
        Vector3 direction = _target.position - transform.position;
        if (direction.sqrMagnitude < 0.5f * 0.5f)
        {
            _isFlying = false; 
            _onCollectCallback?.Invoke(this);
        }
    }
}