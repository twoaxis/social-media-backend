CREATE TABLE users (
    id INT(11) NOT NULL AUTO_INCREMENT PRIMARY KEY,
    username VARCHAR(256) NOT NULL UNIQUE,
    name VARCHAR(2048) NOT NULL,
    email VARCHAR(1024) NOT NULL,
    password VARCHAR(4096) NOT NULL
);

CREATE TABLE revoked_tokens (
    token VARCHAR(1024) NOT NULL
);

CREATE TABLE posts (
    id INT(11) NOT NULL AUTO_INCREMENT PRIMARY KEY,
    author INT(11) NOT NULL REFERENCES users(id),
    content TEXT NOT NULL,
    createdAt TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);
CREATE TABLE follows (
    follower_id INT NOT NULL,
    following_id INT NOT NULL,
    FOREIGN KEY (follower_id) REFERENCES users(id),
    FOREIGN KEY (following_id) REFERENCES users(id),
    PRIMARY KEY (follower_id, following_id)
);
