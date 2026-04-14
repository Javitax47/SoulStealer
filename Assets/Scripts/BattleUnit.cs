using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class BattleUnit : MonoBehaviour
{
    [SerializeField] private SoulData _baseData; 
    [SerializeField] private int _currentHP;
    
    public SoulData baseData => _baseData;
    public int currentHP => _currentHP;
    
 
    [Header("Local UI")]
    [SerializeField] private GameObject _localCombatUI; 
    [SerializeField] private Slider _hpSlider;          

    void Start()
    {
        if (_localCombatUI != null) _localCombatUI.SetActive(false);
    }

    public void TakeDamage(int damage)
    {
        _currentHP -= damage;
        if (_currentHP < 0) _currentHP = 0;
        if (_hpSlider != null) _hpSlider.value = _currentHP;

        StartCoroutine(HitFeedbackRoutine());
        
        Debug.Log($"<color=orange>{_baseData.soulName} takes {damage} damage! HP: {_currentHP}</color>");
    }

    private IEnumerator HitFeedbackRoutine()
    {
        Vector3 originalPos = transform.position;
        float elapsed = 0f;
        float duration = 0.2f; 
        float magnitude = 0.3f; 

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            transform.position = originalPos + Random.insideUnitSphere * magnitude;
            yield return null;
        }
        transform.position = originalPos;
    }

    public void SetCombatMode(bool inCombat)
    {
        if (_localCombatUI != null) _localCombatUI.SetActive(inCombat);
    }

    public void LoadSoulFromInstance(SoulInstance instance)
    {
        if (instance == null || instance.data == null) return;

        _baseData = instance.data;
        _currentHP = instance.currentHP;

        if (_hpSlider != null)
        {
            _hpSlider.maxValue = _baseData.maxHP;
            _hpSlider.value = _currentHP;
        }

        // Actualizar stats finales para cálculos
        baseData.finalAttack = _baseData.attack;
        baseData.finalMAttack = _baseData.magicAttack;
        baseData.finalDefense = _baseData.defense;
        baseData.finalMDefense = _baseData.magicDefense;
        baseData.finalSpeed = _baseData.speed;
    }
}