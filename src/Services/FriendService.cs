using MySql.Data.MySqlClient;
using social_media_backend.Models.Friend;

namespace social_media_backend.Services;

public class FriendService
{
    // إرسال طلب صداقة
    public void SendFriendRequest(int userId, int targetUserId)
    {
        try
        {
            DatabaseService.OpenConnection();

            using var command = new MySqlCommand(
                "INSERT INTO friends (user1_id, user2_id, status) VALUES (@userId, @targetUserId, 'pending') ON DUPLICATE KEY UPDATE status = 'pending';",
                DatabaseService.Connection
            );

            command.Parameters.AddWithValue("@userId", userId);
            command.Parameters.AddWithValue("@targetUserId", targetUserId);

            command.ExecuteNonQuery();
        }
        finally
        {
            DatabaseService.CloseConnection();
        }
    }

    // قبول طلب صداقة
    public void AcceptFriendRequest(int userId, int requesterId)
    {
        try
        {
            DatabaseService.OpenConnection();

            using var command = new MySqlCommand(
                "UPDATE friends SET status = 'accepted' WHERE user1_id = @requesterId AND user2_id = @userId;",
                DatabaseService.Connection
            );

            command.Parameters.AddWithValue("@userId", userId);
            command.Parameters.AddWithValue("@requesterId", requesterId);

            command.ExecuteNonQuery();
        }
        finally
        {
            DatabaseService.CloseConnection();
        }
    }

    // رفض طلب صداقة
    public void RejectFriendRequest(int userId, int requesterId)
    {
        try
        {
            DatabaseService.OpenConnection();

            using var command = new MySqlCommand(
                "UPDATE friends SET status = 'rejected' WHERE user1_id = @requesterId AND user2_id = @userId;",
                DatabaseService.Connection
            );

            command.Parameters.AddWithValue("@userId", userId);
            command.Parameters.AddWithValue("@requesterId", requesterId);

            command.ExecuteNonQuery();
        }
        finally
        {
            DatabaseService.CloseConnection();
        }
    }

    // استرجاع قائمة الأصدقاء
    public List<Friend> GetFriends(int userId)
    {
        List<Friend> friends = new();

        try
        {
            DatabaseService.OpenConnection();

            using var command = new MySqlCommand(
                "SELECT * FROM friends WHERE (user1_id = @userId OR user2_id = @userId) AND status = 'accepted';",
                DatabaseService.Connection
            );

            command.Parameters.AddWithValue("@userId", userId);

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                friends.Add(new Friend(
                    reader.GetInt32("id"),
                    reader.GetInt32("user1_id"),
                    reader.GetInt32("user2_id"),
                    reader.GetString("status"),
                    reader.GetDateTime("createdAt"),
                    reader.GetDateTime("updatedAt")
                ));
            }
        }
        finally
        {
            DatabaseService.CloseConnection();
        }

        return friends;
    }

    // استرجاع طلبات الصداقة
    public List<Friend> GetFriendRequests(int userId)
    {
        List<Friend> requests = new();

        try
        {
            DatabaseService.OpenConnection();

            using var command = new MySqlCommand(
                "SELECT * FROM friends WHERE user2_id = @userId AND status = 'pending';",
                DatabaseService.Connection
            );

            command.Parameters.AddWithValue("@userId", userId);

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                requests.Add(new Friend(
                    reader.GetInt32("id"),
                    reader.GetInt32("user1_id"),
                    reader.GetInt32("user2_id"),
                    reader.GetString("status"),
                    reader.GetDateTime("createdAt"),
                    reader.GetDateTime("updatedAt")
                ));
            }
        }
        finally
        {
            DatabaseService.CloseConnection();
        }

        return requests;
    }
}