﻿using System;
using System.Threading.Tasks;

namespace Supay.Bot
{
    internal static partial class Command
    {
        public static async Task Start(CommandContext bc)
        {
            // get rsn
            string rsn = await bc.GetPlayerName(bc.From.Nickname);

            var p = await Player.FromHiscores(rsn);
            if (!p.Ranked)
            {
                await bc.SendReply(@"\b{0}\b doesn't feature Hiscores.", rsn);
                return;
            }

            // get timer name
            string name = string.Empty;
            int indexOfSharp = bc.Message.IndexOf('#');
            if (indexOfSharp > 0)
            {
                name = bc.Message.Substring(indexOfSharp + 1);
                bc.Message = bc.Message.Substring(0, indexOfSharp - 1);
            }

            // get skill
            string skill = Skill.OVER;
            if (bc.MessageTokens.Length > 1)
            {
                Skill.TryParse(bc.MessageTokens[1], ref skill);
            }

            // remove previous timer with this name, if any
            await Database.ExecuteNonQuery("DELETE FROM timers_exp WHERE fingerprint='" + bc.From.FingerPrint + "' AND name='" + name.Replace("'", "''") + "'");

            // start a new timer with this name
            await Database.Insert("timers_exp", "fingerprint", bc.From.FingerPrint, "name", name, "skill", skill, "exp", p.Skills[skill].Exp.ToStringI(), "datetime", DateTime.UtcNow.ToStringI("yyyyMMddHHmmss"));
            await bc.SendReply(@"\b{0}\b starting exp of \c07{1:e}\c in \u{1:n}\u has been recorded{2}.", rsn, p.Skills[skill], name.Length > 0 ? @" on timer \c07" + name + @"\c" : string.Empty);
        }
    }
}
