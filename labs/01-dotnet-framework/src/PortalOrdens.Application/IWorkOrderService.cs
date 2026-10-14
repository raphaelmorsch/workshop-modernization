using System.Collections.Generic;
using System.IO;
using PortalOrdens.Contracts;

namespace PortalOrdens.Application
{
    public interface IWorkOrderService
    {
        IList<WorkOrder> List();

        WorkOrder Get(int id);

        WorkOrder Open(CreateWorkOrderRequest request);

        WorkOrder Update(int id, string title, string description, string assetCode);

        WorkOrder ChangeStatus(int id, WorkOrderStatus status);

        WorkOrderAttachment Attach(int workOrderId, string fileName, Stream content);

        IList<WorkOrderAttachment> ListAttachments(int workOrderId);

        WorkOrderAttachment GetAttachment(int id);

        byte[] ReadAttachment(WorkOrderAttachment attachment);
    }
}
