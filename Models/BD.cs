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
        string passwordHash = BCrypt.Net.BCrypt.HashPassword(contraseña);
        using (SqlConnection connection = ObtenerConexion())
        {
            var insertSql = @"INSERT INTO Avu (nombre, apellido, password_hash, dni, genero, nacimiento, nacionalidad) VALUES (@nombre, @apellido, @passwordHash, @DNI, @genero, @nacimiento, @nacionalidad)";
            connection.Execute(insertSql, new
            {
                nombre = nombre,
                apellido = apellido,
                passwordHash = passwordHash,
                dni = DNI,
                genero = genero,
                nacimiento = nacimiento,
                nacionalidad = nacionalidad
            });
        }
    }
    public static bool VerificarIniciarSesion(string DNI, string contraseña)
    {
        using (SqlConnection connection = ObtenerConexion())
        {
            var query = @"SELECT dni, password_hash FROM Avu WHERE dni = @DNI";
            var usuario = connection.QuerySingleOrDefault<dynamic>(query, new { DNI = DNI });
            if (usuario == null)
            {
                return false;
            }
            bool contraseñaValida = BCrypt.Net.BCrypt.Verify(contraseña, usuario.password_hash);
            return contraseñaValida;
        }
    }
}