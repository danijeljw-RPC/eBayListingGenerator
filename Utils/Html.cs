using System.Net;

namespace EbayListingGenerator.Utils;

public static class Html
{
    public static string Escape(string s) => WebUtility.HtmlEncode(s ?? string.Empty);
}
