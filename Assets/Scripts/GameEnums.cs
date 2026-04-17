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

public enum EnemyAiStyle
{
    Aggressive,     // Solo considera el mayor daño
    Tactical,       // Prefiere más la ventaja elemental
    Control,        // Prefiere habilidades con debuff/buff
    Defensive,      // Con poca vida prioriza auto-mejora/defensa
    Random          // Aleatorio
}