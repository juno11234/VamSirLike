using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Pool;
using Cysharp.Threading.Tasks;
using System.Threading;

public class SpawnManager : MonoBehaviour
{
    [Header("Spawn Settings")] [SerializeField]
    private string enemyPrefabAddress = "Enemy_Rat"; // 어드레서블 키

    [SerializeField] private float spawnRadius = 15f; // 플레이어로부터 얼마나 멀리서 소환할지
    [SerializeField] private float spawnInterval = 0.5f; // 몇 초마다 소환할지

    private EnemyStat _enemyStat;
    private Transform _playerTransform;
    private GameObject _enemyPrefab;
    private ObjectPool<GameObject> _enemyPool;
    private CancellationTokenSource _cts;

    // VContainer나 외부에서 플레이어 정보를 주입받는다고 가정
    public void Init(Transform playerTransform, EnemyStat enemyStat)
    {
        _playerTransform = playerTransform;
        _cts = new CancellationTokenSource();
        _enemyStat = enemyStat;
        StartSpawnSystemAsync().Forget();
    }

    private async UniTaskVoid StartSpawnSystemAsync()
    {
        // 1. 어드레서블로 몬스터 프리팹 비동기 로드
        _enemyPrefab = await Addressables.LoadAssetAsync<GameObject>(enemyPrefabAddress).ToUniTask();

        // 2. 오브젝트 풀 초기화 (생성, 대여, 반납, 파괴 시 할 일 정의)
        _enemyPool = new ObjectPool<GameObject>(
            createFunc: () => Instantiate(_enemyPrefab, transform),
            actionOnGet: (enemy) => enemy.SetActive(true),
            actionOnRelease: (enemy) => enemy.SetActive(false),
            actionOnDestroy: (enemy) => Destroy(enemy),
            defaultCapacity: 50,
            maxSize: 500 // 최대 500마리까지만 풀에서 관리
        );

        // 3. 무한 스폰 루프 시작
        SpawnLoopAsync(_cts.Token).Forget();
    }

    private async UniTask SpawnLoopAsync(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            SpawnEnemy();

            // spawnInterval(0.5초) 만큼 대기 (Update문 필요 없음!)
            await UniTask.Delay(System.TimeSpan.FromSeconds(spawnInterval), cancellationToken: token);
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
            controller.Setup(_playerTransform, _enemyStat, _enemyPool);
        }
    }

    private void OnDestroy()
    {
        _cts?.Cancel();
        _cts?.Dispose();
    }
}