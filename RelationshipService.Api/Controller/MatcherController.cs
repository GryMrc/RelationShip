namespace RelationshipService.Api.Controller
{
    using Microsoft.AspNetCore.Mvc;
    using RelationshipService.Api.Model.Request;
    using RelationshipService.Api.Model.Response;
    using RelationshipService.Api.Service.Interface;
    using System.Threading.Tasks;

    [ApiController]
    [Route("api/[controller]")]
    public class MatcherController : ControllerBase
    {
        private readonly IMatcherService _matcherService;

        public MatcherController(IMatcherService matcherService)
        {
            _matcherService = matcherService;
        }
    }
}