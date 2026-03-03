using VContainer;
using VContainer.Unity;

public class GameLifetimeScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        builder.Register<DataManager>(Lifetime.Singleton);

        builder.RegisterEntryPoint<GameInitializer>();
    }
}

public class GameInitializer : IStartable
{
    private readonly DataManager _dataManager;

    public GameInitializer(DataManager dataManager)
    {
        _dataManager = dataManager;
    }

    public async void Start()
    {
        // 게임 시작 시 데이터부터 로드
        await _dataManager.InitializeAsync();
        
        // 이후 몹 스포너 활성화나 플레이어 생성 로직 진행
    }
}
