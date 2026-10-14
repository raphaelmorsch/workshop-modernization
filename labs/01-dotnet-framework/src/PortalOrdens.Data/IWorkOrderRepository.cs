using System.Collections.Generic;
using PortalOrdens.Contracts;

namespace PortalOrdens.Data
{
    public interface IWorkOrderRepository
    {
        IList<WorkOrder> List();

        WorkOrder Get(int id);

        int Insert(WorkOrder workOrder);

        void Update(WorkOrder workOrder);

        int InsertAttachment(WorkOrderAttachment attachment);

        IList<WorkOrderAttachment> ListAttachments(int workOrderId);

        WorkOrderAttachment GetAttachment(int id);
    }
}
