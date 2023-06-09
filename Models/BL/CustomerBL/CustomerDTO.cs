
namespace WebApp.Models.BL.CustomerBL
{
    public class CustomerDTO
    {
        public CustomerDTO() { }
        public CustomerDTO(string id, string name, string address, string phone)
        {
            this.Id = id;
            this.Name = name;
            this.Address = address;
            this.Phone = phone;
        }
        public string Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }

    }
}