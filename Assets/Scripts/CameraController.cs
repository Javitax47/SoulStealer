using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Objetivo")]
    public Transform target; 

    [Header("Configuración de Órbita")]
    public float distance = 50f; 
    public float sensitivityX = 4f; 
    public float fixedAngleY = 35f; // Ángulo vertical fijo (cámara desde arriba en diagonal)

    private float currentX = -75f;

    void LateUpdate()
    {
        if (target == null) return;

        // Si mantenemos pulsado el CLIC DERECHO
        if (Input.GetMouseButton(1))
        {
            // Giramos solo horizontalmente
            currentX += Input.GetAxis("Mouse X") * sensitivityX;

            // Ocultamos y bloqueamos el ratón
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            // Al soltar el clic derecho, el ratón vuelve a aparecer para que puedas hacer clic en el suelo
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        // Calculamos la rotación usando el ángulo vertical fijo y el giro horizontal
        Vector3 direction = new Vector3(0, 0, -distance);
        Quaternion rotation = Quaternion.Euler(fixedAngleY, currentX, 0);
        
        transform.position = target.position + rotation * direction;
        transform.LookAt(target.position);
    }
}