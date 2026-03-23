using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyOverworldAI : MonoBehaviour
{
    [SerializeField] private SoulData _enemyData; 
    public SoulData EnemyData => _enemyData;

    [Header("Vision Settings")]
    [SerializeField] private float _viewRadius = 5f;
    [SerializeField] [Range(0, 360)] private float _viewAngle = 90f;
    [SerializeField] private float _escapeRadius = 8f;
    [SerializeField] private float _eyeHeight = 1f;
    [SerializeField] private LayerMask _targetMask; 
    [SerializeField] private LayerMask _obstacleMask; 

    [Header("Movement Settings")]
    [SerializeField] private float _patrolSpeed = 2.5f;
    [SerializeField] private float _chaseSpeed = 5.5f;

    [Header("Patrol Settings")]
    [SerializeField] private Transform[] _patrolPoints;
    private int _currentPatrolIndex;

    private NavMeshAgent _agent;
    private Transform _player;
    private bool _isChasing = false;
    private EnemyAura _aura;
    private bool _combatStarted = false; 

    void Start()
    {
        _agent = GetComponent<NavMeshAgent>();
        _aura = GetComponentInChildren<EnemyAura>(); 
        
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) _player = playerObj.transform;

        _agent.speed = _patrolSpeed;
        GoToNextPatrolPoint();
    }

    void Update()
    {
        if (!_agent.isActiveAndEnabled || !_agent.isOnNavMesh) return;
        if (_player == null) return;

        if (_isChasing)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, _player.position);
            
            if (distanceToPlayer > _escapeRadius)
            {
                LosePlayer();
            }
            else
            {
                _agent.speed = _chaseSpeed;
                _agent.SetDestination(_player.position);
            }
        }
        else
        {
            FindPlayerWithVisionCone();

            _agent.speed = _patrolSpeed;
            if (!_agent.pathPending && _agent.remainingDistance < 0.5f)
            {
                GoToNextPatrolPoint();
            }
        }
    }

    void FindPlayerWithVisionCone()
    {
        Vector3 eyePosition = transform.position + Vector3.up * _eyeHeight;
        Collider[] targetsInViewRadius = Physics.OverlapSphere(eyePosition, _viewRadius, _targetMask);

        for (int i = 0; i < targetsInViewRadius.Length; i++)
        {
            Transform target = targetsInViewRadius[i].transform;
            Vector3 targetEyePosition = target.position + Vector3.up * _eyeHeight; 
            Vector3 dirToTarget = (targetEyePosition - eyePosition).normalized;

            if (Vector3.Angle(transform.forward, dirToTarget) < _viewAngle / 2)
            {
                float dstToTarget = Vector3.Distance(eyePosition, targetEyePosition);

                if (!Physics.Raycast(eyePosition, dirToTarget, dstToTarget, _obstacleMask))
                {
                    StartChasing();
                    return; 
                }
            }
        }
    }

    void StartChasing()
    {
        _isChasing = true;
        if (_aura != null) _aura.TriggerChaseMode(); 
    }

    void LosePlayer()
    {
        _isChasing = false;
        if (_aura != null) _aura.TriggerPatrolMode(); 
        
        _agent.ResetPath(); 
        GoToNextPatrolPoint();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (_combatStarted) return;

        if (collision.gameObject.CompareTag("Player"))
        {
            _combatStarted = true;
            BattleManager.Instance.StartCombat(collision.gameObject, gameObject, _enemyData, false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_combatStarted) return;

        if (other.CompareTag("Player"))
        {
            _combatStarted = true;
            BattleManager.Instance.StartCombat(other.gameObject, gameObject, _enemyData, true);
        }
    }

    void GoToNextPatrolPoint()
    {
        if (_patrolPoints == null || _patrolPoints.Length == 0) return;
        _agent.destination = _patrolPoints[_currentPatrolIndex].position;
        _currentPatrolIndex = (_currentPatrolIndex + 1) % _patrolPoints.Length;
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 eyePosition = transform.position + Vector3.up * _eyeHeight;

        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(eyePosition, _viewRadius);

        Vector3 viewAngleA = DirFromAngle(-_viewAngle / 2, false);
        Vector3 viewAngleB = DirFromAngle(_viewAngle / 2, false);
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(eyePosition, eyePosition + viewAngleA * _viewRadius);
        Gizmos.DrawLine(eyePosition, eyePosition + viewAngleB * _viewRadius);

        Gizmos.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
        Gizmos.DrawWireSphere(eyePosition, _escapeRadius);

        if (_isChasing && _player != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(eyePosition, _player.position + Vector3.up * _eyeHeight);
        }
    }

    public Vector3 DirFromAngle(float angleInDegrees, bool angleIsGlobal)
    {
        if (!angleIsGlobal) angleInDegrees += transform.eulerAngles.y;
        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }
}
