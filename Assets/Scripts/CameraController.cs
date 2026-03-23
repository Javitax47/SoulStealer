using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform _target; 
 
    [Header("Orbit Configuration")]
    [SerializeField] private float _distance = 50f; 
    [SerializeField] private float _sensitivityX = 4f; 
    [SerializeField] private float _fixedAngleY = 35f; 

    [Header("Input System")]
    [SerializeField] private InputActionReference _orbitToggleAction;
    [SerializeField] private InputActionReference _lookAction;

    private float _currentX = -75f;

    void LateUpdate()
    {
        if (_target == null) return;

        bool isOrbiting = false;
        if (_orbitToggleAction != null && _orbitToggleAction.action.IsPressed())
        {
            isOrbiting = true;
        }
        else if (_orbitToggleAction == null && Input.GetMouseButton(1))
        {
            isOrbiting = true;
        }

        if (isOrbiting)
        {
            float lookDelta = 0f;
            if (_lookAction != null)
            {
                lookDelta = _lookAction.action.ReadValue<Vector2>().x;
            }
            else
            {
                lookDelta = Input.GetAxis("Mouse X");
            }

            _currentX += lookDelta * _sensitivityX;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        Vector3 direction = new Vector3(0, 0, -_distance);
        Quaternion rotation = Quaternion.Euler(_fixedAngleY, _currentX, 0);
        
        transform.position = _target.position + rotation * direction;
        transform.LookAt(_target.position);
    }
}