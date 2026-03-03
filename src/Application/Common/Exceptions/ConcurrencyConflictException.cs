namespace Application.Common;

public class ConcurrencyConflictException : Exception
{
    public object ClientValues { get; }
    public object DatabaseValues { get; }

    public ConcurrencyConflictException(
        string message,
        object clientValues,
        object databaseValues) : base(message)
    {
        ClientValues   = clientValues;
        DatabaseValues = databaseValues;
    }
}