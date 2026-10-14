using System;
using System.Web.Http;
using PortalOrdens.Application;
using PortalOrdens.Contracts;

namespace PortalOrdens.Web.Controllers.Api
{
    [RoutePrefix("api/workorders")]
    public class WorkOrdersApiController : ApiController
    {
        private readonly IWorkOrderService _workOrders = CompositionRoot.WorkOrders;

        [HttpGet]
        [Route("")]
        public IHttpActionResult GetAll()
        {
            return Ok(_workOrders.List());
        }

        [HttpGet]
        [Route("{id:int}")]
        public IHttpActionResult GetById(int id)
        {
            var workOrder = _workOrders.Get(id);
            if (workOrder == null)
            {
                return NotFound();
            }

            return Ok(workOrder);
        }

        [HttpPost]
        [Route("")]
        public IHttpActionResult Post(CreateWorkOrderRequest request)
        {
            if (request == null)
            {
                return BadRequest("Corpo da requisição ausente.");
            }

            try
            {
                var created = _workOrders.Open(request);
                var location = new Uri(Request.RequestUri.AbsoluteUri.TrimEnd('/') + "/" + created.Id);
                return Created(location, created);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
