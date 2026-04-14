using UnityEngine;
using System.Collections;

public class BattleActionHandler : MonoBehaviour
{
    public IEnumerator PerformAttackAnim(BattleUnit attacker, BattleUnit defender, int damage, SkillData skill, System.Action onComplete)
    {
        Vector3 originalPos = attacker.transform.position;
        Vector3 attackPos = originalPos + (defender.transform.position - originalPos).normalized * 1.5f;

        // Attack jump
        float t = 0;
        while (t < 1)
        {
            t += Time.deltaTime * 6f;
            attacker.transform.position = Vector3.Lerp(originalPos, attackPos, t);
            yield return null;
        }

        defender.TakeDamage(damage);

        //Aplicar los efectos de la habilidad
        if(skill.efecto)
        {
            ApplySkillEffects(attacker, defender, skill);
        }
       
        // Return to original position
        t = 0;
        while (t < 1)
        {
            t += Time.deltaTime * 6f;
            attacker.transform.position = Vector3.Lerp(attackPos, originalPos, t);
            yield return null;
        }

        onComplete?.Invoke();
    }

    public void ApplySkillEffects(BattleUnit attacker, BattleUnit defender, SkillData skill)
    {
        float modificador = skill.efecto.potencia;
        SoulData statsAttacker = attacker.baseData;
        SoulData statsDefender = defender.baseData;
        switch (skill.efecto.objetivo)
        {
            //Efectos sobre el atacante
            case EffectTarget.Self:
                switch(skill.efecto.stat)
                {
                    //Stat a variar, el stat final que se usará en el cálculo dependerá del stat original, el cambio al stat final
                    //es permanente hasta que se aplique otro cambio, cualquier modificador nuevo ignora todos los anteriores.
                    case BattleStats.Attack:
                        statsAttacker.finalAttack =Mathf.RoundToInt(statsAttacker.attack * modificador);
                        break;
                    case BattleStats.MAttack:
                        statsAttacker.finalMAttack = Mathf.RoundToInt(statsAttacker.magicAttack * modificador);
                        break;
                    case BattleStats.Defense:
                        statsAttacker.finalDefense = Mathf.RoundToInt(statsAttacker.defense * modificador);
                        break;
                    case BattleStats.MDefense:
                        statsAttacker.finalMDefense = Mathf.RoundToInt(statsAttacker.magicDefense * modificador);
                        break;
                    case BattleStats.Speed:
                        statsAttacker.finalSpeed = Mathf.RoundToInt(statsAttacker.speed * modificador);
                        break;
                    default:
                        break;
                }
                break;
                
                
            //Efectos sobre el enemigo
            case EffectTarget.Enemy:
                switch (skill.efecto.stat)
                {
                   
                    case BattleStats.Attack:
                        statsDefender.finalAttack = Mathf.RoundToInt(statsDefender.attack * modificador);
                        break;
                    case BattleStats.MAttack:
                        statsDefender.finalMAttack = Mathf.RoundToInt(statsDefender.magicAttack * modificador);
                        break;
                    case BattleStats.Defense:
                        statsDefender.finalDefense = Mathf.RoundToInt(statsDefender.defense * modificador);
                        break;
                    case BattleStats.MDefense:
                        statsDefender.finalMDefense = Mathf.RoundToInt(statsDefender.magicDefense * modificador);
                        break;
                    case BattleStats.Speed:
                        statsDefender.finalSpeed = Mathf.RoundToInt(statsDefender.speed * modificador);
                        break;
                    default:
                        break;
                }
                break;
            default:
                break;
        }
    }
}
