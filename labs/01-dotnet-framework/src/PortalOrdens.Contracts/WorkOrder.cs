using System;

namespace PortalOrdens.Contracts
{
    public sealed class WorkOrder
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public WorkOrderStatus Status { get; set; }

        public string AssetCode { get; set; }

        public string CreatedBy { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}
