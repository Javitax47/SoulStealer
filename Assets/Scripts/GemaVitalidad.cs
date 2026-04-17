using UnityEngine;

[RequireComponent(typeof(Collider))]
public class GemaVitalidad : MonoBehaviour
{
    [Header("Pickup")]
    [SerializeField] private int _cantidad = 1;
    [SerializeField] private GameObject _efectoRecogida;

    [Header("Visual")]
    [SerializeField] private float _velocidadRotacion = 90f;

    private bool _recogida;

    void Reset()
    {
        Collider col = GetComponent<Collider>();
        if (col != null) col.isTrigger = true;
    }

    void Update()
    {
        transform.Rotate(0f, _velocidadRotacion * Time.deltaTime, 0f, Space.World);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_recogida) return;
        if (!other.CompareTag("Player")) return;

        _recogida = true;

        if (InventarioGemas.Instance != null)
        {
            InventarioGemas.Instance.AddGema(TipoGema.Vitalidad, _cantidad);
        }

        if (_efectoRecogida != null)
        {
            Instantiate(_efectoRecogida, transform.position, Quaternion.identity);
        }

        Destroy(gameObject);
    }
}