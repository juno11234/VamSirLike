using System.Collections;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class GameLifetimeScope : LifetimeScope
{
    [SerializeField] private PlayerController playerController;
    [SerializeField] private SpawnManager spawnManager;
    [SerializeField] private ExpManager expManager;
    [SerializeField] private UIManager uiManager;
    [SerializeField] private LevelUpUI levelUpUI;

    protected override void Configure(IContainerBuilder builder)
    {
        // 순수 C# 클래스
        builder.Register<DataManager>(Lifetime.Singleton);

        // 이씬만(Scope) .인터페이스명찰등록 .본명등록
        builder.Register<CombatSystem>(Lifetime.Scoped)
            .AsImplementedInterfaces()
            .AsSelf();

        // Register 대신 RegisterComponent
        builder.RegisterComponent(spawnManager);
        builder.RegisterComponent(playerController);
        builder.RegisterComponent(expManager);
        builder.RegisterComponent(uiManager);
        builder.RegisterComponent(levelUpUI);
        // GameInitializer가 StartCoroutine 호출을 위해 주입받음
        builder.RegisterComponent(this);

        builder.RegisterEntryPoint<GameInitializer>();
    }
}

public class GameInitializer : IStartable
{
    private readonly DataManager _dataManager;
    private readonly SpawnManager _spawnManager;
    private readonly PlayerController _playerController;
    private readonly CombatSystem _combatSystem;
    private readonly ExpManager _expManager;
    private readonly UIManager _uiManager;
    private readonly GameLifetimeScope _scope;

    private const int PlayerWarriorId = 3001;

    public GameInitializer(DataManager dataManager, SpawnManager spawnManager,
        PlayerController playerController, CombatSystem combatSystem,
        ExpManager expManager, UIManager uiManager, GameLifetimeScope scope)
    {
        _combatSystem = combatSystem;
        _dataManager = dataManager;
        _spawnManager = spawnManager;
        _playerController = playerController;
        _expManager = expManager;
        _uiManager = uiManager;
        _scope = scope;
    }

    public void Start()
    {
        _scope.StartCoroutine(InitCoroutine());
    }

    private IEnumerator InitCoroutine()
    {
        yield return _dataManager.InitializeCoroutine();

        PlayerStat warriorStat = _dataManager.GetPlayerStat(PlayerWarriorId);
        _playerController.Initialize(warriorStat, _combatSystem, _dataManager);

        yield return _spawnManager.InitCoroutine(_playerController, _combatSystem, _expManager, _dataManager);

        _expManager.Init();

        Debug.Log("게임 초기화 완료");
    }
}
