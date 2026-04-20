using System.Collections;
using UnityEditor.Overlays;
using UnityEngine;

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
                        attacker.finalAttack =Mathf.RoundToInt(statsAttacker.attack * modificador);
                        Debug.Log($"{attacker.baseData.name} finalAttack = {attacker.finalAttack} a partir de attack inicial = " +
                            $"{statsAttacker.attack} por {modificador}");
                        break;
                    case BattleStats.MAttack:
                        attacker.finalMAttack = Mathf.RoundToInt(statsAttacker.magicAttack * modificador);
                        Debug.Log($"{attacker.baseData.name} MAttack final = {attacker.finalMAttack} a partir de magicAttack inicial = " +
                            $"{statsAttacker.magicAttack} por {modificador}");
                        break;
                    case BattleStats.Defense:
                        attacker.finalDefense = Mathf.RoundToInt(statsAttacker.defense * modificador);
                        Debug.Log($"{attacker.baseData.name} finalDefense final = {attacker.finalDefense} a partir de defense inicial = " +
                            $"{statsAttacker.defense} por {modificador}");
                        break;
                    case BattleStats.MDefense:
                        attacker.finalMDefense = Mathf.RoundToInt(statsAttacker.magicDefense * modificador);
                        Debug.Log($"{attacker.baseData.name} finalMDefense final = {attacker.finalMDefense} a partir de magicDefense inicial = " +
                            $"{statsAttacker.magicDefense} por {modificador}");
                        break;
                    case BattleStats.Speed:
                        attacker.finalSpeed = Mathf.RoundToInt(statsAttacker.speed * modificador);
                        Debug.Log($"{attacker.baseData.name} finalSpeed final = {attacker.finalSpeed} a partir de speed inicial = " +
                            $"{statsAttacker.speed} por {modificador}");
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
                        defender.finalAttack = Mathf.RoundToInt(statsDefender.attack * modificador);
                        Debug.Log($"{defender.baseData.name} finalAttack = {defender.finalAttack} a partir de attack inicial = " +
                            $"{statsDefender.attack} por {modificador}");
                        break;
                    case BattleStats.MAttack:
                        defender.finalMAttack = Mathf.RoundToInt(statsDefender.magicAttack * modificador);
                        Debug.Log($"{defender.baseData.name} MAttack final = {defender.finalMAttack} a partir de magicAttack inicial = " +
                            $"{statsDefender.magicAttack} por {modificador}");
                        break;
                    case BattleStats.Defense:
                        defender.finalDefense = Mathf.RoundToInt(statsDefender.defense * modificador);
                        Debug.Log($"{defender.baseData.name} finalDefense final = {defender.finalDefense} a partir de defense inicial = " +
                            $"{statsDefender.defense} por {modificador}");
                        break;
                    case BattleStats.MDefense:
                        defender.finalMDefense = Mathf.RoundToInt(statsDefender.magicDefense * modificador);
                        Debug.Log($"{defender.baseData.name} finalMDefense final = {defender.finalMDefense} a partir de magicDefense inicial = " +
                            $"{statsDefender.magicDefense} por {modificador}");
                        break;
                    case BattleStats.Speed:
                        defender.finalSpeed = Mathf.RoundToInt(statsDefender.speed * modificador);
                        Debug.Log($"{defender.baseData.name} finalSpeed final = {defender.finalSpeed} a partir de speed inicial = " +
                            $"{statsDefender.speed} por {modificador}");
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
