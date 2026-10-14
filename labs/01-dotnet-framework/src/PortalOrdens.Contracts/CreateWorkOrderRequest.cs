namespace PortalOrdens.Contracts
{
    public sealed class CreateWorkOrderRequest
    {
        public string Title { get; set; }

        public string Description { get; set; }

        public string AssetCode { get; set; }
    }
}
