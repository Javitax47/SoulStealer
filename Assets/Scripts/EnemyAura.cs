using UnityEngine;
using System.Collections;

public class EnemyAura : MonoBehaviour
{
    public Light auraLight;
    public float pulseSpeed = 2f;

    [Header("Modo Patrulla")]
    public Color patrolColor = Color.red;
    public float patrolMinIntensity = 1f;
    public float patrolMaxIntensity = 3f;

    [Header("Modo Persecución (Alerta)")]
    public Color chaseColor = new Color(1f, 0.5f, 0f); // Naranja/Amarillo por defecto
    public float chaseMinIntensity = 3f;
    public float chaseMaxIntensity = 6f;
    
    [Header("Efecto Destello")]
    public float flashIntensity = 12f; // El pico máximo de luz al detectarte
    public float flashDuration = 0.2f; // Cuánto tarda en apagarse el destello

    private bool isChasing = false;
    private bool isFlashing = false;

    void Start()
    {
        TriggerPatrolMode();
    }

    void Update()
    {
        // Si no hay luz o está en medio del destello, no hacemos el pulso normal
        if (auraLight == null || isFlashing) return;

        // Pulso suave continuo
        float pulse = Mathf.Sin(Time.time * pulseSpeed) * 0.5f + 0.5f; 
        
        // Elegimos los límites dependiendo de si está persiguiendo o no
        float currentMin = isChasing ? chaseMinIntensity : patrolMinIntensity;
        float currentMax = isChasing ? chaseMaxIntensity : patrolMaxIntensity;

        auraLight.intensity = Mathf.Lerp(currentMin, currentMax, pulse);
    }

    // Llama a esto el script de IA cuando ve al jugador
    public void TriggerChaseMode()
    {
        if (!isChasing)
        {
            isChasing = true;
            auraLight.color = chaseColor;
            StartCoroutine(FlashEffect());
        }
    }

    // Llama a esto el script de IA cuando el jugador escapa
    public void TriggerPatrolMode()
    {
        if (isChasing || auraLight.color != patrolColor)
        {
            isChasing = false;
            auraLight.color = patrolColor;
        }
    }

    // Corrutina que crea el "Chispazo" de luz
    private IEnumerator FlashEffect()
    {
        isFlashing = true;
        auraLight.intensity = flashIntensity;

        float timer = 0f;
        while (timer < flashDuration)
        {
            timer += Time.deltaTime;
            // Interpola desde el flash máximo hasta la intensidad máxima de persecución
            auraLight.intensity = Mathf.Lerp(flashIntensity, chaseMaxIntensity, timer / flashDuration);
            yield return null;
        }

        isFlashing = false;
    }
}