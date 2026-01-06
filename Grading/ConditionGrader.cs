using eBayListingGenerator.Models;

namespace eBayListingGenerator.Grading;

public sealed class ConditionGrader
{
    private readonly ConditionOptionsCatalog _catalog;

    public ConditionGrader(ConditionOptionsCatalog catalog)
        => _catalog = catalog;

    public (Grade overall, ResolvedConditionParts resolved) Resolve(ListingCondition condition)
    {
        if (condition.ForPartsNotWorking)
        {
            var resolvedForced = ResolveParts(condition.Parts, force: Grade.ForParts);
            return (Grade.ForParts, resolvedForced);
        }

        var resolved = ResolveParts(condition.Parts, force: null);

        // Overall = worst grade among parts
        var overall = resolved.AllGrades.Max();
        return (overall, resolved);
    }

    private ResolvedConditionParts ResolveParts(ConditionParts parts, Grade? force)
    {
        var lid = ResolvePick("Lid", parts.Lid, force);
        var back = ResolvePick("Back", parts.Back, force);
        var sides = ResolvePick("Sides", parts.Sides, force);
        var kbd = ResolvePick("Keyboard", parts.Keyboard, force);
        var tp = ResolvePick("Trackpad", parts.Trackpad, force);
        var scr = ResolvePick("Screen", parts.Screen, force);

        return new ResolvedConditionParts(lid, back, sides, kbd, tp, scr);
    }

    private ResolvedCondition ResolvePick(string label, ConditionPick pick, Grade? force)
    {
        if (force is not null)
            return new ResolvedCondition(label, "Faults present; sold for repair/parts only.", force.Value);

        if (!string.IsNullOrWhiteSpace(pick.OptionId) &&
            _catalog.Options.TryGetValue(pick.OptionId!, out var opt))
        {
            return new ResolvedCondition(label, opt.Text, opt.Grade);
        }

        // Free text fallback defaults to "Good" unless you choose to change this.
        var text = !string.IsNullOrWhiteSpace(pick.FreeText) ? pick.FreeText! : "good";
        return new ResolvedCondition(label, text, Grade.Good);
    }
}

public sealed record ResolvedCondition(string Label, string Text, Grade Grade);

public sealed class ResolvedConditionParts
{
    public ResolvedCondition Lid { get; }
    public ResolvedCondition Back { get; }
    public ResolvedCondition Sides { get; }
    public ResolvedCondition Keyboard { get; }
    public ResolvedCondition Trackpad { get; }
    public ResolvedCondition Screen { get; }

    public IReadOnlyList<Grade> AllGrades => new[]
    {
        Lid.Grade, Back.Grade, Sides.Grade, Keyboard.Grade, Trackpad.Grade, Screen.Grade
    };

    public ResolvedConditionParts(
        ResolvedCondition lid,
        ResolvedCondition back,
        ResolvedCondition sides,
        ResolvedCondition keyboard,
        ResolvedCondition trackpad,
        ResolvedCondition screen)
    {
        Lid = lid;
        Back = back;
        Sides = sides;
        Keyboard = keyboard;
        Trackpad = trackpad;
        Screen = screen;
    }
}
