namespace CleanOrders.Domain
{
    public class Order
    {
        public int Id { get; private set; }
        public decimal Total { get; private set; }
        public string Status { get; private set; } = "Pending";

        public Order(int id, decimal total)
        {
            Id = id;
            Total = total;
        }

        public void Confirm()
        {
            if (Total <= 0)
                throw new InvalidOperationException(
                    "Order total must be greater than zero.");

            if (Status != "Pending")
                throw new InvalidOperationException(
                    "Only pending orders can be confirmed.");

            Status = "Confirmed";
        }
    }
}
