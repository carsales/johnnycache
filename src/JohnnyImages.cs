using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;

namespace JohnnyCache
{
    public static class JohnnyImages
    {
        public static IHtmlString Render(string virtualPath, string alt = null, string width = null, string height = null, string @class = null, string style = null)
        {
            if (Common.IsRenderedTagCached(virtualPath))
                return Common.GetCachedRenderedTag(virtualPath);

            var johnnyPath = Common.BuildJohnnyPath(virtualPath);

            var tb = new TagBuilder("img");
            tb.MergeAttribute("src", johnnyPath);

            if (!String.IsNullOrWhiteSpace(alt))
                tb.MergeAttribute("alt", alt);
            if (!String.IsNullOrWhiteSpace(width))
                tb.MergeAttribute("width", width);
            if (!String.IsNullOrWhiteSpace(height))
                tb.MergeAttribute("height", height);
            if (!String.IsNullOrWhiteSpace(@class))
                tb.MergeAttribute("class", @class);
            if (!String.IsNullOrWhiteSpace(style))
                tb.MergeAttribute("style", style);

            var renderedTag = MvcHtmlString.Create(tb.ToString());
            Common.CacheRenderedTag(virtualPath, renderedTag);
            return renderedTag;
        }
    }
}
