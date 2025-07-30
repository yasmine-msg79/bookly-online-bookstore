namespace BookStore.Models
{
    public class StockLog
    {
        public int Id { get; set; }
        public int BookId { get; set; }
        public int QuantityChanged { get; set; }
        public DateTime ChangeDate { get; set; }
        public string Reason { get; set; }

        public virtual Book Book { get; set; }
    }
}
