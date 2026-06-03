using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Cysharp.Threading.Tasks;
using System.Threading;
using Random = UnityEngine.Random;

[Serializable]
public class WaveData
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
    [SerializeField] private List<WaveData> waves;
    [SerializeField] private float spawnRadius = 15f;

    private PlayerController _player;
    private CombatSystem _combatSystem;
    private ExpManager _expManager;
    private DataManager _dataManager;
    private CancellationTokenSource _cts;

    private Dictionary<string, GameObject> _loadedPrefabs = new Dictionary<string, GameObject>();

    private float _playTime;

    public async UniTask InitAsync(PlayerController playerTransform, CombatSystem combatSystem,
        ExpManager expManager, DataManager dataManager, CancellationToken ct = default)
    {
        _expManager = expManager;
        _player = playerTransform;
        _combatSystem = combatSystem;
        _dataManager = dataManager;
        _playTime = 0;

        _cts = CancellationTokenSource.CreateLinkedTokenSource(ct);

        foreach (WaveData wave in waves)
        {
            if (_loadedPrefabs.ContainsKey(wave.prefabAddress) == false)
            {
                try
                {
                    GameObject prefab = await Addressables.LoadAssetAsync<GameObject>(wave.prefabAddress)
                        .ToUniTask(cancellationToken: _cts.Token);
                    _loadedPrefabs[wave.prefabAddress] = prefab;
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

        GameObject loadedPrefab = _loadedPrefabs[wave.prefabAddress];
        EnemyController enemy = Instantiate(loadedPrefab, transform).GetComponent<EnemyController>();
        _combatSystem.RegisterMonster(enemy);
        enemy.transform.position = spawnPos;

        EnemyStat stat = _dataManager.GetEnemyStat(wave.id);

        // 람다식: 적 사망 시 경험치 드롭 및 Destroy 처리를 캡처 변수 없이 인라인으로 표현하기 위해 사용
        float expDropAmount = wave.expDropAmount;
        Action<EnemyController> onDeath = (deadEnemy) =>
        {
            _expManager.SpawnExp(deadEnemy.transform.position, expDropAmount);
            _combatSystem.RemoveMonster(deadEnemy);
            Destroy(deadEnemy.gameObject);
        };

        enemy.Setup(_player, stat, onDeath, _combatSystem);
    }

    private void OnDestroy()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        foreach (KeyValuePair<string, GameObject> prefab in _loadedPrefabs)
        {
            Addressables.Release(prefab.Value);
        }
    }
}
