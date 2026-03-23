using UnityEngine;
using System.Collections;

public class EnemyAura : MonoBehaviour
{
    [SerializeField] private Light _auraLight;
    [SerializeField] private float _pulseSpeed = 2f;

    [Header("Patrol Mode")]
    [SerializeField] private Color _patrolColor = Color.red;
    [SerializeField] private float _patrolMinIntensity = 1f;
    [SerializeField] private float _patrolMaxIntensity = 3f;

    [Header("Chase Mode (Alert)")]
    [SerializeField] private Color _chaseColor = new Color(1f, 0.5f, 0f); 
    [SerializeField] private float _chaseMinIntensity = 3f;
    [SerializeField] private float _chaseMaxIntensity = 6f;
    
    [Header("Flash Effect")]
    [SerializeField] private float _flashIntensity = 12f; 
    [SerializeField] private float _flashDuration = 0.2f; 

    private bool _isChasing = false;
    private bool _isFlashing = false;

    void Start()
    {
        TriggerPatrolMode();
    }

    void Update()
    {
        if (_auraLight == null || _isFlashing) return;

        float pulse = Mathf.Sin(Time.time * _pulseSpeed) * 0.5f + 0.5f; 
        
        float currentMin = _isChasing ? _chaseMinIntensity : _patrolMinIntensity;
        float currentMax = _isChasing ? _chaseMaxIntensity : _patrolMaxIntensity;

        _auraLight.intensity = Mathf.Lerp(currentMin, currentMax, pulse);
    }

    public void TriggerChaseMode()
    {
        if (!_isChasing)
        {
            _isChasing = true;
            if (_auraLight != null)
            {
                _auraLight.color = _chaseColor;
                StartCoroutine(FlashEffect());
            }
        }
    }

    public void TriggerPatrolMode()
    {
        if (_isChasing || (_auraLight != null && _auraLight.color != _patrolColor))
        {
            _isChasing = false;
            if (_auraLight != null)
            {
                _auraLight.color = _patrolColor;
            }
        }
    }

    private IEnumerator FlashEffect()
    {
        _isFlashing = true;
        _auraLight.intensity = _flashIntensity;

        float timer = 0f;
        while (timer < _flashDuration)
        {
            timer += Time.deltaTime;
            _auraLight.intensity = Mathf.Lerp(_flashIntensity, _chaseMaxIntensity, timer / _flashDuration);
            yield return null;
        }

        _isFlashing = false;
    }
}
