using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using UnityEngine.SceneManagement;

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance { get; private set; }

    [Header("Specialized Managers")]
    [SerializeField] private BattleNavigator _navigator;
    [SerializeField] private BattleTurnManager _turnManager;
    [SerializeField] private BattleUIManager _uiManager;
    [SerializeField] private BattleActionHandler _actionHandler;

    [Header("Main References")]
    [SerializeField] private Camera _mainCamera;
    [SerializeField] private CameraController _cameraScript; 

    [Header("Transition Settings")]
    [SerializeField] private float _transitionDuration = 1.0f;
    [SerializeField] private bool _alignWithEngagement = false;

    [Header("Visual Effects")]
    [SerializeField] private float _flashDuration = 0.5f;
    [SerializeField] private float _shakeDuration = 0.3f;
    [SerializeField] private float _shakeMagnitude = 0.3f;

    [Header("3D Camera Effect")]
    [SerializeField] private float _combatFOV = 60f;

    [Header("Combat Logic")]
    [SerializeField] private BattleState _state;
    private BattleUnit _playerUnit;
    private BattleUnit _enemyUnit;

    [Header("Tactical Advantage")]
    [SerializeField] private int _surpriseDamageAmount = 25;

    [Header("Game Feel")]
    [SerializeField] private Light _turnSpotlight;
    [SerializeField] private float _spotlightHeight = 5f;
    
    // Previous state for restoration
    private Vector3 _prevCameraPos;
    private Quaternion _prevCameraRot;
    private Vector3 _prevPlayerPos;
    private bool _prevOrthoState;
    private float _prevOrthoSize;
    private float _prevFOV;

    private GameObject _currentPlayer;
    private GameObject _currentEnemy;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        // Auto-discover managers on the same GameObject if not assigned
        if (_navigator == null) _navigator = GetComponent<BattleNavigator>();
        if (_turnManager == null) _turnManager = GetComponent<BattleTurnManager>();
        if (_uiManager == null) _uiManager = GetComponent<BattleUIManager>();
        if (_actionHandler == null) _actionHandler = GetComponent<BattleActionHandler>();
    }

    public void StartCombat(GameObject player, GameObject enemy, SoulData enemyData, bool playerAdvantage)
    {
        _currentPlayer = player;
        _currentEnemy = enemy;

        // Disable overworld components
        SetOverworldComponentsEnabled(false);

        StartCoroutine(TransitionToCombatRoutine(playerAdvantage));
    }

    private void SetOverworldComponentsEnabled(bool enabled)
    {
        // SEGURIDAD PARA EL JUGADOR
        if (_currentPlayer != null)
        {
            if (_currentPlayer.TryGetComponent(out NavMeshAgent pAgent)) pAgent.enabled = enabled;
            if (_currentPlayer.TryGetComponent(out MonoBehaviour pCtrl)) pCtrl.enabled = enabled;
            if (_currentPlayer.TryGetComponent(out Rigidbody pRb)) 
            {
                pRb.isKinematic = !enabled;
                if (!enabled) pRb.linearVelocity = Vector3.zero;
            }
        }

        // SEGURIDAD PARA EL ENEMIGO (Aquí es donde fallaba)
        if (_currentEnemy != null)
        {
            if (_currentEnemy.TryGetComponent(out NavMeshAgent eAgent)) eAgent.enabled = enabled;
            if (_currentEnemy.TryGetComponent(out MonoBehaviour eAI)) eAI.enabled = enabled;
            if (_currentEnemy.TryGetComponent(out Rigidbody eRb)) 
            {
                eRb.isKinematic = !enabled;
                if (!enabled) eRb.linearVelocity = Vector3.zero; 
            }
        }

        // Cámara
        if (_cameraScript != null) _cameraScript.enabled = enabled;
    }

    private IEnumerator TransitionToCombatRoutine(bool playerAdvantage)
    {
        // Visual effects (flash)
        _uiManager.ShowFlash(playerAdvantage ? Color.white : Color.red, _flashDuration);

        // Save state
        _prevCameraPos = _mainCamera.transform.position;
        _prevCameraRot = _mainCamera.transform.rotation;
        _prevPlayerPos = _currentPlayer.transform.position;
        _prevOrthoState = _mainCamera.orthographic;
        _prevOrthoSize = _mainCamera.orthographicSize;
        _prevFOV = _mainCamera.fieldOfView;
        _mainCamera.orthographic = false;

        Vector3 initialMidPoint = (_currentPlayer.transform.position + _currentEnemy.transform.position) / 2f;
        
        if (_alignWithEngagement)
        {
            Vector3 engagementDir = (_currentEnemy.transform.position - _currentPlayer.transform.position).normalized;
            engagementDir.y = 0; 
            if (engagementDir != Vector3.zero) _navigator.MainFormationCenter.rotation = Quaternion.LookRotation(engagementDir);
        }
        else
        {
            _navigator.MainFormationCenter.rotation = Quaternion.identity;
        }

        Vector3 bestCenter = _navigator.FindValidArenaCenter(initialMidPoint);
        _navigator.MainFormationCenter.position = bestCenter;

        Transform selectedCamPos = _navigator.GetBestCameraPosition();
        Vector3 targetCamPos = selectedCamPos.position;
        Quaternion targetCamRot = selectedCamPos.rotation;

        Vector3 targetPlayerPos = _navigator.SnapToNavMesh(_navigator.CombatPlayerPos);
        Quaternion targetPlayerRot = _navigator.CombatPlayerPos.rotation;
        Vector3 targetEnemyPos = _navigator.SnapToNavMesh(_navigator.CombatEnemyPos);
        Quaternion targetEnemyRot = _navigator.CombatEnemyPos.rotation;

        Vector3 startCamPos = _mainCamera.transform.position;
        Quaternion startCamRot = _mainCamera.transform.rotation;
        Vector3 startPlayerPos = _currentPlayer.transform.position;
        Quaternion startPlayerRot = _currentPlayer.transform.rotation;
        Vector3 startEnemyPos = _currentEnemy.transform.position;
        Quaternion startEnemyRot = _currentEnemy.transform.rotation;

        float elapsedTime = 0f;

        while (elapsedTime < _transitionDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsedTime / _transitionDuration);

            Vector3 shakeOffset = Vector3.zero;
            if (!playerAdvantage && elapsedTime < _shakeDuration)
            {
                shakeOffset = Random.insideUnitSphere * _shakeMagnitude;
            }

            _mainCamera.transform.position = Vector3.Lerp(startCamPos, targetCamPos, t) + shakeOffset;
            _mainCamera.transform.rotation = Quaternion.Slerp(startCamRot, targetCamRot, t);
            _mainCamera.fieldOfView = Mathf.Lerp(_prevFOV, _combatFOV, t);

            _currentPlayer.transform.position = Vector3.Lerp(startPlayerPos, targetPlayerPos, t);
            _currentPlayer.transform.rotation = Quaternion.Slerp(startPlayerRot, targetPlayerRot, t);
            _currentEnemy.transform.position = Vector3.Lerp(startEnemyPos, targetEnemyPos, t);
            _currentEnemy.transform.rotation = Quaternion.Slerp(startEnemyRot, targetEnemyRot, t);

            yield return null;
        }

        _mainCamera.transform.SetPositionAndRotation(targetCamPos, targetCamRot);
        _mainCamera.fieldOfView = _combatFOV;
        _currentPlayer.transform.SetPositionAndRotation(targetPlayerPos, targetPlayerRot);
        _currentEnemy.transform.SetPositionAndRotation(targetEnemyPos, targetEnemyRot);

        _uiManager.ShowCombatUI(true);

        _playerUnit = _currentPlayer.GetComponent<BattleUnit>();
        _enemyUnit = _currentEnemy.GetComponent<BattleUnit>();

        if (PlayerTeam.Instance.souls.Count > 0)
        {
            _playerUnit.LoadSoulFromInstance(PlayerTeam.Instance.souls[0]); 
        }

        if (_playerUnit != null) _playerUnit.SetCombatMode(true);
        if (_enemyUnit != null) _enemyUnit.SetCombatMode(true);

        _turnManager.ResetGauges();

        if (playerAdvantage)
        {
            _enemyUnit.TakeDamage(_surpriseDamageAmount);
            if (_enemyUnit.currentHP <= 0)
            {
                EndCombatWithWin();
                yield break;
            }
            _turnManager.SetPlayerAdvantage(); 
        }

        DetermineNextTurn(); 
    }

    private void EndCombatWithWin()
    {
        _state = BattleState.Won;
        _uiManager.ShowCombatUI(false);

        if (_enemyUnit != null && HarvestManager.Instance != null)
        {
            HarvestManager.Instance.OpenHarvestScreen(_enemyUnit.baseData);
        }

        if (_currentEnemy != null) 
        {
            Destroy(_currentEnemy);
            _currentEnemy = null; // IMPORTANTE: Setea a null después de destruir
        }

        if (_turnSpotlight != null) _turnSpotlight.gameObject.SetActive(false);
    }

    public void CloseHarvestScreen()
    {
        _uiManager.CloseHarvestScreen();
        RestoreOverworldState();
    }

    private void RestoreOverworldState()
    {
        _mainCamera.transform.position = _prevCameraPos;
        _mainCamera.transform.rotation = _prevCameraRot;
        _mainCamera.fieldOfView = _prevFOV;
        _mainCamera.orthographic = _prevOrthoState;
        _mainCamera.orthographicSize = _prevOrthoSize;
        _currentPlayer.transform.position = _prevPlayerPos;

        if (PlayerTeam.Instance.souls.Count > 0)
        {
            PlayerTeam.Instance.souls[0].currentHP = _playerUnit.currentHP;
        }

        ExplorationUIManager.Instance.UpdateTeamUI();

        SetOverworldComponentsEnabled(true);
    }

    public void OnPlayerUseSkill(int skillIndex)
    {
        if (_state != BattleState.PlayerTurn) return;
        if (skillIndex >= _playerUnit.baseData.skills.Count) return;

        _state = BattleState.Transitioning; 
        _uiManager.UpdateSkillButtons(_playerUnit, false); 

        SkillData skillUsed = _playerUnit.baseData.skills[skillIndex];
        int damage = BattleCalculator.CalculateDamage(_playerUnit, _enemyUnit, skillUsed);
        
        if (_turnSpotlight != null) _turnSpotlight.gameObject.SetActive(false);
        StartCoroutine(_actionHandler.PerformAttackAnim(_playerUnit, _enemyUnit, damage, skillUsed, () => OnActionComplete(_enemyUnit)));
    }

    private void OnActionComplete(BattleUnit defender)
    {
        if (defender.currentHP <= 0)
        {
            if (defender == _enemyUnit) EndCombatWithWin();
            else StartCoroutine(GameOverRoutine());
        }
        else
        {
            DetermineNextTurn(); 
        }
    }

    private void DetermineNextTurn()
    {
        _state = _turnManager.DetermineNextTurn(_playerUnit, _enemyUnit);
        
        _uiManager.UpdateTimeline(_turnManager.PredictFutureTurns(_playerUnit, _enemyUnit, 7), _playerUnit);
        _uiManager.UpdateSkillButtons(_playerUnit, _state == BattleState.PlayerTurn);

        UpdateTurnHighlight();

        if (_state == BattleState.EnemyTurn)
        {
            StartCoroutine(EnemyTurnRoutine());
        }
    }

    private void UpdateTurnHighlight()
    {
        if (_turnSpotlight == null) return;

        _turnSpotlight.gameObject.SetActive(true);
        Transform activeUnitTransform = (_state == BattleState.PlayerTurn) ? _playerUnit.transform : _enemyUnit.transform;
        _turnSpotlight.transform.position = activeUnitTransform.position + Vector3.up * _spotlightHeight;
        _turnSpotlight.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
    }

    private IEnumerator EnemyTurnRoutine()
    {
        yield return new WaitForSeconds(1.5f);

        SkillData bestSkill = _enemyUnit.baseData.skills[0];
        int maxExpectedDamage = -1;

        foreach (SkillData skill in _enemyUnit.baseData.skills)
        {
            int expectedDamage = BattleCalculator.CalculateDamage(_enemyUnit, _playerUnit, skill);
            if (expectedDamage > maxExpectedDamage)
            {
                maxExpectedDamage = expectedDamage;
                bestSkill = skill;
            }
        }
        
        if (_turnSpotlight != null) _turnSpotlight.gameObject.SetActive(false);
        StartCoroutine(_actionHandler.PerformAttackAnim(_enemyUnit, _playerUnit, maxExpectedDamage, bestSkill, () => OnActionComplete(_playerUnit)));
    }

    private IEnumerator GameOverRoutine()
    {
        _state = BattleState.Lost;
        _uiManager.ShowCombatUI(false);

        yield return _uiManager.FadeToBlack(2.0f);

        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void OnClickViewTeam()
    {
        if (_state != BattleState.PlayerTurn) return;
        
        _uiManager.ShowTeamSelection(true, PlayerTeam.Instance.souls);
    }

    public void OnSelectSoulToSwap(int index)
    {
        if (index >= PlayerTeam.Instance.souls.Count) return;

        SoulInstance selectedInstance = PlayerTeam.Instance.souls[index];

        if (_playerUnit.baseData == selectedInstance.data)
        {
            _uiManager.ShowTeamSelection(false);
            return;
        }

        _playerUnit.LoadSoulFromInstance(selectedInstance);
        
        _uiManager.ShowTeamSelection(false);
        _uiManager.UpdateSkillButtons(_playerUnit, false);
        _uiManager.ShowFlash(Color.cyan, 0.3f);

        DetermineNextTurn(); 
    }

    public void OnCancelSwap()
    {
        _uiManager.ShowTeamSelection(false);
    }
}
