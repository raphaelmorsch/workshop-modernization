using PortalOrdens.Contracts;

namespace PortalOrdens.Web
{
    public static class StatusLabels
    {
        public static string ToLabel(WorkOrderStatus status)
        {
            switch (status)
            {
                case WorkOrderStatus.Aberta:
                    return "Aberta";
                case WorkOrderStatus.EmExecucao:
                    return "Em execução";
                case WorkOrderStatus.Concluida:
                    return "Concluída";
                default:
                    return status.ToString();
            }
        }
    }
}
