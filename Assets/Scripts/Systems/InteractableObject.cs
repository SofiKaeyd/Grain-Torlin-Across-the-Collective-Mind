using UnityEngine;
using UnityEngine.Events;

public class InteractableObject : MonoBehaviour
{
    [SerializeField] private GameObject _uiHint; // Ссылка на спрайт/канвас с буквой "E"
    [SerializeField] private KeyCode _interactKey = KeyCode.E;
    [SerializeField] private UnityEvent _onInteract; // Метод падения

    private bool _isPlayerInside = false;
    private bool _isInteracted = false;

    private void Start() => _uiHint.SetActive(false);

    private void Update()
    {
        if (_isPlayerInside && !_isInteracted)
        {
            if (Input.GetKeyDown(_interactKey))
            {
                Interact();
            }
        }
    }

    private void Interact()
    {
        _isInteracted = true;
        _uiHint.SetActive(false);
        _onInteract?.Invoke(); // Запускаем падение
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Игрок вошел в зону кактуса!");
            _isPlayerInside = true;
            if (!_isInteracted) _uiHint.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            _isPlayerInside = false;
            _uiHint.SetActive(false);
        }
    }
}