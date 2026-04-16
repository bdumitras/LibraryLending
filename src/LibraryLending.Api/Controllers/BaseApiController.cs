using Microsoft.AspNetCore.Mvc;

namespace LibraryLending.Api.Controllers;

[ApiController]
[Produces("application/json")]
[Route("api/[controller]")]
public abstract class BaseApiController : ControllerBase
{
}
