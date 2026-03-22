using System;
using UnityEngine;
using UnityEngine.Pool;

public class ExpManager : MonoBehaviour
{
    [Header("Exp Settings")]
    [SerializeField] private ExpItem expGemPrefab;
    [SerializeField] private float baseRequiredExp = 100f; // 레벨업 필요 경험치 기본값

    private ObjectPool<ExpItem> _expPool;
    private Transform _expContainer;

    public float Score { get; private set; }
    public int CurrentLevel { get; private set; } = 1;
    public float CurrentExp { get; private set; } = 0;
    public float RequiredExp { get; private set; }

    // UI나 시스템에서 구독할 이벤트
    public event Action<int> OnLevelUp;
    public event Action<float, float> OnExpChanged; 

    public void Init()
    {
        Score = 0;
        RequiredExp = baseRequiredExp;

        _expContainer = new GameObject("ExpContainer").transform;
        
        _expPool = new ObjectPool<ExpItem>(
            createFunc: () => Instantiate(expGemPrefab, _expContainer),
            actionOnGet: (obj) => obj.gameObject.SetActive(true),
            actionOnRelease: (obj) => obj.gameObject.SetActive(false),
            actionOnDestroy: (obj) => Destroy(obj.gameObject),
            defaultCapacity: 50,
            maxSize: 300
        );

        ExpItem[] prewarm = new ExpItem[50];
        for (int i = 0; i < 50; i++) prewarm[i] = _expPool.Get();
        for (int i = 0; i < 50; i++) _expPool.Release(prewarm[i]);
    }

    // 적이 죽을 때 호출할 함수
    public void SpawnExp(Vector3 position, float expAmount)
    {
        ExpItem exp = _expPool.Get();
        exp.transform.position = position;
        exp.Init(expAmount, CollectExp);
    }
    
    private void CollectExp(ExpItem exp)
    {
        _expPool.Release(exp); 
        AddExp(exp.expAmount); 
    }

    private void AddExp(float amount)
    {
        CurrentExp += amount;
        Score += amount;
        
        while (CurrentExp >= RequiredExp)
        {
            CurrentExp -= RequiredExp;
            CurrentLevel++;
            RequiredExp = CalculateNextRequiredExp(CurrentLevel);

            OnLevelUp?.Invoke(CurrentLevel); 
        }

        OnExpChanged?.Invoke(CurrentExp, RequiredExp);
    }

    private float CalculateNextRequiredExp(int level)
    {
        return baseRequiredExp * level * 1.5f;
    }
}