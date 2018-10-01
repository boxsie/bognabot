using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using Bognabot.Data.Exchange.Enums;
using Newtonsoft.Json;

namespace Bognabot.Services.Exchange
{
    public static class ExchangeUtils
    {
        public static string BuildQueryString(this IDictionary<string, string> param)
        {
            if (param == null || !param.Any())
                return "";

            var firstParam = param.First();

            return $"{firstParam.Key}={firstParam.Value}{string.Join("", param.Skip(1).Select(x => $"&{x.Key}={x.Value}"))}";
        }

        public static byte[] EncodeText(string text, EncodingType encodingType)
        {
            switch (encodingType)
            {
                case EncodingType.UTF7:
                    return Encoding.UTF7.GetBytes(text);
                case EncodingType.UTF8:
                    return Encoding.UTF8.GetBytes(text);
                case EncodingType.UTF32:
                    return Encoding.UTF32.GetBytes(text);
                case EncodingType.ASCII:
                    return Encoding.ASCII.GetBytes(text);
                case EncodingType.Unicode:
                    return Encoding.Unicode.GetBytes(text);
                case EncodingType.BigEndianUnicode:
                    return Encoding.BigEndianUnicode.GetBytes(text);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static string DecodeText(byte[] buffer, int resultLen, EncodingType encodingType)
        {
            switch (encodingType)
            {
                case EncodingType.UTF7:
                    return Encoding.UTF7.GetString(buffer, 0, resultLen);
                case EncodingType.UTF8:
                    return Encoding.UTF8.GetString(buffer, 0, resultLen);
                case EncodingType.UTF32:
                    return Encoding.UTF32.GetString(buffer, 0, resultLen);
                case EncodingType.ASCII:
                    return Encoding.ASCII.GetString(buffer, 0, resultLen);
                case EncodingType.Unicode:
                    return Encoding.Unicode.GetString(buffer, 0, resultLen);
                case EncodingType.BigEndianUnicode:
                    return Encoding.BigEndianUnicode.GetString(buffer, 0, resultLen);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static DateTime GetTimeOffsetFromDataPoints(TimePeriod period, DateTime start, int dataPoints)
        {
            switch (period)
            {
                case TimePeriod.OneMinute:
                    return start.AddMinutes(-dataPoints);
                case TimePeriod.FiveMinutes:
                    return start.AddMinutes(-dataPoints * 5);
                case TimePeriod.FifteenMinutes:
                    return start.AddMinutes(-dataPoints * 15);
                case TimePeriod.OneHour:
                    return start.AddHours(-dataPoints);
                case TimePeriod.OneDay:
                    return start.AddDays(-dataPoints);
                default:
                    throw new ArgumentOutOfRangeException(nameof(period), period, null);
            }
        }

        public static int GetDataPointsFromTimeSpan(TimePeriod period, TimeSpan span)
        {
            var mins = (int)span.TotalMinutes;

            switch (period)
            {
                case TimePeriod.OneMinute:
                    return mins;
                case TimePeriod.FiveMinutes:
                    return mins / 5;
                case TimePeriod.FifteenMinutes:
                    return mins / 15;
                case TimePeriod.OneHour:
                    return mins / 60;
                case TimePeriod.OneDay:
                    return mins / (24 * 60);
                default:
                    throw new ArgumentOutOfRangeException(nameof(period), period, null);
            }
        }

        public static Dictionary<string, string> AsDictionary(this object source, BindingFlags bindingAttr = BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance)
        {
            return source.GetType().GetProperties(bindingAttr).ToDictionary
            (
                propInfo => propInfo.GetCustomAttributes(typeof(JsonPropertyAttribute))?.Cast<JsonPropertyAttribute>().FirstOrDefault()?.PropertyName ?? propInfo.Name,
                propInfo => propInfo.GetValue(source, null).ToString()
            );
        }

        public static string GetCandleDataKey(string exchangeName, Instrument instrument, TimePeriod period)
        {
            return $"{exchangeName}_{instrument}_{period}_Candles";
        }

        public static string GetExchangePositionKey(string exchangeName, Instrument instrument)
        {
            return $"{exchangeName.ToLower()}_{instrument.ToString().ToLower()}_Position";
        }
    }
}