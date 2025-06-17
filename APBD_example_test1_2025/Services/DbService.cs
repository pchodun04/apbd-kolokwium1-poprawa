using System.Data.Common;
using APBD_example_test1_2025.Exceptions;
using APBD_example_test1_2025.Models.DTOs;
using Microsoft.Data.SqlClient;

namespace APBD_example_test1_2025.Services;

public class DbService : IDbService
{
    private readonly string _connectionString;
    public DbService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("Default") ?? string.Empty;
    }

    public async Task<GetProjectDetailsDto> GetProjectDetailsByIdAsync(int projectId)
    {
        var query =
            @"SELECT p.PROJECT_ID, p.OBJECTIVE, p.STARTDATE, p.ENDDATE, a.NAME, a.ORIGINDATE, i.INSTITUTIONID, i.NAME, i.FOUNDYEAR, s.FIRSTNAME, s.LASTNAME, s.HIREDATE, sa.ROLE
                    FROM PRESERVATION_PROJECT p 
                    JOIN ARTIFACT a ON p.ARTIFACTID = a.ARTIFACTID
                    JOIN INSTITUTION i ON a.INSTITUTIONID = i.INSTITUTIONID
                    JOIN STAFF_ASSIGNMENT sa ON sa.PROJECT_ID = p.PROJECT_ID
                    JOIN STAFF ON sa.STAFFID = s.STAFF_ID 
                    WHERE p.PROJECT_ID = @ProjectId";
        
        await using SqlConnection connection = new SqlConnection(_connectionString);
        await using SqlCommand command = new SqlCommand();
        
        command.Connection = connection;
        command.CommandText = query;
        await connection.OpenAsync();
        
        command.Parameters.AddWithValue("@ProjectId", projectId);
        var reader = await command.ExecuteReaderAsync();

        GetProjectDetailsDto? projects = null;

        while (await reader.ReadAsync())
        {
            if (projects == null)
            {
                projects = new GetProjectDetailsDto()
                {
                    ProjectId = reader.GetInt32(0),
                    Objective = reader.GetString(1),
                    StartDate = reader.GetDateTime(2),
                    EndDate = reader.GetDateTime(3),
                    Artifact = new ArtifactDto()
                    {
                        Name = reader.GetString(4),
                        OriginDate = reader.GetDateTime(5),
                        Institution = new InstitutionDto()
                        {
                            InstitutionId = reader.GetInt32(6),
                            Name = reader.GetString(7),
                            FoundedYear = reader.GetInt32(8)
                        }
                    },
                    StaffAssignments = new List<StaffAssignmentDto>()
                };
            }

            projects.StaffAssignments.Add(new StaffAssignmentDto
            {
                FirstName = reader.GetString(9),
                LastName = reader.GetString(10),
                HireDate = reader.GetDateTime(11),
                Role = reader.GetString(12),
            });
        }

        if (projects is null)
        {
            throw new NotFoundException("No projects found");
        }
        return projects;
    }

    public async Task AddNewArtifactAndProjectAsync(CreateNewArtifactAndProjectDto newArtifactAndProjectAndProject)
    {
        await using SqlConnection connection = new SqlConnection(_connectionString);
        await using SqlCommand command = new SqlCommand();
        
        command.Connection = connection;
        await connection.OpenAsync();
        
        DbTransaction transaction = await connection.BeginTransactionAsync();
        command.Transaction = transaction as SqlTransaction;

        try
        {
            command.Parameters.Clear();
            command.CommandText = "SELECT 1 FROM INSTITUTION WHERE INSTITUTIONID = @InstitutionId";
            command.Parameters.AddWithValue("@InstitutionId",
                newArtifactAndProjectAndProject.NewArtifactDto.InstitutionId);

            var projectExists = await command.ExecuteScalarAsync();
            if (projectExists is null)
            {
                throw new NotFoundException("No projects found");
            }
            command.Parameters.Clear();
            command.CommandText = @"INSERT INTO ARTIFACT VALUES(@ArtifactId, @Name, @OriginDate, @InstitutionId)";
            command.Parameters.AddWithValue("@ArtifactId", newArtifactAndProjectAndProject.NewArtifactDto.ArtifactId);
            command.Parameters.AddWithValue("@Name", newArtifactAndProjectAndProject.NewArtifactDto.Name);
            command.Parameters.AddWithValue("@OriginDate", newArtifactAndProjectAndProject.NewArtifactDto.OriginDate);
            command.Parameters.AddWithValue("@InstitutionId", newArtifactAndProjectAndProject.NewArtifactDto.InstitutionId);
            
            await command.ExecuteNonQueryAsync();
            
            command.Parameters.Clear();
            command.CommandText = @"INSERT INTO PRESERVATION_PROJECT VALUES(@ProjectId, @Objective ,@StartDate, @EndDate, @ArtifactId)";
            command.Parameters.AddWithValue("@ProjectId", newArtifactAndProjectAndProject.NewProjectDto.ProjectId);
            command.Parameters.AddWithValue("@Objective", newArtifactAndProjectAndProject.NewProjectDto.Objective);
            command.Parameters.AddWithValue("@StartDate", newArtifactAndProjectAndProject.NewProjectDto.StartDate);
            command.Parameters.AddWithValue("@EndDate", newArtifactAndProjectAndProject.NewProjectDto.EndDate);
            command.Parameters.AddWithValue("@ArtifactId", newArtifactAndProjectAndProject.NewArtifactDto.ArtifactId);
            
            await command.ExecuteNonQueryAsync();
            
            await transaction.CommitAsync();
        }
        catch (Exception e)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}