namespace Demo.MedTech.Api.Helpers
{
    public interface ICorrelationIdProvider
    {
        string GetCorrelationId();
        string InitializeCorrelationId();
    }
}