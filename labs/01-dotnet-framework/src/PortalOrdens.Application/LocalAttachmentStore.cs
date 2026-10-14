using System;
using System.Configuration;
using System.IO;

namespace PortalOrdens.Application
{
    public sealed class LocalAttachmentStore
    {
        public string Save(int workOrderId, string originalFileName, Stream content)
        {
            if (content == null)
            {
                throw new ArgumentNullException("content");
            }

            var safeName = Path.GetFileName(originalFileName ?? string.Empty);
            if (string.IsNullOrWhiteSpace(safeName) || safeName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            {
                throw new ArgumentException("Nome de arquivo inválido.");
            }

            var root = RootPath();
            Directory.CreateDirectory(root);

            var storedName = workOrderId.ToString("D6")
                + "_"
                + DateTime.Now.ToString("yyyyMMddHHmmss")
                + "_"
                + safeName;
            var fullPath = Path.Combine(root, storedName);

            using (var file = File.Create(fullPath))
            {
                content.CopyTo(file);
            }

            return fullPath;
        }

        public byte[] Read(string storedPath)
        {
            if (string.IsNullOrWhiteSpace(storedPath))
            {
                throw new ArgumentException("Caminho do anexo ausente.");
            }

            var root = Path.GetFullPath(RootPath());
            var separator = Path.DirectorySeparatorChar.ToString();
            if (!root.EndsWith(separator, StringComparison.OrdinalIgnoreCase))
            {
                root = root + separator;
            }

            var full = Path.GetFullPath(storedPath);
            if (!full.StartsWith(root, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Anexo fora do diretório configurado.");
            }

            return File.ReadAllBytes(full);
        }

        private static string RootPath()
        {
            var configured = ConfigurationManager.AppSettings["AttachmentPath"];
            if (string.IsNullOrWhiteSpace(configured))
            {
                throw new InvalidOperationException(
                    "AppSetting AttachmentPath não encontrado. Confira o Web.config.");
            }

            return configured;
        }
    }
}
