using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Common;
using Server.Models;
using Server.Storage;

namespace Server.Controllers
{
    public class UsersController : ApiController
    {
        // GET: /Login
        /// <summary>
        /// Returns the users roles on all workflows.
        /// </summary>
        /// <param name="username">Id of the requested workflow</param>
        /// <returns>RolesOnWorkflowsDto</returns>
        [Route("login/{username}")]
        [HttpGet]
        public RolesOnWorkflowsDto Login(string username)
        {
            using (var serverLogic = new ServerLogic(new ServerStorage()))
            {
                try
                {
                    var result = serverLogic.Login(username);
                    return result;
                }
                catch (Exception ex)
                {
                    throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex));
                }
            }
        }

        [Route("login"), HttpPost]
        public async Task CreateUser([FromBody] UserDto dto)
        {
            if (!ModelState.IsValid)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState));
            }
            using (var serverLogic = new ServerLogic(new ServerStorage()))
            {
                try
                {
                    var user = new ServerUserModel
                    {
                        ID = dto.Id,
                        Name = dto.Name,
                        ServerRolesModels = dto.Roles.Select(role => new ServerRoleModel
                        {
                            ID = role.Role,
                            ServerWorkflowModelID = role.Workflow
                        }).ToList()
                    };
                    await serverLogic.AddUser(user);
                }
                catch (Exception ex)
                {
                    throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex));
                }
            }
        }
    }
}