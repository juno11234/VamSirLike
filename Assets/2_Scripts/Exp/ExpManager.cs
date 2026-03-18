using System;
using UnityEngine;
using UnityEngine.Pool;

public class ExpManager : MonoBehaviour
{
    [Header("Exp Settings")]
    [SerializeField] private ExpItem expGemPrefab;
    [SerializeField] private float baseRequiredExp = 100f; // 레벨업 필요 경험치 기본값

    private ObjectPool<ExpItem> _gemPool;
    private Transform _gemContainer;

    public float Score { get; private set; }
    public int CurrentLevel { get; private set; } = 1;
    public float CurrentExp { get; private set; } = 0;
    public float RequiredExp { get; private set; }

    // UI나 시스템에서 구독할 이벤트
    public event Action<int> OnLevelUp;
    public event Action<float, float> OnExpChanged; // 현재 경험치, 필요 경험치

    public void Init()
    {
        Score = 0;
        RequiredExp = baseRequiredExp;

        _gemContainer = new GameObject("ExpGemContainer").transform;
        _gemPool = new ObjectPool<ExpItem>(
            createFunc: () => Instantiate(expGemPrefab, _gemContainer),
            actionOnGet: (obj) => obj.gameObject.SetActive(true),
            actionOnRelease: (obj) => obj.gameObject.SetActive(false),
            actionOnDestroy: (obj) => Destroy(obj.gameObject),
            defaultCapacity: 50,
            maxSize: 300
        );

        // Pre-warm (미리 50개 생성)
        ExpItem[] prewarm = new ExpItem[50];
        for (int i = 0; i < 50; i++) prewarm[i] = _gemPool.Get();
        for (int i = 0; i < 50; i++) _gemPool.Release(prewarm[i]);
    }

    // 적이 죽을 때 호출할 함수
    public void SpawnGem(Vector3 position, float expAmount)
    {
        ExpItem gem = _gemPool.Get();
        gem.transform.position = position;
        gem.Init(expAmount, CollectGem);
    }

    // 보석 획득 처리
    private void CollectGem(ExpItem gem)
    {
        _gemPool.Release(gem); // 풀에 반납
        AddExp(gem.expAmount); // 경험치 추가
    }

    private void AddExp(float amount)
    {
        CurrentExp += amount;
        Score += amount;
        // 레벨업 체크 (경험치를 초과해서 얻었을 경우 연속 레벨업 처리)
        while (CurrentExp >= RequiredExp)
        {
            CurrentExp -= RequiredExp;
            CurrentLevel++;
            RequiredExp = CalculateNextRequiredExp(CurrentLevel);

            OnLevelUp?.Invoke(CurrentLevel); // 레벨업 이벤트 발생!
        }

        OnExpChanged?.Invoke(CurrentExp, RequiredExp);
    }

    // 다음 레벨 필요 경험치 계산 공식 (임시)
    private float CalculateNextRequiredExp(int level)
    {
        return baseRequiredExp * level * 1.5f;
    }
}