namespace EbayListingGenerator.Models;

public static class GradeStyle
{
    public static string GetCss(ConditionGrade g) => g switch
    {
        ConditionGrade.Excellent => "grade-excellent",
        ConditionGrade.VeryGood  => "grade-verygood",
        ConditionGrade.Good      => "grade-good",
        ConditionGrade.Fair      => "grade-fair",
        ConditionGrade.ForParts  => "grade-forparts",
        _ => "grade-good"
    };
}
