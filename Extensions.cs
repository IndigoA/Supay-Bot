﻿using System;
using System.Globalization;
using System.Text;

namespace BigSister {
  public static partial class Extensions {

    /// <summary>
    ///   Concatenates a specified separator string between each element of a specified string array, yielding a single concatenated string. </summary>
    /// <param name="startIndex">
    ///   The first array element in value to use. </param>
    /// <param name="separator">
    ///   A System.String. </param>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2233:OperationsShouldNotOverflow", MessageId = "startIndex+1")]
    public static string Join(this string[] self, int startIndex, string separator) {
      if (self.Length == startIndex + 1) {
        return self[startIndex];
      } else {
        return string.Join(separator, self, startIndex, self.Length - startIndex);
      }
    }

    /// <summary>
    ///   Concatenates a space between each element of a specified string array, yielding a single concatenated string. </summary>
    /// <param name="startIndex">
    ///   The first array element in value to use. </param>
    public static string Join(this string[] self, int startIndex) {
      return self.Join(startIndex, " ");
    }

    /// <summary>
    ///   Concatenates a space between each element of a specified string array, yielding a single concatenated string. </summary>
    public static string Join(this string[] self) {
      return self.Join(0);
    }

    /// <summary>
    ///   Converts the specified string representation of a date (and time) to its DateTime equivalent. </summary>
    public static DateTime ToDateTime(this string self) {
      switch (self.Length) {
        case 8:
          return DateTime.ParseExact(self, "yyyyMMdd", CultureInfo.InvariantCulture);
        case 12:
          return DateTime.ParseExact(self, "yyyyMMddHHmm", CultureInfo.InvariantCulture);
        case 14:
          return DateTime.ParseExact(self, "yyyyMMddHHmmss", CultureInfo.InvariantCulture);
        default:
          return DateTime.MinValue;
      }
    }

    /// <summary>
    ///   Converts the numeric value of this instance to its equivalent short string (k/m/b) representation up to a maximum number of decimals. </summary>
    /// <param name="decimals">
    ///   Max number of decimals allowed. </param>
    public static string ToShortString(this double self, int decimals) {
      string format = "#,##0." + new string('#', decimals);
      if (self >= 1000000000 || self <= -1000000000) {
        return Math.Round(self / 1000000000, decimals).ToString(format, CultureInfo.InvariantCulture) + "b";
      } else if (self >= 1000000 || self <= -1000000) {
        return Math.Round(self / 1000000, decimals).ToString(format, CultureInfo.InvariantCulture) + "m";
      } else if (self >= 1000 || self <= -1000) {
        return Math.Round(self / 1000, decimals).ToString(format, CultureInfo.InvariantCulture) + "k";
      } else {
        return Math.Round(self, decimals).ToString(format, CultureInfo.InvariantCulture);
      }
    }

    /// <summary>
    ///   Converts the numeric value of this instance to its equivalent short string (k/m/b) representation up to a maximum number of decimals. </summary>
    /// <param name="decimals">
    ///   Max number of decimals allowed. </param>
    public static string ToShortString(this int self, int decimals) {
      return ((double)self).ToShortString(decimals);
    }

    /// <summary>
    ///   Returns the string representation of the value of this instance in the format: ##days ##hours ##mins ##secs. </summary>
    public static string ToLongString(this TimeSpan self) {
      StringBuilder result = new StringBuilder(30);
      if (self.Days > 0)
        result.Append(self.Days + "day" + (self.Days == 1 ? " " : "s "));
      if (self.Hours > 0)
        result.Append(self.Hours + "hour" + (self.Hours == 1 ? " " : "s "));
      if (self.Minutes > 0)
        result.Append(self.Minutes + "min" + (self.Minutes == 1 ? " " : "s "));
      result.Append(self.Seconds + "sec" + (self.Seconds == 1 ? string.Empty : "s"));
      return result.ToString();
    }

    /// <summary>
    ///   Returns a value indicating whether the specified System.String object occurs within this string. (case insensitive) </summary>
    /// <param name="value">
    ///   The System.String object to seek. </param>
    public static bool ContainsI(this string self, string value) {
      return (self.IndexOf(value, StringComparison.OrdinalIgnoreCase) != -1);
    }

    /// <summary>
    ///   Determines whether the beginning of this instance matches the specified string. (case insensitive) </summary>
    /// <param name="value">
    ///   The System.String object to seek. </param>
    public static bool StartsWithI(this string self, string value) {
      return self.StartsWith(value, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    ///   Replaces the format items of this instance with the text equivalent of the value of a corresponding Object instance in a specified array. </summary>
    /// <param name="args">
    ///   An Object array containing zero or more objects to format. </param>
    public static string FormatWith(this string self, params object[] args) {
      return string.Format(CultureInfo.InvariantCulture, self, args);
    }

  } //class Extensions
} //namespace BigSister