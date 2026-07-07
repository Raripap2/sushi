using System.Security.Cryptography.Xml;

namespace MyAPIC_Suhi.models
{
    public class CartAdd
    {
        public int Sushi_id { get; set; }
        public int? Quantity { get; set; }
    }

    public class DeleteCartOne
    {
        public int? Id { get; set; }
        public int? Quantity { get; set; }
    }


    public class CartUpdate
    {
        public int Sushi_id { get; set; }
        public int Quantity { get; set; }
    }

    public class PageSushi
    {
        public int Page { get; set; } = 1;
        public int Limit { get; set; } = 12;
        public string Type { get; set; } = "all";
    }
    public class CurrentSushi
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Cost { get; set; }
        public int Count { get; set; }
        public int Weigth { get; set; }
        public string Photo { get; set; }
    }
}
