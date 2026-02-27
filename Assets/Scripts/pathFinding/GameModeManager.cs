
public abstract class GameMode
{
    public virtual GameModeEnum gameMode => GameModeEnum.RPG;

    public abstract void OnMode();
    public abstract void OffMode();

    
    public abstract void HandleAttack(Unit unit, Unit target = null);
    public abstract void HandleMove(Unit unit);
}

public class RTSMode : GameMode
{
    public override GameModeEnum gameMode => GameModeEnum.RTS;
    
    public override void OnMode()
    {
        
    }

    public override void OffMode()
    {
        
    }

    public override void HandleAttack(Unit unit, Unit target = null)
    {
        if (target)
        {
            unit.attack.TryAttack(target);
        }
    }

    public override void HandleMove(Unit unit)
    {
        
    }
}

public class RPGMode : GameMode
{
    public override GameModeEnum gameMode => GameModeEnum.RPG;
    
    public override void OnMode()
    {
        
    }

    public override void OffMode()
    {
        
    }

    public override void HandleAttack(Unit unit, Unit target = null)
    {
        unit.attack.StartAttack();
    }

    public override void HandleMove(Unit unit)
    {
        
    }
}



public class GameModeManager : Singleton<GameModeManager>
{
    private GameMode currentGameMode;
    

    public GameModeEnum CurrentGameMode()
    {
        return currentGameMode.gameMode;
    }
    
    public void Init(GameMode gameMode)
    {
        if(currentGameMode != null)
            currentGameMode.OffMode();
        currentGameMode = gameMode;
        currentGameMode.OnMode();
    }

    public void SwitchGameMode(GameMode newGameMode)
    {
        Init(newGameMode);
        
        EventManager.Emit(EventName.GameModeChange);
    }

    public void Attack(Unit unit, Unit target = null)
    {
        currentGameMode.HandleAttack(unit, target);
    }
}
