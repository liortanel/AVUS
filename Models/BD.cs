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
        // Normalizamos el DNI
        string dniNormalizado = DNI.Trim().ToLowerInvariant();
        using (SqlConnection connection = ObtenerConexion())
        {
            // Chequear si el DNI ya existe
            var existe = connection.QuerySingleOrDefault("SELECT 1 FROM Avu WHERE LOWER(LTRIM(RTRIM(dni))) = @DNI", new { DNI = dniNormalizado });
            if (existe != null)
            {
                Console.WriteLine($"[REGISTRO] Registro falló: DNI ya existe: '{dniNormalizado}'");
                throw new Exception("El DNI ya está registrado.");
            }
        }
        string passwordHash = BCrypt.Net.BCrypt.HashPassword(contraseña);
        Console.WriteLine($"[REGISTRO] DNI: '{dniNormalizado}' Hash generado: '{passwordHash}'");
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
        Console.WriteLine($"[LOGIN] DNI recibido normalizado: '{dniNormalizado}' | ASCII: {string.Join(",", dniNormalizado.Select(c => ((int)c).ToString()))}");
        using (SqlConnection connection = ObtenerConexion())
        {
            var query = @"SELECT dni, password_hash FROM Avu WHERE LOWER(LTRIM(RTRIM(dni))) = @DNI";
            var usuario = connection.QuerySingleOrDefault(query, new { DNI = dniNormalizado });
            if (usuario == null)
            {
                var all = connection.Query("SELECT dni FROM Avu");
                foreach (var u in all)
                {
                    string s = (string)u.dni;
                    Console.WriteLine($"[LOGIN] DNI en base de datos: '{s}' | ASCII: {string.Join(",", s.Select(c => ((int)c).ToString()))}");
                }
            }
            if (usuario == null || contraseña == null || usuario.password_hash == null)
            {
                Console.WriteLine($"[LOGIN] Usuario no encontrado o hash nulo para DNI: {dniNormalizado}");
                return false;
            }
            string hash = (string)usuario.password_hash;
            Console.WriteLine($"[LOGIN] Comparando contraseña '{contraseña}' con hash {hash.Substring(0,2)}...{hash.Substring(hash.Length-2,2)} (largo={hash.Length})");
            bool contraseñaValida = BCrypt.Net.BCrypt.Verify(contraseña, hash);
            Console.WriteLine($"[LOGIN] Resultado bcrypt: {contraseñaValida}");
            return contraseñaValida;
        }
    }
}