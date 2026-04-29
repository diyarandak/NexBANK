using NexBank.Core.Entities;
using NexBank.Core.Interfaces;
using NexBank.Application.Patterns.Observer;

namespace NexBank.Application.Patterns.Command;

/// <summary>
/// COMMAND PATTERN - Invoker
/// Komutları yürütür ve bir geçmiş listesinde tutar.
/// OBSERVER PATTERN - İşlem onaylandığında abonelere bildirir.
/// </summary>
public class TransactionInvoker
{
    private readonly Stack<ITransactionCommand> _history = new();
    private readonly ITransactionRepository _transactionRepository;
    private readonly TransactionSubject _subject;

    public TransactionInvoker(ITransactionRepository transactionRepository, TransactionSubject subject)
    {
        _transactionRepository = transactionRepository;
        _subject = subject;
    }

    public async Task<bool> ExecuteCommandAsync(ITransactionCommand command, Transaction transactionToSave)
    {
        var result = await command.ExecuteAsync();

        if (result)
        {
            _history.Push(command);
            await _transactionRepository.UpdateAsync(transactionToSave);
            
            // Observer ile abonelere (SMS, Email, Log) bildir!
            await _subject.NotifyAsync(transactionToSave);
        }
        else
        {
            transactionToSave.Status = TransactionStatus.Rejected;
            await _transactionRepository.UpdateAsync(transactionToSave);
        }

        return result;
    }

    public async Task<bool> UndoLastCommandAsync(Transaction transactionToUpdate)
    {
        if (_history.Any())
        {
            var command = _history.Pop();
            var success = await command.UndoAsync();
            if (success)
            {
                await _transactionRepository.UpdateAsync(transactionToUpdate);
            }
            return success;
        }
        return false;
    }
}
