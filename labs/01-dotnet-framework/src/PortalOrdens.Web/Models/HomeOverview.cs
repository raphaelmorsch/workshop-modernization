namespace PortalOrdens.Web.Models
{
    public sealed class HomeOverview
    {
        public int Abertas { get; set; }

        public int EmExecucao { get; set; }

        public int Concluidas { get; set; }

        public int? LastWorkOrderId { get; set; }

        public string WindowsUser { get; set; }

        public string DatabaseError { get; set; }
    }
}
