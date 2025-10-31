namespace FoundryLocalLabDemo;

public class StudentProfile
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public CitizenshipStatus? CitizenshipStatus { get; set; }
    public string? SSN { get; set; }
    public HighSchoolStatus? HighSchoolStatus { get; set; }

    /// <summary>
    /// Whether they have had any issues with federal loans in the past.
    /// </summary>
    public bool? HasFederalLoanIssues { get; set; }
    public double? GPA { get; set; }
}

public enum CitizenshipStatus
{
    USCitizen,
    PermanentResident,
    NonResidentAlien,
    Other
}

public enum HighSchoolStatus
{
    Graduated,
    NotGraduated,
    GED,
    Other
}
