using System;
using System.Web.Mvc;
using PortalOrdens.Application;
using PortalOrdens.Contracts;
using PortalOrdens.Web.Models;

namespace PortalOrdens.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IWorkOrderService _workOrders = CompositionRoot.WorkOrders;

        public ActionResult Index()
        {
            var overview = new HomeOverview
            {
                WindowsUser = CurrentUser()
            };

            try
            {
                var all = _workOrders.List();
                foreach (var item in all)
                {
                    if (item.Status == WorkOrderStatus.Aberta)
                    {
                        overview.Abertas++;
                    }
                    else if (item.Status == WorkOrderStatus.EmExecucao)
                    {
                        overview.EmExecucao++;
                    }
                    else if (item.Status == WorkOrderStatus.Concluida)
                    {
                        overview.Concluidas++;
                    }
                }

                overview.LastWorkOrderId = Session[PortalSession.LastWorkOrderId] as int?;
            }
            catch (Exception ex)
            {
                overview.DatabaseError = ex.Message;
            }

            return View(overview);
        }

        private string CurrentUser()
        {
            if (User == null || User.Identity == null || string.IsNullOrWhiteSpace(User.Identity.Name))
            {
                return "anônimo";
            }

            return User.Identity.Name;
        }
    }
}
