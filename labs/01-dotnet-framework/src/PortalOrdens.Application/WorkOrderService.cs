using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using PortalOrdens.Contracts;
using PortalOrdens.Data;

namespace PortalOrdens.Application
{
    public sealed class WorkOrderService : IWorkOrderService
    {
        private readonly IWorkOrderRepository _repository;
        private readonly LocalAttachmentStore _attachments;

        public WorkOrderService(IWorkOrderRepository repository, LocalAttachmentStore attachments)
        {
            if (repository == null)
            {
                throw new ArgumentNullException("repository");
            }

            if (attachments == null)
            {
                throw new ArgumentNullException("attachments");
            }

            _repository = repository;
            _attachments = attachments;
        }

        public IList<WorkOrder> List()
        {
            return _repository.List();
        }

        public WorkOrder Get(int id)
        {
            return _repository.Get(id);
        }

        public WorkOrder Open(CreateWorkOrderRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            var title = NormalizeRequired(request.Title, "Informe o título da ordem.", 200, "O título deve ter no máximo 200 caracteres.");
            var assetCode = NormalizeRequired(request.AssetCode, "Informe o código do ativo.", 50, "O código do ativo deve ter no máximo 50 caracteres.");
            var description = NormalizeOptional(request.Description);

            var workOrder = new WorkOrder
            {
                Title = title,
                Description = description,
                Status = WorkOrderStatus.Aberta,
                AssetCode = assetCode,
                CreatedBy = CurrentWindowsUser(),
                CreatedAt = DateTime.Now
            };

            workOrder.Id = _repository.Insert(workOrder);
            return workOrder;
        }

        public WorkOrder Update(int id, string title, string description, string assetCode)
        {
            var current = Require(id);
            current.Title = NormalizeRequired(title, "Informe o título da ordem.", 200, "O título deve ter no máximo 200 caracteres.");
            current.AssetCode = NormalizeRequired(assetCode, "Informe o código do ativo.", 50, "O código do ativo deve ter no máximo 50 caracteres.");
            current.Description = NormalizeOptional(description);
            current.UpdatedAt = DateTime.Now;
            _repository.Update(current);
            return current;
        }

        public WorkOrder ChangeStatus(int id, WorkOrderStatus status)
        {
            var current = Require(id);
            if (current.Status == status)
            {
                return current;
            }

            if (!CanTransition(current.Status, status))
            {
                throw new InvalidOperationException("Transição de status não permitida.");
            }

            if (status == WorkOrderStatus.Concluida && string.IsNullOrWhiteSpace(current.Description))
            {
                throw new InvalidOperationException("Informe uma descrição antes de concluir a ordem.");
            }

            current.Status = status;
            current.UpdatedAt = DateTime.Now;
            _repository.Update(current);
            return current;
        }

        public WorkOrderAttachment Attach(int workOrderId, string fileName, Stream content)
        {
            Require(workOrderId);
            var storedPath = _attachments.Save(workOrderId, fileName, content);
            var attachment = new WorkOrderAttachment
            {
                WorkOrderId = workOrderId,
                FileName = Path.GetFileName(fileName),
                StoredPath = storedPath,
                UploadedBy = CurrentWindowsUser(),
                UploadedAt = DateTime.Now
            };
            attachment.Id = _repository.InsertAttachment(attachment);
            return attachment;
        }

        public IList<WorkOrderAttachment> ListAttachments(int workOrderId)
        {
            return _repository.ListAttachments(workOrderId);
        }

        public WorkOrderAttachment GetAttachment(int id)
        {
            return _repository.GetAttachment(id);
        }

        public byte[] ReadAttachment(WorkOrderAttachment attachment)
        {
            if (attachment == null)
            {
                throw new ArgumentNullException("attachment");
            }

            return _attachments.Read(attachment.StoredPath);
        }

        private WorkOrder Require(int id)
        {
            var current = _repository.Get(id);
            if (current == null)
            {
                throw new InvalidOperationException("Ordem de serviço não encontrada.");
            }

            return current;
        }

        private static bool CanTransition(WorkOrderStatus from, WorkOrderStatus to)
        {
            if (from == WorkOrderStatus.Aberta && to == WorkOrderStatus.EmExecucao)
            {
                return true;
            }

            if (from == WorkOrderStatus.EmExecucao && to == WorkOrderStatus.Concluida)
            {
                return true;
            }

            if (from == WorkOrderStatus.EmExecucao && to == WorkOrderStatus.Aberta)
            {
                return true;
            }

            if (from == WorkOrderStatus.Concluida && to == WorkOrderStatus.EmExecucao)
            {
                return true;
            }

            return false;
        }

        private static string NormalizeRequired(string value, string missingMessage, int maxLength, string lengthMessage)
        {
            var normalized = (value ?? string.Empty).Trim();
            if (normalized.Length == 0)
            {
                throw new ArgumentException(missingMessage);
            }

            if (normalized.Length > maxLength)
            {
                throw new ArgumentException(lengthMessage);
            }

            return normalized;
        }

        private static string NormalizeOptional(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            return value.Trim();
        }

        // Legado: a regra de negócio lê o usuário do pipeline ASP.NET.
        // HttpContext.Current não existe da mesma forma no ASP.NET Core.
        private static string CurrentWindowsUser()
        {
            var context = HttpContext.Current;
            if (context == null || context.User == null || context.User.Identity == null)
            {
                return "desconhecido";
            }

            var name = context.User.Identity.Name;
            if (string.IsNullOrWhiteSpace(name))
            {
                return "desconhecido";
            }

            return name;
        }
    }
}
