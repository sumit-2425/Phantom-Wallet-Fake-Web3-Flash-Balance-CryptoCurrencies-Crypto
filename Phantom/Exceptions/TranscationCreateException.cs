namespace Backend.Exceptions;

public class TransactionCreateValidationException : Exception
{

    public List<string> Errors { get; set; }
    public TransactionCreateValidationException(List<string> errors) : base("Validation failed.")
    {
        Errors = errors;
    }
}