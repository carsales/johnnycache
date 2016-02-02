using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace JohnnyCache
{
    public static class JohnnyScripts
    {
        public static IHtmlString Render(params string[] paths)
        {
            var scripts = paths.Select(path => Render(path).ToHtmlString()).ToList();
            return MvcHtmlString.Create(String.Join(Environment.NewLine, scripts));
        }

        public static IHtmlString Render(string virtualPath)
        {
            if (Common.RenderedTags.ContainsKey(virtualPath))
                return Common.RenderedTags[virtualPath];

            var johnnyPath = Common.BuildJohnnyPath(virtualPath);
            var resolvedPath = Common.ResolveVirtualPath(johnnyPath);

            var tagBuilder = new TagBuilder("script");
            tagBuilder.MergeAttribute("src", resolvedPath);
            tagBuilder.MergeAttribute("type", "text/javascript");

            var renderedTag = MvcHtmlString.Create(tagBuilder.ToString(TagRenderMode.Normal));
            Common.RenderedTags[virtualPath] = renderedTag;
            return renderedTag;
        }
    }
}
