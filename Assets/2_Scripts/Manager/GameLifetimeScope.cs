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

        // 이씬만(Scope) .인터페이스명찰등록 .본명등록
        builder.Register<CombatSystem>(Lifetime.Scoped)
            .AsImplementedInterfaces()
            .AsSelf();

        // Register 대신 RegisterComponent로 변경!
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
    private readonly CombatSystem _combatSystem;

    private const int PlayerWarriorId = 3001;
    private const int EnemyRatId = 1001;
    private const int DaggerId = 4002;

    public GameInitializer(DataManager dataManager, SpawnManager spawnManager, PlayerController playerController,
        CombatSystem combatSystem)
    {
        _combatSystem = combatSystem;
        _dataManager = dataManager;
        _spawnManager = spawnManager;
        _playerController = playerController;
    }

    public async UniTask StartAsync(CancellationToken cancellationToken)
    {
        await _dataManager.InitializeAsync(cancellationToken);

        // [개선 포인트] return 대신 C# 표준 방식인 예외 던지기를 사용합니다.
        // VContainer가 이 예외를 감지하고 안전하게 초기화 프로세스를 중단해 줍니다.
        cancellationToken.ThrowIfCancellationRequested();

        // 플레이어 초기화
        PlayerStat warriorStat = _dataManager.GetPlayerStat(PlayerWarriorId);
        SkillData daggerData = _dataManager.GetSkillData(DaggerId);
        _playerController.Initialize(warriorStat, _combatSystem, daggerData);

        // 몬스터 스폰 매니저 초기화
        EnemyStat enemyStat = _dataManager.GetEnemyStat(EnemyRatId);

        await _spawnManager.InitAsync(_playerController.transform, enemyStat, _combatSystem, cancellationToken);

        Debug.Log("게임 초기화 완료");
    }
}