

using System.Collections.Generic;

public class ReactionFactory:Singleton<ReactionFactory>
{

    public UnitReactionConfig reactionConfig;

    public List<Reaction> CreateReactions(Unit attacker, Unit target)
    {
        List<Reaction> reactions = new List<Reaction>();
        
        UnitType type = attacker.unitData.unitType;
        var unitReaction = reactionConfig.unitReacts.Find(e => e.unitType == type);
        if (unitReaction != null)
        {
            foreach (var reactInfo in unitReaction.defaultReactions)
            {
                reactions.Add(reactionConfig.CreateReaction(reactInfo));
            }
        }
        
        return reactions;
    }
}
