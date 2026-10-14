using PortalOrdens.Application;
using PortalOrdens.Data;

namespace PortalOrdens.Web
{
    public static class CompositionRoot
    {
        public static IWorkOrderService WorkOrders { get; private set; }

        public static void Start()
        {
            WorkOrders = new WorkOrderService(
                new SqlWorkOrderRepository(),
                new LocalAttachmentStore());
        }
    }
}
