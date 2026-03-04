using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class DataManager
{
    private GameDataContainer _container;
    private Dictionary<int, PlayerStat> _playerStatDict = new Dictionary<int, PlayerStat>();
    private Dictionary<int, EnemyStat> _enemyStatDict = new Dictionary<int, EnemyStat>();
    
    // 초기화할 때 (게임 시작 시 한 번만 실행)
    public void Initialize()
    {
        // 리스트를 딕셔너리로 미리 싹 바꿔둡니다.
        foreach (var stat in _container.PlayerStats)
            _playerStatDict[stat.id] = stat;
        
        foreach (var stat in _container.EnemyStats)
            _enemyStatDict[stat.id] = stat;
    }
    // 비동기로 데이터 로드
    public async UniTask InitializeAsync(CancellationToken ct = default)
    {
        _container = await Addressables.LoadAssetAsync<GameDataContainer>("GameData")
            .ToUniTask(cancellationToken: ct);

        Initialize();
        Debug.Log("데이터 로드 완료");
    }
    public PlayerStat GetPlayerStat(int id)
    {
        // 루프도 없고, 람다도 없고, 가비지도 없습니다. 빛의 속도( O(1) )로 가져옵니다!
        if (_playerStatDict.TryGetValue(id, out var stat))
            return stat;
        
        Debug.LogError($"ID {id}에 해당하는 플레이어 스탯이 없습니다!");
        return null;
    }
    public EnemyStat GetEnemyStat(int id)
    {
        if(_enemyStatDict.TryGetValue(id, out var stat))
            return stat;
        
        Debug.LogError($"ID {id}에 해당하는 몬스터 스탯이 없습니다!");
        return null;
    }
    public List<SkillData> GetSkillData() => _container.SkillData;
}