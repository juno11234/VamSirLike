using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;
using UnityEngine.AddressableAssets;

// IDisposable을 상속받아 VContainer가 알아서 메모리를 정리하도록 돕습니다.
public class DataManager : IDisposable
{
    private GameDataContainer _container;
    private readonly Dictionary<int, PlayerStat> _playerStatDict = new Dictionary<int, PlayerStat>();
    private readonly Dictionary<int, EnemyStat> _enemyStatDict = new Dictionary<int, EnemyStat>();
    private readonly Dictionary<int, SkillData> _playerSkillDict = new Dictionary<int, SkillData>();

    public async UniTask InitializeAsync(CancellationToken ct = default)
    {
        if (_container != null) return;

        _container = await Addressables.LoadAssetAsync<GameDataContainer>("GameData")
            .ToUniTask(cancellationToken: ct);

        Initialize();
        Debug.Log("데이터 로드 완료");
    }

    private void Initialize()
    {
        _playerStatDict.Clear();
        _enemyStatDict.Clear();

        // 리스트를 딕셔너리로
        foreach (PlayerStat stat in _container.PlayerStats)
            _playerStatDict[stat.id] = stat;

        foreach (EnemyStat stat in _container.EnemyStats)
            _enemyStatDict[stat.id] = stat;

        foreach (SkillData skillData in _container.SkillData)
        {
            _playerSkillDict[skillData.id] = skillData;
        }
    }

    public PlayerStat GetPlayerStat(int id)
    {
        if (_playerStatDict.TryGetValue(id, out var stat))
            return stat;

        Debug.LogError($"ID {id}에 해당하는 플레이어 스탯이 없습니다!");
        return null;
    }

    public EnemyStat GetEnemyStat(int id)
    {
        if (_enemyStatDict.TryGetValue(id, out var stat))
            return stat;

        Debug.LogError($"ID {id}에 해당하는 몬스터 스탯이 없습니다!");
        return null;
    }

    public SkillData GetSkillData(int id)
    {
        if (_playerSkillDict.TryGetValue(id, out var skillData))
            return skillData;

        Debug.LogError($"ID {id}에 해당하는 스킬 데이터가 없습니다!");
        return null;
    }

    public List<SkillData> GetSkillList()
    {
        List<SkillData> skillList = new List<SkillData>();
        foreach (var skills in _container.SkillData)
        {
            if (skills.job is JobType.Warrior)
            {
                skillList.Add(skills);
            }
        }

        return skillList;
    }

    // VContainer의 생명주기가 끝날 때(게임 종료, 씬 전환 등) 자동으로 호출됩니다.
    public void Dispose()
    {
        if (_container != null)
        {
            Addressables.Release(_container);
            _container = null;
        }

        _playerStatDict.Clear();
        _enemyStatDict.Clear();
        Debug.Log("DataManager 메모리 해제 완료");
    }
}