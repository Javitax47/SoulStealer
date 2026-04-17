using UnityEngine;

public class Cofre : MonoBehaviour
{
    [Header("Vida del cofre")]
    [SerializeField] private int _vidaMaxima = 1;

    [Header("Drop")]
    [SerializeField] private GameObject _prefabGemaVitalidad;
    [SerializeField] private Transform _puntoDrop;
    [SerializeField] private float _fuerzaDrop = 2f;

    [Header("Debug")]
    [SerializeField] private bool _permitirDestruirConClick = true;

    private int _vidaActual;
    private bool _destruido;
    private Collider[] _colliders;
    private Renderer[] _renderers;

    // Start is called before the first frame update
    void Awake()
    {
        _vidaActual = Mathf.Max(1, _vidaMaxima);
        _colliders = GetComponentsInChildren<Collider>();
        _renderers = GetComponentsInChildren<Renderer>();
    }

    public void RecibirDanio(int cantidad)
    {
        if (_destruido) return;
        if (cantidad <= 0) return;

        _vidaActual -= cantidad;

        if (_vidaActual <= 0)
        {
            DestruirCofre();
        }
    }

    private void OnMouseDown()
    {
        if (!_permitirDestruirConClick) return;
        RecibirDanio(_vidaMaxima);
    }

    private void DestruirCofre()
    {
        if (_destruido) return;
        _destruido = true;

        SoltarGemaVitalidad();

        for (int i = 0; i < _colliders.Length; i++)
        {
            if (_colliders[i] != null) _colliders[i].enabled = false;
        }

        for (int i = 0; i < _renderers.Length; i++)
        {
            if (_renderers[i] != null) _renderers[i].enabled = false;
        }

        Destroy(gameObject);
    }

    private void SoltarGemaVitalidad()
    {
        if (_prefabGemaVitalidad == null) return;

        Vector3 posicionDrop = _puntoDrop != null ? _puntoDrop.position : transform.position + Vector3.up * 0.5f;
        GameObject gema = Instantiate(_prefabGemaVitalidad, posicionDrop, Quaternion.identity);

        Rigidbody rb = gema.GetComponent<Rigidbody>();
        if (rb != null)
        {
            Vector3 direccion = (Vector3.up + Random.insideUnitSphere * 0.25f).normalized;
            rb.AddForce(direccion * _fuerzaDrop, ForceMode.Impulse);
        }
    }
}
