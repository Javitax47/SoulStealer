using UnityEngine;

public static class BattleCalculator
{
    public static float GetElementalMultiplier(ElementType attackElement, ElementType defenderElement)
    {
        // Phantom wins over Physical, Physical wins over Magic, Magic wins over Phantom.
        if (attackElement == ElementType.Phantom && defenderElement == ElementType.Physical) return 2f; 
        if (attackElement == ElementType.Physical && defenderElement == ElementType.Magic) return 2f;
        if (attackElement == ElementType.Magic && defenderElement == ElementType.Phantom) return 2f;

        if (attackElement == ElementType.Physical && defenderElement == ElementType.Phantom) return 0.5f; 
        if (attackElement == ElementType.Magic && defenderElement == ElementType.Physical) return 0.5f;
        if (attackElement == ElementType.Phantom && defenderElement == ElementType.Magic) return 0.5f;

        return 1.0f;
    }

    public static int CalculateDamage(BattleUnit attacker, BattleUnit defender, SkillData skill)
    {
        int atkStat = (skill.type == SkillType.Physical) ? attacker.baseData.attack : attacker.baseData.magicAttack;
        int defStat = (skill.type == SkillType.Physical) ? defender.baseData.defense : defender.baseData.magicDefense;

        float baseDamage = Mathf.Max(1, atkStat * skill.power - defStat);
        float multiplier = GetElementalMultiplier(skill.element, defender.baseData.element);

        return Mathf.Max(1, Mathf.RoundToInt(baseDamage * multiplier));
    }
}
