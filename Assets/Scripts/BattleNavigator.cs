using UnityEngine;
using UnityEngine.AI;

public class BattleNavigator : MonoBehaviour
{
    [Header("Formation Settings")]
    [SerializeField] private Transform _combatFormationCenter;
    [SerializeField] private Transform _combatPlayerPos;
    [SerializeField] private Transform _combatEnemyPos;
    [SerializeField] private Transform[] _combatCameraPositions;

    [Header("Collision & Constraints")]
    [SerializeField] private LayerMask _obstacleMask;
    [SerializeField] private float _cameraCollisionRadius = 0.5f;
    [SerializeField] private float _characterCollisionRadius = 0.4f;
    [SerializeField] private float _maxSearchDistance = 6f;

    public Transform MainFormationCenter => _combatFormationCenter;
    public Transform CombatPlayerPos => _combatPlayerPos;
    public Transform CombatEnemyPos => _combatEnemyPos;
    public Transform[] CombatCameraPositions => _combatCameraPositions;

    public Vector3 FindValidArenaCenter(Vector3 originalCenter)
    {
        Quaternion originalRotation = _combatFormationCenter.rotation;
        int rotationSteps = 8;

        for (int r = 0; r < rotationSteps; r++)
        {
            float rotAngle = r * (360f / rotationSteps);
            _combatFormationCenter.rotation = originalRotation * Quaternion.Euler(0, rotAngle, 0);
            if (IsFormationValidAtPosition(originalCenter)) return originalCenter;
        }

        int angleSteps = 8;
        for (float dist = 1f; dist <= _maxSearchDistance; dist += 1f)
        {
            for (int i = 0; i < angleSteps; i++)
            {
                float posAngle = i * (360f / angleSteps);
                Vector3 offset = Quaternion.Euler(0, posAngle, 0) * Vector3.forward * dist;
                Vector3 testPosition = originalCenter + offset;

                if (NavMesh.SamplePosition(testPosition, out NavMeshHit hit, 1.5f, NavMesh.AllAreas))
                {
                    for (int r = 0; r < rotationSteps; r++)
                    {
                        float rotAngle = r * (360f / rotationSteps);
                        _combatFormationCenter.rotation = originalRotation * Quaternion.Euler(0, rotAngle, 0);
                        if (IsFormationValidAtPosition(hit.position)) return hit.position;
                    }
                }
            }
        }

        _combatFormationCenter.rotation = originalRotation;
        return originalCenter;
    }

    private bool IsFormationValidAtPosition(Vector3 testCenter)
    {
        _combatFormationCenter.position = testCenter;

        Vector3 pPos = _combatPlayerPos.position;
        Vector3 ePos = _combatEnemyPos.position;
        Vector3 cPos = _combatFormationCenter.position;

        Vector3 pPosHigh = pPos + Vector3.up * 1.5f;
        Vector3 ePosHigh = ePos + Vector3.up * 1.5f;
        Vector3 cPosHigh = cPos + Vector3.up * 1.5f;

        if (Physics.CheckSphere(pPos, _characterCollisionRadius, _obstacleMask)) return false;
        if (Physics.CheckSphere(ePos, _characterCollisionRadius, _obstacleMask)) return false;

        if (Physics.Linecast(cPosHigh, pPosHigh, _obstacleMask)) return false;
        if (Physics.Linecast(cPosHigh, ePosHigh, _obstacleMask)) return false;

        if (!NavMesh.SamplePosition(pPos, out NavMeshHit pHit, 2.0f, NavMesh.AllAreas)) return false;
        if (!NavMesh.SamplePosition(ePos, out NavMeshHit eHit, 2.0f, NavMesh.AllAreas)) return false;

        foreach (Transform camTarget in _combatCameraPositions)
        {
            Vector3 camPosHigh = camTarget.position;
            if (!Physics.CheckSphere(camPosHigh, _cameraCollisionRadius, _obstacleMask) &&
                !Physics.Linecast(camPosHigh, cPosHigh, _obstacleMask))
            {
                return true;
            }
        }
        return false;
    }

    public Vector3 SnapToNavMesh(Transform pivotTransform)
    {
        Vector3 targetPosition = pivotTransform.position;
        if (NavMesh.SamplePosition(targetPosition, out NavMeshHit hit, 2.0f, NavMesh.AllAreas))
        {
            return hit.position + Vector3.up * pivotTransform.localPosition.y;
        }
        return targetPosition;
    }

    public Transform GetBestCameraPosition()
    {
        foreach (Transform camTarget in _combatCameraPositions)
        {
            if (!Physics.CheckSphere(camTarget.position, _cameraCollisionRadius, _obstacleMask))
            {
                return camTarget;
            }
        }
        return _combatCameraPositions[0];
    }

    private void OnDrawGizmosSelected()
    {
        if (_combatCameraPositions == null || _combatCameraPositions.Length == 0) return;
        foreach (Transform camTarget in _combatCameraPositions)
        {
            if (camTarget == null) continue;
             Gizmos.color = Physics.CheckSphere(camTarget.position, _cameraCollisionRadius, _obstacleMask) 
                ? new Color(1f, 0f, 0f, 0.5f) : new Color(0f, 1f, 0f, 0.5f);
            Gizmos.DrawSphere(camTarget.position, _cameraCollisionRadius);
        }
    }
}
