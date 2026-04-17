using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewSoul", menuName = "Soul Stealer/Soul Data")]
public class SoulData : ScriptableObject
{
    [SerializeField] private Sprite _icon;
    [SerializeField] private string _soulName;
    [SerializeField] private ElementType _element;

    [Header("Enemy AI")]
    [SerializeField] private EnemyAiStyle _enemyAiStyle = EnemyAiStyle.Aggressive;
    
    [Header("Stats")]
    [SerializeField] private int _maxHP;
    [SerializeField] private int _attack;
    [SerializeField] private int _defense;
    [SerializeField] private int _magicAttack;
    [SerializeField] private int _magicDefense;
    [SerializeField] private int _speed;

    [Header("Skills")]
    [SerializeField] private List<SkillData> _skills;

    public Sprite icon => _icon;
    public string soulName => _soulName;
    public ElementType element => _element;
    public EnemyAiStyle enemyAiStyle => _enemyAiStyle;
    public int maxHP => _maxHP;
    public int attack => _attack;
    public int defense => _defense;
    public int magicAttack => _magicAttack;
    public int magicDefense => _magicDefense;
    public int speed => _speed;
    
    public List<SkillData> skills => _skills;

    // Valores usados para cálculos en combate
    public int finalAttack { get; set; }
    public int finalMAttack { get; set; }
    public int finalDefense { get; set; }
    public int finalMDefense { get; set; }
    public int finalSpeed { get; set; }
}