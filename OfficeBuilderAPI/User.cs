using System.ComponentModel.DataAnnotations;

public class User
{
    public int Id { get; set; }
    
    [Required, MaxLength(50)]
    public string Username { get; set; }
    
    [Required]
    public byte[] PasswordHash { get; set; }
    
    [Required]
    public byte[] PasswordSalt { get; set; }
    
    public List<World> Worlds { get; set; }
}
