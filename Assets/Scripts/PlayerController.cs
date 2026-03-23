using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

[RequireComponent(typeof(NavMeshAgent))]
public class PlayerController : MonoBehaviour
{
    private NavMeshAgent _agent;
    private Camera _mainCamera;

    [Header("Visual Effects")]
    [SerializeField] private GameObject _clickIndicator; 

    [Header("Input System")]
    [SerializeField] private InputActionReference _moveClickAction;
    [SerializeField] private InputActionReference _pointAction;

    void Start()
    {
        _agent = GetComponent<NavMeshAgent>();
        _mainCamera = Camera.main;

        if (_clickIndicator != null)
        {
            _clickIndicator.SetActive(false);
        }
    }

    void Update()
    {
        if (EventSystem.current.IsPointerOverGameObject()) return;
        if (!_agent.isActiveAndEnabled || !_agent.isOnNavMesh) return;
        
        bool isMoving = false;
        if (_moveClickAction != null && _moveClickAction.action.IsPressed())
        {
            isMoving = true;
        }
        else if (_moveClickAction == null && Input.GetMouseButton(0))
        {
            isMoving = true;
        }

        if (isMoving)
        {
            MoveToCursor();
        }
        
        if (_clickIndicator != null && _clickIndicator.activeSelf)
        {
            if (!_agent.pathPending && _agent.remainingDistance <= _agent.stoppingDistance)
            {
                _clickIndicator.SetActive(false);
            }
        }
    }

    private void MoveToCursor()
    {
        if (_mainCamera == null) return;

        Vector2 screenPos = Vector2.zero;
        if (_pointAction != null)
        {
            screenPos = _pointAction.action.ReadValue<Vector2>();
        }
        else
        {
            screenPos = Input.mousePosition;
        }

        Ray ray = _mainCamera.ScreenPointToRay(screenPos);
        
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            _agent.SetDestination(hit.point);

            if (_clickIndicator != null)
            {
                _clickIndicator.transform.position = hit.point + new Vector3(0, 0.1f, 0);
                _clickIndicator.SetActive(true);
            }
        }
    }

    private void OnDisable()
    {
        if (_clickIndicator != null)
        {
            _clickIndicator.SetActive(false);
        }
    }
}
