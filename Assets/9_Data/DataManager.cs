using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class DataManager
{
    private GameDataContainer _container;

    // 비동기로 데이터 로드
    public async UniTask InitializeAsync(CancellationToken ct = default)
    {
        _container = await Addressables.LoadAssetAsync<GameDataContainer>("GameData")
            .ToUniTask(cancellationToken: ct);

        Debug.Log("데이터 로드 완료");
    }
    public EnemyStat GetEnemyStat(int id)=>_container.EnemyStats.Find(x=>x.id==id);
    public List<SkillData> GetSkillData() => _container.SkillData;
}