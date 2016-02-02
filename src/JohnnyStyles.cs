using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace JohnnyCache
{
    public static class JohnnyStyles
    {
        public static IHtmlString Render(params string[] paths)
        {
            var styles = paths.Select(path => Render(path).ToHtmlString()).ToList();
            return MvcHtmlString.Create(String.Join(Environment.NewLine, styles));
        }

        public static IHtmlString Render(string virtualPath)
        {
            if (Common.RenderedTags.ContainsKey(virtualPath))
                return Common.RenderedTags[virtualPath];

            var johnnyPath = Common.BuildJohnnyPath(virtualPath);
            var resolvedPath = Common.ResolveVirtualPath(johnnyPath);

            var tagBuilder = new TagBuilder("link");
            tagBuilder.MergeAttribute("href", resolvedPath);
            tagBuilder.MergeAttribute("rel", "stylesheet");

            var renderedTag = MvcHtmlString.Create(tagBuilder.ToString(TagRenderMode.SelfClosing));
            Common.RenderedTags[virtualPath] = renderedTag;
            return renderedTag;
        }
    }
}
