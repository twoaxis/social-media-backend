CREATE TABLE users (
    id INT(11) NOT NULL AUTO_INCREMENT PRIMARY KEY,
    username VARCHAR(256) NOT NULL UNIQUE,
    name VARCHAR(2048) NOT NULL,
    bio VARCHAR(2048),
    email VARCHAR(1024) NOT NULL,
    email_verified BOOLEAN NOT NULL DEFAULT FALSE,
    password VARCHAR(4096) NOT NULL
);

CREATE TABLE revoked_tokens (
    token VARCHAR(1024) NOT NULL
);
CREATE TABLE email_verification_codes(
    uid INT(11) NOT NULL REFERENCES users(id),
    session_id VARCHAR(1024) UNIQUE NOT NULL,
    code VARCHAR(6) NOT NULL
);
CREATE TABLE forget_password_codes(
    uid INT(11) NOT NULL REFERENCES users(id),
    session_id VARCHAR(1024) UNIQUE NOT NULL,
    code VARCHAR(6) NOT NULL
);
CREATE TABLE posts (
    id INT(11) NOT NULL AUTO_INCREMENT PRIMARY KEY,
    author INT(11) NOT NULL REFERENCES users(id),
    content TEXT NOT NULL,
    createdAt TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);
CREATE TABLE follows (
    follower_id INT(11) NOT NULL REFERENCES users(id),
    following_id INT(11) NOT NULL REFERENCES users(id),
    UNIQUE (follower_id, following_id)
);
CREATE TABLE post_likes (
    id INT(11) NOT NULL AUTO_INCREMENT PRIMARY KEY,
    user_id INT(11) NOT NULL REFERENCES users(id),
    post_id INT(11) NOT NULL REFERENCES posts(id),
    UNIQUE (user_id, post_id)
);

CREATE TABLE post_comments (
    id INT(11) NOT NULL AUTO_INCREMENT PRIMARY KEY,
    user_id INT(11) NOT NULL REFERENCES users(id),
    post_id INT(11) NOT NULL REFERENCES posts(id),
    content TEXT NOT NULL,
    createdAt TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);
CREATE TABLE friends (
    id INT(11) AUTO_INCREMENT PRIMARY KEY,
    user1_id INT(11) NOT NULL REFERENCES users(id),
    user2_id INT(11) NOT NULL REFERENCES users(id),
    status ENUM('pending', 'accepted', 'rejected') NOT NULL,
    createdAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    UNIQUE (user1_id, user2_id)
);
