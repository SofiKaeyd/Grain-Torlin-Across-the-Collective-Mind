using System;
using Unity.Cinemachine;
using UnityEngine;

public class Player : MonoBehaviour
{
    public static Player Instance { get; private set; }

    [SerializeField] private float _movingSpeed = 4f;
    [SerializeField] private int _health = 3;
    [SerializeField] private float _iFramesTime = 2f;

    private float _iFrames;

    public bool IsIFrames { get; private set; } = false;

    Vector2 inputVector;
    private Rigidbody2D _rb;

    private float minMovingSpeed = 0.1f;
    private bool isRunning = false;

    private CinemachineImpulseSource _impulseSource;

    public static event Action<int> OnHealthChanged;

    public int Health
    {
        get => _health;
        set
        {
            _health = value;
            OnHealthChanged?.Invoke(_health);
        }
    }

    private void Awake() // Запускается всегда раньше Start() и в случайном порядке (если не указывать)
                         // вызывается у классов (у одного раньше, у другого позже: порядок каждый раз разный)
    {
        Instance = this;

        _rb = GetComponent<Rigidbody2D>();
        _iFrames = _iFramesTime;

        _impulseSource = GetComponent<CinemachineImpulseSource>();
    }

    private void Start()
    {
        GameInput.Instance.OnPlayerAttack += GameInput_OnPlayerAttack;
    }

    private void OnTriggerStay2D(Collider2D collision)
    {

    }

    private void GameInput_OnPlayerAttack(object sender, System.EventArgs e)
    {
        //ActiveWeapon.Instance.GetActiveWeapon().Attack();
    }

    private void Update()
    {
        inputVector = GameInput.Instance.GetMovementVector();

        if (IsIFrames)
        {
            _iFrames -= Time.deltaTime;
            if (_iFrames <= 0)
            {
                IsIFrames = false;
                _iFrames = _iFramesTime;
            }
        }
    }

    private void FixedUpdate() // Вызывается через равные промежутки времени; Update(Запускается (обновляется) каждый фрейм (кадр))
    {
        HandlMovement();
    }

    private void HandlMovement()
    {
        _rb.MovePosition(_rb.position + inputVector * (_movingSpeed * Time.fixedDeltaTime)); // Для лучшей оптимизации ( ()), чтобы умножения float меньше нагружали

        if (Mathf.Abs(inputVector.x) > minMovingSpeed || Mathf.Abs(inputVector.y) > minMovingSpeed)
        {
            isRunning = true;
        }
        else
        {
            isRunning = false;
        }
    }

    public bool IsRunning()
    {
        return isRunning;
    }

    public Vector3 GetPlayerScreenPosition()
    {
        Vector3 playerScreenPosition = Camera.main.WorldToScreenPoint(transform.position);
        return playerScreenPosition;
    }

    public void TakeDamage(int damage)
    {
        _impulseSource.GenerateImpulse(Vector3.one);
        EffectsManager.HitStop(0.05f);    // Заморозка на 50 миллисекунд

        Health -= damage;
        IsIFrames = true;
        if (Health <= 0) Die();
    }

    private void Die()
    {
        Debug.Log($"DIE");
    }

}
