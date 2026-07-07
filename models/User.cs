using System.Drawing;

namespace MyAPIC_Suhi.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Login { get; set; }
        public string? Email { get; set; }
        public string? Username { get; set; }
        public string Password { get; set; }
    }

    public class UserAut
    {
        public string Login {  set; get; }
        public string Password { set; get; }
    }

    public class UserEdit
    {
        public int Id { get; set; }
        public string? Login { get; set; }
        public string? Email { get; set; }
        public string? Username { get; set; }
        public string? Password { get; set; }
        public string? photo {  get; set; }
    }

    public class UserAddress
    {
        public string? City { get; set; }
        public string? Street { get; set; }
        public string? House { get; set; }
        public int? Entrance { get; set; }
        public int? Floor { get; set; }
        public int? Flat { get; set; }
        public bool? Intercom { get; set; }
    }

}
