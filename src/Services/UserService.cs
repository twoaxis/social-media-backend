﻿using MySql.Data.MySqlClient;
using social_media_backend.Models.Post;
using social_media_backend.Models.User;
using social_media_backend.Services;
using social_media_backend.src.Exceptions;

namespace social_media_backend.src.Services
{
    public class UserService
    {
        private readonly PostService _postService = new();
        
        public UserProfile GetUserProfile(string username)
        {
            UserProfile profile;
            DatabaseService.OpenConnection();

            try
            {
                if (!DoesUserExistByUsername(username))
                {
                    throw new UserNotFoundException();
                }

                using var command = new MySqlCommand("SELECT id, username, name FROM users WHERE username = @username",
                    DatabaseService.Connection);
                command.Parameters.AddWithValue("@username", username);

                using var reader = command.ExecuteReader();
                if (reader.Read())
                {
                    profile = new UserProfile(
                        reader.GetInt32("id"),
                        reader.GetString("username"),
                        reader.GetString("name"),
                        []
                    );
                    reader.Close();
                }
                else throw new UserNotFoundException();

                profile.posts = _postService.GetAllPosts(profile.id);
            }
            finally
            {
                DatabaseService.CloseConnection();
            }
            return profile;
        }
        public bool DoesUserExistByUsername(string username)
        {
            using var command = new MySqlCommand("SELECT COUNT(*) FROM users WHERE username = @username", DatabaseService.Connection);
            command.Parameters.AddWithValue("@username", username);
            var result = command.ExecuteScalar();
            return Convert.ToInt32(result) > 0;
        }
        public bool DoesUserExistByEmail(string email)
        {
            using var command = new MySqlCommand("SELECT COUNT(*) FROM users WHERE email = @email", DatabaseService.Connection);
            command.Parameters.AddWithValue("@email", email);
            var result = command.ExecuteScalar();
            return Convert.ToInt32(result) > 0;
        }
        
        public bool DoesUserExistById(int id)
        {
            using var command = new MySqlCommand("SELECT COUNT(*) FROM users WHERE id = @id", DatabaseService.Connection);
            command.Parameters.AddWithValue("@id", id);
            var result = command.ExecuteScalar();
            return Convert.ToInt32(result) > 0;
        }

        public bool FollowUser(int followerId, int followingId)
        {
            DatabaseService.OpenConnection();
            try
            {
                using (var command = new MySqlCommand("INSERT IGNORE INTO follows (follower_id, following_id) VALUES (@followerId, @followingId)", DatabaseService.Connection))
                {
                    command.Parameters.AddWithValue("@followerId", followerId);
                    command.Parameters.AddWithValue("@followingId", followingId);
                    int result = command.ExecuteNonQuery();

                    return result > 0;
                }
            }
            finally
            {
                DatabaseService.CloseConnection();
            }
        }

        public int GetUserIdByUsername(string username)
        {
            using (var command = new MySqlCommand("SELECT id FROM users WHERE username = @username", DatabaseService.Connection))
            {
                command.Parameters.AddWithValue("@username", username);
                var result = command.ExecuteScalar();
                if (result != null)
                    return Convert.ToInt32(result);

                throw new UserNotFoundException();
            }
        }


        public List<Dictionary<string, object>> GetFollowers(string username)
        {
            DatabaseService.OpenConnection();
            int userId = GetUserIdByUsername(username);

            var followers = new List<Dictionary<string, object>>();

            using (var command = new MySqlCommand("SELECT u.id, u.username, u.name FROM follows f JOIN users u ON f.follower_id = u.id WHERE f.following_id = @userId", DatabaseService.Connection))
            {
                command.Parameters.AddWithValue("@userId", userId);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var user = new Dictionary<string, object>
                        {
                            { "id", reader.GetInt32("id") },
                            { "Username", reader.GetString("username") },
                            { "Name", reader.GetString("name") }
                        };
                        followers.Add(user);
                    }
                }
            }

            DatabaseService.CloseConnection();
            return followers;
        }

        public List<Dictionary<string, object>> GetFollowing(string username)
        {
            DatabaseService.OpenConnection();
            int userId = GetUserIdByUsername(username);

            var following = new List<Dictionary<string, object>>();

            using (var command = new MySqlCommand("SELECT u.id, u.username, u.name FROM follows f JOIN users u ON f.following_id = u.id WHERE f.follower_id = @userId", DatabaseService.Connection))
            {
                command.Parameters.AddWithValue("@userId", userId);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var user = new Dictionary<string, object>
                        {
                            { "id", reader.GetInt32("id") },
                            { "Username", reader.GetString("username") },
                            { "Name", reader.GetString("name") }
                        };
                        following.Add(user);
                    }
                }
            }

            DatabaseService.CloseConnection();
            return following;
        }


    }
}
