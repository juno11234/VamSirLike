using System;
using UnityEngine;

public class ExpItem : MonoBehaviour
{
    public float expAmount { get; private set; }
    private Action<ExpItem> _onCollectCallback;

    public void Init(float amount, Action<ExpItem> onCollectCallback)
    {
        expAmount = amount;
        _onCollectCallback = onCollectCallback;
    }

    public void Collect()
    {
        _onCollectCallback?.Invoke(this);
    }
}
