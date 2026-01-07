using System.Text.RegularExpressions;

namespace EbayListingGenerator.Parsing;

public sealed class FullTextExtract
{
    public string? WifiGeneration { get; set; }
    public List<string> WifiRadios { get; set; } = new();
    public string? WifiDriverVendor { get; set; }
    public string? WifiDriverVersion { get; set; }

    public string? DiskModel { get; set; }
    public double? DiskSizeGiB { get; set; }

    public string? SystemDrive { get; set; }
    public string? SystemDriveFs { get; set; }
    public double? SystemDriveSizeGiB { get; set; }
    public double? SystemDriveFreeGiB { get; set; }
    public string? DiskInterfaceType { get; set; }
    public double? DiskPhysicalSizeGiB { get; set; }

    public int? WindowsBuild { get; set; }
    public int? WindowsUbr { get; set; }
    public DateTime? WindowsInstalledUtc { get; set; }

    // CPU
    public int? CpuMaxClockMHz { get; set; }

    // RAM
    public int? RamModuleCount { get; set; }
    public List<RamModuleExtract> RamModules { get; set; } = new();
}

public sealed class RamModuleExtract
{
    public int Slot { get; set; }
    public long? CapacityBytes { get; set; }
    public double? CapacityGiB { get; set; }
    public string? Manufacturer { get; set; }
    public string? PartNumber { get; set; }
    public string? SerialNumber { get; set; }
    public int? SpeedMHz { get; set; }
    public int? ConfiguredSpeedMHz { get; set; }
    public string? DeviceLocator { get; set; }
    public string? BankLabel { get; set; }
}

public static class FullParser
{
    // YAML-ish Wi-Fi block
    private static readonly Regex WifiBlock = new(
        @"Wi-?Fi:\s*(?:\r?\n|\r)+\s*generation:\s*(?<gen>[^\r\n]+)(?:\r?\n|\r)+\s*radios:\s*(?<radios>[^\r\n]+)(?:\r?\n|\r)+\s*driver:\s*(?<drv>[^\r\n]+)",
        RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled
    );

    // INI-like Wi-Fi
    private static readonly Regex WifiIniVendor = new(@"^Vendor\s*=\s*(?<v>.+)$", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled);
    private static readonly Regex WifiIniDriverVersion = new(@"^DriverVersion\s*=\s*(?<v>.*)$", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled);

    private static readonly Regex DiskSummary = new(
        @"^Physical Disk:\s*(?<model>.+?)\s*\|\s*(?<size>[0-9.]+)\s*GiB\s*$",
        RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled);

    private static readonly Regex SysDriveSummary = new(
        @"^System Drive:\s*(?<drive>[^|]+)\|\s*(?<fs>[^|]+)\|\s*Size\s*:\s*(?<size>[0-9.]+)\s*GiB\s*\|\s*Free\s*:\s*(?<free>[0-9.]+)\s*GiB\s*$",
        RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled);

    public static FullTextExtract Parse(string text)
    {
        text ??= string.Empty;

        var r = new FullTextExtract();

        // ---- Keep your existing summary parsing (Wi-Fi / disk / system drive) ----

        var m = WifiBlock.Match(text);
        if (m.Success)
        {
            r.WifiGeneration = m.Groups["gen"].Value.Trim();

            var radios = m.Groups["radios"].Value.Trim();
            r.WifiRadios = radios.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();

            r.WifiDriverVendor = m.Groups["drv"].Value.Trim();
        }

        var mv = WifiIniVendor.Match(text);
        if (mv.Success && string.IsNullOrWhiteSpace(r.WifiDriverVendor))
            r.WifiDriverVendor = mv.Groups["v"].Value.Trim();

        var mdv = WifiIniDriverVersion.Match(text);
        if (mdv.Success)
        {
            var v = mdv.Groups["v"].Value.Trim();
            if (!string.IsNullOrWhiteSpace(v))
                r.WifiDriverVersion = v;
        }

        var d = DiskSummary.Match(text);
        if (d.Success)
        {
            r.DiskModel = d.Groups["model"].Value.Trim();
            if (double.TryParse(d.Groups["size"].Value.Trim(), out var size))
                r.DiskSizeGiB = size;
        }

        var sd = SysDriveSummary.Match(text);
        if (sd.Success)
        {
            r.SystemDrive = sd.Groups["drive"].Value.Trim();
            r.SystemDriveFs = sd.Groups["fs"].Value.Trim();

            if (double.TryParse(sd.Groups["size"].Value.Trim(), out var s))
                r.SystemDriveSizeGiB = s;
            if (double.TryParse(sd.Groups["free"].Value.Trim(), out var f))
                r.SystemDriveFreeGiB = f;
        }

        // ---- FIX: Use INI parsing for CPU/RAM/Windows/Disk physical ----

        var ini = IniFile.Parse(text);

        // Windows
        var win = ini.GetSection("Windows");
        r.WindowsBuild = win.GetInt("Build");
        r.WindowsUbr = win.GetInt("UBR");

        var installed = win.GetString("InstalledUtc");
        if (!string.IsNullOrWhiteSpace(installed) && DateTime.TryParse(installed, out var dt))
            r.WindowsInstalledUtc = dt;

        // Disk physical (authoritative)
        var disk0 = ini.GetSection("Disk.Physical.0");
        r.DiskInterfaceType = disk0.GetString("InterfaceType");
        r.DiskPhysicalSizeGiB = disk0.GetDouble("SizeGiB");

        // System drive (authoritative if present)
        var sys = ini.GetSection("Disk.SystemDrive");
        r.SystemDrive = sys.GetString("Drive") ?? r.SystemDrive;
        r.SystemDriveFs = sys.GetString("FileSystem") ?? r.SystemDriveFs;
        r.SystemDriveSizeGiB = sys.GetDouble("SizeGiB") ?? r.SystemDriveSizeGiB;
        r.SystemDriveFreeGiB = sys.GetDouble("FreeGiB") ?? r.SystemDriveFreeGiB;

        // CPU max clock MHz (from [CPU.N])
        var cpuRoot = ini.GetSection("CPU");
        var cpuCount = cpuRoot.GetInt("Count") ?? 0;

        int? maxCpu = null;
        for (int i = 0; i < cpuCount; i++)
        {
            var c = ini.GetSection($"CPU.{i}");
            var mhz = c.GetInt("MaxClockMHz");
            if (mhz is null) continue;

            if (maxCpu is null || mhz.Value > maxCpu.Value)
                maxCpu = mhz.Value;
        }
        r.CpuMaxClockMHz = maxCpu;

        // RAM module count
        var ram = ini.GetSection("RAM");
        r.RamModuleCount = ram.GetInt("ModuleCount");

        // RAM modules (use moduleCount, fallback to discovered sections)
        var modules = new List<RamModuleExtract>();
        var moduleCount = r.RamModuleCount;

        if (moduleCount is int cnt && cnt > 0)
        {
            for (int i = 0; i < cnt; i++)
                TryAddRamModule(i);
        }
        else
        {
            // Fallback: discover [RAM.Module.N] sections
            foreach (var name in ini.SectionNames)
            {
                if (!name.StartsWith("RAM.Module.", StringComparison.OrdinalIgnoreCase))
                    continue;

                var idxPart = name["RAM.Module.".Length..];
                if (int.TryParse(idxPart, out var slot))
                    TryAddRamModule(slot);
            }

            if (modules.Count > 0)
                r.RamModuleCount = modules.Count;
        }

        r.RamModules = modules.OrderBy(x => x.Slot).ToList();
        return r;

        void TryAddRamModule(int slot)
        {
            var s = ini.GetSection($"RAM.Module.{slot}");
            // if section doesn't exist, ignore
            var capBytes = s.GetLong("CapacityBytes");
            var capGiB = s.GetDouble("CapacityGiB");
            var manu = NullIfEmpty(s.GetString("Manufacturer"));
            var devLoc = NullIfEmpty(s.GetString("DeviceLocator"));

            // If it’s totally empty, don’t add noise
            if (capBytes is null && capGiB is null && manu is null && devLoc is null)
                return;

            modules.Add(new RamModuleExtract
            {
                Slot = slot,
                CapacityBytes = capBytes,
                CapacityGiB = capGiB,
                Manufacturer = manu,
                PartNumber = NullIfEmpty(s.GetString("PartNumber")),
                SerialNumber = NullIfEmpty(s.GetString("SerialNumber")),
                SpeedMHz = s.GetInt("SpeedMHz"),
                ConfiguredSpeedMHz = s.GetInt("ConfiguredSpeedMHz"),
                DeviceLocator = devLoc,
                BankLabel = NullIfEmpty(s.GetString("BankLabel"))
            });
        }
    }

    private static string? NullIfEmpty(string? s) => string.IsNullOrWhiteSpace(s) ? null : s;
}
