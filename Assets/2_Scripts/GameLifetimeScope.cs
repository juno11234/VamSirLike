using UnityEngine;
using UnityEngine.Serialization;
using VContainer;
using VContainer.Unity;

public class GameLifetimeScope : LifetimeScope
{
    [SerializeField] private PlayerController playerController;

    protected override void Configure(IContainerBuilder builder)
    {
        builder.Register<DataManager>(Lifetime.Singleton);
        builder.RegisterComponent(playerController);
        builder.RegisterEntryPoint<GameInitializer>();
    }
}

public class GameInitializer : IStartable
{
    private readonly DataManager _dataManager;
    private readonly PlayerController _playerController;

    public GameInitializer(DataManager dataManager, PlayerController playerController)
    {
        _dataManager = dataManager;
        _playerController = playerController;
    }

    public async void Start()
    {
        // 게임 시작 시 데이터부터 로드
        await _dataManager.InitializeAsync();

        // 이후 몹 스포너 활성화나 플레이어 생성 로직 진행
        PlayerStat warriorStat = _dataManager.GetPlayerStat(3001);
        _playerController.Initialize(warriorStat);
        
    }
}