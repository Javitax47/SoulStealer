using UnityEngine;
using System.Collections;

public class BattleActionHandler : MonoBehaviour
{
    public IEnumerator PerformAttackAnim(BattleUnit attacker, BattleUnit defender, int damage, System.Action onComplete)
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
}
