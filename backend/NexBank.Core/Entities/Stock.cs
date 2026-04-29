namespace NexBank.Core.Entities;

public class Stock
{
    public int Id { get; set; }
    public string Symbol { get; set; } = string.Empty; // Örn: THYAO, GARAN, AAPL
    public string Name { get; set; } = string.Empty;
    public decimal CurrentPrice { get; set; }
    public decimal OpeningPrice { get; set; }
    public decimal ChangePercent => OpeningPrice == 0 ? 0 : Math.Round(((CurrentPrice - OpeningPrice) / OpeningPrice) * 100, 2);
    public string Sector { get; set; } = "Genel";
}

public class UserStock
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string StockSymbol { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal AverageCost { get; set; } // Ortalama Maliyet

    public User User { get; set; } = null!;
}
