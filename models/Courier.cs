namespace MyAPIC_Suhi.models
{
    public class CourierGetOrders
    {
        public int Id { get; set; }
        public DateTime CancelledTime { get; set; } 
        public int TotalAmount { get; set; }
        public string City { get; set; }
        public string Street {  get; set; }
        public string House { get; set; }
    }
}
