namespace NexBank.Application.Patterns.Singleton;

/// <summary>
/// SINGLETON PATTERN: Uygulama boyunca veritabanı kayıt istatistiklerini 
/// ve genel veritabanı durumunu yöneten tek bir nesne sağlar.
/// </summary>
public sealed class DatabaseManager
{
    private static DatabaseManager? _instance;
    private static readonly object _lock = new object();
    
    private int _totalSaveCount = 0;
    private DateTime _lastOperationTime;

    // Private constructor (Dışarıdan new'lenemez)
    private DatabaseManager() 
    {
        _lastOperationTime = DateTime.Now;
    }

    // Thread-safe Instance erişimi
    public static DatabaseManager Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new DatabaseManager();
                    }
                }
            }
            return _instance;
        }
    }

    public void IncrementSaveCount()
    {
        lock (_lock)
        {
            _totalSaveCount++;
            _lastOperationTime = DateTime.Now;
        }
    }

    public int GetTotalSaveCount() => _totalSaveCount;
    public DateTime GetLastOperationTime() => _lastOperationTime;
}
