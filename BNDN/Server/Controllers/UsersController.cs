using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Common.DTO.Event;
using Common.DTO.History;
using Common.DTO.Server;
using Common.Exceptions;
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
        public async Task<IHttpActionResult> Login([FromBody] LoginDto loginDto)
        {
            // Check input
            if (!ModelState.IsValid)
            {
                await _historyLogic.SaveNoneWorkflowSpecificHistory(new HistoryModel
                {
                    HttpRequestType = "POST",
                    Message = ModelState.ToString(),
                    MethodCalledOnSender = "PostWorkflow"
                });
                return BadRequest(ModelState);
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

                return Ok(toReturn);
            }
            catch (ArgumentNullException e)
            {
                _historyLogic.SaveHistory(new HistoryModel
                {
                    EventId = "",
                    HttpRequestType = "POST",
                    Message = "Threw: " + e.GetType(),
                    MethodCalledOnSender = "Login"
                }).Wait();
                return BadRequest("Seems input was not satisfactory");
            }
            catch (UnauthorizedException e)
            {
                _historyLogic.SaveNoneWorkflowSpecificHistory(new HistoryModel
                {
                    HttpRequestType = "POST",
                    Message = "Threw: " + e.GetType(),
                    MethodCalledOnSender = "Login",
                }).Wait();
                
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.Unauthorized,
                    "Username or password does not correspond to a user."));
            }
            catch (Exception e)
            { 
                _historyLogic.SaveNoneWorkflowSpecificHistory(new HistoryModel
                {
                    HttpRequestType = "POST",
                    Message = "Threw: " + e.GetType(),
                    MethodCalledOnSender = "Login",
                }).Wait();
                return InternalServerError(e);
            }
        }

        /// <summary>
        /// CreateUser attempts to create a user given the provided UserDto
        /// </summary>
        /// <param name="dto">Contains login-information and given roles for the user</param>
        /// <returns></returns>
        [Route("users")] 
        [HttpPost]
        public async Task<IHttpActionResult> CreateUser([FromBody] UserDto dto)
        {
            if (!ModelState.IsValid)
            {
                await _historyLogic.SaveNoneWorkflowSpecificHistory(new HistoryModel
                {
                    HttpRequestType = "POST",
                    Message = ModelState.ToString(),
                    MethodCalledOnSender = "CreateUser",
                });

                return BadRequest(ModelState);
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
                return Ok();
            }
            catch (ArgumentNullException e)
            {
                _historyLogic.SaveHistory(new HistoryModel
                {
                    HttpRequestType = "POST",
                    Message = "Threw: " + e.GetType() + " with username: " + dto.Name,
                    MethodCalledOnSender = "CreateUser"
                }).Wait();
                return BadRequest("Seems input was not satisfactory");
            }
            catch (NotFoundException e)
            {
                _historyLogic.SaveHistory(new HistoryModel
                {
                    HttpRequestType = "POST",
                    Message = "Threw: " + e.GetType() + " with username: " + dto.Name,
                    MethodCalledOnSender = "CreateUser"
                }).Wait();
                return NotFound();
            }
            catch (UserExistsException e)
            {
                _historyLogic.SaveHistory(new HistoryModel
                {
                    HttpRequestType = "POST",
                    Message = "Threw: " + e.GetType() + " with username: " + dto.Name,
                    MethodCalledOnSender = "CreateUser"
                }).Wait();
                return Conflict();
            }
            catch (InvalidOperationException e)
            {
                _historyLogic.SaveNoneWorkflowSpecificHistory(new HistoryModel
                {
                    HttpRequestType = "POST",
                    Message = "Threw: " + e.GetType() + " with username: " + dto.Name,
                    MethodCalledOnSender = "CreateUser",
                }).Wait();

                return NotFound();

            }
            catch (ArgumentException e)
            {
                if (e.ParamName != null && e.ParamName.Equals("user"))
                {
                    _historyLogic.SaveNoneWorkflowSpecificHistory(new HistoryModel
                    {
                        HttpRequestType = "POST",
                        Message = "Threw: " + e.GetType() + " with username: " + dto.Name,
                        MethodCalledOnSender = "CreateUser",
                    }).Wait();
                    return Conflict();
                }
                else {
                    _historyLogic.SaveNoneWorkflowSpecificHistory(new HistoryModel
                    {
                        HttpRequestType = "POST",
                        Message = "Threw: " + e.GetType() + " with username: " + dto.Name,
                        MethodCalledOnSender = "CreateUser",
                    }).Wait();
                    return BadRequest();
                }
            }
            catch (Exception e)
            {
                _historyLogic.SaveNoneWorkflowSpecificHistory(new HistoryModel
                {
                    HttpRequestType = "POST",
                    Message = "Threw: " + e.GetType(),
                    MethodCalledOnSender = "CreateUser",
                }).Wait();
                return InternalServerError(e);
            }
        }

        /// <summary>
        /// Add roles to an already existing user.
        /// </summary>
        /// <param name="username">The username of the user which should have the roles added.</param>
        /// <param name="roles">The roles to add.</param>
        /// <returns></returns>
        [Route("users/{username}/roles")]
        [HttpPost]
        public async Task<IHttpActionResult> AddRolesToUser(string username, [FromBody] IEnumerable<WorkflowRole> roles)
        {
            if (!ModelState.IsValid)
            {
                await _historyLogic.SaveNoneWorkflowSpecificHistory(new HistoryModel
                {
                    HttpRequestType = "POST",
                    Message = ModelState.ToString(),
                    MethodCalledOnSender = "AddRolesToUser",
                });

                return BadRequest(ModelState);
            }

            try
            {
                await _logic.AddRolesToUser(username, roles);
                await _historyLogic.SaveNoneWorkflowSpecificHistory(new HistoryModel
                {
                    HttpRequestType = "POST",
                    Message = "Called: AddRolesToUser with username: " + username,
                    MethodCalledOnSender = "AddRolesToUser",
                });
                return Ok();
            }
            catch (ArgumentNullException e)
            {
                _historyLogic.SaveHistory(new HistoryModel
                {
                    HttpRequestType = "POST",
                    Message = "Threw: " + e.GetType() + " with username: " + username,
                    MethodCalledOnSender = "AddRolesToUser"
                }).Wait();
                return BadRequest("Seems input was not satisfactory");
            }
            catch (NotFoundException e)
            {
                _historyLogic.SaveHistory(new HistoryModel
                {
                    HttpRequestType = "POST",
                    Message = "Threw: " + e.GetType() + " with username: " + username,
                    MethodCalledOnSender = "AddRolesToUser"
                }).Wait();
                return NotFound();
            }
            catch (Exception e)
            {
                _historyLogic.SaveNoneWorkflowSpecificHistory(new HistoryModel
                {
                    HttpRequestType = "POST",
                    Message = "Threw: " + e.GetType(),
                    MethodCalledOnSender = "AddRolesToUser",
                }).Wait();

                return InternalServerError(e);
            }
        }

        protected override void Dispose(bool disposing)
        {
            _logic.Dispose();
            base.Dispose(disposing);
        }
    }
}