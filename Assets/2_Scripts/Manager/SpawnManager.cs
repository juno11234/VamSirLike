using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Pool;
using Cysharp.Threading.Tasks;
using System.Threading;
using Random = UnityEngine.Random;

public class SpawnManager : MonoBehaviour
{
    // todo: 지금은 한종류지만 다양한 몬스터 종류 추가
    [Header("Spawn Settings")] [SerializeField]
    private string enemyPrefabAddress = "Enemy_Rat";

    [SerializeField] private float spawnRadius = 15f;
    [SerializeField] private float spawnInterval = 0.5f;

    private EnemyStat _enemyStat;
    private Transform _playerTransform;
    private GameObject _enemyPrefab;
    private ObjectPool<GameObject> _enemyPool;
    private CancellationTokenSource _cts;

    private Action<GameObject> _releaseEnemyAction;

    // 1. Init을 UniTask로 변경하여 로딩 완료를 외부에서 대기할 수 있게 합니다.
    public async UniTask InitAsync(Transform playerTransform, EnemyStat enemyStat, CancellationToken ct = default)
    {
        _playerTransform = playerTransform;
        _enemyStat = enemyStat;


        // 외부 토큰과 연결하여 씬 전환 시 안전하게 같이 취소되도록 합니다.
        _cts = CancellationTokenSource.CreateLinkedTokenSource(ct);

        try
        {
            // 프리팹 로드가 끝날 때까지 대기
            _enemyPrefab = await Addressables.LoadAssetAsync<GameObject>(enemyPrefabAddress)
                .ToUniTask(cancellationToken: _cts.Token);

            _enemyPool = new ObjectPool<GameObject>(
                createFunc: () => Instantiate(_enemyPrefab, transform),
                actionOnGet: (enemy) => enemy.SetActive(true),
                actionOnRelease: (enemy) => enemy.SetActive(false),
                actionOnDestroy: (enemy) => Destroy(enemy),
                defaultCapacity: 50,
                maxSize: 500
            );
            _releaseEnemyAction = ReleaseEnemyToPool;
            // 로딩과 초기화가 끝났으므로 스폰 루프 시작
            SpawnLoopAsync(_cts.Token).Forget();
        }
        catch (OperationCanceledException)
        {
            Debug.Log("SpawnManager 초기화가 취소되었습니다.");
        }
    }

    private async UniTask SpawnLoopAsync(CancellationToken token)
    {
        while (token.IsCancellationRequested == false)
        {
            SpawnEnemy();

            // spawnInterval(0.5초) 만큼 대기 (Update문 필요 없음!)
            await UniTask.Delay(TimeSpan.FromSeconds(spawnInterval), cancellationToken: token);
        }
    }

    private void SpawnEnemy()
    {
        if (_playerTransform == null) return;

        // 플레이어 주변 360도 무작위 방향의 테두리 위치 계산
        Vector2 randomDir = Random.insideUnitCircle.normalized;
        Vector3 spawnPos = _playerTransform.position + (Vector3)(randomDir * spawnRadius);

        // 풀에서 적을 하나 꺼내고 위치 지정
        GameObject enemy = _enemyPool.Get();
        enemy.transform.position = spawnPos;

        if (enemy.TryGetComponent(out EnemyController controller))
        {
            // 2. OOP 설계: 풀 전체를 넘기지 않고, '풀로 되돌아가는 행동(Action)'만 넘겨줍니다.
            controller.Setup(_playerTransform, _enemyStat, _releaseEnemyAction);
        }
    }

   // 풀에 반납하는 전용 메서드
    private void ReleaseEnemyToPool(GameObject enemy)
    {
        _enemyPool.Release(enemy);
    }

    private void OnDestroy()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        // 3. 메모리 누수 방지: 로드했던 프리팹 원본 에셋 해제
        if (_enemyPrefab != null)
        {
            Addressables.Release(_enemyPrefab);
        }
    }
}