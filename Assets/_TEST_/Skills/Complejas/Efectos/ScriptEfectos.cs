using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewEffect", menuName = "Soul Stealer/Effect Data")]
public class ScriptEfectos : ScriptableObject
{
    [SerializeField] private EffectTarget _objetivo;
    [SerializeField] private float _potencia;
    [SerializeField] private BattleStats _stat;
    public float potencia => _potencia;
    public EffectTarget objetivo => _objetivo;
    public BattleStats stat => _stat;
}
