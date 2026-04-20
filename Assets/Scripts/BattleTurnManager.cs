using UnityEngine;
using System.Collections.Generic;

public class BattleTurnManager : MonoBehaviour
{
    private float _playerGauge = 0f;
    private float _enemyGauge = 0f;
    private const float TURN_THRESHOLD = 100f;

    public void ResetGauges()
    {
        _playerGauge = 0f;
        _enemyGauge = 0f;
    }

    public void SetPlayerAdvantage()
    {
        _playerGauge = TURN_THRESHOLD;
    }

    public BattleState DetermineNextTurn(BattleUnit player, BattleUnit enemy)
    {
        while (_playerGauge < TURN_THRESHOLD && _enemyGauge < TURN_THRESHOLD)
        {
            _playerGauge += Mathf.Max(1, player.finalSpeed);
            _enemyGauge += Mathf.Max(1, enemy.finalSpeed);
        }

        if (_playerGauge >= TURN_THRESHOLD && _enemyGauge >= TURN_THRESHOLD)
        {
            if (_playerGauge >= _enemyGauge)
            {
                _playerGauge -= TURN_THRESHOLD;
                return BattleState.PlayerTurn;
            }
            else
            {
                _enemyGauge -= TURN_THRESHOLD;
                return BattleState.EnemyTurn;
            }
        }
        else if (_playerGauge >= TURN_THRESHOLD)
        {
            _playerGauge -= TURN_THRESHOLD;
            return BattleState.PlayerTurn;
        }
        else
        {
            _enemyGauge -= TURN_THRESHOLD;
            return BattleState.EnemyTurn;
        }
    }

    public List<BattleUnit> PredictFutureTurns(BattleUnit player, BattleUnit enemy, int count)
    {
        List<BattleUnit> prediction = new List<BattleUnit>();
        float simPlayerGauge = _playerGauge;
        float simEnemyGauge = _enemyGauge;

        for (int i = 0; i < count; i++)
        {
            while (simPlayerGauge < TURN_THRESHOLD && simEnemyGauge < TURN_THRESHOLD)
            {
                simPlayerGauge += Mathf.Max(1, player.finalSpeed);
                simEnemyGauge += Mathf.Max(1, enemy.finalSpeed);
            }

            if (simPlayerGauge >= TURN_THRESHOLD && simEnemyGauge >= TURN_THRESHOLD)
            {
                if (simPlayerGauge >= simEnemyGauge) { prediction.Add(player); simPlayerGauge -= TURN_THRESHOLD; }
                else { prediction.Add(enemy); simEnemyGauge -= TURN_THRESHOLD; }
            }
            else if (simPlayerGauge >= TURN_THRESHOLD)
            {
                prediction.Add(player); simPlayerGauge -= TURN_THRESHOLD;
            }
            else
            {
                prediction.Add(enemy); simEnemyGauge -= TURN_THRESHOLD;
            }
        }
        return prediction;
    }
}
