using System.Collections.Generic;
using PortalOrdens.Contracts;

namespace PortalOrdens.Web.Models
{
    public sealed class WorkOrderDetailsView
    {
        public WorkOrder WorkOrder { get; set; }

        public IList<WorkOrderAttachment> Attachments { get; set; }
    }
}
