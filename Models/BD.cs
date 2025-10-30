using Microsoft.Data.SqlClient;
using Dapper;
using System.Collections.Generic;
using System.Linq;
using BCrypt.Net;

public static class BD
{
    private static string _connectionString = @"Server=localhost; DataBase=AVUS; Integrated Security=True; TrustServerCertificate=True;";

    public static SqlConnection ObtenerConexion()
    {
        return new SqlConnection(_connectionString);
    }
    public static void AgregarAvu(string nombre, string apellido, string contraseña, string DNI, string genero, DateTime nacimiento, string nacionalidad)
    {
        string dniNormalizado = DNI.Trim().ToLowerInvariant();
        using (SqlConnection connection = ObtenerConexion())
        {
            var existe = connection.QuerySingleOrDefault("SELECT 1 FROM Avu WHERE LOWER(LTRIM(RTRIM(dni))) = @DNI", new { DNI = dniNormalizado });
            if (existe != null)
            {
                throw new Exception("El DNI ya está registrado.");
            }
        }
        string passwordHash = BCrypt.Net.BCrypt.HashPassword(contraseña);
        using (SqlConnection connection = ObtenerConexion())
        {
            var insertSql = @"INSERT INTO Avu (nombre, apellido, password_hash, dni, genero, nacimiento, nacionalidad) VALUES (@nombre, @apellido, @passwordHash, @DNI, @genero, @nacimiento, @nacionalidad)";
            connection.Execute(insertSql, new
            {
                nombre = nombre,
                apellido = apellido,
                passwordHash = passwordHash,
                DNI = dniNormalizado,
                genero = genero,
                nacimiento = nacimiento,
                nacionalidad = nacionalidad
            });
        }
    }
    public static bool VerificarIniciarSesion(string DNI, string contraseña)
    {
        string dniNormalizado = DNI.Trim().ToLowerInvariant();
        using (SqlConnection connection = ObtenerConexion())
        {
            var query = @"SELECT dni, password_hash FROM Avu WHERE LOWER(LTRIM(RTRIM(dni))) = @DNI";
            var usuario = connection.QuerySingleOrDefault(query, new { DNI = dniNormalizado });
            if (usuario == null || contraseña == null || usuario.password_hash == null)
            {
                return false;
            }
            string hash = (string)usuario.password_hash;
            bool contraseñaValida = BCrypt.Net.BCrypt.Verify(contraseña, hash);
            return contraseñaValida;
        }
    }
}