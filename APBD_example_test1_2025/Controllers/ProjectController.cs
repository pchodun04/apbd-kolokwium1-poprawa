using APBD_example_test1_2025.Exceptions;
using APBD_example_test1_2025.Models.DTOs;
using APBD_example_test1_2025.Services;
using Microsoft.AspNetCore.Mvc;

namespace APBD_example_test1_2025.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectController : ControllerBase
    {
        private readonly IDbService _dbService;
        public ProjectController(IDbService dbService)
        {
            _dbService = dbService;
        }

        [HttpGet("{id}/projects")]
        public async Task<IActionResult> GetCustomerRentals(int id)
        {
            try
            {
                var res = await _dbService.GetProjectDetailsByIdAsync(id);
                return Ok(res);
            }
            catch (NotFoundException e)
            {
                return NotFound(e.Message);
            }
        }

        [HttpPost("{id}/artifacts")]
        public async Task<IActionResult> AddNewRental(int id, CreateNewArtifactAndProjectDto createNewArtifactAndProject)
        {
            try
            {
                await _dbService.AddNewArtifactAndProjectAsync(createNewArtifactAndProject);
            }
            catch (ConflictException e)
            {
                return Conflict(e.Message);
            }
            catch (NotFoundException e)
            {
                return NotFound(e.Message);
            }
            
            return CreatedAtAction(nameof(GetCustomerRentals), createNewArtifactAndProject);
        }    
    }
}
