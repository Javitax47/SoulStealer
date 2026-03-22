using UnityEngine;
using UnityEngine.UI;
using System.Collections; // Necesario para corrutinas

public class BattleUnit : MonoBehaviour
{
    public SoulData baseData; 
    public int currentHP;
    
    [Header("UI Local (Opcional)")]
    public GameObject localCombatUI; 
    public Slider hpSlider;          

    void Start()
    {
        currentHP = baseData.maxHP;
        if (hpSlider != null)
        {
            hpSlider.maxValue = baseData.maxHP;
            hpSlider.value = currentHP;
        }
        if (localCombatUI != null) localCombatUI.SetActive(false);
    }

    public void TakeDamage(int damage)
    {
        currentHP -= damage;
        if (currentHP < 0) currentHP = 0;
        if (hpSlider != null) hpSlider.value = currentHP;

        // --- NUEVO: Llamamos al efecto de impacto ---
        StartCoroutine(HitFeedbackRoutine());
        
        Debug.Log($"<color=orange>{baseData.soulName} recibe {damage} de daño! HP: {currentHP}</color>");
    }

    // Corrutina que hace vibrar al modelo
    private IEnumerator HitFeedbackRoutine()
    {
        Vector3 originalPos = transform.position;
        float elapsed = 0f;
        float duration = 0.2f; // El temblor dura 0.2 segundos
        float magnitude = 0.3f; // Fuerza del temblor

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            // Lo movemos a una posición aleatoria muy cercana
            transform.position = originalPos + Random.insideUnitSphere * magnitude;
            yield return null;
        }
        // Nos aseguramos de devolverlo exactamente a su sitio
        transform.position = originalPos;
    }

    public void SetCombatMode(bool inCombat)
    {
        if (localCombatUI != null) localCombatUI.SetActive(inCombat);
    }
}