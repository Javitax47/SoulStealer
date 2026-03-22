using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI; // ¡Necesario para controlar la imagen del destello!
using System.Collections;

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance;

    [Header("Referencias Principales")]
    public Camera mainCamera;
    public CameraController cameraScript; 
    public GameObject combatUI;[Header("Posicionamiento Libre (Formación)")]
    public Transform combatFormationCenter; 
    public Transform combatPlayerPos;       
    public Transform combatEnemyPos;        
    public Transform[] combatCameraPositions;[Header("Ajustes de Colisión (Anti-Muros)")]
    public LayerMask obstacleMask; 
    public float cameraCollisionRadius = 0.5f; 
    public float characterCollisionRadius = 0.4f; 
    public float maxSearchDistance = 6f; 

    [Header("Ajustes de Transición")]
    public float transitionDuration = 1.0f;
    public bool alignWithEngagement = false;

    [Header("Efectos Visuales (Game Feel)")]
    public Image flashScreen;           // Arrastra aquí tu nueva imagen Flash_Screen
    public float flashDuration = 0.5f;  // Cuánto dura el destello
    public float shakeDuration = 0.3f;  // Cuánto dura el temblor
    public float shakeMagnitude = 0.3f; // Fuerza del temblor[Header("Efecto de Cámara 3D")]
    public float combatFOV = 60f; 

    private Vector3 prevCameraPos;
    private Quaternion prevCameraRot;
    private Vector3 prevPlayerPos;
    private bool prevOrthoState;
    private float prevOrthoSize;
    private float prevFOV;

    private GameObject currentPlayer;
    private GameObject currentEnemy;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // AHORA RECIBIMOS LA VENTAJA Y LA PASAMOS A LA CORRUTINA
    // AHORA RECIBIMOS LA VENTAJA Y LA PASAMOS A LA CORRUTINA
    public void StartCombat(GameObject player, GameObject enemy, EnemyData enemyData, bool playerAdvantage)
    {
        currentPlayer = player;
        currentEnemy = enemy;

        // 1. Apagar movimiento e inteligencia artificial
        if (player.TryGetComponent(out NavMeshAgent pAgent)) pAgent.enabled = false;
        if (enemy.TryGetComponent(out NavMeshAgent eAgent)) eAgent.enabled = false;
        if (player.TryGetComponent(out MonoBehaviour pCtrl)) pCtrl.enabled = false;
        if (enemy.TryGetComponent(out MonoBehaviour eAI)) eAI.enabled = false;
        if (cameraScript != null) cameraScript.enabled = false;

        // --- NUEVO: Desactivar Físicas (Rigidbody) ---
        // Ponemos el enemigo en modo Kinematic y frenamos cualquier empuje
        if (enemy.TryGetComponent(out Rigidbody eRb)) 
        {
            eRb.isKinematic = true;
            eRb.linearVelocity = Vector3.zero; 
        }

        // Hacemos lo mismo para el Gólem por seguridad
        if (player.TryGetComponent(out Rigidbody pRb)) 
        {
            pRb.isKinematic = true;
            pRb.linearVelocity = Vector3.zero;
        }

        // Iniciar la transición pasándole si atacamos por la espalda o no
        StartCoroutine(TransitionToCombatRoutine(playerAdvantage));
    }

    private IEnumerator TransitionToCombatRoutine(bool playerAdvantage)
    {
        // --- 1. PREPARAR EFECTOS VISUALES (DESTELLO) ---
        Color currentFlashColor = Color.clear;
        if (flashScreen != null)
        {
            flashScreen.gameObject.SetActive(true);
            // Si hay ventaja: Blanco. Si no hay ventaja (normal): Rojo.
            currentFlashColor = playerAdvantage ? Color.white : Color.red;
            currentFlashColor.a = 1f; // Opacidad al máximo
            flashScreen.color = currentFlashColor;
        }

        // --- 2. GUARDAR ESTADO Y ALINEAR FORMACIÓN ---
        prevCameraPos = mainCamera.transform.position;
        prevCameraRot = mainCamera.transform.rotation;
        prevPlayerPos = currentPlayer.transform.position;

        prevOrthoState = mainCamera.orthographic;
        prevOrthoSize = mainCamera.orthographicSize;
        prevFOV = mainCamera.fieldOfView;
        mainCamera.orthographic = false;

        Vector3 initialMidPoint = (currentPlayer.transform.position + currentEnemy.transform.position) / 2f;
        
        if (alignWithEngagement)
        {
            Vector3 engagementDir = (currentEnemy.transform.position - currentPlayer.transform.position).normalized;
            engagementDir.y = 0; 
            if (engagementDir != Vector3.zero) combatFormationCenter.rotation = Quaternion.LookRotation(engagementDir);
        }
        else
        {
            combatFormationCenter.rotation = Quaternion.identity;
        }

        Vector3 bestCenter = FindValidArenaCenter(initialMidPoint);
        combatFormationCenter.position = bestCenter;

        Transform selectedCamPos = combatCameraPositions[0]; 
        foreach (Transform camTarget in combatCameraPositions)
        {
            if (!Physics.CheckSphere(camTarget.position, cameraCollisionRadius, obstacleMask))
            {
                selectedCamPos = camTarget;
                break; 
            }
        }

        Vector3 targetCamPos = selectedCamPos.position;
        Quaternion targetCamRot = selectedCamPos.rotation;
        Vector3 targetPlayerPos = SnapToNavMesh(combatPlayerPos);
        Quaternion targetPlayerRot = combatPlayerPos.rotation;
        Vector3 targetEnemyPos = SnapToNavMesh(combatEnemyPos);
        Quaternion targetEnemyRot = combatEnemyPos.rotation;

        Vector3 startCamPos = mainCamera.transform.position;
        Quaternion startCamRot = mainCamera.transform.rotation;
        Vector3 startPlayerPos = currentPlayer.transform.position;
        Quaternion startPlayerRot = currentPlayer.transform.rotation;
        Vector3 startEnemyPos = currentEnemy.transform.position;
        Quaternion startEnemyRot = currentEnemy.transform.rotation;

        float elapsedTime = 0f;

        // --- 3. BUCLE DE ANIMACIÓN ---
        while (elapsedTime < transitionDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsedTime / transitionDuration);

            // --- DESTELLO (Se desvanece gradualmente) ---
            if (flashScreen != null && elapsedTime < flashDuration)
            {
                currentFlashColor.a = Mathf.Lerp(1f, 0f, elapsedTime / flashDuration);
                flashScreen.color = currentFlashColor;
            }

            // --- TEMBLOR DE CÁMARA (Solo si NO hay ventaja del jugador) ---
            Vector3 shakeOffset = Vector3.zero;
            if (!playerAdvantage && elapsedTime < shakeDuration)
            {
                // Genera una posición aleatoria dentro de una esfera para simular el impacto
                shakeOffset = Random.insideUnitSphere * shakeMagnitude;
            }

            // Mover cámara sumando el temblor a su trayectoria
            mainCamera.transform.position = Vector3.Lerp(startCamPos, targetCamPos, t) + shakeOffset;
            mainCamera.transform.rotation = Quaternion.Slerp(startCamRot, targetCamRot, t);
            mainCamera.fieldOfView = Mathf.Lerp(prevFOV, combatFOV, t);

            currentPlayer.transform.position = Vector3.Lerp(startPlayerPos, targetPlayerPos, t);
            currentPlayer.transform.rotation = Quaternion.Slerp(startPlayerRot, targetPlayerRot, t);
            currentEnemy.transform.position = Vector3.Lerp(startEnemyPos, targetEnemyPos, t);
            currentEnemy.transform.rotation = Quaternion.Slerp(startEnemyRot, targetEnemyRot, t);

            yield return null;
        }

        // --- 4. FINALIZAR TRANSICIÓN ---
        mainCamera.transform.SetPositionAndRotation(targetCamPos, targetCamRot);
        mainCamera.fieldOfView = combatFOV;
        currentPlayer.transform.SetPositionAndRotation(targetPlayerPos, targetPlayerRot);
        currentEnemy.transform.SetPositionAndRotation(targetEnemyPos, targetEnemyRot);

        if (flashScreen != null) flashScreen.gameObject.SetActive(false); // Apagamos el destello
        if (combatUI != null) combatUI.SetActive(true);
    }

    private Vector3 FindValidArenaCenter(Vector3 originalCenter)
    {
        Quaternion originalRotation = combatFormationCenter.rotation;
        int rotationSteps = 8; 

        for (int r = 0; r < rotationSteps; r++)
        {
            float rotAngle = r * (360f / rotationSteps);
            combatFormationCenter.rotation = originalRotation * Quaternion.Euler(0, rotAngle, 0);

            if (IsFormationValidAtPosition(originalCenter)) return originalCenter;
        }

        int angleSteps = 8; 
        for (float dist = 1f; dist <= maxSearchDistance; dist += 1f)
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
                        combatFormationCenter.rotation = originalRotation * Quaternion.Euler(0, rotAngle, 0);

                        if (IsFormationValidAtPosition(hit.position)) return hit.position;
                    }
                }
            }
        }

        combatFormationCenter.rotation = originalRotation;
        return originalCenter;
    }

    private bool IsFormationValidAtPosition(Vector3 testCenter)
    {
        combatFormationCenter.position = testCenter;

        Vector3 pPos = combatPlayerPos.position;
        Vector3 ePos = combatEnemyPos.position;
        Vector3 cPos = combatFormationCenter.position;

        Vector3 pPosHigh = pPos + Vector3.up * 1.5f;
        Vector3 ePosHigh = ePos + Vector3.up * 1.5f;
        Vector3 cPosHigh = cPos + Vector3.up * 1.5f;

        if (Physics.CheckSphere(pPos, characterCollisionRadius, obstacleMask)) return false;
        if (Physics.CheckSphere(ePos, characterCollisionRadius, obstacleMask)) return false;

        if (Physics.Linecast(cPosHigh, pPosHigh, obstacleMask)) return false;
        if (Physics.Linecast(cPosHigh, ePosHigh, obstacleMask)) return false;

        if (!NavMesh.SamplePosition(pPos, out NavMeshHit pHit, 2.0f, NavMesh.AllAreas)) return false;
        if (!NavMesh.SamplePosition(ePos, out NavMeshHit eHit, 2.0f, NavMesh.AllAreas)) return false;

        bool hasValidCamera = false;
        foreach (Transform camTarget in combatCameraPositions)
        {
            Vector3 camPosHigh = camTarget.position;
            if (!Physics.CheckSphere(camPosHigh, cameraCollisionRadius, obstacleMask) && 
                !Physics.Linecast(camPosHigh, cPosHigh, obstacleMask))
            {
                hasValidCamera = true;
                break;
            }
        }
        
        if (!hasValidCamera) return false; 

        return true; 
    }

    private Vector3 SnapToNavMesh(Transform pivotTransform)
    {
        Vector3 targetPosition = pivotTransform.position;
        
        if (NavMesh.SamplePosition(targetPosition, out NavMeshHit hit, 2.0f, NavMesh.AllAreas))
        {
            return hit.position + Vector3.up * pivotTransform.localPosition.y;
        }
        return targetPosition; 
    }

    public void EndCombat()
    {
        StartCoroutine(TransitionToExplorationRoutine());
    }

    private IEnumerator TransitionToExplorationRoutine()
    {
        if (combatUI != null) combatUI.SetActive(false);
        if (currentEnemy != null) Destroy(currentEnemy);

        Vector3 startCamPos = mainCamera.transform.position;
        Quaternion startCamRot = mainCamera.transform.rotation;
        Vector3 startPlayerPos = currentPlayer.transform.position;
        
        float elapsedTime = 0f;

        while (elapsedTime < transitionDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsedTime / transitionDuration);

            mainCamera.transform.position = Vector3.Lerp(startCamPos, prevCameraPos, t);
            mainCamera.transform.rotation = Quaternion.Slerp(startCamRot, prevCameraRot, t);
            mainCamera.fieldOfView = Mathf.Lerp(combatFOV, prevFOV, t);

            currentPlayer.transform.position = Vector3.Lerp(startPlayerPos, prevPlayerPos, t);

            yield return null;
        }

        mainCamera.orthographic = prevOrthoState;
        mainCamera.orthographicSize = prevOrthoSize;

        if (cameraScript != null) cameraScript.enabled = true;
        if (currentPlayer.TryGetComponent(out NavMeshAgent pAgent)) pAgent.enabled = true;
        if (currentPlayer.TryGetComponent(out MonoBehaviour pCtrl)) pCtrl.enabled = true;
    }

    private void OnDrawGizmosSelected()
    {
        if (combatCameraPositions == null || combatCameraPositions.Length == 0) return;

        foreach (Transform camTarget in combatCameraPositions)
        {
            if (camTarget == null) continue;
            Gizmos.color = Physics.CheckSphere(camTarget.position, cameraCollisionRadius, obstacleMask) 
                ? new Color(1f, 0f, 0f, 0.5f) : new Color(0f, 1f, 0f, 0.5f);
            Gizmos.DrawSphere(camTarget.position, cameraCollisionRadius);
        }
    }
}