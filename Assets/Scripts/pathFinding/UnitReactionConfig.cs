using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class UnitReact
{
    public UnitType unitType;
    public List<ReactInfo> defaultReactions;
}

[System.Serializable]
public class ReactInfo
{
    public React react;
    public float duration;
    public Vector2 konckbackInfo;
    public float positionOffset;
}

[CreateAssetMenu(fileName = "UnitReactionConfig", menuName = "Config/UnitReactionConfig")]
public class UnitReactionConfig:ScriptableObject
{
    public List<UnitReact> unitReacts;


    public Reaction CreateReaction(ReactInfo reactInfo)
    {
        switch (reactInfo.react)
        {
            case React.HitStop:
                return new HitStopReaction(reactInfo.positionOffset, reactInfo.duration);
                break;
            case React.Knockback:
                return new KnockbackReaction(reactInfo.konckbackInfo, reactInfo.duration);
                break;
            case React.Stun:
                break;
            case React.Fire:
                break;
            case React.Ice:
                break;
            case React.Lightning:
                break;
            case React.Water:
                break;
            default:
                break;
        }

        return null;
    }
    
}

