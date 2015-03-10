using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Models;
using Storage;

namespace WebAPI.Controllers
{
    public class WorkflowController : ApiController
    {
        private StorageContext db = new StorageContext();

        // GET: api/Workflow
        public IQueryable<Workflow> GetWorkflows()
        {
            return db.Workflows;
        }

        // GET: api/Workflow/5
        [ResponseType(typeof(Workflow))]
        public async Task<IHttpActionResult> GetWorkflow(int id)
        {
            Workflow workflow = await db.Workflows.FindAsync(id);
            if (workflow == null)
            {
                return NotFound();
            }

            return Ok(workflow);
        }

        // PUT: api/Workflow/5
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> PutWorkflow(int id, Workflow workflow)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != workflow.Id)
            {
                return BadRequest();
            }

            db.Entry(workflow).State = EntityState.Modified;

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!WorkflowExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return StatusCode(HttpStatusCode.NoContent);
        }

        // POST: api/Workflow
        [ResponseType(typeof(Workflow))]
        public async Task<IHttpActionResult> PostWorkflow(Workflow workflow)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.Workflows.Add(workflow);
            await db.SaveChangesAsync();

            return CreatedAtRoute("DefaultApi", new { id = workflow.Id }, workflow);
        }

        // DELETE: api/Workflow/5
        [ResponseType(typeof(Workflow))]
        public async Task<IHttpActionResult> DeleteWorkflow(int id)
        {
            Workflow workflow = await db.Workflows.FindAsync(id);
            if (workflow == null)
            {
                return NotFound();
            }

            db.Workflows.Remove(workflow);
            await db.SaveChangesAsync();

            return Ok(workflow);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool WorkflowExists(int id)
        {
            return db.Workflows.Count(e => e.Id == id) > 0;
        }
    }
}