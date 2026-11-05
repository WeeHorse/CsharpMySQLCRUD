using MySqlConnector;

// ANSLUTNING TILL DATABASEN
string connectionString = "Server=localhost;Database=shoppy;User ID=root;Password=rootroot;UseAffectedRows=false;";


await using var connection = new MySqlConnection(connectionString);
await connection.OpenAsync();

// REPOSITORY (klass med databas-queries)
var repository = new ProductRepository(connection);


// ANVÄNDER repository för att hämta data

// 1. READ: hämta alla produkter som en lista.
List<Product> products = await repository.GetProductsAsync();
Console.WriteLine("Alla produkter:");
foreach (Product product in products)
{
    Console.WriteLine(product);
}

// 2. CREATE: skapa en produkt och ta emot databasens nya id.
int newId = await repository.CreateProductAsync("Nalle", 129.50m);
Console.WriteLine($"Skapad produkt med id: {newId}");

// 3. READ: hämta den nya produkten som ett enstaka objekt.
Product? created = await repository.GetProductAsync(newId);
Console.WriteLine(created is null ? "Produkten saknas." : created.ToString());

// 4. UPDATE: ändra namn och pris på just den nya produkten.
int updatedRows = await repository.UpdateProductAsync(newId, "Nallebjörn", 159.90m);
Console.WriteLine($"Matchade rader vid UPDATE: {updatedRows}");

// Läs igen för att se förändringen i databasen.
Product? updated = await repository.GetProductAsync(newId);
Console.WriteLine(updated is null ? "Produkten saknas." : updated.ToString());

// 5. DELETE: ta bort produkten som detta exempel skapade.
int deletedRows = await repository.DeleteProductAsync(newId);
Console.WriteLine($"Borttagna rader: {deletedRows}");

// Läs igen: nu ska metoden returnera null.
Product? deleted = await repository.GetProductAsync(newId);
Console.WriteLine(deleted is null ? "Produkten är borttagen." : deleted.ToString());
