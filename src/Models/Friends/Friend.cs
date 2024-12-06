namespace social_media_backend.Models.Friend;

[Serializable]
public class Friend(int id, int user1Id, int user2Id, string status, DateTime createdAt, DateTime updatedAt)
{
    public int Id { get; set; } = id;
    public int User1Id { get; set; } = user1Id;
    public int User2Id { get; set; } = user2Id;
    public string Status { get; set; } = status; // pending, accepted, rejected
    public DateTime CreatedAt { get; set; } = createdAt;
    public DateTime UpdatedAt { get; set; } = updatedAt;
}