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
            P("USB-A (USB 3.2 Gen 1)", 1, "5Gbps"),
            P("USB-A (USB 3.2 Gen 1, Always On)", 1, "Always On / charging port"),
            P("USB-A (USB 3.1 Gen 2)", 1, "10Gbps"),
            P("USB-A (USB 3.2 Gen 2)", 1, "10Gbps (naming used on some PSREFs)"),
            P("USB-A (USB 3.2 Gen 2, Always On)", 1, "10Gbps + Always On / charging"),
            P("USB-A (Powered USB / Always On / Charging)", 1, "Powered USB-A (sleep/off charging)"),

            // USB-C / TB
            P("USB-C (USB 3.1 Gen 1)", 1, "5Gbps; may support DP Alt Mode"),
            P("USB-C (USB 3.1 Gen 2)", 1, "10Gbps; may support DP Alt Mode"),
            P("USB-C (USB 10Gbps, data transfer only)", 1, "No video/charging on some models"),
            P("USB-C (USB 3.2 Gen 1, with Power Delivery + DisplayPort 1.2)", 1, "USB-PD + DP 1.2 (PSREF wording)"),
            P("USB-C (USB 3.2 Gen 2x2)", 1, "20Gbps"),
            P("USB-C (USB 20Gbps / USB 3.2 Gen 2x2, with USB-PD + DisplayPort 1.4)", 1, "PSREF wording (20Gbps + PD + DP 1.4)"),
            P("USB4 (USB-C)", 1, "Up to 40Gbps (device dependent)"),
            P("Thunderbolt 3 (USB-C)", 1, "40Gbps; DP + charging"),
            P("Thunderbolt 4 (USB-C)", 1, "40Gbps; DP + charging"),
            P("Thunderbolt 4 / USB4 (USB-C, with USB-PD 3.0 + DisplayPort 1.4a)", 1, "40Gbps; PD 3.0 + DP 1.4a (PSREF wording)"),
            P("Thunderbolt 4 / USB4 (USB-C, with USB-PD + DisplayPort 2.1)", 1, "PSREF wording (DP 2.1 on newer models)"),
            P("USB-C Power Delivery", 1, "Charging via USB-C"),

            // Video
            P("HDMI", 1, "Version varies by model/config"),
            P("HDMI 1.4b", 1, "Often used on business models"),
            P("HDMI 2.0", 1, "4K@60Hz capable (model dependent)"),
            P("HDMI 2.1", 1, "Up to 4K@60Hz (per PSREF on some models)"),
            P("DisplayPort", 1, "Full-size DP"),
            P("Mini DisplayPort", 1, "mDP"),
            P("USB-C DisplayPort Alt Mode", 1, "Video over USB-C"),
            P("VGA", 1, "Legacy"),
            P("Ethernet + video via docking", 1, "If provided by proprietary dock/connector"),

            // Networking
            P("RJ45 Ethernet", 1, "Built-in Ethernet port"),
            P("Ethernet extension connector (mini RJ-45)", 1, "RJ45 via proprietary dongle/adapter"),
            P("Lenovo OneLink connector", 1, "Proprietary OneLink docking connector (older ThinkPads)"),
            P("Kensington lock slot", 1, "Standard Kensington security slot"),
            P("Kensington Nano Security Slot", 1, "2.5mm x 6mm Nano slot (PSREF wording)"),

            // Audio
            P("3.5mm headphone / microphone combo jack", 1, "TRRS combo"),
            P("3.5mm headphone out", 1, "Audio out only"),
            P("3.5mm microphone in", 1, "Mic in only"),

            // Cards / SIM
            P("SD card reader", 1, "Full-size SD"),
            P("SD Express 7.0 card reader", 1, "SD Express (PSREF wording on some models)"),
            P("4-in-1 card reader (MMC/SD/SDHC/SDXC)", 1, "Older PSREF wording"),
            P("microSD card reader", 1, "microSD"),
            P("Nano-SIM slot", 1, "WWAN/SIM models only"),
            P("Smart card reader", 1, "Optional on some configs"),

            // Docking / proprietary
            P("Side docking connector", 1, "Proprietary dock"),
            P("Bottom docking connector", 1, "Proprietary dock"),
            P("OneLink docking connector", 1, "Lenovo OneLink dock port"),
            P("Barrel power connector", 1, "Round power input (common on IdeaPad/Legion etc)"),
            P("Slim Tip power connector", 1, "Lenovo rectangular 'Slim Tip' DC-in"),
            P("Power connector (proprietary)", 1, "PSREF often labels simply as 'Power connector'"),
        };

    private static PortOption P(string name, int count, string? notes)
        => new() { Enabled = false, Name = name, Count = count, Notes = notes };
}
