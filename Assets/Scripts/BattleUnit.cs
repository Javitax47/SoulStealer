using UnityEngine;
using UnityEngine.UI;

public class BattleUnit : MonoBehaviour
{
    public SoulData baseData; // Arrastra aquí el SoulData del personaje
    public int currentHP;[Header("UI Local (Opcional)")]
    public GameObject localCombatUI; // Arrastra aquí el Enemy_UI_Canvas flotante
    public Slider hpSlider;          // Arrastra aquí la barra de vida del enemigo

    void Start()
    {
        currentHP = baseData.maxHP;
        
        if (hpSlider != null)
        {
            hpSlider.maxValue = baseData.maxHP;
            hpSlider.value = currentHP;
        }

        // Apagamos la UI flotante al empezar a explorar
        if (localCombatUI != null) localCombatUI.SetActive(false);
    }

    public void TakeDamage(int damage)
    {
        currentHP -= damage;
        if (currentHP < 0) currentHP = 0;

        if (hpSlider != null) hpSlider.value = currentHP;

        // Feedback visual temporal
        Debug.Log($"<color=orange>{baseData.soulName} recibe {damage} de daño! HP Restante: {currentHP}</color>");
    }

    public void SetCombatMode(bool inCombat)
    {
        // Enciende o apaga la barra flotante dependiendo de si estamos en batalla
        if (localCombatUI != null) localCombatUI.SetActive(inCombat);
    }
}