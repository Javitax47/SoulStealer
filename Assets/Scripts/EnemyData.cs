using UnityEngine;

public enum ElementType { Fantasmal, Fisico, Magico }

[CreateAssetMenu(fileName = "NewEnemyData", menuName = "Soul Stealer/Enemy Data")]
public class EnemyData : ScriptableObject
{
    public string enemyName;
    public ElementType elementType;
    
    [Header("Stats")]
    public int maxHP;
    public int attack;
    public int defense;
    public int magicAttack;
    public int magicDefense;
    public int speed; // SPD para la línea de tiempo
}