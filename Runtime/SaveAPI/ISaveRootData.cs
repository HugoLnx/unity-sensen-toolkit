namespace SensenToolkit
{
    public interface ISaveRootData
    {
        System.DateTime Timestamp { get; set; }
        string UserId { get; set; }
        string EnvId { get; set; }
    }
}
