using System;

namespace PortalOrdens.Contracts
{
    public sealed class WorkOrderAttachment
    {
        public int Id { get; set; }

        public int WorkOrderId { get; set; }

        public string FileName { get; set; }

        public string StoredPath { get; set; }

        public string UploadedBy { get; set; }

        public DateTime UploadedAt { get; set; }
    }
}
