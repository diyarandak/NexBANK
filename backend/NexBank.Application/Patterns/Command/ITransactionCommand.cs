namespace NexBank.Application.Patterns.Command;

/// <summary>
/// COMMAND PATTERN
/// Her bir işlem bir komut nesnesidir. Geri alınabilmesi (Undo) hedeflenir.
/// </summary>
public interface ITransactionCommand
{
    Task<bool> ExecuteAsync();
    Task<bool> UndoAsync();
    int GetTransactionId();
}
