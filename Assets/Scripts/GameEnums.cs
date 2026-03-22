// No necesitamos heredar de MonoBehaviour ni nada, solo declarar los Enums globales

public enum ElementType 
{ 
    Fantasmal, 
    Fisico, 
    Magico 
}

public enum SkillType 
{ 
    Fisico, 
    Magico 
}

public enum BattleState 
{ 
    Transitioning, 
    PlayerTurn, 
    EnemyTurn, 
    Won, 
    Lost 
}