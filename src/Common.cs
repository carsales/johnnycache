using System;
using System.Collections.Generic;
using System.IO;
using System.Web;

namespace JohnnyCache
{
    internal class Common
    {
        internal static readonly Dictionary<string, IHtmlString> RenderedTags = new Dictionary<string, IHtmlString>();

        internal static string BuildJohnnyPath(string virtualPath)
        {
            var johnnyPath = virtualPath;
            var ctx = HttpContext.Current;
            var physicalPath = ctx.Server.MapPath(virtualPath);
            if (File.Exists(physicalPath))
            {
                var insertAt = 0;
                var hash = Math.Abs(File.ReadAllText(physicalPath).GetHashCode());
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


    }
}
