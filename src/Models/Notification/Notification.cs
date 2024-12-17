namespace social_media_backend.Models.Notification;

[Serializable]
public class Notification(int id, int userId, string title, string description, DateTime createdAt, bool isRead)
{
    public int Id { get; set; } = id;
    public int UserId { get; set; } = userId;
    public string Title { get; set; } = title;
    public string Description { get; set; } = description;
    public DateTime CreatedAt { get; set; } = createdAt;
    public bool IsRead { get; set; } = isRead;
}

