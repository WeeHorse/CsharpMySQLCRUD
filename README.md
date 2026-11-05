# Gosedjursbutiken – SQL från C#

Ett litet konsolbaserat kodexempel för HAK MAI26. MySQL, .NET 10 och MySqlConnector; direkt SQL med samma syntaxformer som i [bokens kapitel 22](https://weehorse.github.io/Studera-SQL-prata-med-data/sv/chapters/22-sql-fran-applikationer.html).

## Kom igång

1. Ha en MySQL-server tillgänglig (8.0.16 eller senare).
2. Skapa databasen med `shop.sql` i din databasklient. Scriptet skapar databasen `shop`, tabellen `products` och fem gosedjur. Kör det en gång; om tabellen redan finns stoppas CREATE TABLE. Kör inte hela scriptet upprepade gånger eftersom INSERT då kan lägga in dubbletter.
3. Öppna en terminal i mappen med `ShopDemo.csproj`.
4. Ange anslutningen i Program.cs med ditt databas-konto och lösenord. 


## Kodexemplets flöde

Programmet kör alla operationer direkt, utan meny eller inmatning:

1. Hämtar och skriver ut alla produkter som en `List<Product>`.
2. Skapar `Nalle` för 129,50 kr och tar emot det genererade id:t.
3. Hämtar den skapade produkten som `Product?`.
4. Ändrar dess namn och pris och hämtar den igen för att visa resultatet.
5. Tar bort den skapade produkten och hämtar den igen för att visa att resultatet är `null`.

Exemplet ändrar och tar bort endast produkten som skapades under samma körning. Startprodukterna lämnas kvar. Om körningen avbryts före DELETE kan exempelprodukten ligga kvar.

`Program.cs` öppnar anslutningen, skapar en `ProductRepository` och kör exemplen. `ProductRepository.cs` samlar de fem databasmetoderna och deras SQL. Anslutningen skickas in via klassens primärkonstruktor; metoderna behöver därför inte ta emot anslutningen som argument. `Program.cs` äger och stänger anslutningen; repository använder den men stänger den inte. `Product.cs` innehåller:

```csharp
public sealed record Product(int Id, string Name, decimal Price);
```

Rader mappas till objekt med konstruktorn:

```csharp
new Product(
    reader.GetInt32("id"),
    reader.GetString("name"),
    reader.GetDecimal("price"));
```

`Console.WriteLine(product)` använder record-typens genererade `ToString()` för att visa egenskaperna.

| Operation | Metod | Resultat |
|---|---|---|
| Lista | `GetProductsAsync` | `List<Product>`, eventuellt tom |
| Hämta en | `GetProductAsync` | `Product?`, null om id saknas |
| Skapa | `CreateProductAsync` | Nytt id som `int`, från `LastInsertedId` |
| Ändra | `UpdateProductAsync` | Antal matchade rader med vår anslutningsinställning |
| Ta bort | `DeleteProductAsync` | Antal borttagna rader |

Tabellen använder `INT`, `VARCHAR(100)` och `DECIMAL(10,2)`. I C# används `int`, `string` och `decimal`. Priser i C# skrivs exempelvis `129.50m`; de skickas som decimalvärden via parametrarna.

Visa gärna metoderna i ordningen lista → hämta en → skapa → ändra → ta bort. Jämför `while` för flera rader med `if` för högst en rad, och uppmärksamma `WHERE` i UPDATE och DELETE.

## Parametriserad SQL och prepared statements

Det återkommande mönstret är:

```csharp
await using var command = new MySqlCommand(
    "SELECT id, name, price FROM products WHERE id = @id;", connection);

command.Parameters.AddWithValue("@id", id);
await command.PrepareAsync();
await using var reader = await command.ExecuteReaderAsync();
```

- SQL-strängen beskriver instruktionen. `@id` är en plats för ett värde, inte en C#-variabel i strängen.
- `AddWithValue` kopplar värdet i C# till parametern. Skriv inte citattecken runt `@id` i SQL-strängen.
- `PrepareAsync()` ber servern förbereda kommandot. Exemplet använder det uttryckligen i samtliga parametriserade CRUD-kommandon.
- Parametrisering och explicit serverförberedelse är olika saker. Med MySqlConnector kan parametriserade kommandon köras säkert även utan `PrepareAsync()`; parametrarna ska fortfarande användas. Bokens grundexempel gör just det.
- Förberedelse kan vara användbar när samma kommando körs många gånger med nya parametervärden. I den här appen visas syntaxen; vi påstår ingen prestandavinst när varje kommando bara körs en gång.
- Parametrar används för värden. De ersätter normalt inte tabellnamn, kolumnnamn eller SQL-nyckelord.

MySqlConnector stödjer `AddWithValue` för detta användningssätt. Generella råd om att undvika metoden i andra databasdrivrutiner behöver inte gälla här.

## Vilken Execute-metod?

| Metod | Användning |
|---|---|
| `ExecuteReaderAsync()` | Läser rader och kolumner: både produktlistan och en hel produkt |
| `ExecuteNonQueryAsync()` | Kör INSERT, UPDATE eller DELETE och ger antal påverkade rader |
| `ExecuteScalarAsync()` | Hämtar första kolumnen på första raden, exempelvis `SELECT COUNT(*) FROM products` |

Att hämta **en produkt** är inte samma sak som att hämta **ett enda värde**. Produkten har tre kolumner, därför använder vi en reader även där. Scalar visas som jämförelse och behövs inte i CRUD-flödet.

Anslutningssträngen anger `UseAffectedRows=false` uttryckligen. Då rapporterar UPDATE antal matchade rader även när namn och pris redan har samma värden. Med `UseAffectedRows=true` kan 0 också betyda att en befintlig rad inte behövde ändras; då vore meddelandet ”Produkten finns inte” missvisande.

## Körning och verifiering

Kör `dotnet run` efter att databasen och miljövariabeln är förberedda. Utskriften visar startprodukterna, nytt id, produkten före och efter UPDATE, antal borttagna rader och slutligen att produkten är borttagen.

Anslutningen är öppen under körningen. Varje metod frigör sin command och reader med `await using`. Exemplet har ingen egen felhantering; databasfel avbryter körningen och visas som exceptions. Transaktioner, ORM och samtidighetskontroll ingår inte.

Koden är genomläst. Miljön där materialet skapades saknar .NET SDK och MySQL, så projektet har inte kompilerats eller körts mot en server där.

Källor:
- [Boken, kapitel 22](https://weehorse.github.io/Studera-SQL-prata-med-data/sv/chapters/22-sql-fran-applikationer.html)
- [MySqlConnector: Basic API](https://mysqlconnector.net/tutorials/basic-api/)
- [MySqlConnector: AddWithValue](https://mysqlconnector.net/overview/using-addwithvalue/)
- [MySqlConnector: MySqlCommand](https://mysqlconnector.net/api/mysqlconnector/mysqlcommandtype/)
- [MySqlConnector: anslutningsinställningar](https://mysqlconnector.net/connection-options/)
