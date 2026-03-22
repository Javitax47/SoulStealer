using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewSoul", menuName = "Soul Stealer/Soul Data")]
public class SoulData : ScriptableObject
{
    public Sprite icon;
    public string soulName;
    public ElementType element;
    
    [Header("Stats")]
    public int maxHP;
    public int attack;
    public int defense;
    public int magicAttack;
    public int magicDefense;
    public int speed;

    [Header("Habilidades")]
    public List<SkillData> skills; // Lista de hasta 4 ataques
}