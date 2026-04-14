
public enum ElementType 
{ 
    Phantom, 
    Physical, 
    Magic 
}

public enum SkillType 
{ 
    Physical, 
    Magic 
}

public enum BattleState 
{ 
    Transitioning, 
    PlayerTurn, 
    EnemyTurn, 
    Won, 
    Lost 
}

public enum EffectTarget
{
    Self,
    Enemy
}

public enum BattleStats
{
    Attack,
    MAttack,
    Defense,
    MDefense,
    Speed,
    None
}