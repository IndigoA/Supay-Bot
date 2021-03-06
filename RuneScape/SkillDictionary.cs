﻿using System.Collections.Generic;
using System.Linq;

namespace Supay.Bot
{
    internal class SkillDictionary : OrderedDictionary<string, Skill>
    {
        public SkillDictionary()
            : base(27)
        {
        }

        public long F2pExp
        {
            get
            {
                return this[Skill.ATTA].Exp + this[Skill.DEFE].Exp + this[Skill.STRE].Exp + this[Skill.HITP].Exp + this[Skill.RANG].Exp + this[Skill.PRAY].Exp + this[Skill.MAGI].Exp + this[Skill.COOK].Exp + this[Skill.WOOD].Exp + this[Skill.FISH].Exp + this[Skill.FIRE].Exp + this[Skill.CRAF].Exp + this[Skill.SMIT].Exp + this[Skill.MINI].Exp + this[Skill.RUNE].Exp + this[Skill.DUNG].Exp;
            }
        }

        public List<Skill> Lowest
        {
            get
            {
                var lowest = new List<Skill>(this.Values);
                lowest.RemoveAll(s => s.Name == Skill.OVER || s.Name == Skill.COMB);
                lowest.Sort((s1, s2) => s1.Exp.CompareTo(s2.Exp));
                return lowest;
            }
        }

        public List<Skill> Highest
        {
            get
            {
                var highest = new List<Skill>(this.Values);
                highest.RemoveAll(s => s.Name == Skill.OVER || s.Name == Skill.COMB);
                highest.Sort((s1, s2) => -s1.Exp.CompareTo(s2.Exp));
                return highest;
            }
        }

        public List<Skill> LowestRanked
        {
            get
            {
                var lowest = new List<Skill>(this.Values);
                lowest.RemoveAll(s => s.Name == Skill.OVER || s.Name == Skill.COMB);
                foreach (Skill s in lowest.FindAll(s => s.Rank == -1))
                {
                    s.Rank = int.MaxValue;
                }
                lowest.Sort((s1, s2) => -s1.Rank.CompareTo(s2.Rank));
                return lowest;
            }
        }

        public List<Skill> HighestRanked
        {
            get
            {
                var highest = new List<Skill>(this.Values);
                highest.RemoveAll(s => s.Name == Skill.OVER || s.Name == Skill.COMB);
                foreach (Skill s in highest.FindAll(s => s.Rank == -1))
                {
                    s.Rank = int.MaxValue;
                }
                highest.Sort((s1, s2) => s1.Rank.CompareTo(s2.Rank));
                return highest;
            }
        }

        public IEnumerable<Skill> SortedByExpToNextVLevel
        {
            get
            {
                return from skill in Values
                       where skill.ExpToVLevel != 0 && skill.Name != Skill.OVER && skill.Name != Skill.COMB
                       orderby skill.ExpToVLevel
                       select skill;
            }
        }
    }
}
