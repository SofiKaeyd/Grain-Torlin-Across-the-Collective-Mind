using UnityEngine;

public class Player : MonoBehaviour
{
    public static Player Instance { get; private set; }

    [SerializeField] private float movingSpeed = 2f;
    Vector2 inputVector;

    private Rigidbody2D rb;

    private float minMovingSpeed = 0.1f;
    private bool isRunning = false;

    private void Awake() // Запускается всегда раньше Start() и в случайном порядке (если не указывать)
                         // вызывается у классов (у одного раньше, у другого позже: порядок каждый раз разный)
    {
        Instance = this;

        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        GameInput.Instance.OnPlayerAttack += GameInput_OnPlayerAttack;
    }

    private void GameInput_OnPlayerAttack(object sender, System.EventArgs e)
    {
        //ActiveWeapon.Instance.GetActiveWeapon().Attack();
    }

    private void Update()
    {
        inputVector = GameInput.Instance.GetMovementVector();
    }

    private void FixedUpdate() // Вызывается через равные промежутки времени; Update(Запускается (обновляется) каждый фрейм (кадр))
    {
        HandlMovement();
    }

    private void HandlMovement()
    {
        rb.MovePosition(rb.position + inputVector * (movingSpeed * Time.fixedDeltaTime)); // Для лучшей оптимизации ( ()), чтобы умножения float меньше нагружали

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
        Vector3 pplayerScreenPosition = Camera.main.WorldToScreenPoint(transform.position);
        return pplayerScreenPosition;
    }
}
