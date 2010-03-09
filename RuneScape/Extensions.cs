﻿using System;
using System.Text.RegularExpressions;

namespace Supay.Bot {
  public static partial class Extensions {

    public static string ToRsn(this string rsn) {
      return Regex.Replace(rsn.Substring(0, Math.Min(12, rsn.Length)), @"\W", "_");
    }

    public static int ToExp(this int level) {
      int exp = 0;
      while (level > 1)
        exp += --level + (int)(300.0 * Math.Pow(2.0, (double)level / 7.0));
      return exp / 4;
    }

    public static int ToLevel(this int exp) {
      int level = 0;
      int levelExp = 0;
      while (levelExp / 4 <= exp)
        levelExp += ++level + (int)(300.0 * Math.Pow(2.0, (double)level / 7.0));
      return level;
    }

  } //class Extensions
} //namespace Supay.Bot