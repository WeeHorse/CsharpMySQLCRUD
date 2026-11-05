-- Kör i MySQL 8.0.16 eller senare, exempelvis i MySQL Workbench.
-- Scriptet skapar nya objekt. Det raderar eller återställer inga befintliga data.
CREATE DATABASE IF NOT EXISTS shoppy CHARACTER SET utf8mb4;
USE shoppy;

CREATE TABLE products (
    id INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
    name VARCHAR(100) NOT NULL,
    price DECIMAL(10, 2) NOT NULL,
    CONSTRAINT chk_products_price CHECK (price >= 0)
);

INSERT INTO products (name, price) VALUES
('Piglet', 149.00),
('Kaninis', 179.50),
('Sharky', 249.00),
('Pingu', 99.00),
('Dilen', 199.00);

SELECT id, name, price FROM products ORDER BY id;
