using System;
using UnityEngine;

public class ExpManager : MonoBehaviour
{
    [Header("Exp Settings")]
    [SerializeField] private ExpItem expGemPrefab;
    [SerializeField] private float baseRequiredExp = 100f;

    public float Score { get; private set; }
    public int CurrentLevel { get; private set; } = 1;
    public float CurrentExp { get; private set; } = 0;
    public float RequiredExp { get; private set; }

    public event Action<int> OnLevelUp;
    public event Action<float, float> OnExpChanged;

    public void Init()
    {
        Score = 0;
        RequiredExp = baseRequiredExp;
    }

    public void SpawnExp(Vector3 position, float expAmount)
    {
        ExpItem exp = Instantiate(expGemPrefab);
        exp.transform.position = position;
        exp.Init(expAmount, CollectExp);
    }

    private void CollectExp(ExpItem exp)
    {
        AddExp(exp.expAmount);
        Destroy(exp.gameObject);
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
