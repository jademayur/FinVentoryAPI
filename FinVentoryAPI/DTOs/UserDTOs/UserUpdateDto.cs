namespace FinVentoryAPI.DTOs.UserDTOs
{
    public class UserUpdateDto
    {
        public int UserId { get; set; }
        public string FullName { get; set; }
        public string Mobile { get; set; }
        public bool IsPlatformAdmin { get; set; }
    }
}
