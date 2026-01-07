using EbayListingGenerator.Models;
using EbayListingGenerator.Parsing;

namespace EbayListingGenerator.Services;

public static class ListingBuilder
{
    public static ListingRoot Build(string serial, string normalizedText, string fullText, string compactText)
    {
        var n = NormalizedParser.Parse(normalizedText);
        var f = FullParser.Parse(fullText);
        var c = CompactParser.Parse(compactText);

        var listing = new ListingRoot
        {
            GeneratedUtc = DateTime.UtcNow
        };

        listing.Identity.Serial = serial;

        // Identity/model (prefer compact model line if present)
        if (!string.IsNullOrWhiteSpace(c.Model))
        {
            listing.Identity.Model = c.Model;
            listing.Identity.ModelCode = c.Model.Split(' ').LastOrDefault();
            listing.Identity.Manufacturer = c.Model.Split(' ').FirstOrDefault();
        }
        else
        {
            listing.Identity.Model = NormalizedParser.StrOpt(n, "Model");
            listing.Identity.ModelCode = listing.Identity.Model?.Split(' ').LastOrDefault();
            listing.Identity.Manufacturer = listing.Identity.Model?.Split(' ').FirstOrDefault();
        }

        // CPU
        listing.Specs.Cpu.Name = NormalizedParser.StrOpt(n, "CPU") ?? c.Cpu;
        listing.Specs.Cpu.Cores = NormalizedParser.IntOpt(n, "CPU_Cores");
        listing.Specs.Cpu.Threads = NormalizedParser.IntOpt(n, "CPU_Threads");
        listing.Specs.Cpu.MaxClockMHz = f.CpuMaxClockMHz;

        // RAM (precise + readable)
        listing.Specs.Ram.TotalGiB = NormalizedParser.DoubleOpt(n, "RAM_GiB");

        // readableGB: prefer compact (e.g. "16 GB"), fallback to rounding totalGiB
        listing.Specs.Ram.ReadableGB = ParseReadableGbFromCompact(c.Ram)
                                       ?? (listing.Specs.Ram.TotalGiB is double tg && tg > 0.1 ? (int)Math.Round(tg) : null);

        // moduleCount from full [RAM] ModuleCount (fallback: count modules if parsed)
        listing.Specs.Ram.ModuleCount = f.RamModuleCount;

        // modules from full [RAM.Module.N]
        listing.Specs.Ram.Modules.Clear();
        foreach (var rm in f.RamModules.OrderBy(x => x.Slot))
        {
            listing.Specs.Ram.Modules.Add(new RamModule
            {
                Slot = rm.Slot,
                CapacityBytes = rm.CapacityBytes,
                CapacityGiB = rm.CapacityGiB,
                Manufacturer = rm.Manufacturer,
                PartNumber = rm.PartNumber,
                SerialNumber = rm.SerialNumber,
                SpeedMHz = rm.SpeedMHz,
                ConfiguredSpeedMHz = rm.ConfiguredSpeedMHz,
                DeviceLocator = rm.DeviceLocator,
                BankLabel = rm.BankLabel
            });
        }

        // GPUs (full preferred)
        listing.Specs.Gpus = GpuExtractor.FromFullText(fullText, n);

        // Storage (no guessing)
        listing.Specs.Storage.Primary.Model = f.DiskModel;
        listing.Specs.Storage.Primary.SizeGiB = f.DiskPhysicalSizeGiB ?? f.DiskSizeGiB;
        listing.Specs.Storage.Primary.Interface = f.DiskInterfaceType ?? "Unknown";
        listing.Specs.Storage.Primary.InterfaceHint = null;

        // Normalised marketed storage size (human-readable)
        if (listing.Specs.Storage.Primary.SizeGiB is double sg)
        {
            listing.Specs.Storage.Primary.SizeGB =
                sg >= 230 && sg <= 245 ? 256 :
                sg >= 450 && sg <= 490 ? 512 :
                sg >= 900 && sg <= 990 ? 1024 :
                (int)Math.Round(sg);
        }

        var drv = NormalizedParser.StrOpt(n, "SystemDrive") ?? f.SystemDrive;
        listing.Specs.Storage.SystemDrive.Drive = drv?.EndsWith("::") == true
            ? drv.Substring(0, drv.Length - 1)
            : drv;
        listing.Specs.Storage.SystemDrive.FileSystem = NormalizedParser.StrOpt(n, "SystemDrive_FS") ?? f.SystemDriveFs;
        listing.Specs.Storage.SystemDrive.SizeGiB = NormalizedParser.DoubleOpt(n, "SystemDrive_Size_GiB") ?? f.SystemDriveSizeGiB;
        listing.Specs.Storage.SystemDrive.FreeGiB = NormalizedParser.DoubleOpt(n, "SystemDrive_Free_GiB") ?? f.SystemDriveFreeGiB;

        // Display
        listing.Specs.Display.MaxResolution = NormalizedParser.StrOpt(n, "Display_MaxResolution");
        listing.Specs.Display.RefreshHz = NormalizedParser.IntOpt(n, "Display_RefreshHz");
        listing.Specs.Display.SizeInches = NormalizedParser.DoubleOpt(n, "Display_ScreenSizeInches");

        // Wi-Fi (full preferred because it has radios + driver)
        listing.Specs.Wifi.Generation = f.WifiGeneration ?? NormalizedParser.StrOpt(n, "WiFi_Generation") ?? c.Wifi;
        listing.Specs.Wifi.Radios = f.WifiRadios.Count > 0 ? f.WifiRadios : SplitRadios(NormalizedParser.StrOpt(n, "WiFi_Radios"));
        listing.Specs.Wifi.DriverVendor = f.WifiDriverVendor;
        listing.Specs.Wifi.DriverVersion = f.WifiDriverVersion;

        // Windows
        listing.Windows.Product = NormalizedParser.StrOpt(n, "Windows_Product");
        listing.Windows.DisplayVersion = NormalizedParser.StrOpt(n, "Windows_DisplayVersion");
        listing.Windows.Version = NormalizedParser.StrOpt(n, "Windows_Version");
        listing.Windows.Build = NormalizedParser.IntOpt(n, "Windows_Build") ?? f.WindowsBuild;
        listing.Windows.Ubr = f.WindowsUbr;
        listing.Windows.InstalledUtc = f.WindowsInstalledUtc;

        // Battery
        var health = NormalizedParser.DoubleOpt(n, "Battery_HealthPct");
        var design = NormalizedParser.IntOpt(n, "Battery_Design_mWh");
        var full = NormalizedParser.IntOpt(n, "Battery_Full_mWh");
        var cycles = NormalizedParser.IntOpt(n, "Battery_CycleCount");

        if (health is not null || design is not null || full is not null || cycles is not null)
        {
            listing.Battery = new BatteryInfo
            {
                HealthPct = health,
                DesignCapacitymWh = design,
                FullChargeCapacitymWh = full,
                CycleCount = cycles
            };
        }

        // Ports catalogue (all disabled until you toggle them in JSON)
        listing.Ports = PortCatalog.CreateDefault();

        // Included + Notes (with your corrected line)
        listing.Included.Items = new() { "Laptop only", "AC charger" };
        listing.Notes.Items = new()
        {
            "Windows is activated using an OEM license. Changes to Windows activation status depends on your Microsoft account / licensing and is not guaranteed.",
            "Photos form part of the description."
        };

        // Headline
        listing.Headline.Title = HeadlineBuilder.BuildTitle(listing);
        listing.Headline.Subtitle = HeadlineBuilder.BuildSubtitle(listing);

        return listing;
    }

    private static int? ParseReadableGbFromCompact(string? ram)
    {
        // expects "16 GB" or "16GB"
        if (string.IsNullOrWhiteSpace(ram)) return null;

        var s = ram.Trim().ToUpperInvariant();
        // strip "GB"
        s = s.Replace("GB", "").Trim();
        if (int.TryParse(s, out var v) && v > 0) return v;

        return null;
    }

    private static List<string> SplitRadios(string? radios)
        => string.IsNullOrWhiteSpace(radios)
            ? new List<string>()
            : radios.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
}
