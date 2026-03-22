using UnityEngine;

[CreateAssetMenu(fileName = "NewSkill", menuName = "Soul Stealer/Skill Data")]

public class SkillData : ScriptableObject
{
    public string skillName;
    public ElementType element; 
    public SkillType type;      
    public int power = 10;      
}