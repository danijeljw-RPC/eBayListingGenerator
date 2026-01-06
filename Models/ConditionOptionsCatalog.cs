using System.Text.Json;
using System.Text.Json.Serialization;
using eBayListingGenerator.Grading;

namespace eBayListingGenerator.Models;

public sealed class ConditionOptionsCatalog
{
    public string Schema { get; set; } = "condition-options-v1";

    /// <summary>
    /// Key = optionId. Each option includes: Part, Text, Grade.
    /// </summary>
    public Dictionary<string, ConditionOption> Options { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    public static ConditionOptionsCatalog CreateDefaultExtensive()
    {
        var c = new ConditionOptionsCatalog();

        // LID (top cover)
        Add(c, "lid.excellent_near_new", Part.Lid, "Excellent (near-new; tiny marks if any)", Grade.Excellent);
        Add(c, "lid.light_scuffs", Part.Lid, "Light scuffs / light scratches (normal use)", Grade.VeryGood);
        Add(c, "lid.visible_scratches", Part.Lid, "Visible scratches (noticeable under light)", Grade.Good);
        Add(c, "lid.deep_scratches", Part.Lid, "Deep scratches / heavier wear", Grade.Fair);
        Add(c, "lid.dents", Part.Lid, "Dents or deformation (cosmetic) but functional", Grade.Fair);
        Add(c, "lid.cracks", Part.Lid, "Cracks in lid plastics/cover", Grade.ForParts);

        // BACK (bottom base)
        Add(c, "base.excellent", Part.Back, "Excellent base condition", Grade.Excellent);
        Add(c, "base.light_scuffs", Part.Back, "Light scuffs / light scratches", Grade.VeryGood);
        Add(c, "base.visible_scuffs", Part.Back, "Visible scuffs / scratches", Grade.Good);
        Add(c, "base.rubber_feet_missing", Part.Back, "One or more rubber feet missing", Grade.Fair);
        Add(c, "base.cover_cracked", Part.Back, "Bottom cover cracked", Grade.ForParts);

        // SIDES (edges, corners)
        Add(c, "sides.excellent", Part.Sides, "Excellent edges/corners", Grade.Excellent);
        Add(c, "sides.light_edge_wear", Part.Sides, "Light edge wear (minor marks)", Grade.VeryGood);
        Add(c, "sides.corner_scuffs", Part.Sides, "Corner scuffs / edge scuffs", Grade.Good);
        Add(c, "sides.paint_chips_visible", Part.Sides, "Paint chips / missing paint (noticeable)", Grade.Fair);
        Add(c, "sides.corner_dents", Part.Sides, "Corner dents (cosmetic)", Grade.Fair);
        Add(c, "sides.hinge_damage", Part.Sides, "Hinge area damage / separation", Grade.ForParts);

        // KEYBOARD
        Add(c, "kbd.excellent", Part.Keyboard, "Excellent keyboard (clean, even keycaps)", Grade.Excellent);
        Add(c, "kbd.very_good_minor_shine", Part.Keyboard, "Minor shine on common keys", Grade.VeryGood);
        Add(c, "kbd.good_shine_normal", Part.Keyboard, "Normal wear/shine; all keys present", Grade.Good);
        Add(c, "kbd.worn_lettering", Part.Keyboard, "Key lettering worn/faded", Grade.Fair);
        Add(c, "kbd.keys_missing", Part.Keyboard, "Missing keys / broken scissor mechanisms", Grade.ForParts);
        Add(c, "kbd.backlight_not_working", Part.Keyboard, "Backlight not working (if fitted)", Grade.Fair);
        Add(c, "kbd.sticky_keys", Part.Keyboard, "Sticky keys / liquid history suspected", Grade.ForParts);

        // TRACKPAD
        Add(c, "tp.excellent", Part.Trackpad, "Excellent trackpad (smooth, even)", Grade.Excellent);
        Add(c, "tp.very_good", Part.Trackpad, "Very good trackpad (minor surface wear)", Grade.VeryGood);
        Add(c, "tp.good", Part.Trackpad, "Good trackpad (normal wear)", Grade.Good);
        Add(c, "tp.shiny_worn", Part.Trackpad, "Worn/shiny trackpad surface", Grade.Fair);
        Add(c, "tp.buttons_worn", Part.Trackpad, "Trackpad buttons worn but working", Grade.Fair);
        Add(c, "tp.not_working", Part.Trackpad, "Trackpad not working / erratic", Grade.ForParts);

        // SCREEN
        Add(c, "scr.excellent", Part.Screen, "Excellent screen (clean, no visible marks)", Grade.Excellent);
        Add(c, "scr.very_good_minor_cleaning_marks", Part.Screen, "Minor cleaning marks (only under light)", Grade.VeryGood);
        Add(c, "scr.good_light_marks", Part.Screen, "Light marks/scratches (visible under light)", Grade.Good);
        Add(c, "scr.fair_visible_scratches", Part.Screen, "Visible scratches / pressure marks", Grade.Fair);
        Add(c, "scr.dead_pixels", Part.Screen, "Dead/stuck pixels present", Grade.Fair);
        Add(c, "scr.bright_spots", Part.Screen, "Bright spots / backlight bleed noticeable", Grade.Fair);
        Add(c, "scr.cracked", Part.Screen, "Cracked LCD / damaged panel", Grade.ForParts);
        Add(c, "scr.touch_not_working", Part.Screen, "Touch not working (if touchscreen)", Grade.Fair);

        // Generic fallback IDs you might reuse:
        Add(c, "gen.excellent", Part.Generic, "Excellent", Grade.Excellent);
        Add(c, "gen.very_good", Part.Generic, "Very good", Grade.VeryGood);
        Add(c, "gen.good", Part.Generic, "Good", Grade.Good);
        Add(c, "gen.fair", Part.Generic, "Fair", Grade.Fair);
        Add(c, "gen.for_parts", Part.Generic, "For parts / not working", Grade.ForParts);

        return c;

        static void Add(ConditionOptionsCatalog c, string id, Part part, string text, Grade grade)
            => c.Options[id] = new ConditionOption { Id = id, Part = part, Text = text, Grade = grade };
    }

    public string ToJsonIndented()
    {
        var opts = ListingDocument.JsonOptions();
        opts.WriteIndented = true;
        return JsonSerializer.Serialize(this, opts);
    }
}

public sealed class ConditionOption
{
    public string Id { get; set; } = "";
    public Part Part { get; set; }
    public string Text { get; set; } = "";
    public Grade Grade { get; set; }
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Part
{
    Generic,
    Lid,
    Back,
    Sides,
    Keyboard,
    Trackpad,
    Screen
}
