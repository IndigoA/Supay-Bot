﻿using System.Threading.Tasks;

namespace Supay.Bot
{
    internal static partial class Command
    {
        public static async Task Compare(CommandContext bc)
        {
            if (bc.MessageTokens.Length == 1)
            {
                await bc.SendReply("Syntax: !compare [skill] <player1> [player2]");
                return;
            }

            string skill1 = null,
                   activity1 = null;
            string rsn1,
                   rsn2;
            if (Skill.TryParse(bc.MessageTokens[1], ref skill1))
            {
                if (bc.MessageTokens.Length == 3)
                {
                    // !compare <skill> <player2>
                    rsn1 = await bc.GetPlayerName(bc.From.Nickname);
                    rsn2 = await bc.GetPlayerName(bc.MessageTokens[2]);
                }
                else if (bc.MessageTokens.Length > 3)
                {
                    // !compare <skill> <player1> <player2>
                    rsn1 = await bc.GetPlayerName(bc.MessageTokens[2]);
                    rsn2 = await bc.GetPlayerName(bc.MessageTokens.Join(3));
                }
                else
                {
                    // !compare <player2>
                    skill1 = Skill.OVER;
                    rsn1 = await bc.GetPlayerName(bc.From.Nickname);
                    rsn2 = await bc.GetPlayerName(bc.MessageTokens[1]);
                }
            }
            else if (Bot.Activity.TryParse(bc.MessageTokens[1], ref activity1))
            {
                if (bc.MessageTokens.Length == 3)
                {
                    // !compare <activity> <player2>
                    rsn1 = await bc.GetPlayerName(bc.From.Nickname);
                    rsn2 = await bc.GetPlayerName(bc.MessageTokens[2]);
                }
                else if (bc.MessageTokens.Length > 3)
                {
                    // !compare <activity> <player1> <player2>
                    rsn1 = await bc.GetPlayerName(bc.MessageTokens[2]);
                    rsn2 = await bc.GetPlayerName(bc.MessageTokens.Join(3));
                }
                else
                {
                    // !compare <player2>
                    skill1 = Skill.OVER;
                    rsn1 = await bc.GetPlayerName(bc.From.Nickname);
                    rsn2 = await bc.GetPlayerName(bc.MessageTokens[1]);
                }
            }
            else if (bc.MessageTokens.Length == 2)
            {
                // !compare <player2>
                skill1 = Skill.OVER;
                rsn1 = await bc.GetPlayerName(bc.From.Nickname);
                rsn2 = await bc.GetPlayerName(bc.MessageTokens[1]);
            }
            else
            {
                // !compare <player1> <player2>
                skill1 = Skill.OVER;
                rsn1 = await bc.GetPlayerName(bc.MessageTokens[1]);
                rsn2 = await bc.GetPlayerName(bc.MessageTokens.Join(2));
            }

            var p1 = await Player.FromHiscores(rsn1);
            if (!p1.Ranked)
            {
                await bc.SendReply(@"\b{0}\b doesn't feature Hiscores.", rsn1);
                return;
            }

            var p2 = await Player.FromHiscores(rsn2);
            if (!p2.Ranked)
            {
                await bc.SendReply(@"\b{0}\b doesn't feature Hiscores.", rsn2);
                return;
            }

            string reply;
            if (activity1 == null)
            {
                // compare skills
                Skill pskill1 = p1.Skills[skill1];
                Skill pskill2 = p2.Skills[skill1];

                if (pskill1.Level == pskill2.Level)
                {
                    reply = @"Both \b{0}\b and \b{1}\b have level \c07{2}\c".FormatWith(p1.Name, p2.Name, pskill1.Level);
                    if (pskill1.Exp == pskill2.Exp)
                    {
                        reply += @" and \c07{0:e}\c experience.".FormatWith(pskill1);
                    }
                    else if (pskill1.Exp > pskill2.Exp)
                    {
                        reply += @", but \b{0}\b has \c07{1:N0}\c more experience.".FormatWith(p1.Name, pskill1.Exp - pskill2.Exp);
                    }
                    else
                    {
                        reply += @", but \b{0}\b has \c07{1:N0}\c less experience.".FormatWith(p1.Name, pskill2.Exp - pskill1.Exp);
                    }
                }
                else if (pskill1.Level > pskill2.Level)
                {
                    reply = @"\b{0}\b has \c07{2}\c more level{3} than \b{1}\b.".FormatWith(p1.Name, p2.Name, pskill1.Level - pskill2.Level, pskill1.Level - pskill2.Level == 1 ? string.Empty : "s");
                    if (pskill1.Exp == pskill2.Exp)
                    {
                        reply += @", but both have \c07{0:e}\c experience.".FormatWith(pskill1);
                    }
                    else if (pskill1.Exp > pskill2.Exp)
                    {
                        reply += @" and has \c07{0:N0}\c more experience.".FormatWith(pskill1.Exp - pskill2.Exp);
                    }
                    else
                    {
                        reply += @", but \b{0}\b has \c07{1:N0}\c less experience.".FormatWith(p1.Name, pskill2.Exp - pskill1.Exp);
                    }
                }
                else
                {
                    reply = @"\b{0}\b has \c07{2}\c less level{3} than \b{1}\b.".FormatWith(p1.Name, p2.Name, pskill2.Level - pskill1.Level, pskill2.Level - pskill1.Level == 1 ? string.Empty : "s");
                    if (pskill1.Exp == pskill2.Exp)
                    {
                        reply += @", but both have \c07{0:e}\c experience.".FormatWith(pskill1);
                    }
                    else if (pskill1.Exp > pskill2.Exp)
                    {
                        reply += @", but \b{0}\b has \c07{1:N0}\c more experience.".FormatWith(p1.Name, pskill1.Exp - pskill2.Exp);
                    }
                    else
                    {
                        reply += @" and has \c07{0:N0}\c less experience.".FormatWith(pskill2.Exp - pskill1.Exp);
                    }
                }
                await bc.SendReply(reply);

                // get these players last update time
                var dblastupdate = await Database.LastUpdate(rsn1);
                if (dblastupdate != null && dblastupdate.Length == 8)
                {
                    p1 = await Player.FromDatabase(rsn1, dblastupdate.ToDateTime());
                    if (p1.Ranked)
                    {
                        dblastupdate = await Database.LastUpdate(rsn2);
                        if (dblastupdate != null && dblastupdate.Length == 8)
                        {
                            p2 = await Player.FromDatabase(rsn2, dblastupdate.ToDateTime());
                            if (p2.Ranked)
                            {
                                Skill skilldif1 = pskill1 - p1.Skills[skill1];
                                Skill skilldif2 = pskill2 - p2.Skills[skill1];
                                await bc.SendReply(@"Today \b{0}\b did \c07{1:e}\c exp. while \b{2}\b did \c07{3:e}\c exp.", rsn1, skilldif1, rsn2, skilldif2);
                            }
                        }
                    }
                }
            }
            else
            {
                // compare activities
                Activity p1Activity = p1.Activities[activity1];
                Activity p2Activity = p2.Activities[activity1];

                if (p1Activity.Rank == -1)
                {
                    await bc.SendReply(@"\b{0}\b doesn't feature Hiscores.", rsn1);
                    return;
                }
                if (p2Activity.Rank == -1)
                {
                    await bc.SendReply(@"\b{0}\b doesn't feature Hiscores.", rsn2);
                    return;
                }

                if (p1Activity.Score == p2Activity.Score)
                {
                    reply = @"Both \b{0}\b and \b{1}\b have \c07{2}\c score.".FormatWith(rsn1, rsn2, p1Activity.Score);
                }
                else if (p1Activity.Score > p2Activity.Score)
                {
                    reply = @"\b{0}\b has \c07{2}\c more score than \b{1}\b.".FormatWith(rsn1, rsn2, p1Activity.Score - p2Activity.Score);
                }
                else
                {
                    reply = @"\b{0}\b has \c07{2}\c less score than \b{1}\b.".FormatWith(rsn1, rsn2, p2Activity.Score - p1Activity.Score);
                }
                await bc.SendReply(reply);

                // get these players last update time
                var dblastupdate = await Database.LastUpdate(rsn1);
                if (dblastupdate != null && dblastupdate.Length == 8)
                {
                    p1 = await Player.FromDatabase(rsn1, dblastupdate.ToDateTime());
                    if (p1.Ranked && p1.Activities[activity1].Rank > 0)
                    {
                        dblastupdate = await Database.LastUpdate(rsn2);
                        if (dblastupdate != null && dblastupdate.Length == 8)
                        {
                            p2 = await Player.FromDatabase(rsn2, dblastupdate.ToDateTime());
                            if (p2.Ranked && p2.Activities[activity1].Rank > 0)
                            {
                                Activity p1ActivityDelta = p1Activity - p1.Activities[activity1];
                                Activity p2ActivityDelta = p2Activity - p2.Activities[activity1];
                                await bc.SendReply(@"Today \b{0}\b did \c07{1:s}\c score while \b{2}\b did \c07{3:s}\c score.", rsn1, p1ActivityDelta, rsn2, p2ActivityDelta);
                            }
                        }
                    }
                }
            }
        }
    }
}
