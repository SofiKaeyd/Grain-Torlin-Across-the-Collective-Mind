using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;
using NavMeshPlus.Components; // Проверь правильность namespace в твоей версии

public class FallingObject : MonoBehaviour
{
    [SerializeField] private float _fallDuration = 0.5f;
    [SerializeField] private Vector3 _fallRotation = new Vector3(0, 0, 0);
    [SerializeField] private NavMeshSurface _navMeshSurface;
    [SerializeField] private Collider2D _fallenObstacleCollider;
    [SerializeField] private LayerMask _obstacleMask;

    private NavMeshModifier _modifier;

    private void Awake()
    {
        _modifier = GetComponent<NavMeshModifier>();
        // Изначально кактус может быть "проходимым" или не влиять на сетку,
        // если он тонкий, но мы включим блокировку после падения.
        if (_modifier != null) _modifier.overrideArea = false;

        if (_fallenObstacleCollider != null) _fallenObstacleCollider.enabled = false;
    }

    public async void Fall()
    {
        Quaternion startRot = transform.rotation;
        Quaternion endRot = Quaternion.Euler(_fallRotation);
        float elapsed = 0;

        while (elapsed < _fallDuration)
        {
            elapsed += Time.deltaTime;
            transform.rotation = Quaternion.Slerp(startRot, endRot, elapsed / _fallDuration);
            await UniTask.Yield();
        }

        if (_fallenObstacleCollider != null)
            _fallenObstacleCollider.enabled = true;

        if (_modifier != null)
        {
            _modifier.overrideArea = true;
            _modifier.area = 1; // 1 — "Not Walkable"
        }

        gameObject.layer = LayerMask.NameToLayer("Obstacle");
        foreach (Transform child in transform)
        {
            child.gameObject.layer = LayerMask.NameToLayer("Obstacle");
        }

        if (_navMeshSurface != null)
        {
            _navMeshSurface.BuildNavMesh();
        }
    }
}