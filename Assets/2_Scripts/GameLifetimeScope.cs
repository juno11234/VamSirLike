using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Serialization;
using VContainer;
using VContainer.Unity;

public class GameLifetimeScope : LifetimeScope
{
    [SerializeField] private PlayerController playerController;
    [SerializeField] private SpawnManager spawnManager; 

    protected override void Configure(IContainerBuilder builder)
    {
        // DataManager가 순수 C# 클래스(MonoBehaviour 상속 X)
        builder.Register<DataManager>(Lifetime.Singleton); 
        
        // 2. Register 대신 RegisterComponent로 변경!
        builder.RegisterComponent(spawnManager); 
        builder.RegisterComponent(playerController);
        
        builder.RegisterEntryPoint<GameInitializer>();
    }
}

public class GameInitializer : IAsyncStartable
{
    private readonly DataManager _dataManager;
    private readonly SpawnManager _spawnManager;
    private readonly PlayerController _playerController;

    private const int PlayerWarriorId = 3001;
    private const int EnemyRatId = 1001;
    public GameInitializer(DataManager dataManager, SpawnManager spawnManager, PlayerController playerController)
    {
        _dataManager = dataManager;
        _spawnManager = spawnManager;
        _playerController = playerController;
    }

    public async UniTask StartAsync(CancellationToken cancellationToken)
    {
        // DataManager의 InitializeAsync()도 UniTask를 반환하도록 작성되어 있다고 가정합니다.
        // WithCancellation을 통해 씬이 종료되거나 스코프가 파괴될 때 비동기 대기를 안전하게 취소합니다.
        await _dataManager.InitializeAsync(cancellationToken);
        
        // 플레이어 초기화
        PlayerStat warriorStat = _dataManager.GetPlayerStat(PlayerWarriorId);
        _playerController.Initialize(warriorStat);

        // 몬스터 스폰 매니저 초기화
        EnemyStat enemyStat = _dataManager.GetEnemyStat(EnemyRatId);
        await _spawnManager.InitAsync(_playerController.transform, enemyStat, cancellationToken);
        
        Debug.Log("게임 초기화 완료");
    }
}