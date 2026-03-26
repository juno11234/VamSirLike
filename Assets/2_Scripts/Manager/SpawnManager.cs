using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Pool;
using Cysharp.Threading.Tasks;
using System.Threading;
using Random = UnityEngine.Random;

[Serializable]
public struct WaveData
{
    public int id;
    public float waveStartTime;
    public float waveEndTime;
    public string prefabAddress;
    public float spawnInterval;
    public float expDropAmount;
}

public class SpawnManager : MonoBehaviour
{
    [Header("Wave Settings")]
    [SerializeField] private List<WaveData> waves; // 에디터에서 웨이브를 여러 개 세팅할 수 있는 리스트
    [SerializeField] private float spawnRadius = 15f;

    private PlayerController _player;
    private CombatSystem _combatSystem;
    private ExpManager _expManager;
    private DataManager _dataManager;
    private CancellationTokenSource _cts;

    private Dictionary<string, GameObject> _loadedPrefabs = new();
    private Dictionary<string, ObjectPool<EnemyController>> _enemyPools = new();
    private Dictionary<string, Action<EnemyController>> _releaseActions = new();

    private float _playTime;

    // 1. Init을 UniTask로 변경하여 로딩 완료를 외부에서 대기할 수 있게 합니다.
    public async UniTask InitAsync(PlayerController playerTransform, CombatSystem combatSystem,
        ExpManager expManager, DataManager dataManager, CancellationToken ct = default)
    {
        _expManager = expManager;
        _player = playerTransform;
        _combatSystem = combatSystem;
        _dataManager = dataManager;
        _playTime = 0;

        // 외부 토큰과 연결하여 씬 전환 시 안전하게 같이 취소되도록 합니다.
        _cts = CancellationTokenSource.CreateLinkedTokenSource(ct);

        // 등록된 모든 웨이브를 순회하며 필요한 프리팹을 전부 로드하고 풀을 만듭니다.
        foreach (WaveData wave in waves)
        {
            // 이미 로드한 프리팹이라면 건너뜀 (예: 1웨이브와 3웨이브가 같은 쥐(Rat)일 경우)
            if (_loadedPrefabs.ContainsKey(wave.prefabAddress)==false)
            {
                try
                {
                    GameObject prefab = await Addressables.LoadAssetAsync<GameObject>(wave.prefabAddress)
                        .ToUniTask(cancellationToken: _cts.Token);
                    _loadedPrefabs[wave.prefabAddress] = prefab;

                    // 해당 프리팹 전용 오브젝트 풀 생성
                    var pool = new ObjectPool<EnemyController>(
                        createFunc: () =>
                        {
                            EnemyController controller = Instantiate(prefab, transform).GetComponent<EnemyController>();
                            _combatSystem.RegisterMonster(controller);
                            return controller;
                        },
                        actionOnGet: (enemy) => enemy.gameObject.SetActive(true),
                        actionOnRelease: (enemy) => enemy.gameObject.SetActive(false),
                        actionOnDestroy: (enemy) =>
                        {
                            _combatSystem.RemoveMonster(enemy);
                            Destroy(enemy.gameObject);
                        },
                        defaultCapacity: 50,
                        maxSize: 500
                    );
                    _enemyPools[wave.prefabAddress] = pool;

                    // 해당 프리팹 전용 반납 함수 (가비지 컬렉션 최적화)
                    Action<EnemyController> releaseAction = (enemy) =>
                    {
                        _expManager.SpawnExp(enemy.transform.position, wave.expDropAmount);
                        pool.Release(enemy);
                    };
                    _releaseActions[wave.prefabAddress] = releaseAction;

                    // 프리웜(Pre-warm): 렉 방지를 위해 미리 50개씩 찍어내기
                    EnemyController[] prewarmArray = new EnemyController[50];
                    for (int i = 0; i < 50; i++) prewarmArray[i] = pool.Get();
                    for (int i = 0; i < 50; i++) pool.Release(prewarmArray[i]);
                }
                catch (OperationCanceledException)
                {
                    Debug.Log($"초기화 취소됨: {wave.prefabAddress}");
                    return;
                }
            }
        }

        foreach (WaveData wave in waves)
        {
            SpawnLoopAsync(wave, _cts.Token).Forget();
        }
    }

    private void Update()
    {
        // 타임 스케일의 영향을 받는 실제 게임 시간을 누적 (레벨업 창이 뜨면 알아서 멈춥니다!)
        _playTime += Time.deltaTime;
    }

    private async UniTask SpawnLoopAsync(WaveData wave, CancellationToken token)
    {
        await UniTask.WaitUntil(() => _playTime >= wave.waveStartTime, cancellationToken: token);

        while (token.IsCancellationRequested == false)
        {
            if (wave.waveEndTime > 0 && _playTime > wave.waveEndTime)
            {
                break; 
            }

            SpawnEnemy(wave);
            
            int delayMs = Mathf.RoundToInt(wave.spawnInterval * 1000f);
            await UniTask.Delay(delayMs, ignoreTimeScale: false, cancellationToken: token);
        }
    }

    private void SpawnEnemy(WaveData wave)
    {
        if (_player == null) return;

        Vector2 randomDir = Random.insideUnitCircle.normalized;
        Vector3 spawnPos = _player.transform.position + (Vector3)(randomDir * spawnRadius);

        ObjectPool<EnemyController> pool = _enemyPools[wave.prefabAddress];
        EnemyController enemy = pool.Get();
        enemy.transform.position = spawnPos;
        EnemyStat stat = _dataManager.GetEnemyStat(wave.id);
        
        enemy.Setup(_player, stat, _releaseActions[wave.prefabAddress], _combatSystem);
    }

    private void OnDestroy()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        // 로드했던 모든 프리팹 메모리에서 깔끔하게 해제
        foreach (var prefab in _loadedPrefabs.Values)
        {
            Addressables.Release(prefab);
        }
    }
}