using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace Bognabot.Net
{
    public static class NetUtils
    {
        public static string BuildQueryString(this IDictionary<string, string> param)
        {
            if (param == null || !param.Any())
                return "";

            var firstParam = param.First();

            return $"?{firstParam.Key}={firstParam.Value}{string.Join("", param.Skip(1).Select(x => $"&{x.Key}={x.Value}"))}";
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
    }
}