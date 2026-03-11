using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class PlayerController : MonoBehaviour
{
    private NavMeshAgent agent;
    private Camera mainCamera;

    [Header("Efectos Visuales")]
    [Tooltip("Arrastra aquí el GameObject que servirá como indicador de clic")]
    public GameObject clickIndicator; 

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        mainCamera = Camera.main;

        // Si hemos asignado un indicador, lo ocultamos al empezar el juego
        if (clickIndicator != null)
        {
            clickIndicator.SetActive(false);
        }
    }

    void Update()
    {
        // Usamos GetMouseButton para el movimiento continuo
        if (Input.GetMouseButton(0))
        {
            MoveToCursor();
        }
        
        // Que el indicador desaparezca cuando el Gólem llegue a su destino
        if (clickIndicator != null && clickIndicator.activeSelf)
        {
            // Si la distancia al destino es muy pequeña, ocultamos el indicador
            if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
            {
                clickIndicator.SetActive(false);
            }
        }
    }

    private void MoveToCursor()
    {
        if (mainCamera == null) return;

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            agent.SetDestination(hit.point);

            // Movemos el indicador a la posición donde chocó el rayo (el suelo)
            if (clickIndicator != null)
            {
                // Lo levantamos (0.1f en Y) para que no se superponga o se hunda en el suelo (Z-fighting)
                clickIndicator.transform.position = hit.point + new Vector3(0, 0.1f, 0);
                
                // Lo activamos para que sea visible
                clickIndicator.SetActive(true);
            }
        }
    }
}