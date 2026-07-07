namespace MyAPIC_Suhi.models
{
    public class GetOrders
    {
        public int Id { get; set; }
    }

    public class OrderAdd
    {
        public Dictionary<int, int> Sushi { get; set; }
        public int TotalAmmount { get; set; }
        public string Comment { get; set; }
        public string Type { get; set; }
    }
    public class CurrentOrderInfo
    {
        public int Id { get; set; }
        public string Comment { get; set; }
        public int total_amount { get; set; }
        public string Status { get; set; }
    }
    public class CurrentOrderItems
    {
        public string Name { get; set; }
        public string Photo { get; set; }
        public int Quantity { get; set; }
        public int Price { get; set; }
    }
}
