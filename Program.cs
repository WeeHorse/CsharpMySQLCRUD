using MySqlConnector;

// ANSLUTNING

string? connectionString = Environment.GetEnvironmentVariable("SHOP_CONNECTION_STRING");
if (string.IsNullOrWhiteSpace(connectionString))
{
    Console.WriteLine("Ange miljövariabeln SHOP_CONNECTION_STRING först. Se README.md.");
    return;
}

await using var connection = new MySqlConnection(connectionString);
await connection.OpenAsync();


// INTERAKTIONER MED DATABASEN

// 1. READ: hämta alla produkter som en lista.
List<Product> products = await GetProductsAsync(connection);
Console.WriteLine("Alla produkter:");
foreach (Product product in products)
{
    Console.WriteLine(product);
}

// 2. CREATE: skapa en produkt och ta emot databasens nya id.
int newId = await CreateProductAsync(connection, "O'Briens nalle", 129.50m);
Console.WriteLine($"Skapad produkt med id: {newId}");

// 3. READ: hämta den nya produkten som ett enstaka objekt.
Product? created = await GetProductAsync(connection, newId);
Console.WriteLine(created is null ? "Produkten saknas." : created.ToString());

// 4. UPDATE: ändra namn och pris på just den nya produkten.
int updatedRows = await UpdateProductAsync(connection, newId, "O'Briens stora nalle", 159.90m);
Console.WriteLine($"Matchade rader vid UPDATE: {updatedRows}");

// Läs igen för att se förändringen i databasen.
Product? updated = await GetProductAsync(connection, newId);
Console.WriteLine(updated is null ? "Produkten saknas." : updated.ToString());

// 5. DELETE: ta bort produkten som detta exempel skapade.
int deletedRows = await DeleteProductAsync(connection, newId);
Console.WriteLine($"Borttagna rader: {deletedRows}");

// Läs igen: nu ska metoden returnera null.
Product? deleted = await GetProductAsync(connection, newId);
Console.WriteLine(deleted is null ? "Produkten är borttagen." : deleted.ToString());

// READ: flera rader → en lista med objekt.
static async Task<List<Product>> GetProductsAsync(MySqlConnection connection)
{
    await using var command = new MySqlCommand(
        "SELECT id, name, price FROM products ORDER BY id;", connection);
    await using var reader = await command.ExecuteReaderAsync();

    var products = new List<Product>();
    while (await reader.ReadAsync())
    {
        products.Add(new Product(
            reader.GetInt32("id"),
            reader.GetString("name"),
            reader.GetDecimal("price")));
    }
    return products;
}

// READ: en rad → ett objekt, eller null när id saknas.
static async Task<Product?> GetProductAsync(MySqlConnection connection, int id)
{
    await using var command = new MySqlCommand(
        "SELECT id, name, price FROM products WHERE id = @id;", connection);
    command.Parameters.AddWithValue("@id", id);
    await command.PrepareAsync();
    await using var reader = await command.ExecuteReaderAsync();

    if (await reader.ReadAsync())
    {
        return new Product(
            reader.GetInt32("id"),
            reader.GetString("name"),
            reader.GetDecimal("price"));
    }
    return null;
}

// CREATE: id genereras av databasen.
static async Task<int> CreateProductAsync(MySqlConnection connection, string name, decimal price)
{
    await using var command = new MySqlCommand(
        "INSERT INTO products (name, price) VALUES (@name, @price);", connection);
    command.Parameters.AddWithValue("@name", name);
    command.Parameters.AddWithValue("@price", price);
    await command.PrepareAsync();
    await command.ExecuteNonQueryAsync();
    return checked((int)command.LastInsertedId);
}

// UPDATE: WHERE avgränsar vilken produkt som ska ändras.
static async Task<int> UpdateProductAsync(MySqlConnection connection, int id, string name, decimal price)
{
    await using var command = new MySqlCommand(
        """
        UPDATE products
        SET name = @name, price = @price
        WHERE id = @id;
        """, connection);
    command.Parameters.AddWithValue("@name", name);
    command.Parameters.AddWithValue("@price", price);
    command.Parameters.AddWithValue("@id", id);
    await command.PrepareAsync();
    return await command.ExecuteNonQueryAsync();
}

// DELETE: WHERE avgränsar vilken produkt som ska tas bort.
static async Task<int> DeleteProductAsync(MySqlConnection connection, int id)
{
    await using var command = new MySqlCommand(
        "DELETE FROM products WHERE id = @id;", connection);
    command.Parameters.AddWithValue("@id", id);
    await command.PrepareAsync();
    return await command.ExecuteNonQueryAsync();
}

