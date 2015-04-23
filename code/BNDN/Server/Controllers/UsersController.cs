using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Common;
using Common.Exceptions;
using Common.History;
using Server.Interfaces;
using Server.Logic;
using Server.Storage;

namespace Server.Controllers
{
    public class UsersController : ApiController
    {
        private readonly IServerLogic _logic;
        private readonly IWorkflowHistoryLogic _historyLogic;

        public UsersController()
        {
            _logic = new ServerLogic(new ServerStorage());
            _historyLogic = new WorkflowHistoryLogic();
        }

        public UsersController(IServerLogic logic, IWorkflowHistoryLogic historyLogic)
        {
            _logic = logic;
            _historyLogic = historyLogic;
        }

        // POST: /login loginDto
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
                var toReturn = await _logic.Login(loginDto);
                await _historyLogic.SaveHistory(new HistoryModel
                {
                    HttpRequestType = "POST",
                    Message = "Called: Login with username: " + loginDto.Username,
                    MethodCalledOnSender = "Login",
                    WorkflowId = loginDto.Username
                });

                return toReturn;
            }
            catch (UnauthorizedException e)
            {
                var toThrow = new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.Unauthorized,
                    "Username or password does not correspond to a user."));
                _historyLogic.SaveHistory(new HistoryModel
                {
                    HttpRequestType = "POST",
                    Message = "Threw: " + toThrow.GetType(),
                    MethodCalledOnSender = "Login"
                });

                throw toThrow;
            }
            catch (Exception ex)
            {
                var toThrow = new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex));
                _historyLogic.SaveHistory(new HistoryModel
                {
                    HttpRequestType = "POST",
                    Message = "Threw: " + toThrow.GetType(),
                    MethodCalledOnSender = "Login"
                });

                throw toThrow;
            }
        }

        [Route("users"), HttpPost]
        public async Task CreateUser([FromBody] UserDto dto)
        {
            if (!ModelState.IsValid)
            {
                var toThrow = new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState));
                await _historyLogic.SaveHistory(new HistoryModel
                {
                    HttpRequestType = "POST",
                    Message = "Threw: " + toThrow.GetType(),
                    MethodCalledOnSender = "CreateUser"
                });
            }

            try
            {
                await _logic.AddUser(dto);
                await _historyLogic.SaveHistory(new HistoryModel
                {
                    HttpRequestType = "POST",
                    Message = "Called: CreateUser with username: " + dto.Name,
                    MethodCalledOnSender = "CreateUser",
                });
            }
            catch (InvalidOperationException)
            {
                var toThrow = new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.NotFound,
                    "One of the roles does not exist."));
                _historyLogic.SaveHistory(new HistoryModel
                {
                    HttpRequestType = "POST",
                    Message = "Threw: " + toThrow.GetType() + " with username: " + dto.Name,
                    MethodCalledOnSender = "CreateUser"
                });

                throw toThrow;

            }
            catch (ArgumentException ex)
            {
                if (ex.ParamName == "user")
                {
                    var toThrow = new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.Conflict,
                        "A user with that username already exists."));

                    _historyLogic.SaveHistory(new HistoryModel
                    {
                        HttpRequestType = "POST",
                        Message = "Threw: " + toThrow.GetType() + " with username: " + dto.Name,
                        MethodCalledOnSender = "CreateUser"
                    });

                    throw toThrow;
                }
                else {
                    var toThrow = new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex));

                    _historyLogic.SaveHistory(new HistoryModel
                    {
                        HttpRequestType = "POST",
                        Message = "Threw: " + toThrow.GetType() + " with username: " + dto.Name,
                        MethodCalledOnSender = "CreateUser"
                    });

                    throw toThrow;
                }
            }
            catch (Exception ex)
            {
                var toThrow = new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex));

                _historyLogic.SaveHistory(new HistoryModel
                {
                    HttpRequestType = "POST",
                    Message = "Threw: " + toThrow.GetType(),
                    MethodCalledOnSender = "CreateUser"
                });

                throw toThrow;
            }
        }

        protected override void Dispose(bool disposing)
        {
            _logic.Dispose();
            base.Dispose(disposing);
        }
    }
}