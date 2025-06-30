using System;
using Terraria;

namespace Stratum.Core.Systems
{
    /// <summary>
    /// Represents a single phase of a boss fight with an entry condition and a one-time trigger.
    /// </summary>
    public class BossPhase
    {
        /// <summary>Phase ID, used for ordering.</summary>
        public int Id;

        /// <summary>Human-readable name for debugging or phase-specific logic.</summary>
        public string Name;

        /// <summary>Function that determines whether this phase can be entered.</summary>
        public Func<NPC, bool> EntryCondition;

        /// <summary>Logic to execute the moment this phase starts.</summary>
        public Action<NPC> OnEnter;

        public BossPhase(int id, string name, Func<NPC, bool> entryCondition, Action<NPC> onEnter = null)
        {
            Id = id;
            Name = name;
            EntryCondition = entryCondition;
            OnEnter = onEnter ?? (_ => {  });
        }
    }
}
