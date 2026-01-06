using EbayListingGenerator.Services;
using EbayListingGenerator.Utils;

namespace EbayListingGenerator;

public static class Program
{
    public static int Main(string[] args)
    {
        try
        {
            var parsed = Argv.Parse(args);

            if (parsed.Command is null || parsed.Command is "help" or "--help" or "-h")
            {
                Console.WriteLine(HelpText.Text);
                return 0;
            }

            return parsed.Command switch
            {
                "extract" => new ExtractCommand().Run(parsed),
                "render"  => new RenderCommand().Run(parsed),
                _ => Fail($"Unknown command '{parsed.Command}'.\n\n{HelpText.Text}")
            };
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex.ToString());
            return 1;
        }
    }

    private static int Fail(string message)
    {
        Console.Error.WriteLine(message);
        return 2;
    }
}
