using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using PRNPortal.Application.Constants;
using PRNPortal.Application.Options;

namespace PRNPortal.UI.Controllers;

public class PackagingRecyclingNoteController : Controller
{

    private readonly IAuthorizationService _authorizationService;
    private readonly ILogger<PackagingRecyclingNoteController> _logger;

    public PackagingRecyclingNoteController(
        ILogger<PackagingRecyclingNoteController> logger,
        IAuthorizationService authorizationService,
        IOptions<GlobalVariables> globalVariables)
    {
        _authorizationService = authorizationService;
        _logger = logger;
    }

    [HttpGet]
    [Route(PagePaths.PrnView)]
    public ActionResult Get()
    {
        return View();
    }

    [HttpGet]
    [Route(PagePaths.PrnDetail)]
    public ActionResult PrnDetail()
    {
        return View();
    }
}