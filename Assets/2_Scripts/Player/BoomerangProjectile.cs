using UnityEngine;

public class BoomerangProjectile : MonoBehaviour
{
    private enum State
    {
        FlyingOut,
        Returning
    }

    private State _currentState;

    [Header("Boomerang Settings")]
    [SerializeField] private float flySpeed = 10f;
    [SerializeField] private float returnSpeed = 15f;
    [SerializeField] private float maxDistance = 8f;
    [SerializeField] private float spinSpeed = 1000f;
    [SerializeField] private LayerMask enemyLayer;

    private Vector3 _startPos;
    private Vector3 _flyDirection;

    private Transform _playerTransform;
    private CombatSystem _combatSystem;
    private IFighter _sender;
    private float _damage;

    private float _hitRadius = 0.5f;

    public void Init(CombatSystem combatSystem, IFighter sender, Transform player, Vector3 direction, float damage)
    {
        _combatSystem = combatSystem;
        _sender = sender;
        _playerTransform = player;

        _flyDirection = direction;
        _damage = damage;

        _startPos = transform.position;
        _currentState = State.FlyingOut;
    }

    private void Update()
    {
        transform.Rotate(0, 0, spinSpeed * Time.deltaTime);

        if (_currentState == State.FlyingOut)
        {
            transform.position += _flyDirection * (flySpeed * Time.deltaTime);
            float currentDistance = Vector3.Distance(_startPos, transform.position);
            if (currentDistance >= maxDistance)
            {
                _currentState = State.Returning;
            }
        }
        else if (_currentState == State.Returning)
        {
            if (_playerTransform == null)
            {
                Destroy(gameObject);
                return;
            }

            Vector3 returnDir = (_playerTransform.position - transform.position).normalized;
            transform.position += returnDir * (returnSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.position, _playerTransform.position) < 0.5f)
            {
                Destroy(gameObject);
                return;
            }
        }

        Collider2D[] results = Physics2D.OverlapCircleAll(transform.position, _hitRadius, enemyLayer);
        for (int i = 0; i < results.Length; i++)
        {
            IFighter targetMonster = _combatSystem.GetMonster(results[i]);
            if (targetMonster != null)
            {
                InGameEvent evt = new InGameEvent
                {
                    Type = InGameEvent.EventType.Combat,
                    Sender = _sender,
                    Receiver = targetMonster,
                    Amount = _damage
                };
                _combatSystem.AddInGameEvent(evt);
            }
        }
    }
}
