using System;
using System.Web;
using System.Web.Mvc;
using PortalOrdens.Application;
using PortalOrdens.Contracts;
using PortalOrdens.Web.Models;

namespace PortalOrdens.Web.Controllers
{
    public class WorkOrdersController : Controller
    {
        private readonly IWorkOrderService _workOrders = CompositionRoot.WorkOrders;

        public ActionResult Index()
        {
            try
            {
                return View(_workOrders.List());
            }
            catch (Exception ex)
            {
                ViewBag.DatabaseError = ex.Message;
                return View(new WorkOrder[0]);
            }
        }

        public ActionResult Details(int id)
        {
            var workOrder = _workOrders.Get(id);
            if (workOrder == null)
            {
                return HttpNotFound();
            }

            Session[PortalSession.LastWorkOrderId] = id;
            return View(new WorkOrderDetailsView
            {
                WorkOrder = workOrder,
                Attachments = _workOrders.ListAttachments(id)
            });
        }

        public ActionResult Create()
        {
            return View(new CreateWorkOrderRequest());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(CreateWorkOrderRequest request)
        {
            try
            {
                var created = _workOrders.Open(request);
                return RedirectToAction("Details", new { id = created.Id });
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(request ?? new CreateWorkOrderRequest());
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Update(int id, string title, string description, string assetCode)
        {
            try
            {
                _workOrders.Update(id, title, description, assetCode);
            }
            catch (ArgumentException ex)
            {
                TempData["Error"] = ex.Message;
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction("Details", new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangeStatus(int id, string status)
        {
            try
            {
                WorkOrderStatus parsed;
                if (!Enum.TryParse(status, false, out parsed))
                {
                    throw new ArgumentException("Status inválido.");
                }

                _workOrders.ChangeStatus(id, parsed);
            }
            catch (ArgumentException ex)
            {
                TempData["Error"] = ex.Message;
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction("Details", new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Attach(int id, HttpPostedFileBase arquivo)
        {
            try
            {
                if (arquivo == null || arquivo.ContentLength == 0)
                {
                    throw new ArgumentException("Selecione um arquivo.");
                }

                _workOrders.Attach(id, arquivo.FileName, arquivo.InputStream);
            }
            catch (ArgumentException ex)
            {
                TempData["Error"] = ex.Message;
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction("Details", new { id });
        }

        public ActionResult Download(int id)
        {
            var attachment = _workOrders.GetAttachment(id);
            if (attachment == null)
            {
                return HttpNotFound();
            }

            var bytes = _workOrders.ReadAttachment(attachment);
            return File(bytes, "application/octet-stream", attachment.FileName);
        }
    }
}
