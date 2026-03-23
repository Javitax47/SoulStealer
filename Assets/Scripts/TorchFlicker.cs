using UnityEngine;

[RequireComponent(typeof(Light))]
public class TorchFlicker : MonoBehaviour
{
    [Header("Intensity Settings")]
    [SerializeField] private float _minIntensity = 1.5f;
    [SerializeField] private float _maxIntensity = 3.0f;
    [SerializeField] private float _flickerSpeed = 2.5f;

    private Light _torchLight;
    private float _randomOffset;

    void Start()
    {
        _torchLight = GetComponent<Light>();
        _randomOffset = Random.Range(800f, 1100f);
    }

    void Update()
    {
        float mathNoise = Mathf.PerlinNoise(Time.time * _flickerSpeed, _randomOffset);
        _torchLight.intensity = Mathf.Lerp(_minIntensity, _maxIntensity, mathNoise);
    }
}