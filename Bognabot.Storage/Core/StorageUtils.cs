using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Bognabot.Storage.Core
{
    public static class StorageUtils
    {
        public static string PathCombine(string first, string second, bool trailing = false)
        {
            var pathSeperator = Path.DirectorySeparatorChar;

            var path = Path.Combine(first, second).Replace(pathSeperator == '/' ? '\\' : '/', pathSeperator);

            if (trailing)
                path += pathSeperator;

            return path;
        }
    }
}