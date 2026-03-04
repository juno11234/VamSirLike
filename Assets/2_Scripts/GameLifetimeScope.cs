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
        // DataManager가 순수 C# 클래스(MonoBehaviour 상속 X)라면 이건 정답입니다!
        builder.Register<DataManager>(Lifetime.Singleton); 
        
        // 2. Register 대신 RegisterComponent로 변경!
        builder.RegisterComponent(spawnManager); 
        builder.RegisterComponent(playerController);
        builder.RegisterEntryPoint<GameInitializer>();
    }
}

public class GameInitializer : IStartable
{
    private readonly DataManager _dataManager;
    private readonly SpawnManager _spawnManager;
    private readonly PlayerController _playerController;

    public GameInitializer(DataManager dataManager, SpawnManager spawnManager, PlayerController playerController)
    {
        _dataManager = dataManager;
        _spawnManager = spawnManager;
        _playerController = playerController;
    }

    public async void Start()
    {
        // 게임 시작 시 데이터부터 로드
        await _dataManager.InitializeAsync();

        // 이후 몹 스포너 활성화나 플레이어 생성 로직 진행
        PlayerStat warriorStat = _dataManager.GetPlayerStat(3001);
        _playerController.Initialize(warriorStat);

        EnemyStat enemyStat = _dataManager.GetEnemyStat(1001);
        _spawnManager.Init(_playerController.transform, enemyStat);
    }
}