using UnityEngine;

[RequireComponent(typeof(Light))]
public class TorchFlicker : MonoBehaviour
{
    [Header("Ajustes de Intensidad")]
    public float minIntensity = 1.5f;
    public float maxIntensity = 3.0f;
    public float flickerSpeed = 2.5f;

    private Light torchLight;
    private float randomOffset;

    void Start()
    {
        torchLight = GetComponent<Light>();
        randomOffset = Random.Range(800f, 1100f);
    }

    void Update()
    {
        float mathNoise = Mathf.PerlinNoise(Time.time * flickerSpeed, randomOffset);
        torchLight.intensity = Mathf.Lerp(minIntensity, maxIntensity, mathNoise);
    }
}