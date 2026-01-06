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
            GeneratedUtc = DateTime.UtcNow,
            Identity =
            {
                Serial = serial,
                Manufacturer = NormalizedParser.StrOpt(n, "Model")?.Split(' ', 2).FirstOrDefault(),
                Model = NormalizedParser.StrOpt(n, "Model"),
                ModelCode = NormalizedParser.StrOpt(n, "Model")?.Split(' ').LastOrDefault(),
            }
        };

        // Prefer compact model if present (it is user-friendly and stable)
        if (!string.IsNullOrWhiteSpace(c.Model))
        {
            listing.Identity.Model = c.Model;
            listing.Identity.ModelCode = c.Model.Split(' ').LastOrDefault();
            listing.Identity.Manufacturer = c.Model.Split(' ').FirstOrDefault();
        }

        // CPU
        listing.Specs.Cpu.Name = NormalizedParser.StrOpt(n, "CPU") ?? c.Cpu;
        listing.Specs.Cpu.Cores = NormalizedParser.IntOpt(n, "CPU_Cores");
        listing.Specs.Cpu.Threads = NormalizedParser.IntOpt(n, "CPU_Threads");

        // Prefer FullParser CPU max clock (supports multiple CPUs); fallback to normalized if present
        listing.Specs.Cpu.MaxClockMHz =
            f.CpuMaxClockMHz
            ?? TryIntFromNormalizedFallback(n, "CPU_MaxClockMHz");

        // RAM total
        listing.Specs.Ram.TotalGiB = NormalizedParser.DoubleOpt(n, "RAM_GiB");

        // RAM module count: prefer FullParser; fallback to normalized
        listing.Specs.Ram.ModuleCount =
            f.RamModuleCount
            ?? TryIntFromNormalizedFallback(n, "RAM_ModuleCount");

        // RAM modules: from full parser [RAM.Module.x]
        listing.Specs.Ram.Modules.Clear();
        foreach (var rm in f.RamModules.OrderBy(x => x.Slot))
        {
            listing.Specs.Ram.Modules.Add(new RamModule
            {
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

        // GPUs: deterministic extraction from full file [GPU.x], fallback to normalized primary.
        listing.Specs.Gpus = GpuExtractor.FromFullText(fullText, n);

        // Storage
        listing.Specs.Storage.Primary.Model = f.DiskModel;
        listing.Specs.Storage.Primary.SizeGiB = f.DiskSizeGiB;
        listing.Specs.Storage.Primary.Interface = "Unknown"; // do not guess
        listing.Specs.Storage.Primary.InterfaceHint = null;

        listing.Specs.Storage.SystemDrive.Drive = NormalizedParser.StrOpt(n, "SystemDrive");
        listing.Specs.Storage.SystemDrive.FileSystem = NormalizedParser.StrOpt(n, "SystemDrive_FS") ?? f.SystemDriveFs;
        listing.Specs.Storage.SystemDrive.SizeGiB = NormalizedParser.DoubleOpt(n, "SystemDrive_Size_GiB") ?? f.SystemDriveSizeGiB;
        listing.Specs.Storage.SystemDrive.FreeGiB = NormalizedParser.DoubleOpt(n, "SystemDrive_Free_GiB") ?? f.SystemDriveFreeGiB;

        // Display
        var maxRes = NormalizedParser.StrOpt(n, "Display_MaxResolution");
        if (!string.IsNullOrWhiteSpace(maxRes))
            listing.Specs.Display.MaxResolution = maxRes;

        listing.Specs.Display.SizeInches = NormalizedParser.DoubleOpt(n, "Display_ScreenSizeInches");
        listing.Specs.Display.RefreshHz = NormalizedParser.IntOpt(n, "Display_RefreshHz");

        // Wi-Fi (prefer full because it has radios + driver)
        listing.Specs.Wifi.Generation = f.WifiGeneration ?? NormalizedParser.StrOpt(n, "WiFi_Generation") ?? c.Wifi;
        listing.Specs.Wifi.Radios = f.WifiRadios.Count > 0
            ? f.WifiRadios
            : SplitRadios(NormalizedParser.StrOpt(n, "WiFi_Radios"));
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

        // Model reference defaults (can be edited)
        listing.ModelReference.HasTouchScreenPotential = null; // unknown unless you enrich from PSREF etc
        listing.ModelReference.MobileData = null;              // default to omitted until you set it

        // Ports catalogue (all disabled until you enable them)
        listing.Ports = PortCatalog.CreateDefault();

        // Included/Notes default scaffolding (safe to edit)
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

    private static int? TryIntFromNormalizedFallback(Dictionary<string, string> n, string key)
        => n.TryGetValue(key, out var s) && int.TryParse(s, out var v) ? v : null;

    private static List<string> SplitRadios(string? radios)
        => string.IsNullOrWhiteSpace(radios)
            ? new List<string>()
            : radios.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
}
