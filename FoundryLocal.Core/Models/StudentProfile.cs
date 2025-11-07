
namespace FoundryLocal.Core;

public record StudentProfile
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

    /// <summary>
    /// JSON scheme for the <see cref="StudentProfile"> object that we want returned.
    /// </summary>
    /// <returns>string containing JSON schema representation</returns>
    public static string GetJSONSchema()
    {
        return """
            {
              "type": "object",
              "properties": {
                "FirstName": {
                  "type": ["string", "null"]
                },
                "LastName": {
                  "type": ["string", "null"]
                },
                "CitizenshipStatus": {
                  "type": ["string", "null"],
                  "enum": [null, "USCitizen", "PermanentResident", "NonResidentAlien", "Other"]
                },
                "SSN": {
                  "type": ["string", "null"]
                },
                "HighSchoolStatus": {
                  "type": ["string", "null"],
                  "enum": [null, "Graduated", "NotGraduated", "GED", "Other"]
                },
                "HasFederalLoanIssues": {
                  "type": ["boolean", "null"]
                },
                "GPA": {
                  "type": ["number", "null"]
                }
              }
            }
            """;
    }
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
