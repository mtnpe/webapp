namespace WebApp.Models.BL.UserBL
{
    public class UserDTO
    {
        public UserDTO() { }
        public UserDTO(int id, string username, string password, string role)
        {
            this.Id = id;
            this.UserName = username;
            this.Password = password;
            this.Role = role;
        }
        public int Id { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Role { get; set; }
    }
}
