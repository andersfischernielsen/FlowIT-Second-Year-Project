using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Common;
using Common.Exceptions;
using Server.Logic;
using Server.Storage;

namespace Server.Controllers
{
    public class UsersController : ApiController
    {
        private readonly IServerLogic _logic;

        public UsersController() : this(new ServerLogic(new ServerStorage())) { }

        public UsersController(IServerLogic logic)
        {
            _logic = logic;
        }

        // GET: /Logins
        /// <summary>
        /// Returns the users roles on all workflows.
        /// </summary>
        /// <param name="loginDto"></param>
        /// <returns></returns>
        [Route("login")]
        [HttpPost]
        public async Task<RolesOnWorkflowsDto> Login([FromBody] LoginDto loginDto)
        {
            try
            {
                return await _logic.Login(loginDto);
            }
            catch (UnauthorizedException)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.Unauthorized,
                    "Username or password does not correspond to a user."));
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex));
            }
        }

        [Route("users"), HttpPost]
        public async Task CreateUser([FromBody] UserDto dto)
        {
            if (!ModelState.IsValid)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState));
            }

            try
            {
                await _logic.AddUser(dto);
            }
            catch (InvalidOperationException)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.NotFound,
                    "One of the roles does not exist."));
            }
            catch (ArgumentException ex)
            {
                if (ex.ParamName == "user")
                {
                    throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.Conflict,
                        "A user with that username already exists."));
                }

                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex));
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex));
            }
        }

        protected override void Dispose(bool disposing)
        {
            _logic.Dispose();
            base.Dispose(disposing);
        }
    }
}