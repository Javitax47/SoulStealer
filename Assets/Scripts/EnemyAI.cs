using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyOverworldAI : MonoBehaviour
{
    public EnemyData enemyData; 

    [Header("Vision Settings")]
    public float viewRadius = 5f;
    [Range(0, 360)] public float viewAngle = 90f;
    public float escapeRadius = 8f;
    public float eyeHeight = 1f;
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
    private EnemyAura aura;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        aura = GetComponentInChildren<EnemyAura>(); 
        
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if(playerObj != null) player = playerObj.transform;

        agent.speed = patrolSpeed;
        GoToNextPatrolPoint();
    }

    void Update()
    {
        if (player == null) return;

        if (isChasing)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);
            
            // Si el jugador logra salir del radio de escape, el enemigo se rinde
            if (distanceToPlayer > escapeRadius)
            {
                LosePlayer();
            }
            else
            {
                agent.speed = chaseSpeed;
                agent.SetDestination(player.position);
            }
        }
        else
        {
            FindPlayerWithVisionCone();

            agent.speed = patrolSpeed;
            if (!agent.pathPending && agent.remainingDistance < 0.5f)
            {
                GoToNextPatrolPoint();
            }
        }
    }

    void FindPlayerWithVisionCone()
    {
        Vector3 eyePosition = transform.position + Vector3.up * eyeHeight;
        Collider[] targetsInViewRadius = Physics.OverlapSphere(eyePosition, viewRadius, targetMask);

        for (int i = 0; i < targetsInViewRadius.Length; i++)
        {
            Transform target = targetsInViewRadius[i].transform;
            Vector3 targetEyePosition = target.position + Vector3.up * eyeHeight; 
            Vector3 dirToTarget = (targetEyePosition - eyePosition).normalized;

            if (Vector3.Angle(transform.forward, dirToTarget) < viewAngle / 2)
            {
                float dstToTarget = Vector3.Distance(eyePosition, targetEyePosition);

                if (!Physics.Raycast(eyePosition, dirToTarget, dstToTarget, obstacleMask))
                {
                    StartChasing();
                    return; 
                }
            }
        }
    }

    void StartChasing()
    {
        isChasing = true;
        if (aura != null) aura.TriggerChaseMode(); 
    }

    void LosePlayer()
    {
        isChasing = false;
        if (aura != null) aura.TriggerPatrolMode(); 
        
        agent.ResetPath(); 
        GoToNextPatrolPoint();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("¡Combate iniciado contra: " + enemyData.enemyName + "!");
        }
    }

    void GoToNextPatrolPoint()
    {
        if (patrolPoints == null || patrolPoints.Length == 0) return;
        agent.destination = patrolPoints[currentPatrolIndex].position;
        currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
    }

    // --- VISUALIZACIÓN EN EL EDITOR ---
    private void OnDrawGizmosSelected()
    {
        Vector3 eyePosition = transform.position + Vector3.up * eyeHeight;

        // Cono de visión (Detección) - Blanco
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(eyePosition, viewRadius);

        Vector3 viewAngleA = DirFromAngle(-viewAngle / 2, false);
        Vector3 viewAngleB = DirFromAngle(viewAngle / 2, false);
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(eyePosition, eyePosition + viewAngleA * viewRadius);
        Gizmos.DrawLine(eyePosition, eyePosition + viewAngleB * viewRadius);

        // --- NUEVO: LÍMITE DE ESCAPE - Gris (Debe verse MÁS GRANDE que el blanco) ---
        Gizmos.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
        Gizmos.DrawWireSphere(eyePosition, escapeRadius);

        // Línea roja apuntando al jugador si está persiguiendo
        if (isChasing && player != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(eyePosition, player.position + Vector3.up * eyeHeight);
        }
    }

    public Vector3 DirFromAngle(float angleInDegrees, bool angleIsGlobal)
    {
        if (!angleIsGlobal) angleInDegrees += transform.eulerAngles.y;
        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }
}