using EbayListingGenerator.Models;

namespace EbayListingGenerator.Services;

/// <summary>
/// Human-editable catalogue of common laptop ports.
/// Extraction does NOT attempt to guess which are present on a unit.
/// You flip ports[].enabled to true before rendering.
/// </summary>
public static class PortCatalog
{
    public static List<PortOption> CreateDefault()
        => new()
        {
            // USB-A
            P("USB-A (USB 2.0)", 1, "Typical legacy USB-A"),
            P("USB-A (USB 3.0 / 3.1 Gen 1)", 1, "Often blue port"),
            P("USB-A (USB 3.1 Gen 2)", 1, "10Gbps"),
            P("USB-A (Always On / Charging)", 1, "USB-A with charging icon"),

            // USB-C / TB
            P("USB-C (USB 3.1 Gen 1)", 1, "5Gbps; may support DP Alt Mode"),
            P("USB-C (USB 3.1 Gen 2)", 1, "10Gbps; may support DP Alt Mode"),
            P("USB-C (USB 3.2 Gen 2x2)", 1, "20Gbps"),
            P("USB4 (USB-C)", 1, "40Gbps (device dependent)"),
            P("Thunderbolt 3 (USB-C)", 1, "40Gbps; DP + charging"),
            P("Thunderbolt 4 (USB-C)", 1, "40Gbps; DP + charging"),

            // Video
            P("HDMI", 1, "Version varies by model/config"),
            P("HDMI 2.0", 1, "4K@60Hz capable (model dependent)"),
            P("DisplayPort", 1, "Full-size DP"),
            P("Mini DisplayPort", 1, "mDP"),
            P("USB-C DisplayPort Alt Mode", 1, "Video over USB-C"),
            P("VGA", 1, "Legacy"),

            // Networking
            P("RJ45 Ethernet", 1, "Built-in Ethernet port"),
            P("Ethernet extension connector", 1, "RJ45 via proprietary adapter"),
            P("Kensington lock slot", 1, "Security slot"),

            // Audio
            P("3.5mm headphone / microphone combo jack", 1, "TRRS combo"),
            P("3.5mm headphone out", 1, "Audio out only"),
            P("3.5mm microphone in", 1, "Mic in only"),

            // Cards / SIM
            P("SD card reader", 1, "Full-size SD"),
            P("microSD card reader", 1, "microSD"),
            P("Nano-SIM slot", 1, "WWAN/SIM models only"),
            P("Smart card reader", 1, "Optional on some configs"),

            // Docking / proprietary
            P("Side docking connector", 1, "Proprietary dock"),
            P("Bottom docking connector", 1, "Proprietary dock"),
            P("Barrel power connector", 1, "Round power input"),
            P("USB-C Power Delivery", 1, "Charging via USB-C"),
        };

    private static PortOption P(string name, int count, string? notes)
        => new() { Enabled = false, Name = name, Count = count, Notes = notes };
}
