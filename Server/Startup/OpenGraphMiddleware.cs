using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace MountainStates.MSSA.Server.Startup
{
    // Oqtane's own rendered HTML has no Open Graph tags at all, so a link shared on
    // Facebook (or anywhere else reading OG tags) falls back to Facebook's own
    // heuristic of grabbing a prominent <img> from the page - which ends up being the
    // site logo in the navbar, center-cropped to Facebook's preview aspect ratio. This
    // injects an explicit og:image (a dedicated 1200x630 image, not the banner) plus
    // og:title/og:description/og:url into every HTML page response, so sharing tools
    // use that instead of guessing.
    public static class OpenGraphMiddleware
    {
        private static readonly Regex TitleTagRegex = new(@"<title>(.*?)</title>", RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.IgnoreCase);

        public static async Task InvokeAsync(HttpContext context, Func<Task> next)
        {
            // Only worth intercepting a GET for an actual page - API calls, uploads,
            // and static assets are never text/html, and buffering the whole response
            // body to rewrite it isn't free.
            if (context.Request.Method != HttpMethods.Get || context.Request.Path.StartsWithSegments("/api"))
            {
                await next();
                return;
            }

            var originalBody = context.Response.Body;
            using var buffer = new MemoryStream();
            context.Response.Body = buffer;

            try
            {
                await next();
            }
            finally
            {
                context.Response.Body = originalBody;
            }

            buffer.Seek(0, SeekOrigin.Begin);

            if (!context.Response.ContentType?.Contains("text/html", StringComparison.OrdinalIgnoreCase) ?? true)
            {
                await buffer.CopyToAsync(originalBody);
                return;
            }

            string html;
            using (var reader = new StreamReader(buffer, Encoding.UTF8, leaveOpen: true))
            {
                html = await reader.ReadToEndAsync();
            }

            if (html.Contains("property=\"og:image\"", StringComparison.OrdinalIgnoreCase) || !html.Contains("</head>", StringComparison.OrdinalIgnoreCase))
            {
                // Already has OG tags (unexpected, but don't double them up), or isn't
                // a full HTML document - pass through unmodified either way.
                context.Response.ContentLength = buffer.Length;
                buffer.Seek(0, SeekOrigin.Begin);
                await buffer.CopyToAsync(originalBody);
                return;
            }

            var titleMatch = TitleTagRegex.Match(html);
            var pageTitle = titleMatch.Success ? titleMatch.Groups[1].Value : "MSSA - Mountain States Stockdog Association";

            var requestUrl = $"{context.Request.Scheme}://{context.Request.Host}{context.Request.Path}{context.Request.QueryString}";
            var imageUrl = $"{context.Request.Scheme}://{context.Request.Host}/images/sharinglogo.jpg";

            var ogTags = $"""
                <meta property="og:type" content="website" />
                <meta property="og:site_name" content="Mountain States Stockdog Association" />
                <meta property="og:title" content="{System.Net.WebUtility.HtmlEncode(pageTitle)}" />
                <meta property="og:description" content="Mountain States Stockdog Association - herding trial results, run order, and event information." />
                <meta property="og:url" content="{System.Net.WebUtility.HtmlEncode(requestUrl)}" />
                <meta property="og:image" content="{System.Net.WebUtility.HtmlEncode(imageUrl)}" />
                <meta property="og:image:width" content="1200" />
                <meta property="og:image:height" content="630" />
                <meta name="twitter:card" content="summary_large_image" />
                </head>
                """;

            var updatedHtml = Regex.Replace(html, "</head>", ogTags, RegexOptions.IgnoreCase);
            var updatedBytes = Encoding.UTF8.GetBytes(updatedHtml);

            context.Response.ContentLength = updatedBytes.Length;
            await originalBody.WriteAsync(updatedBytes);
        }
    }
}
