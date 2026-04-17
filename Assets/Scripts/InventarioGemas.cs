using UnityEngine;

public class InventarioGemas : MonoBehaviour
{
    public static InventarioGemas Instance { get; private set; }

    [Header("Configuración")]
    [SerializeField] private float _bonificacionHpMaxPorGema = 0.05f;

    [Header("Inventario")]
    [SerializeField] private int _gemasVitalidad;

    public int gemasVitalidad => _gemasVitalidad;
    public float bonificacionHpMaxPorGema => _bonificacionHpMaxPorGema;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    public void AddGema(TipoGema tipo, int cantidad = 1)
    {
        if (cantidad <= 0) return;

        switch (tipo)
        {
            case TipoGema.Vitalidad:
                int gemasAntes = _gemasVitalidad;
                _gemasVitalidad += cantidad;
                AplicarBonificacionVitalidadAlEquipo(gemasAntes, _gemasVitalidad);
                break;
        }

        if (ExplorationUIManager.Instance != null)
        {
            ExplorationUIManager.Instance.UpdateTeamUI();
        }
    }

    public int GetCantidad(TipoGema tipo)
    {
        switch (tipo)
        {
            case TipoGema.Vitalidad:
                return _gemasVitalidad;
            default:
                return 0;
        }
    }

    public int GetMaxHPConBonificacion(int hpBase)
    {
        float multiplicador = 1f + (_gemasVitalidad * _bonificacionHpMaxPorGema);
        return Mathf.Max(1, Mathf.RoundToInt(hpBase * multiplicador));
    }

    private int GetMaxHPConConteo(int hpBase, int conteoGemasVitalidad)
    {
        float multiplicador = 1f + (conteoGemasVitalidad * _bonificacionHpMaxPorGema);
        return Mathf.Max(1, Mathf.RoundToInt(hpBase * multiplicador));
    }

    private void AplicarBonificacionVitalidadAlEquipo(int gemasAntes, int gemasDespues)
    {
        if (PlayerTeam.Instance == null) return;

        for (int i = 0; i < PlayerTeam.Instance.souls.Count; i++)
        {
            SoulInstance soul = PlayerTeam.Instance.souls[i];
            if (soul == null || soul.data == null) continue;

            int hpMaxAntes = GetMaxHPConConteo(soul.data.maxHP, gemasAntes);
            int hpMaxDespues = GetMaxHPConConteo(soul.data.maxHP, gemasDespues);
            int incrementoMaxHp = hpMaxDespues - hpMaxAntes;

            soul.currentHP = Mathf.Min(soul.currentHP + Mathf.Max(0, incrementoMaxHp), hpMaxDespues);
        }
    }
}