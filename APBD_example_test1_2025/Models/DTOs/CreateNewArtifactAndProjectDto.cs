namespace APBD_example_test1_2025.Models.DTOs;

public class CreateNewArtifactAndProjectDto
{
    public NewArtifactDto NewArtifactDto { get; set; }
    public NewProjectDto NewProjectDto { get; set; }
}

public class NewArtifactDto
{
    public int ArtifactId { get; set; }
    public string Name { get; set; }
    public DateTime OriginDate { get; set; }
    public int InstitutionId { get; set; }
}

public class NewProjectDto
{
    public int ProjectId { get; set; }
    public string Objective { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}