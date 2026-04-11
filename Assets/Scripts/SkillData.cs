using UnityEngine;

[CreateAssetMenu(fileName = "NewSkill", menuName = "Soul Stealer/Skill Data")]
public class SkillData : ScriptableObject
{
    [SerializeField] private string _skillName;
    [SerializeField] private ElementType _element;
    [SerializeField] private SkillType _type;
    [SerializeField] private float _power = 10;
    [SerializeField] private ScriptEfectos _efecto;

    public string skillName => _skillName;
    public ElementType element => _element;
    public SkillType type => _type;
    public float power => _power;
    public ScriptEfectos efecto => _efecto;
}