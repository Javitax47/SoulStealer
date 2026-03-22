using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI; // ¡Necesario para controlar la imagen del destello!
using System.Collections;
using TMPro;
using UnityEngine.SceneManagement;

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

    [Header("UI de Combate - Botones")]
    public Button[] skillButtons;

    [Header("Lógica de Combate")]
    public BattleState state;
    private BattleUnit playerUnit;
    private BattleUnit enemyUnit;

    [Header("Ventaja Táctica")]
    public int surpriseDamageAmount = 25; // Cuánto daño hace el ataque por la espalda[Header("Línea de Tiempo (UI)")]
    
    public Transform timelineContainer;   // Arrastra aquí tu Panel_Timeline
    public TimelineSegment timelineSegmentPrefab; // Crearemos un prefab de UI muy simple para esto

    private float playerGauge = 0f;
    private float enemyGauge = 0f;
    private const float turnThreshold = 100f;

    [Header("Game Feel (Animaciones y Luces)")]
    public Light turnSpotlight;         // El foco que iluminará a quien le toca
    public float spotlightHeight = 5f;  // A qué altura flota el foco
    
    [Header("Pantalla de Cosecha")]
    public GameObject harvestUI;
    public UnityEngine.UI.Image harvestEnemyIcon;
    public TMPro.TextMeshProUGUI harvestEnemyName;

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

    public void StartCombat(GameObject player, GameObject enemy, SoulData enemyData, bool playerAdvantage)
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

        // 4. FINALIZAR TRANSICIÓN (Posiciones exactas)
        mainCamera.transform.SetPositionAndRotation(targetCamPos, targetCamRot);
        mainCamera.fieldOfView = combatFOV;
        currentPlayer.transform.SetPositionAndRotation(targetPlayerPos, targetPlayerRot);
        currentEnemy.transform.SetPositionAndRotation(targetEnemyPos, targetEnemyRot);

        // Apagar el destello y encender la UI de botones
        if (flashScreen != null) flashScreen.gameObject.SetActive(false); 
        if (combatUI != null) combatUI.SetActive(true);

        // --- 5. INICIAR LÓGICA DE TURNOS (¡AHORA SÍ!) ---
        playerUnit = currentPlayer.GetComponent<BattleUnit>();
        enemyUnit = currentEnemy.GetComponent<BattleUnit>();

        if (playerUnit != null) playerUnit.SetCombatMode(true);
        if (enemyUnit != null) enemyUnit.SetCombatMode(true);

        playerGauge = 0f;
        enemyGauge = 0f;

        // Aplicamos la ventaja y el daño sorpresa YA CON LA CÁMARA EN SU SITIO
        if (playerAdvantage)
        {
            Debug.Log($"<color=green>¡GOLPE CRÍTICO SORPRESA! Enemigo recibe {surpriseDamageAmount} de daño.</color>");
            enemyUnit.TakeDamage(surpriseDamageAmount);
            
            // Si el enemigo muere del golpe sorpresa, nos vamos directo a la Cosecha
            if (enemyUnit.currentHP <= 0)
            {
                state = BattleState.Won;
                EndCombatWithHarvest();
                yield break; // Cortamos la corrutina aquí
            }
            
            // Si sobrevive, el jugador tiene el medidor lleno
            playerGauge = turnThreshold; 
        }

        // 6. CEDER EL CONTROL AL MOTOR DE TIEMPO
        // Esto encenderá el foco en el personaje correcto al instante
        DetermineNextTurn(); 
        
        // Actualizamos los botones por primera vez
        UpdateSkillButtonsUI(state == BattleState.PlayerTurn);

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

    private void EndCombatWithHarvest()
    {
        // 1. Apagar la UI de combate y las luces
        if (combatUI != null) combatUI.SetActive(false);
        if (turnSpotlight != null) turnSpotlight.gameObject.SetActive(false);

        // --- NUEVO: EXTRAER LOS DATOS DEL ENEMIGO ANTES DE DESTRUIRLO ---
        if (currentEnemy != null)
        {
            BattleUnit defeatedEnemy = currentEnemy.GetComponent<BattleUnit>();
            
            // Asignar el Icono
            if (harvestEnemyIcon != null && defeatedEnemy.baseData.icon != null)
            {
                harvestEnemyIcon.sprite = defeatedEnemy.baseData.icon;
                harvestEnemyIcon.color = Color.white; // Aseguramos que sea visible
            }
            
            // Asignar el Nombre
            if (harvestEnemyName != null)
            {
                harvestEnemyName.text = defeatedEnemy.baseData.soulName;
            }
        }

        // 2. Encender la Pantalla de Cosecha para tapar la cámara
        if (harvestUI != null) harvestUI.SetActive(true);

        // 3. El enemigo se desintegra / es cosechado
        if (currentEnemy != null) Destroy(currentEnemy);

        // 4. TELETRANSPORTE INSTANTÁNEO DE CÁMARA
        mainCamera.transform.position = prevCameraPos;
        mainCamera.transform.rotation = prevCameraRot;
        mainCamera.fieldOfView = prevFOV;
        mainCamera.orthographic = prevOrthoState;
        mainCamera.orthographicSize = prevOrthoSize;
        currentPlayer.transform.position = prevPlayerPos;

        // 5. Devolver los controles 
        if (cameraScript != null) cameraScript.enabled = true;
        if (currentPlayer.TryGetComponent(out NavMeshAgent pAgent)) pAgent.enabled = true;
        if (currentPlayer.TryGetComponent(out MonoBehaviour pCtrl)) pCtrl.enabled = true;
        if (currentPlayer.TryGetComponent(out Rigidbody pRb)) pRb.isKinematic = false;
    }

    // Esta función la llamarás desde un botón "Aceptar" en tu menú de Cosecha
    public void CloseHarvestScreen()
    {
        if (harvestUI != null) harvestUI.SetActive(false);
        Debug.Log("Volviendo a la exploración normal.");
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

    public void OnPlayerUseSkill(int skillIndex)
    {
        if (state != BattleState.PlayerTurn) return;
        
        // --- NUEVO: Seguridad. Si el índice no existe, no hacemos nada ---
        if (skillIndex >= playerUnit.baseData.skills.Count) return;

        state = BattleState.Transitioning; 

        // --- NUEVO: Apagar botones instantáneamente para evitar doble clic ---
        UpdateSkillButtonsUI(false); 

        SkillData skillUsed = playerUnit.baseData.skills[skillIndex];
        int damage = CalculateDamage(playerUnit, enemyUnit, skillUsed);
        
        StartCoroutine(PerformAttackAnim(playerUnit, enemyUnit, damage));
    }

    private IEnumerator EnemyTurn()
    {
        yield return new WaitForSeconds(1.5f); // Piensa dos segundos

        SkillData bestSkill = enemyUnit.baseData.skills[0];
        int maxExpectedDamage = -1;

        foreach (SkillData skill in enemyUnit.baseData.skills)
        {
            int expectedDamage = CalculateDamage(enemyUnit, playerUnit, skill);
            if (expectedDamage > maxExpectedDamage)
            {
                maxExpectedDamage = expectedDamage;
                bestSkill = skill;
            }
        }
        
        // Arrancar la animación
        StartCoroutine(PerformAttackAnim(enemyUnit, playerUnit, maxExpectedDamage));
    }

    private float GetElementalMultiplier(ElementType attackElement, ElementType defenderElement)
    {
        // GDD: Fantasmal vence a Físico, Físico vence a Mágico, Mágico vence a Fantasmal.
        if (attackElement == ElementType.Fantasmal && defenderElement == ElementType.Fisico) return 1.5f; // Ventaja
        if (attackElement == ElementType.Fisico && defenderElement == ElementType.Magico) return 1.5f;
        if (attackElement == ElementType.Magico && defenderElement == ElementType.Fantasmal) return 1.5f;

        // Resistencias (A la inversa)
        if (attackElement == ElementType.Fisico && defenderElement == ElementType.Fantasmal) return 0.5f; // Débil
        if (attackElement == ElementType.Magico && defenderElement == ElementType.Fisico) return 0.5f;
        if (attackElement == ElementType.Fantasmal && defenderElement == ElementType.Magico) return 0.5f;

        return 1.0f; // Elementos iguales o sin interacción
    }

    private int CalculateDamage(BattleUnit attacker, BattleUnit defender, SkillData skill)
    {
        // 1. Elegimos qué stats usar según el tipo de ataque (Físico o Mágico)
        int atkStat = (skill.type == SkillType.Fisico) ? attacker.baseData.attack : attacker.baseData.magicAttack;
        int defStat = (skill.type == SkillType.Fisico) ? defender.baseData.defense : defender.baseData.magicDefense;

        // 2. Daño Base
        float baseDamage = Mathf.Max(1, atkStat + skill.power - defStat);

        // 3. Multiplicador Elemental
        float multiplier = GetElementalMultiplier(skill.element, defender.baseData.element);
        
        if (multiplier > 1.0f) Debug.Log("<color=yellow>¡Ataque Súper Efectivo!</color>");
        else if (multiplier < 1.0f) Debug.Log("<color=grey>Es poco efectivo...</color>");

        return Mathf.Max(1, Mathf.RoundToInt(baseDamage * multiplier));
    }

    private void UpdateTimelineUI()
    {
        if (timelineContainer == null || timelineSegmentPrefab == null) return;

        // 1. Crear un simulador de tiempo
        BattleUnit[] turnPrediction = new BattleUnit[7];
        float simPlayerGauge = playerGauge;
        float simEnemyGauge = enemyGauge;

        // Simulamos el futuro para adivinar los próximos 7 turnos
        for (int i = 0; i < turnPrediction.Length; i++)
        {
            while (simPlayerGauge < turnThreshold && simEnemyGauge < turnThreshold)
            {
                simPlayerGauge += Mathf.Max(1, playerUnit.baseData.speed);
                simEnemyGauge += Mathf.Max(1, enemyUnit.baseData.speed);
            }

            if (simPlayerGauge >= turnThreshold && simEnemyGauge >= turnThreshold)
            {
                if (simPlayerGauge >= simEnemyGauge) { turnPrediction[i] = playerUnit; simPlayerGauge -= turnThreshold; }
                else { turnPrediction[i] = enemyUnit; simEnemyGauge -= turnThreshold; }
            }
            else if (simPlayerGauge >= turnThreshold)
            {
                turnPrediction[i] = playerUnit; simPlayerGauge -= turnThreshold;
            }
            else
            {
                turnPrediction[i] = enemyUnit; simEnemyGauge -= turnThreshold;
            }
        }

        // 2. Dibujar la simulación en la Flecha (Reciclando los prefabs)
        while (timelineContainer.childCount < turnPrediction.Length) Instantiate(timelineSegmentPrefab, timelineContainer);

        for (int i = 0; i < timelineContainer.childCount; i++)
        {
            Transform child = timelineContainer.GetChild(i);
            
            if (i >= turnPrediction.Length)
            {
                child.gameObject.SetActive(false);
                continue;
            }

            child.gameObject.SetActive(true);
            TimelineSegment segmentUI = child.GetComponent<TimelineSegment>();
            
            bool isPlayer = (turnPrediction[i] == playerUnit);
            bool isLast = (i == turnPrediction.Length - 1); 
            Sprite soulIcon = turnPrediction[i].baseData.icon;

            segmentUI.Setup(isPlayer, soulIcon, isLast);
        }
    }

    private void UpdateSkillButtonsUI(bool isPlayerTurn)
    {
        if (skillButtons == null || playerUnit == null) return;

        for (int i = 0; i < skillButtons.Length; i++)
        {
            // Buscamos el componente de texto del botón (TextMeshPro)
            TextMeshProUGUI btnText = skillButtons[i].GetComponentInChildren<TextMeshProUGUI>();
            // (Si usaste botones antiguos de Unity, cambia TextMeshProUGUI por Text)

            if (i < playerUnit.baseData.skills.Count)
            {
                // El personaje SÍ tiene una habilidad en este hueco
                if (btnText != null) btnText.text = playerUnit.baseData.skills[i].skillName;
                
                // Solo se puede pulsar si es el turno del jugador
                skillButtons[i].interactable = isPlayerTurn; 
            }
            else
            {
                // El personaje NO tiene habilidad en este hueco
                if (btnText != null) btnText.text = "---";
                
                // Botón bloqueado (grisáceo y no clickeable)
                skillButtons[i].interactable = false; 
            }
        }
    }
    
    private void DetermineNextTurn()
    {
        // Hacemos avanzar el tiempo sumando la SPD hasta que alguien llegue a 100
        while (playerGauge < turnThreshold && enemyGauge < turnThreshold)
        {
            // Protegemos con Mathf.Max para que si tienen 0 SPD el juego no se congele
            playerGauge += Mathf.Max(1, playerUnit.baseData.speed);
            enemyGauge += Mathf.Max(1, enemyUnit.baseData.speed);
        }

        // Si ambos llegan a la vez, desempata el que tenga más gauge o el jugador
        if (playerGauge >= turnThreshold && enemyGauge >= turnThreshold)
        {
            if (playerGauge >= enemyGauge) AssignPlayerTurn();
            else AssignEnemyTurn();
        }
        else if (playerGauge >= turnThreshold)
        {
            AssignPlayerTurn();
        }
        else
        {
            AssignEnemyTurn();
        }
    }

    private void AssignPlayerTurn()
    {
        playerGauge -= turnThreshold;
        state = BattleState.PlayerTurn;
        UpdateTimelineUI();
        
        // --- NUEVO: Encender botones ---
        UpdateSkillButtonsUI(true); 

        if (turnSpotlight != null)
        {
            turnSpotlight.gameObject.SetActive(true);
            turnSpotlight.transform.position = playerUnit.transform.position + Vector3.up * spotlightHeight;
            turnSpotlight.transform.rotation = Quaternion.Euler(90f, 0f, 0f); 
        }
    }

    private void AssignEnemyTurn()
    {
        enemyGauge -= turnThreshold;
        state = BattleState.EnemyTurn;
        UpdateTimelineUI();
        
        // --- NUEVO: Apagar botones ---
        UpdateSkillButtonsUI(false); 

        if (turnSpotlight != null)
        {
            turnSpotlight.gameObject.SetActive(true);
            turnSpotlight.transform.position = enemyUnit.transform.position + Vector3.up * spotlightHeight;
            turnSpotlight.transform.rotation = Quaternion.Euler(90f, 0f, 0f); 
        }
        
        StartCoroutine(EnemyTurn());
    }

    // --- NUEVO: ANIMACIÓN FÍSICA DE ATAQUE ---
    private IEnumerator PerformAttackAnim(BattleUnit attacker, BattleUnit defender, int damage)
    {
        // 1. Apagar temporalmente el foco durante la animación
        if (turnSpotlight != null) turnSpotlight.gameObject.SetActive(false);

        Vector3 originalPos = attacker.transform.position;
        // Calculamos un punto un poco más adelante hacia el enemigo
        Vector3 attackPos = originalPos + (defender.transform.position - originalPos).normalized * 1.5f;

        // 2. Saltar hacia adelante (Ataque)
        float t = 0;
        while (t < 1)
        {
            t += Time.deltaTime * 6f; // Velocidad del salto
            attacker.transform.position = Vector3.Lerp(originalPos, attackPos, t);
            yield return null;
        }

        // 3. ¡Impacto!
        defender.TakeDamage(damage);

        // 4. Volver atrás a su posición original
        t = 0;
        while (t < 1)
        {
            t += Time.deltaTime * 6f;
            attacker.transform.position = Vector3.Lerp(attackPos, originalPos, t);
            yield return null;
        }

        // 5. Evaluar si alguien murió o pasar el turno
        if (defender.currentHP <= 0)
        {
            if (defender == enemyUnit)
            {
                state = BattleState.Won;
                Debug.Log("<color=green>¡Enemigo Derrotado! Abriendo Cosecha...</color>");
                EndCombatWithHarvest(); 
            }
            else
            {
                // --- NUEVA LÓGICA DE DERROTA ---
                state = BattleState.Lost;
                Debug.Log("<color=red>Has muerto... El bucle se reinicia.</color>");
                StartCoroutine(GameOverRoutine()); // Llamamos al fundido y reinicio
            }
        }
        else
        {
            DetermineNextTurn(); 
        }
    }
    // --- RUTINA DE GAME OVER (ROGUELIKE) ---
    private IEnumerator GameOverRoutine()
    {
        Debug.Log("<color=red>Iniciando secuencia de reinicio Roguelike...</color>");

        // 1. Apagamos la UI de combate para que desaparezcan los botones
        if (combatUI != null) combatUI.SetActive(false);

        // 2. Fundido a negro usando tu Flash Screen
        if (flashScreen != null)
        {
            flashScreen.gameObject.SetActive(true);
            Color fadeColor = Color.black;
            fadeColor.a = 0f;
            flashScreen.color = fadeColor;

            float fadeDuration = 2.0f; // Tarda 2 segundos en oscurecerse
            float t = 0f;

            while (t < fadeDuration)
            {
                t += Time.deltaTime;
                fadeColor.a = Mathf.Lerp(0f, 1f, t / fadeDuration);
                flashScreen.color = fadeColor;
                yield return null;
            }
        }
        else
        {
            // Si no tienes el flashScreen asignado, simplemente esperamos 2 segundos
            yield return new WaitForSeconds(2.0f);
        }

        // 3. Nos aseguramos de que el tiempo corra normal (por si acaso)
        Time.timeScale = 1f;

        // 4. ¡RECARGAMOS LA ESCENA ACTUAL!
        // Esto destruye todo y vuelve a cargar la sala inicial desde cero.
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}