-- Kör i MySQL 8.0.16 eller senare, exempelvis i MySQL Workbench.
-- Scriptet skapar nya objekt. Det raderar eller återställer inga befintliga data.
CREATE DATABASE IF NOT EXISTS shop CHARACTER SET utf8mb4;
USE shop;

CREATE TABLE products (
    id INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
    name VARCHAR(100) NOT NULL,
    price DECIMAL(10, 2) NOT NULL,
    CONSTRAINT chk_products_price CHECK (price >= 0)
);

INSERT INTO products (name, price) VALUES
('Nallebjörn', 149.00),
('Kanin med långa öron', 179.50),
('Mjuk haj', 249.00),
('Liten pingvin', 99.00),
('Sovande räv', 199.00);

SELECT id, name, price FROM products ORDER BY id;
