using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class SoulInstance // Clase para guardar el estado actual de un alma
{
    public SoulData data;
    public int currentHP;

    public SoulInstance(SoulData data)
    {
        this.data = data;
        this.currentHP = data.maxHP;
    }
}

public class PlayerTeam : MonoBehaviour
{
    public static PlayerTeam Instance;
    
    public List<SoulInstance> souls = new List<SoulInstance>(); 
    public int maxSouls = 3;
    public SoulData startingSoul; 
    public int soulFragments = 0; // Moneda del juego

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        if (startingSoul != null && souls.Count == 0)
        {
            AddSoul(startingSoul);
        }
    }

    public void AddSoul(SoulData data)
    {
        if (souls.Count < maxSouls)
            souls.Add(new SoulInstance(data));
    }

    public void AddOrReplaceSoul(SoulData newSoul, int replaceIndex)
    {
        if (replaceIndex >= 0 && replaceIndex < souls.Count)
        {
            souls[replaceIndex] = new SoulInstance(newSoul);
        }
        else if (souls.Count < maxSouls)
        {
            AddSoul(newSoul);
        }
        // Actualizar UI tras cambio
        ExplorationUIManager.Instance.UpdateTeamUI();
    }
}