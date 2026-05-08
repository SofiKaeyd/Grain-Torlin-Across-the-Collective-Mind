using Grain_Torlin_Across_the_Collective_Mind.Utils;
using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    private enum State
    {
        Idle,
        Roaming,
        Chasing,
        Attacking,
        Death
    }

    public static EnemyAI Instance { get; private set; }

    [SerializeField] private State _startingState;

    [Header("Movement & Patrol")]
    [SerializeField] private List<Transform> _patrolPoints; // ����� ��������������
    //[SerializeField] private float _roamingDistanceMax = 7f;
    //[SerializeField] private float _roamingDistanceMin = 3f;
    [SerializeField] private float _roamingTimerMax = 2f;

    [Header("Detection Settings")]
    [SerializeField] private LayerMask _obstacleMask; // ���� ����/�����������
    [SerializeField] private LayerMask _playerMask;   // ���� ������
    [SerializeField] private bool _isChasingEnemy = true;
    //[SerializeField] private float _chasingDistance = 4f;
    [SerializeField] private float _chasingSpeedMultiplier = 1.15f;
    [SerializeField] private float _detectRadius = 5f;
    [SerializeField] private float _loseRadius = 10f;
    [SerializeField] private float _chasingDistanceSqr = 10f;
    [SerializeField] private float _maxActiveDistanceSqr = 50f;

    [Header("Attack Settings")]
    [SerializeField] private float _knockbackForce = 10f;
    [SerializeField] private float _knockbackDuration = 0.2f;
    [SerializeField] private float _attackCooldown = 1.0f;

    private Rigidbody2D _rb;

    private static int _enemiesChasingCount = 0;
    public static event Action<bool> OnPlayerSpotted; // �������: true � ���� �������������, false � ����������� � �������

    private NavMeshAgent _navMeshAgent;
    private State _currentState;
    private float _roamingTimer; // ����� ��������� (��: ������� �� �����)
    private int _currentPatrolIndex = 0;

    private bool _isCurrentlyDetectingPlayer = false; // ����� �� ������ ���� ���� ������
    private float _roamingSpeed;
    private float _chasingSpeed;
    private float _nextCheckDirectionTime = 0f; // ��� �������� �������� � ����������� ��������
    private float _checkDirectionDuration = 0.1f; // ��� ����� ���� �������� ����������� �������� (0.1f = 10 ��� � ���)
    private Vector3 _lastPosition;
    private float _pathUpdateTimer = 0.2f; // ��������� ���� 5 ��� � �������
    private float _nextPathUpdateTime;

    //[SerializeField] private bool _isAttackingEnemy = false;
    //[SerializeField] private float _attackingDistance = 2f;
    //[SerializeField] private float _attackRate = 2f;
    private float _nextAttackTime = 0f;
    private CancellationTokenSource _knockbackCTS;
    private bool _isKnockedBack = false;

    private Vector3 _roamPosition;
    private Vector3 _startingPosition;

    private List<Vector3> _fixedPatrolPositions = new List<Vector3>();

    public event EventHandler OnEnemyAttack;

    public bool IsRunning => _navMeshAgent.velocity.sqrMagnitude > 0.01f;

    private void Awake()
    {
        Instance = this;

        foreach (Transform point in _patrolPoints)
        {
            _fixedPatrolPositions.Add(point.position);
        }

        _rb = GetComponent<Rigidbody2D>();

        _navMeshAgent = GetComponent<NavMeshAgent>();
        _navMeshAgent.updateRotation = false; // ����� ���� �� �������� �� ����� ��������
        _navMeshAgent.updateUpAxis = false; // ����� ����������(y) ������� �� ������ �� ���������� �������

        _currentState = _startingState;
        _roamingSpeed = _navMeshAgent.speed;
        _chasingSpeed = _navMeshAgent.speed * _chasingSpeedMultiplier;
        //_startingPosition = transform.position;

        //_chasingDistanceSqr *= _chasingDistanceSqr;
        _maxActiveDistanceSqr *= _maxActiveDistanceSqr;

        OnPlayerSpotted += HandleGlobalPlayerSpotted;
    }

    private void Update()
    {
        // Если враг в нокбеке или агент выключен - ничего не делать
        if (_isKnockedBack || !_navMeshAgent.enabled) return;

        UpdateDetection();
        StateHandler();
        MovementDirectionHandler();
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (((1 << other.gameObject.layer) & _playerMask) == 0) return;

        if (Time.time < _nextAttackTime) return;        

        if (Player.Instance != null && Player.Instance.Health > 0)
        {
            if (Player.Instance.IsIFrames) return;            

            _nextAttackTime = Time.time + _attackCooldown;
            Player.Instance.TakeDamage(1);
            Debug.Log("Player.Instance.TakeDamage(1)");

            Vector2 direction = (transform.position - other.transform.position).normalized;
            if (direction == Vector2.zero) direction = Vector2.up;
            ApplyKnockbackAsync(direction).Forget();
        }
    }

    // ����� ������� ��������, ����� �������� ����
    public void UpdatePatrolPath(List<Transform> newPoints)
    {
        _patrolPoints = newPoints;
        _currentPatrolIndex = 0;
    }

    private void StateHandler()
    {
        switch (_currentState)
        {
            case State.Roaming:
                _roamingTimer -= Time.deltaTime;
                if (_roamingTimer <= 0)
                {
                    //Roaming();
                    Patrol();
                }
                //CheckCurrentState();
                break;
            case State.Chasing:
                ChasingTarget();
                //CheckCurrentState();
                break;
            case State.Attacking:
                //AttackingTarget();
                //CheckCurrentState();
                break;
            case State.Death:
                break;
            default:
            case State.Idle:
                break;
        }
    }

    private void UpdateDetection()
    {
        if (Player.Instance == null) return;

        float distToPlayer = Vector3.Distance(transform.position, Player.Instance.transform.position);
        bool canSeePhysically = false;

        // ���������� �����: ���� ��� �����, ���������� ������ ������, ���� ��� - ������ �����������
        //float currentThreshold = _isCurrentlyDetectingPlayer ? _loseRadius : _detectRadius;

        if (distToPlayer <= _detectRadius)
        {
            Vector2 directionToPlayer = (Player.Instance.transform.position - transform.position).normalized;
            RaycastHit2D hit = Physics2D.Raycast(transform.position, directionToPlayer, _detectRadius, _obstacleMask | _playerMask);

            if (hit.collider != null && ((1 << hit.collider.gameObject.layer) & _playerMask) != 0)
            {
                canSeePhysically = true;
            }
        }

        // ������ ������������ ���������� ��������� � ����� ����������� �������
        if (canSeePhysically && !_isCurrentlyDetectingPlayer)
        {
            _isCurrentlyDetectingPlayer = true;
            _enemiesChasingCount++;
            if (_enemiesChasingCount == 1) OnPlayerSpotted?.Invoke(true);
        }
        else if (distToPlayer >= _loseRadius && _isCurrentlyDetectingPlayer)
        {
            _isCurrentlyDetectingPlayer = false;
            _enemiesChasingCount--;
            if (_enemiesChasingCount == 0) OnPlayerSpotted?.Invoke(false);
        }
    }

    public void HandleGlobalPlayerSpotted(bool playerSpotted)
    {
        if (!_isChasingEnemy) return;

        if (playerSpotted)
        {
            float distSqr = (transform.position - Player.Instance.transform.position).sqrMagnitude;

            if (distSqr <= _maxActiveDistanceSqr)
            {
                ApplyState(State.Chasing);
            }
        }
        else
        {
            ApplyState(State.Roaming);
        }
    }

    private void Patrol()
    {
        if (_fixedPatrolPositions == null || _fixedPatrolPositions.Count == 0) return;

        _navMeshAgent.SetDestination(_fixedPatrolPositions[_currentPatrolIndex]);

        // ���� ����� �� �����
        if (!_navMeshAgent.pathPending && _navMeshAgent.remainingDistance <= _navMeshAgent.stoppingDistance)
        {
            _currentPatrolIndex = (_currentPatrolIndex + 1) % _fixedPatrolPositions.Count;
            _roamingTimer = _roamingTimerMax; // ���� ����� ��������� ������
        }
    }

    private void ChasingTarget()
    {
        if (Time.time > _nextPathUpdateTime)
        {
            _nextPathUpdateTime = Time.time + _pathUpdateTimer;
            _navMeshAgent.SetDestination(Player.Instance.transform.position);
        }
    }

    private void ApplyState(State newState)
    {
        if (newState == State.Chasing)
        {
            _navMeshAgent.ResetPath();
            _navMeshAgent.speed = _chasingSpeed;
        }
        else if (newState == State.Roaming)
        {
            //_roamingTimer = 0f; // ���� �� ���������� ������, �� ����� �������� � ��������� ���� �����(!) �����-�� ����� ������, ���� ������ �� ���������, ����� ���, ��� ������ ��������
            _navMeshAgent.speed = _roamingSpeed;
        }
        else if (newState == State.Attacking)
        {
            _navMeshAgent.ResetPath();
        }

        _currentState = newState;
    }

    private void MovementDirectionHandler()
    {
        if (Time.time > _nextAttackTime) // ���� ����� ��������� �������� ���������
        {
            if (IsRunning)
            {
                ChangeFacingDirection(_lastPosition, transform.position);
            }
            else if (_currentState == State.Attacking)
            {
                ChangeFacingDirection(transform.position, Player.Instance.transform.position);
            }

            _lastPosition = transform.position;
            _nextCheckDirectionTime = Time.time + _checkDirectionDuration;
        }
    }

    // ������� � ������� targetPosition.
    private void ChangeFacingDirection(Vector3 sourcePosition, Vector3 targetPosition)
    {
        if (Mathf.Abs(sourcePosition.x - targetPosition.x) < 0.01f) return;

        if (sourcePosition.x > targetPosition.x)
        {
            transform.rotation = Quaternion.Euler(0, -180, 0); // ��� �������� ������� ������ ���
        }
        else
        {
            transform.rotation = Quaternion.Euler(0, 0, 0);
        }
    }

    private async UniTaskVoid ApplyKnockbackAsync(Vector2 direction)
    {
        if (_knockbackCTS != null)
        {
            // Отмена предыдушего отталкивания, если оно идёт
            // Чтобы отталкивание не дублировалось
            _knockbackCTS.Cancel();
            _knockbackCTS.Dispose();
        }
        _knockbackCTS = new CancellationTokenSource();
        var token = _knockbackCTS.Token;

        try
        {
            _isKnockedBack = true;
            _navMeshAgent.enabled = false;

            if (_rb != null)
            {
                _rb.linearVelocity = Vector2.zero;
                _rb.AddForce(direction * _knockbackForce, ForceMode2D.Impulse);
            }

            // Токен отмены на случай уничтожения объекта
            await UniTask.Delay(TimeSpan.FromSeconds(_knockbackDuration), cancellationToken: token);

            if (_rb != null) _rb.linearVelocity = Vector2.zero;
            _navMeshAgent.enabled = true;
            _isKnockedBack = false;
        }
        catch (OperationCanceledException)
        {
            // To Do: если отталкивание было прервано (например, враг умер)
        }
    }

    private void OnDisable()
    {
        _knockbackCTS?.Cancel();
        _knockbackCTS?.Dispose();
    }

    private void OnDestroy()
    {
        OnPlayerSpotted -= HandleGlobalPlayerSpotted;
        if (_isCurrentlyDetectingPlayer)
        {
            _enemiesChasingCount--;
            if (_enemiesChasingCount <= 0)
            {
                _enemiesChasingCount = 0;
                OnPlayerSpotted?.Invoke(false);
            }
        }
    }









    //public bool IsRunning
    //{
    //    get
    //    {
    //        if (_navMeshAgent.velocity == Vector3.zero)
    //        {
    //            return false;
    //        }
    //        else
    //        {
    //            return true;
    //        }
    //    }
    //}

    //private void OnValidate()
    //{
    //    _startingPosition = transform.position;
    //}

    //private void OnDrawGizmos()
    //{
    //    Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
    //    Gizmos.DrawSphere(_startingPosition, _roamingDistanceMax);
    //}









    //private void Roaming()
    //{
    //    //_startingPosition = transform.position; // ���������� ��������� �����,
    //    // ���� ����� ������������ �� ���� �����
    //    _roamPosition = GetRoamingPosition();
    //    _navMeshAgent.SetDestination(_roamPosition);
    //}

    //private Vector3 GetRoamingPosition()
    //{
    //    return _startingPosition + Utils.GetRandomDir() *
    //        UnityEngine.Random.Range(_roamingDistanceMin, _roamingDistanceMax);

    //    //Vector3 randomPos = _startingPosition + Utils.GetRandomDir() * UnityEngine.Random.Range(_roamingDistanceMin, _roamingDistanceMax);

    //    //// ���� ��������� ����� �� NavMesh � ������� 2 ������ �� randomPos
    //    //NavMeshHit hit;
    //    //if (NavMesh.SamplePosition(randomPos, out hit, 2f, NavMesh.AllAreas))
    //    //{
    //    //    return hit.position;
    //    //}

    //    //return _startingPosition; // ���� �� �����, ������������ �� �����
    //}

    //private void CheckCurrentState()
    //{
    //    float distanseToPlayer = Vector3.Distance(transform.position, Player.Instance.transform.position);
    //    State newState = State.Roaming;

    //    if (_isChasingEnemy)
    //    {
    //        if (distanseToPlayer <= _chasingDistance)
    //        {
    //            newState = State.Chasing;
    //            OnPlayerSpotted?.Invoke();
    //        }
    //    }

    //    //if (_isAttackingEnemy)
    //    //{
    //    //    if (distanseToPlayer <= _attackingDistance)
    //    //    {
    //    //        newState = State.Attacking;
    //    //    }
    //    //}

    //    if (newState != _currentState)
    //    {
    //        ApplyState();
    //    }
    //}

    //private void AttackingTarget()
    //{
    //    if (Time.time > _nextAttackTime) // ��� ����������� �������� ����� �����, ����������� 2�
    //    {
    //        OnEnemyAttack?.Invoke(this, EventArgs.Empty);

    //        _nextAttackTime = Time.time + _attackRate;
    //    }
    //}

    //public float GetRoamingAnimationSpeed()
    //{
    //    return _navMeshAgent.speed / _roamingSpeed; // 1 ��� ������� ��������, 2 ��� ���������
    //}









    // ���� ������ ����� ����� ������ ��������� �����
    //private void Start()
    //{
    //    startingPosition = transform.position;
    //}

    //private void Roaming()
    //{
    //    roamPosition = GetRoamingPosition();
    //    navMeshAgent.SetDestination(roamPosition);
    //}

    //private Vector3 GetRoamingPosition()
    //{
    //    return startingPosition + Utils.GetRandomDir() * 
    //        UnityEngine.Random.Range(roamingDistanceMin,  roamingDistanceMax); // ��������� ����� ������� ������� �� ��������� �����,
    //                                                                           // �.�. ���� ������ ����� ����� ������ ��������� �����
    //}
}
