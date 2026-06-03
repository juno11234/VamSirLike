using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
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
    private Dictionary<string, GameObject> _loadedPrefabs = new Dictionary<string, GameObject>();

    private float _playTime;

    public IEnumerator InitCoroutine(PlayerController player, CombatSystem combatSystem,
        ExpManager expManager, DataManager dataManager)
    {
        _expManager = expManager;
        _player = player;
        _combatSystem = combatSystem;
        _dataManager = dataManager;
        _playTime = 0;

        foreach (WaveData wave in waves)
        {
            if (_loadedPrefabs.ContainsKey(wave.prefabAddress) == false)
            {
                AsyncOperationHandle<GameObject> handle =
                    Addressables.LoadAssetAsync<GameObject>(wave.prefabAddress);
                yield return handle;
                _loadedPrefabs[wave.prefabAddress] = handle.Result;
            }
        }

        foreach (WaveData wave in waves)
        {
            StartCoroutine(SpawnLoopCoroutine(wave));
        }
    }

    private void Update()
    {
        _playTime += Time.deltaTime;
    }

    private IEnumerator SpawnLoopCoroutine(WaveData wave)
    {
        // 람다식: WaitUntil에 조건을 인라인으로 전달하기 위해 사용
        yield return new WaitUntil(() => _playTime >= wave.waveStartTime);

        while (true)
        {
            if (wave.waveEndTime > 0 && _playTime > wave.waveEndTime)
            {
                yield break;
            }

            SpawnEnemy(wave);
            yield return new WaitForSeconds(wave.spawnInterval);
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
        StopAllCoroutines();
        foreach (KeyValuePair<string, GameObject> prefab in _loadedPrefabs)
        {
            Addressables.Release(prefab.Value);
        }
    }
}
