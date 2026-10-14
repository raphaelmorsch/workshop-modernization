using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using PortalOrdens.Contracts;

namespace PortalOrdens.Data
{
    public sealed class SqlWorkOrderRepository : IWorkOrderRepository
    {
        private const string WorkOrderColumns =
            "Id, Title, Description, Status, AssetCode, CreatedBy, CreatedAt, UpdatedAt";

        private const string AttachmentColumns =
            "Id, WorkOrderId, FileName, StoredPath, UploadedBy, UploadedAt";

        public IList<WorkOrder> List()
        {
            var items = new List<WorkOrder>();
            using (var connection = Open())
            using (var command = connection.CreateCommand())
            {
                command.CommandText =
                    "SELECT " + WorkOrderColumns + " FROM dbo.WorkOrders ORDER BY CreatedAt DESC, Id DESC";
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        items.Add(MapWorkOrder(reader));
                    }
                }
            }

            return items;
        }

        public WorkOrder Get(int id)
        {
            using (var connection = Open())
            using (var command = connection.CreateCommand())
            {
                command.CommandText =
                    "SELECT " + WorkOrderColumns + " FROM dbo.WorkOrders WHERE Id = @id";
                Add(command, "@id", id);
                using (var reader = command.ExecuteReader())
                {
                    if (!reader.Read())
                    {
                        return null;
                    }

                    return MapWorkOrder(reader);
                }
            }
        }

        public int Insert(WorkOrder workOrder)
        {
            using (var connection = Open())
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
INSERT INTO dbo.WorkOrders
    (Title, Description, Status, AssetCode, CreatedBy, CreatedAt, UpdatedAt)
VALUES
    (@title, @description, @status, @assetCode, @createdBy, @createdAt, @updatedAt);
SELECT CAST(SCOPE_IDENTITY() AS INT);";
                BindWorkOrder(command, workOrder);
                return (int)command.ExecuteScalar();
            }
        }

        public void Update(WorkOrder workOrder)
        {
            using (var connection = Open())
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
UPDATE dbo.WorkOrders
SET Title = @title,
    Description = @description,
    Status = @status,
    AssetCode = @assetCode,
    UpdatedAt = @updatedAt
WHERE Id = @id";
                BindWorkOrder(command, workOrder);
                Add(command, "@id", workOrder.Id);
                command.ExecuteNonQuery();
            }
        }

        public int InsertAttachment(WorkOrderAttachment attachment)
        {
            using (var connection = Open())
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
INSERT INTO dbo.Attachments
    (WorkOrderId, FileName, StoredPath, UploadedBy, UploadedAt)
VALUES
    (@workOrderId, @fileName, @storedPath, @uploadedBy, @uploadedAt);
SELECT CAST(SCOPE_IDENTITY() AS INT);";
                Add(command, "@workOrderId", attachment.WorkOrderId);
                AddString(command, "@fileName", attachment.FileName);
                AddString(command, "@storedPath", attachment.StoredPath);
                AddString(command, "@uploadedBy", attachment.UploadedBy);
                AddDate(command, "@uploadedAt", attachment.UploadedAt);
                return (int)command.ExecuteScalar();
            }
        }

        public IList<WorkOrderAttachment> ListAttachments(int workOrderId)
        {
            var items = new List<WorkOrderAttachment>();
            using (var connection = Open())
            using (var command = connection.CreateCommand())
            {
                command.CommandText =
                    "SELECT " + AttachmentColumns + " FROM dbo.Attachments WHERE WorkOrderId = @id ORDER BY UploadedAt, Id";
                Add(command, "@id", workOrderId);
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        items.Add(MapAttachment(reader));
                    }
                }
            }

            return items;
        }

        public WorkOrderAttachment GetAttachment(int id)
        {
            using (var connection = Open())
            using (var command = connection.CreateCommand())
            {
                command.CommandText =
                    "SELECT " + AttachmentColumns + " FROM dbo.Attachments WHERE Id = @id";
                Add(command, "@id", id);
                using (var reader = command.ExecuteReader())
                {
                    if (!reader.Read())
                    {
                        return null;
                    }

                    return MapAttachment(reader);
                }
            }
        }

        private static SqlConnection Open()
        {
            var settings = ConfigurationManager.ConnectionStrings["PortalOrdensDb"];
            if (settings == null || string.IsNullOrWhiteSpace(settings.ConnectionString))
            {
                throw new InvalidOperationException(
                    "Connection string PortalOrdensDb não encontrada. Confira o Web.config.");
            }

            var connection = new SqlConnection(settings.ConnectionString);
            try
            {
                connection.Open();
                return connection;
            }
            catch
            {
                connection.Dispose();
                throw;
            }
        }

        private static void BindWorkOrder(SqlCommand command, WorkOrder workOrder)
        {
            AddString(command, "@title", workOrder.Title);
            AddString(command, "@description", workOrder.Description);
            AddString(command, "@status", workOrder.Status.ToString());
            AddString(command, "@assetCode", workOrder.AssetCode);
            AddString(command, "@createdBy", workOrder.CreatedBy);
            AddDate(command, "@createdAt", workOrder.CreatedAt);
            AddDate(command, "@updatedAt", workOrder.UpdatedAt);
        }

        private static void Add(SqlCommand command, string name, int value)
        {
            command.Parameters.Add(name, SqlDbType.Int).Value = value;
        }

        private static void AddString(SqlCommand command, string name, string value)
        {
            var parameter = command.Parameters.Add(name, SqlDbType.NVarChar, -1);
            parameter.Value = (object)value ?? DBNull.Value;
        }

        private static void AddDate(SqlCommand command, string name, DateTime? value)
        {
            var parameter = command.Parameters.Add(name, SqlDbType.DateTime2);
            parameter.Value = value.HasValue ? (object)value.Value : DBNull.Value;
        }

        private static WorkOrder MapWorkOrder(SqlDataReader reader)
        {
            return new WorkOrder
            {
                Id = reader.GetInt32(0),
                Title = reader.GetString(1),
                Description = reader.IsDBNull(2) ? null : reader.GetString(2),
                Status = ParseStatus(reader.GetString(3)),
                AssetCode = reader.GetString(4),
                CreatedBy = reader.GetString(5),
                CreatedAt = reader.GetDateTime(6),
                UpdatedAt = reader.IsDBNull(7) ? (DateTime?)null : reader.GetDateTime(7)
            };
        }

        private static WorkOrderAttachment MapAttachment(SqlDataReader reader)
        {
            return new WorkOrderAttachment
            {
                Id = reader.GetInt32(0),
                WorkOrderId = reader.GetInt32(1),
                FileName = reader.GetString(2),
                StoredPath = reader.GetString(3),
                UploadedBy = reader.GetString(4),
                UploadedAt = reader.GetDateTime(5)
            };
        }

        private static WorkOrderStatus ParseStatus(string value)
        {
            WorkOrderStatus status;
            if (!Enum.TryParse(value, false, out status))
            {
                throw new InvalidOperationException("Status desconhecido no banco: " + value);
            }

            return status;
        }
    }
}
