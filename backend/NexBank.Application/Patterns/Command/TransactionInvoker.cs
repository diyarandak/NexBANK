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
    private static readonly List<ITransactionCommand> _commandHistory = new();
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
            _commandHistory.Add(command);
            
            await _subject.NotifyAsync(transactionToSave);
            await _transactionRepository.UpdateAsync(transactionToSave);
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
        // İlgili işleme ait komutu geçmişten bul (TransferCommand içindeki _transaction.Id ile eşleşmeli)
        var command = _commandHistory.FirstOrDefault(c => 
            c is TransferCommand tc && tc.GetTransactionId() == transactionToUpdate.Id);

        if (command != null)
        {
            var success = await command.UndoAsync();
            if (success)
            {
                _commandHistory.Remove(command);
                await _transactionRepository.UpdateAsync(transactionToUpdate);
            }
            return success;
        }
        return false;
    }
}
