namespace Application.Common.Exceptions;

public class AlreadyExistsException : Exception
{
    public AlreadyExistsException(string entityName, string field, object value) : base($"{entityName} with {field} '{value}' already exists.") { }
}