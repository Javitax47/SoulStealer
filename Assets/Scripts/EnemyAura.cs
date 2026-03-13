using UnityEngine;

public class PulsingAura : MonoBehaviour
{
    public Light auraLight; // Asigna una Point Light roja desde el inspector
    public float pulseSpeed = 2f;
    public float minIntensity = 1f;
    public float maxIntensity = 3f;

    void Update()
    {
        if (auraLight != null)
        {
            // Usamos una onda senoidal para el efecto pulsante
            float pulse = Mathf.Sin(Time.time * pulseSpeed) * 0.5f + 0.5f; 
            auraLight.intensity = Mathf.Lerp(minIntensity, maxIntensity, pulse);
        }
    }
}