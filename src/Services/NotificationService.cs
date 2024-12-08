using MySql.Data.MySqlClient;
using social_media_backend.Models.Notification;

namespace social_media_backend.Services
{
    public class NotificationService
    {
        public void CreateNotification(int userId, string title, string description)
        {
            try
            {
                DatabaseService.OpenConnection();

                using var command = new MySqlCommand(
                    "INSERT INTO notifications (user_id, title, description) VALUES (@userId, @title, @description);",
                    DatabaseService.Connection
                );

                command.Parameters.AddWithValue("@userId", userId);
                command.Parameters.AddWithValue("@title", title);
                command.Parameters.AddWithValue("@description", description);

                command.ExecuteNonQuery();
            }
            finally
            {
                DatabaseService.CloseConnection();
            }
        }

        public List<Notification> GetNotifications(int userId)
        {
            List<Notification> notifications = new();

            try
            {
                DatabaseService.OpenConnection();

                using var updateCommand = new MySqlCommand(
                    "UPDATE notifications SET isRead = TRUE WHERE user_id = @userId AND isRead = FALSE;",
                    DatabaseService.Connection
                );

                updateCommand.Parameters.AddWithValue("@userId", userId);
                updateCommand.ExecuteNonQuery();

                using var command = new MySqlCommand(
                    "SELECT * FROM notifications WHERE user_id = @userId ORDER BY createdAt DESC;",
                    DatabaseService.Connection
                );

                command.Parameters.AddWithValue("@userId", userId);

                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    notifications.Add(new Notification(
                        reader.GetInt32("id"),
                        reader.GetInt32("user_id"),
                        reader.GetString("title"),
                        reader.GetString("description"),
                        reader.GetDateTime("createdAt"),
                        reader.GetBoolean("isRead")
                    ));
                }
            }
            finally
            {
                DatabaseService.CloseConnection();
            }

            return notifications;
        }
    }
}
