using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Common;
using Common.Exceptions;
using Common.History;
using Server.Exceptions;
using Server.Interfaces;
using Server.Logic;
using Server.Storage;

namespace Server.Controllers
{
    /// <summary>
    /// UsersController handles HTTP-request regarding users on Server
    /// </summary>
    public class UsersController : ApiController
    {
        private readonly IServerLogic _logic;
        private readonly IWorkflowHistoryLogic _historyLogic;

        /// <summary>
        /// Default constructor used during runtime
        /// </summary>
        public UsersController()
        {
            _logic = new ServerLogic(new ServerStorage());
            _historyLogic = new WorkflowHistoryLogic();
        }

        /// <summary>
        /// Constructor used for dependency-injection during testing
        /// </summary>
        /// <param name="logic"></param>
        /// <param name="historyLogic"></param>
        public UsersController(IServerLogic logic, IWorkflowHistoryLogic historyLogic)
        {
            _logic = logic;
            _historyLogic = historyLogic;
        }

 
        /// <summary>
        /// Returns the users roles on all workflows.
        /// </summary>
        /// <param name="loginDto">Contains the login-information needed for login-attempt</param>
        /// <returns></returns>
        [Route("login")]
        [HttpPost]
        public async Task<RolesOnWorkflowsDto> Login([FromBody] LoginDto loginDto)
        {
            // Check input
            if (!ModelState.IsValid)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                    "The provided input could not be mapped onto an instance of LoginDto"));
                // TODO: Save to history...? Explain Morten how to do so precisely...!
            }

            try
            {
                var toReturn = await _logic.Login(loginDto);
                await _historyLogic.SaveNoneWorkflowSpecificHistory(new HistoryModel
                {
                    HttpRequestType = "POST",
                    Message = "Called: Login with username: " + loginDto.Username,
                    MethodCalledOnSender = "Login",
                });

                return toReturn;
            }
            catch (ArgumentNullException)
            {
                var toThrow = new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                    "Seems input was not satisfactory"));
                _historyLogic.SaveHistory(new HistoryModel { EventId = "", HttpRequestType = "POST", Message = "Threw: " + toThrow.GetType(), MethodCalledOnSender = "Login" });
                throw toThrow;
            }
            catch (UnauthorizedException e)
            {
                var toThrow = new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.Unauthorized,
                    "Username or password does not correspond to a user."));
                _historyLogic.SaveNoneWorkflowSpecificHistory(new HistoryModel
                {
                    HttpRequestType = "POST",
                    Message = "Threw: " + toThrow.GetType(),
                    MethodCalledOnSender = "Login",
                });

                throw toThrow;
            }
            catch (Exception ex)
            {
                var toThrow = new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex));
                _historyLogic.SaveNoneWorkflowSpecificHistory(new HistoryModel
                {
                    HttpRequestType = "POST",
                    Message = "Threw: " + toThrow.GetType(),
                    MethodCalledOnSender = "Login",
                });

                throw toThrow;
            }
        }

        /// <summary>
        /// CreateUser attempts to create a user given the provided UserDto
        /// </summary>
        /// <param name="dto">Contains login-information and given roles for the user</param>
        /// <returns></returns>
        [Route("users")] 
        [HttpPost]
        public async Task CreateUser([FromBody] UserDto dto)
        {
            if (!ModelState.IsValid)
            {
                var toThrow = new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState));
                await _historyLogic.SaveNoneWorkflowSpecificHistory(new HistoryModel
                {
                    HttpRequestType = "POST",
                    Message = "Threw: " + toThrow.GetType(),
                    MethodCalledOnSender = "CreateUser",
                });
            }

            try
            {
                await _logic.AddUser(dto);
                await _historyLogic.SaveNoneWorkflowSpecificHistory(new HistoryModel
                {
                    HttpRequestType = "POST",
                    Message = "Called: CreateUser with username: " + dto.Name,
                    MethodCalledOnSender = "CreateUser",
                });
            }
            catch (ArgumentNullException)
            {
                var toThrow = new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                    "Seems input was not satisfactory"));
                _historyLogic.SaveHistory(new HistoryModel
                {
                    HttpRequestType = "POST",
                    Message = "Threw: " + toThrow.GetType() + " with username: " + dto.Name,
                    MethodCalledOnSender = "CreateUser"
                });
                throw toThrow;
            }
            catch (NotFoundException)
            {
                var toThrow = new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.NotFound,
                    "A role attached to the provided user could not be found"));
                _historyLogic.SaveHistory(new HistoryModel
                {
                    HttpRequestType = "POST",
                    Message = "Threw: " + toThrow.GetType() + " with username: " + dto.Name,
                    MethodCalledOnSender = "CreateUser"
                }).Wait();
                throw toThrow;
            }
            catch (UserExistsException)
            {
                var toThrow = new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                    "The provided user already exists at Server."));
                _historyLogic.SaveHistory(new HistoryModel
                {
                    HttpRequestType = "POST",
                    Message = "Threw: " + toThrow.GetType() + " with username: " + dto.Name,
                    MethodCalledOnSender = "CreateUser"
                }).Wait();
                throw toThrow;
            }
            catch (InvalidOperationException)
            {
                var toThrow = new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.NotFound,
                    "One of the roles does not exist."));
                _historyLogic.SaveNoneWorkflowSpecificHistory(new HistoryModel
                {
                    HttpRequestType = "POST",
                    Message = "Threw: " + toThrow.GetType() + " with username: " + dto.Name,
                    MethodCalledOnSender = "CreateUser",
                }).Wait();

                throw toThrow;

            }
            catch (ArgumentException ex)
            {
                if (ex.ParamName == "user")
                {
                    var toThrow = new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.Conflict,
                        "A user with that username already exists."));

                    _historyLogic.SaveNoneWorkflowSpecificHistory(new HistoryModel
                    {
                        HttpRequestType = "POST",
                        Message = "Threw: " + toThrow.GetType() + " with username: " + dto.Name,
                        MethodCalledOnSender = "CreateUser",
                    });

                    throw toThrow;
                }
                else {
                    var toThrow = new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex));

                    _historyLogic.SaveNoneWorkflowSpecificHistory(new HistoryModel
                    {
                        HttpRequestType = "POST",
                        Message = "Threw: " + toThrow.GetType() + " with username: " + dto.Name,
                        MethodCalledOnSender = "CreateUser",
                    });

                    throw toThrow;
                }
            }
            catch (Exception ex)
            {
                var toThrow = new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex));

                _historyLogic.SaveNoneWorkflowSpecificHistory(new HistoryModel
                {
                    HttpRequestType = "POST",
                    Message = "Threw: " + toThrow.GetType(),
                    MethodCalledOnSender = "CreateUser",
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