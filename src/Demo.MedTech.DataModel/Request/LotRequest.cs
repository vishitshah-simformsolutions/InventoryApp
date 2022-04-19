namespace Demo.MedTech.DataModel.Request
{
    public class LotRequest
    {
        // this can not be null
        public dynamic LotDetail { get; set; }

        // this must be positive int
        public string PlatformCode { get; set; }

        public string AuctioneerId { get; set; }
    }
}
