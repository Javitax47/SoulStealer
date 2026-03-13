using UnityEngine;
using UnityEngine.AI;[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAI : MonoBehaviour
{
    public EnemyData enemyData; 

    [Header("Vision Settings")]
    public float viewRadius = 5f;[Range(0, 360)] public float viewAngle = 90f;
    public float eyeHeight = 1f; // Altura desde la que sale el rayo visual
    public LayerMask targetMask; 
    public LayerMask obstacleMask; 

    [Header("Movement Settings")]
    public float patrolSpeed = 2.5f;
    public float chaseSpeed = 5.5f;

    [Header("Patrol Settings")]
    public Transform[] patrolPoints;
    private int currentPatrolIndex;

    private NavMeshAgent agent;
    private Transform player;
    private bool isChasing = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if(playerObj != null) player = playerObj.transform;

        agent.speed = patrolSpeed;
        GoToNextPatrolPoint();
    }

    void Update()
    {
        if (player == null) return;

        FindPlayerWithVisionCone();

        if (isChasing)
        {
            agent.speed = chaseSpeed;
            agent.SetDestination(player.position);
        }
        else
        {
            agent.speed = patrolSpeed;
            // Si llegó a su destino y está patrullando, va al siguiente punto
            if (!agent.pathPending && agent.remainingDistance < 0.5f)
            {
                GoToNextPatrolPoint();
            }
        }
    }

    void FindPlayerWithVisionCone()
    {
        // Elevamos el punto de origen para que el rayo no choque con el suelo
        Vector3 eyePosition = transform.position + Vector3.up * eyeHeight;
        
        Collider[] targetsInViewRadius = Physics.OverlapSphere(eyePosition, viewRadius, targetMask);

        for (int i = 0; i < targetsInViewRadius.Length; i++)
        {
            Transform target = targetsInViewRadius[i].transform;
            Vector3 targetEyePosition = target.position + Vector3.up * eyeHeight; // Apuntamos al "pecho" del jugador
            Vector3 dirToTarget = (targetEyePosition - eyePosition).normalized;

            // Comprobamos el ángulo
            if (Vector3.Angle(transform.forward, dirToTarget) < viewAngle / 2)
            {
                float dstToTarget = Vector3.Distance(eyePosition, targetEyePosition);

                // Comprobamos que no haya obstáculos
                if (!Physics.Raycast(eyePosition, dirToTarget, dstToTarget, obstacleMask))
                {
                    isChasing = true; // ¡Te vio!
                    return; 
                }
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("¡Transición a Combate iniciada contra: " + enemyData.enemyName + "!");
        }
    }

    void GoToNextPatrolPoint()
    {
        if (patrolPoints == null || patrolPoints.Length == 0) return;
        agent.destination = patrolPoints[currentPatrolIndex].position;
        currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
    }

    // --- VISUALIZACIÓN DEL CONO EN EL EDITOR ---
    private void OnDrawGizmosSelected()
    {
        Vector3 eyePosition = transform.position + Vector3.up * eyeHeight;

        // Dibujar el radio máximo (Círculo blanco)
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(eyePosition, viewRadius);

        // Calcular los límites del cono
        Vector3 viewAngleA = DirFromAngle(-viewAngle / 2, false);
        Vector3 viewAngleB = DirFromAngle(viewAngle / 2, false);

        // Dibujar las líneas del cono (Líneas amarillas)
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(eyePosition, eyePosition + viewAngleA * viewRadius);
        Gizmos.DrawLine(eyePosition, eyePosition + viewAngleB * viewRadius);

        // Si persigue, dibujamos una línea roja hacia el jugador para confirmar
        if (isChasing && player != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(eyePosition, player.position + Vector3.up * eyeHeight);
        }
    }

    // Función auxiliar matemática para los Gizmos
    public Vector3 DirFromAngle(float angleInDegrees, bool angleIsGlobal)
    {
        if (!angleIsGlobal)
        {
            angleInDegrees += transform.eulerAngles.y;
        }
        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }
}