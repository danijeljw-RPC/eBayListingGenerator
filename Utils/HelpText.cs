namespace EbayListingGenerator.Utils;

public static class HelpText
{
    public const string Text = @"
eBay Listing Generator (v2)

USAGE:
  ebaygen extract --dir <folder> --serial <SERIAL> [--out <file.json>]
  ebaygen render  --json <file.json> (--template <name> | --template-file <file.html>) --out <file.html>

EXTRACT INPUTS (inside --dir):
  <serial>.txt
  <serial>_compact.txt
  <serial>_normalized.txt

EXAMPLES:
  dotnet run -- extract --dir ""./specs"" --serial PF26X6RQ --out ""./out/PF26X6RQ.listing.json""
  dotnet run -- render --json ""./out/PF26X6RQ.listing.json"" --template default --out ""./html/PF26X6RQ.html""
  dotnet run -- render --json ""./out/PF26X6RQ.listing.json"" --template-file ""./Templates/default.html"" --out ""./html/PF26X6RQ.html""

TEMPLATE TOKENS (default renderer):
  {{TITLE}}
  {{SUBTITLE}}
  {{SECTION_QUICK_HIGHLIGHTS}}
  {{SECTION_SPECS_THIS_UNIT}}
  {{SECTION_WIFI}}
  {{SECTION_DISPLAY}}
  {{SECTION_BATTERY}}
  {{SECTION_MOBILE_DATA}}
  {{SECTION_PORTS}}
  {{SECTION_INCLUDED}}
  {{SECTION_NOTES}}
  {{SECTION_CONDITION}}

NOTES:
  - extract is deterministic and prefers: normalized -> full -> compact
  - render omits entire sections when data is missing/disabled
";
}
