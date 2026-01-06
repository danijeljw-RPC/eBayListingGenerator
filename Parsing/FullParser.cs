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

    public int? WindowsBuild { get; set; }
    public int? WindowsUbr { get; set; }
    public DateTime? WindowsInstalledUtc { get; set; }

    // CPU (max across CPU.N blocks)
    public int? CpuMaxClockMHz { get; set; }

    // RAM
    public int? RamModuleCount { get; set; }
    public List<RamModuleExtract> RamModules { get; set; } = new();
}

public sealed class RamModuleExtract
{
    public int Slot { get; set; }
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
    // Top "YAML-ish" Wi-Fi block
    private static readonly Regex WifiBlock = new(
        @"Wi-?Fi:\s*(?:\r?\n|\r)+\s*generation:\s*(?<gen>[^\r\n]+)(?:\r?\n|\r)+\s*radios:\s*(?<radios>[^\r\n]+)(?:\r?\n|\r)+\s*driver:\s*(?<drv>[^\r\n]+)",
        RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled
    );

    // INI-style values later in text (e.g. [WiFi] Vendor=..., DriverVersion=...)
    private static readonly Regex WifiIniVendor = new(@"^Vendor\s*=\s*(?<v>.+)$", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled);
    private static readonly Regex WifiIniDriverVersion = new(@"^DriverVersion\s*=\s*(?<v>.*)$", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled);

    private static readonly Regex DiskSummary = new(
        @"^Physical Disk:\s*(?<model>.+?)\s*\|\s*(?<size>[0-9.]+)\s*GiB\s*$",
        RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled);

    private static readonly Regex SysDriveSummary = new(
        @"^System Drive:\s*(?<drive>[^|]+)\|\s*(?<fs>[^|]+)\|\s*Size\s*:\s*(?<size>[0-9.]+)\s*GiB\s*\|\s*Free\s*:\s*(?<free>[0-9.]+)\s*GiB\s*$",
        RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled);

    // Windows section (INI-like)
    private static readonly Regex WindowsIni = new(@"^Build\s*=\s*(?<b>[0-9]+)\s*$", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled);
    private static readonly Regex WindowsUbrIni = new(@"^UBR\s*=\s*(?<u>[0-9]+)\s*$", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled);
    private static readonly Regex WindowsInstalledIni = new(@"^InstalledUtc\s*=\s*(?<dt>.+)\s*$", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled);

    // CPU blocks: [CPU.0], [CPU.1], ...
    private static readonly Regex CpuBlock = new(
        @"\[CPU\.(?<i>[0-9]+)\](?:\r?\n|\r)+(?<body>(?:.|\r|\n)*?)(?=(?:\r?\n|\r)\[|\Z)",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex CpuMaxClock = new(@"^MaxClockMHz\s*=\s*(?<v>[0-9]+)\s*$", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled);

    // RAM: [RAM] + [RAM.Module.N]
    private static readonly Regex RamBlock = new(
        @"\[RAM\](?:\r?\n|\r)+(?<body>(?:.|\r|\n)*?)(?=(?:\r?\n|\r)\[RAM\.|\Z)",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex RamModuleBlock = new(
        @"\[RAM\.Module\.(?<i>[0-9]+)\](?:\r?\n|\r)+(?<body>(?:.|\r|\n)*?)(?=(?:\r?\n|\r)\[|\Z)",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex RamModuleCount = new(@"^ModuleCount\s*=\s*(?<v>[0-9]+)\s*$", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled);

    private static readonly Regex CapGiB = new(@"^CapacityGiB\s*=\s*(?<v>[0-9.]+)\s*$", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled);
    private static readonly Regex Manufacturer = new(@"^Manufacturer\s*=\s*(?<v>.*)\s*$", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled);
    private static readonly Regex PartNumber = new(@"^PartNumber\s*=\s*(?<v>.*)\s*$", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled);
    private static readonly Regex SerialNumber = new(@"^SerialNumber\s*=\s*(?<v>.*)\s*$", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled);
    private static readonly Regex SpeedMHz = new(@"^SpeedMHz\s*=\s*(?<v>[0-9]+)\s*$", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled);
    private static readonly Regex ConfiguredSpeedMHz = new(@"^ConfiguredSpeedMHz\s*=\s*(?<v>[0-9]+)\s*$", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled);
    private static readonly Regex DeviceLocator = new(@"^DeviceLocator\s*=\s*(?<v>.*)\s*$", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled);
    private static readonly Regex BankLabel = new(@"^BankLabel\s*=\s*(?<v>.*)\s*$", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled);

    public static FullTextExtract Parse(string text)
    {
        text ??= string.Empty;
        var r = new FullTextExtract();

        // Wi-Fi (YAML-ish block at top)
        var m = WifiBlock.Match(text);
        if (m.Success)
        {
            r.WifiGeneration = m.Groups["gen"].Value.Trim();

            var radios = m.Groups["radios"].Value.Trim();
            r.WifiRadios = radios
                .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .ToList();

            r.WifiDriverVendor = m.Groups["drv"].Value.Trim();
        }

        // Wi-Fi (INI-like later)
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

        // Disk summary
        var d = DiskSummary.Match(text);
        if (d.Success)
        {
            r.DiskModel = d.Groups["model"].Value.Trim();
            if (double.TryParse(d.Groups["size"].Value.Trim(), out var size))
                r.DiskSizeGiB = size;
        }

        // System drive summary
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

        // Windows INI
        var wb = WindowsIni.Match(text);
        if (wb.Success && int.TryParse(wb.Groups["b"].Value.Trim(), out var build))
            r.WindowsBuild = build;

        var wu = WindowsUbrIni.Match(text);
        if (wu.Success && int.TryParse(wu.Groups["u"].Value.Trim(), out var ubr))
            r.WindowsUbr = ubr;

        var wi = WindowsInstalledIni.Match(text);
        if (wi.Success && DateTime.TryParse(wi.Groups["dt"].Value.Trim(), out var dt))
            r.WindowsInstalledUtc = dt;

        // CPU: max MaxClockMHz across all [CPU.N] blocks
        int? maxCpu = null;
        foreach (Match cpu in CpuBlock.Matches(text))
        {
            var body = cpu.Groups["body"].Value;
            var mc = CpuMaxClock.Match(body);
            if (!mc.Success) continue;

            if (int.TryParse(mc.Groups["v"].Value.Trim(), out var mhz))
            {
                if (maxCpu is null || mhz > maxCpu.Value)
                    maxCpu = mhz;
            }
        }
        r.CpuMaxClockMHz = maxCpu;

        // RAM: module count from [RAM]
        var rb = RamBlock.Match(text);
        if (rb.Success)
        {
            var body = rb.Groups["body"].Value;
            var mmc = RamModuleCount.Match(body);
            if (mmc.Success && int.TryParse(mmc.Groups["v"].Value.Trim(), out var cnt))
                r.RamModuleCount = cnt;
        }

        // RAM modules: [RAM.Module.N]
        foreach (Match mm in RamModuleBlock.Matches(text))
        {
            if (!int.TryParse(mm.Groups["i"].Value, out var slot))
                continue;

            var body = mm.Groups["body"].Value;

            var mod = new RamModuleExtract { Slot = slot };

            var cg = CapGiB.Match(body);
            if (cg.Success && double.TryParse(cg.Groups["v"].Value.Trim(), out var giB))
                mod.CapacityGiB = giB;

            mod.Manufacturer = NullIfEmpty(Manufacturer.Match(body).Success ? Manufacturer.Match(body).Groups["v"].Value.Trim() : null);
            mod.PartNumber = NullIfEmpty(PartNumber.Match(body).Success ? PartNumber.Match(body).Groups["v"].Value.Trim() : null);
            mod.SerialNumber = NullIfEmpty(SerialNumber.Match(body).Success ? SerialNumber.Match(body).Groups["v"].Value.Trim() : null);

            var sp = SpeedMHz.Match(body);
            if (sp.Success && int.TryParse(sp.Groups["v"].Value.Trim(), out var speed))
                mod.SpeedMHz = speed;

            var csp = ConfiguredSpeedMHz.Match(body);
            if (csp.Success && int.TryParse(csp.Groups["v"].Value.Trim(), out var cspeed))
                mod.ConfiguredSpeedMHz = cspeed;

            mod.DeviceLocator = NullIfEmpty(DeviceLocator.Match(body).Success ? DeviceLocator.Match(body).Groups["v"].Value.Trim() : null);
            mod.BankLabel = NullIfEmpty(BankLabel.Match(body).Success ? BankLabel.Match(body).Groups["v"].Value.Trim() : null);

            r.RamModules.Add(mod);
        }

        // If moduleCount wasn't present but we did parse modules, infer it deterministically.
        if (r.RamModuleCount is null && r.RamModules.Count > 0)
            r.RamModuleCount = r.RamModules.Count;

        return r;
    }

    private static string? NullIfEmpty(string? s) => string.IsNullOrWhiteSpace(s) ? null : s;
}
