namespace FinVentoryAPI.DTOs.UserDTOs
{
    public class UserResponseDto
    {
        public int UserId { get; set; }

        public string FullName { get; set; }

        public string Email { get; set; }

        public string? Mobile { get; set; }

        public bool IsPlatformAdmin { get; set; }

        public bool IsActive { get; set; }
    }
}
