namespace Folke.Elm
{
    public interface IDatabaseSettings
    {
        string Host { get; }
        string Database { get; }
        string User { get; }
        string Password { get; }
    }
}
