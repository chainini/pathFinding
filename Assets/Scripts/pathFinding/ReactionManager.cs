using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ReactionManager : Singleton<ReactionManager>
{
    private Dictionary<Unit, List<Reaction>> activeReactions = new Dictionary<Unit, List<Reaction>>();
    private Dictionary<Unit, Queue<Pending>> pendingReactions = new Dictionary<Unit, Queue<Pending>>();
    private Dictionary<Unit, int> blockingCount = new Dictionary<Unit, int>();



    /// <summary>
    /// 普通攻击 根据自己信息来判断什么反应
    /// </summary>
    /// <param name="attacker"></param>
    /// <param name="target"></param>
    public void ApplyAttack(Unit attacker, List<Unit> targets)
    {
        Dictionary<Unit, List<Reaction>> pendingReacts = new Dictionary<Unit, List<Reaction>>();

        foreach (Unit target in targets)
        {
            pendingReacts[target] = ReactionFactory.Instance.CreateReactions(attacker, target);
        }

        foreach (var tagretReact in pendingReacts)
        {
            foreach (Reaction reaction in tagretReact.Value)
            {
                ApplyReation(attacker, tagretReact.Key, reaction);
            }
            
        }
    }
    

    public void ApplyReation(Unit attacker, Unit target, Reaction reaction)
    {
        if (!activeReactions.ContainsKey(target)) activeReactions[target] = new List<Reaction>();
        if (!pendingReactions.ContainsKey(target)) pendingReactions[target] = new Queue<Pending>();
        if (!blockingCount.ContainsKey(target)) blockingCount[target] = 0;
        
        var reactions = activeReactions[target];

        if (!reaction.canStack)
        {
            var existing = activeReactions[target].FirstOrDefault((r => r.GetType() == reaction.GetType()));
            if (existing != null)
            {
                existing.duration = reaction.duration;
                return;
            }
        }

        if (blockingCount[target] > 0)
        {
            pendingReactions[target].Enqueue(new Pending(attacker, reaction));
            return;
        }
        
        // reactions.Add(reaction);
        // reaction.OnTrigger(attacker, target, 0);
        
        TriggerNow(attacker, target, reaction);
    }
    
    public struct Pending
    {
        public Unit  attacker;
        public Reaction reaction;

        public Pending(Unit attacker, Reaction reaction)
        {
            this.attacker = attacker;
            this.reaction = reaction;
        }
    }

    public void TriggerNow(Unit attacker, Unit target, Reaction reaction)
    {
        activeReactions[target].Add(reaction);
        
        if(reaction.isBloacking) blockingCount[target]++;
        
        reaction.Ended += OnReactionEnded;
        reaction.OnTrigger(attacker, target, 0);
    }

    public void OnReactionEnded(Unit target)
    {
        var list = activeReactions[target];
        for (int i = list.Count - 1; i >= 0; i--)
        {
            if (list[i].duration <= 0)
            {
                if (list[i].isBloacking)
                {
                    blockingCount[target] = Mathf.Max(0, blockingCount[target] - 1);
                } 
                list[i].Ended -= OnReactionEnded;
                list.RemoveAt(i);
            }
        }

        if (blockingCount[target] == 0 && pendingReactions[target].Count > 0)
        {
            var next = pendingReactions[target].Dequeue();
            TriggerNow(next.attacker, target, next.reaction);
        }
    }
    

    public void Update()
    {
        if (activeReactions.Count > 0)
        {
            UpdateReations();
        }
    }


    public void UpdateReations()
    {
        foreach (var kvp in activeReactions.ToList())
        {
            var unit = kvp.Key;
            var reactions = kvp.Value;

            for (int i = reactions.Count - 1; i >= 0; i--)
            {
                var reaction = reactions[i];
                reaction.OnUpdate(unit, Time.deltaTime);
                reaction.duration -= Time.deltaTime;

                if (reaction.duration <= 0)
                {
                    reaction.OnEnd(unit);
                    reactions.RemoveAt(i);
                }
            }

            if (reactions.Count == 0)
            {
                activeReactions.Remove(unit);
            }
        }
    }

    public Reaction ReactLogic(List<Reaction> reactions)
    {
        return null;
    }
}
