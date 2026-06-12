using Npgsql;
using PetHaven.Infrastructure.Services;

var email = RequiredEnvironmentVariable("PETHAVEN_ADMIN_EMAIL");
var password = RequiredEnvironmentVariable("PETHAVEN_ADMIN_PASSWORD");
var connectionString = RequiredEnvironmentVariable("ConnectionStrings__DefaultConnection");

var passwordHash = new PasswordService().HashPassword(password);

await using var connection = new NpgsqlConnection(connectionString);
await connection.OpenAsync();

await using var updateCommand = connection.CreateCommand();
updateCommand.CommandText = """
UPDATE "Users" SET
    "FirstName" = @firstName,
    "LastName" = @lastName,
    "PasswordHash" = @passwordHash,
    "Role" = @role,
    "UpdatedAt" = @createdAt
WHERE "Email" = @email;
""";

AddCommonParameters(updateCommand, DateTime.UtcNow);

var rows = await updateCommand.ExecuteNonQueryAsync();

if (rows == 0)
{
    await using var insertCommand = connection.CreateCommand();
    insertCommand.CommandText = """
    INSERT INTO "Users" ("Id", "FirstName", "LastName", "Email", "PasswordHash", "Role", "CreatedAt", "UpdatedAt")
    VALUES (@id, @firstName, @lastName, @email, @passwordHash, @role, @createdAt, NULL);
    """;

    insertCommand.Parameters.AddWithValue("id", Guid.NewGuid());
    AddCommonParameters(insertCommand, DateTime.UtcNow);
    rows = await insertCommand.ExecuteNonQueryAsync();
}

Console.WriteLine($"Admin user ready: {email}");
Console.WriteLine($"Rows affected: {rows}");

void AddCommonParameters(NpgsqlCommand command, DateTime timestamp)
{
    command.Parameters.AddWithValue("firstName", "PetHaven");
    command.Parameters.AddWithValue("lastName", "Admin");
    command.Parameters.AddWithValue("email", email);
    command.Parameters.AddWithValue("passwordHash", passwordHash);
    command.Parameters.AddWithValue("role", 3);
    command.Parameters.AddWithValue("createdAt", timestamp);
}

static string RequiredEnvironmentVariable(string name) =>
    Environment.GetEnvironmentVariable(name)
    ?? throw new InvalidOperationException($"Required environment variable '{name}' is not configured.");
