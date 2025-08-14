using BookStore.Models;

public class StockLog
{
    public int Id { get; set; }
    public int BookId { get; set; }
    public int QuantityChanged { get; set; }  // e.g., -3 or +10
    public DateTime ChangeDate { get; set; }
    public string Reason { get; set; }
    public int? OrderId { get; set; }

    public virtual Book Book { get; set; }
    public virtual Order Order { get; set; }
}