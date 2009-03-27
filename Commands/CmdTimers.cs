﻿using System;
using System.Data.SQLite;
using System.Globalization;

namespace BigSister {
  static class CmdTimers {

    public static void Start(CommandContext bc) {
      // get rsn
      string rsn = bc.From.Rsn;

      Player p = new Player(rsn);
      if (!p.Ranked) {
        bc.SendReply("\\b{0}\\b doesn't feature Hiscores.".FormatWith(rsn));
        return;
      }

      // get timer name
      string name = string.Empty;
      int indexofsharp = bc.Message.IndexOf('#');
      if (indexofsharp > 0) {
        name = bc.Message.Substring(indexofsharp + 1);
        bc.Message = bc.Message.Substring(0, indexofsharp - 1);
      }

      // get skill
      string skill = Skill.OVER;
      if (bc.MessageTokens.Length > 1)
        Skill.TryParse(bc.MessageTokens[1], ref skill);

      // remove previous timer with this name, if any
      Database.ExecuteNonQuery("DELETE FROM timers_exp WHERE fingerprint='" + bc.From.FingerPrint + "' AND name='" + name.Replace("'", "''") + "';");

      // start a new timer with this name
      Database.Insert("timers_exp", "fingerprint", bc.From.FingerPrint,
                                    "name", name,
                                    "skill", skill,
                                    "exp", p.Skills[skill].Exp.ToStringI(),
                                    "datetime", DateTime.Now.ToStringI("yyyyMMddHHmmss"));
      bc.SendReply("\\b{0}\\b starting exp of \\c07{1:e}\\c in \\u{1:n}\\u has been recorded{2}.".FormatWith(rsn, p.Skills[skill], name.Length > 0 ? " on timer \\c07" + name + "\\c" : string.Empty));
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase")]
    public static void Check(CommandContext bc) {
      // get rsn
      string rsn = bc.From.Rsn;

      Player p = new Player(rsn);
      if (!p.Ranked) {
        bc.SendReply("\\b{0}\\b doesn't feature Hiscores.".FormatWith(rsn));
        return;
      }

      // get timer name
      string name = string.Empty;
      int indexofsharp = bc.Message.IndexOf('#');
      if (indexofsharp > 0) {
        name = bc.Message.Substring(indexofsharp + 1);
        bc.Message = bc.Message.Substring(0, indexofsharp - 1);
      }

      SQLiteDataReader rs = Database.ExecuteReader("SELECT skill, exp, datetime FROM timers_exp WHERE fingerprint='" + bc.From.FingerPrint + "' AND name='" + name.Replace("'", "''") + "' LIMIT 1;");
      if (rs.Read()) {
        string skill = rs.GetString(0);

        int gained_exp = p.Skills[skill].Exp - rs.GetInt32(1);
        TimeSpan time = DateTime.Now - rs.GetString(2).ToDateTime();

        string reply = "You gained \\c07{0:N0}\\c \\u{1}\\u exp in \\c07{2}\\c. That's \\c07{3:N0}\\c exp/h.".FormatWith(gained_exp, skill.ToLowerInvariant(), time.ToLongString(), (double)gained_exp / (double)time.TotalHours);
        if (gained_exp > 0 && skill != Skill.OVER && skill != Skill.COMB && p.Skills[skill].VLevel < 126)
          reply += " Estimated time to level up: \\c07{0}\\c".FormatWith(TimeSpan.FromSeconds((double)p.Skills[skill].ExpToVLevel * (double)time.TotalSeconds / (double)gained_exp).ToLongString());
        bc.SendReply(reply);
      } else {
        if (name.Length > 0)
          bc.SendReply("You must start timing a skill on that timer first.");
        else
          bc.SendReply("You must start timing a skill first.");
      }
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase")]
    public static void Stop(CommandContext bc) {
      // get rsn
      string rsn = bc.From.Rsn;

      Player p = new Player(rsn);
      if (!p.Ranked) {
        bc.SendReply("\\b{0}\\b doesn't feature Hiscores.".FormatWith(rsn));
        return;
      }

      // get timer name
      string name = string.Empty;
      int indexofsharp = bc.Message.IndexOf('#');
      if (indexofsharp > 0) {
        name = bc.Message.Substring(indexofsharp + 1);
        bc.Message = bc.Message.Substring(0, indexofsharp - 1);
      }

      SQLiteDataReader rs = Database.ExecuteReader("SELECT skill, exp, datetime FROM timers_exp WHERE fingerprint='" + bc.From.FingerPrint + "' AND name='" + name.Replace("'", "''") + "' LIMIT 1;");
      if (rs.Read()) {
        string skill = rs.GetString(0);

        int gained_exp = p.Skills[skill].Exp - rs.GetInt32(1);
        TimeSpan time = DateTime.Now - rs.GetString(2).ToDateTime();

        string reply = "You gained \\c07{0:N0}\\c \\u{1}\\u exp in \\c07{2}\\c. That's \\c07{3:N0}\\c exp/h.".FormatWith(gained_exp, skill.ToLowerInvariant(), time.ToLongString(), (double)gained_exp / (double)time.TotalHours);
        if (gained_exp > 0 && skill != Skill.OVER && skill != Skill.COMB && p.Skills[skill].VLevel < 126)
          reply += " Estimated time to level up: \\c07{0}\\c".FormatWith(TimeSpan.FromSeconds((double)p.Skills[skill].ExpToVLevel / ((double)gained_exp / (double)time.TotalSeconds)).ToLongString());
        bc.SendReply(reply);

        // remove the timer with this name
        Database.ExecuteNonQuery("DELETE FROM timers_exp WHERE fingerprint='" + bc.From.FingerPrint + "' AND name='" + name.Replace("'", "''") + "';");
      } else {
        if (name.Length > 0)
          bc.SendReply("You must start timing a skill on that timer first.");
        else
          bc.SendReply("You must start timing a skill first.");
      }
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase")]
    public static void Timer(CommandContext bc) {
      if (bc.MessageTokens.Length == 1) {
        SQLiteDataReader rsTimer = Database.ExecuteReader("SELECT name, duration, started FROM timers WHERE fingerprint='" + bc.From.FingerPrint + "' OR nick='" + bc.From.Nick + "';");
        int timers = 0;
        string reply = string.Empty;
        while (rsTimer.Read()) {
          timers++;
          DateTime start = rsTimer.GetString(2).ToDateTime();
          DateTime end = start.AddSeconds(rsTimer.GetDouble(1));
          reply += " \\b#{0}\\b timer (\\c07{1}\\c) ends in \\c07{2}\\c, at \\c07{3}\\c;".FormatWith(timers, rsTimer.GetString(0), (end - DateTime.Now).ToLongString(), end.ToStringI("yyyy/MM/dd HH:mm:ss"));
        }
        rsTimer.Close();
        if (timers > 0)
          bc.SendReply("Found \\c07{0}\\c timers:".FormatWith(timers) + reply);
        else
          bc.SendReply("Syntax: !timer <duration>");

        return;
      }

      // get duration
      int duration;
      string name = null;
      switch (bc.MessageTokens[1].ToUpperInvariant()) {
        case "FARM":
        case "HERB":
        case "HERBS":
          duration = 75;
          name = bc.MessageTokens[1].ToLowerInvariant();
          break;
        case "DAY":
          duration = 1440;
          name = bc.MessageTokens[1].ToLowerInvariant();
          break;
        case "WEEK":
        case "TOG":
          duration = 10080;
          name = bc.MessageTokens[1].ToLowerInvariant();
          break;
        default:
          if (!int.TryParse(bc.MessageTokens[1], out duration)) {
            bc.SendReply("Error: Invalid duration. Duration must be in minutes.");
            return;
          }
          name = duration + " mins";
          break;
      }

      // start a new timer for this duration
      Database.Insert("timers", "fingerprint", bc.From.FingerPrint,
                                "nick", bc.From.Nick,
                                "name", name,
                                "duration", (duration * 60).ToStringI(),
                                "started", DateTime.Now.ToStringI("yyyyMMddHHmmss"));
      bc.SendReply("Timer started to \\b{0}\\b. Timer will end at \\c07{1}\\c.".FormatWith(bc.From.Nick, DateTime.Now.AddMinutes(duration).ToStringI("yyyy/MM/dd HH:mm:ss")));
    }

  } //class CmdTimers
} //namespace BigSister