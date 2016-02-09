using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace JohnnyCache
{
    internal class Common
    {
        private static readonly Dictionary<string, IHtmlString> RenderedTags = new Dictionary<string, IHtmlString>();
        private static readonly List<FileSystemWatcher> FileSystemWatchers = new List<FileSystemWatcher>();
        public static event EventHandler<ErrorEventArgs> ExceptionOccurred;

        internal static string BuildJohnnyPath(string virtualPath)
        {
            if (ConfigurationManager.AppSettings["johnnycache:enabled"] != "true")
                return virtualPath;

            var johnnyPath = virtualPath;
            var ctx = HttpContext.Current;
            var physicalPath = ctx.Server.MapPath(virtualPath);
            if (File.Exists(physicalPath))
            {
                var insertAt = 0;
                var hash = CalculateMd5Hash(File.ReadAllText(physicalPath));
                if (johnnyPath.StartsWith("~/"))
                    insertAt = 2;
                else if (johnnyPath.StartsWith("/"))
                    insertAt = 1;

                johnnyPath = johnnyPath.Insert(insertAt, "johnnycache/" + hash + "/");
            }
            return johnnyPath;
        }

        internal static string ResolveVirtualPath(string virtualPath)
        {
            Uri result;
            if (Uri.TryCreate(virtualPath, UriKind.Absolute, out result))
                return virtualPath;
            var ctx = HttpContext.Current;
            var str = ctx.Request.AppRelativeCurrentExecutionFilePath;
            return Url(str, virtualPath);
        }

        private static string Url(string basePath, string path)
        {
            if (basePath != null)
                path = VirtualPathUtility.Combine(basePath, path);
            path = VirtualPathUtility.ToAbsolute(path);
            return HttpUtility.UrlPathEncode(path);
        }

        private static string CalculateMd5Hash(string input)
        {
            // step 1, calculate MD5 hash from input
            var sha1 = SHA1.Create();
            var inputBytes = Encoding.UTF8.GetBytes(input);
            var hash = sha1.ComputeHash(inputBytes);

            // step 2, convert byte array to hex string
            var sb = new StringBuilder();
            for (var i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("x2"));
            }
            return sb.ToString();
        }

        internal static bool IsRenderedTagCached(string virtualPath)
        {
            return RenderedTags.ContainsKey(virtualPath);
        }

        internal static IHtmlString GetCachedRenderedTag(string virtualPath)
        {
            return RenderedTags[virtualPath];
        }

        internal static void CacheRenderedTag(string virtualPath, IHtmlString renderedTag)
        {
            RenderedTags[virtualPath] = renderedTag;

            var ctx = HttpContext.Current;
            var physicalPath = ctx.Server.MapPath(virtualPath);
            if (File.Exists(physicalPath))
            {
                // add file watcher
                var fsw = new FileSystemWatcher(
                    Path.GetDirectoryName(physicalPath), Path.GetFileName(physicalPath));
                FileSystemWatchers.Add(fsw);

                // remove from lookup on change, to allow for creation of new md5 hash
                fsw.Renamed += (sender, args) =>
                {
                    RenderedTags.Remove(virtualPath);
                };
                fsw.Changed += (sender, args) =>
                {
                    RenderedTags.Remove(virtualPath);
                };
                fsw.Deleted += (sender, args) =>
                {
                    RenderedTags.Remove(virtualPath);
                };
                fsw.Created += (sender, args) =>
                {
                    RenderedTags.Remove(virtualPath);
                };

                fsw.EnableRaisingEvents = true;
            }
        }

        internal static void OnException(Exception ex)
        {
            var handler = ExceptionOccurred;
            if (handler != null)
            {
                var eev = new ErrorEventArgs(ex);
                handler(null, eev);
            }
        }
    }
}
