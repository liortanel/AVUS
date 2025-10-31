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
            var query = @"INSERT INTO Avu (nombre, apellido, password_hash, dni, genero, nacimiento, nacionalidad) VALUES (@nombre, @apellido, @passwordHash, @DNI, @genero, @nacimiento, @nacionalidad)";
            connection.Execute(query, new
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
    public static Dictionary<string, object> DatosPerfil(string DNI)
    {
        using (SqlConnection connection = ObtenerConexion())
        {
            var query = @"SELECT nombre, apellido, dni, genero, nacimiento, nacionalidad, email, telefono FROM Avu WHERE dni = @DNI";
            var row = connection.QuerySingleOrDefault<dynamic>(query, new { DNI = DNI });
            if (row == null)
            {
                return new Dictionary<string, object>();
            }
            var dict = (IDictionary<string, object>)row;
            return dict.ToDictionary(k => k.Key, v => v.Value);
        }
    }

    public static void ActualizarEmail(string DNI, string email)
    {
        using (SqlConnection connection = ObtenerConexion())
        {
            var query = @"UPDATE Avu SET email = @email WHERE dni = @DNI";
            connection.Execute(query, new { email = email, DNI = DNI });
        }
    }

    public static void ActualizarTelefono(string DNI, string telefono)
    {
        using (SqlConnection connection = ObtenerConexion())
        {
            var query = @"UPDATE Avu SET telefono = @telefono WHERE dni = @DNI";
            connection.Execute(query, new { telefono = telefono, DNI = DNI });
        }
    }

    public static void EliminarEmail(string DNI)
    {
        using (SqlConnection connection = ObtenerConexion())
        {
            var query = @"UPDATE Avu SET email = NULL WHERE dni = @DNI";
            connection.Execute(query, new { DNI = DNI });
        }
    }

    public static void EliminarTelefono(string DNI)
    {
        using (SqlConnection connection = ObtenerConexion())
        {
            var query = @"UPDATE Avu SET telefono = NULL WHERE dni = @DNI";
            connection.Execute(query, new { DNI = DNI });
        }
    }

    public static int? ObtenerAvuIdPorDni(string dni)
    {
        using (SqlConnection connection = ObtenerConexion())
        {
            var query = @"SELECT avu_id FROM Avu WHERE dni = @dni";
            return connection.QuerySingleOrDefault<int?>(query, new { dni });
        }
    }

    public static List<Evento> ObtenerEventosDelMes(int avuId, int year, int month)
    {
        using (SqlConnection connection = ObtenerConexion())
        {
            var query = @"SELECT evento_id, avu_id, nombre, fecha, hora
                           FROM Evento
                           WHERE avu_id = @avuId AND YEAR(fecha) = @year AND MONTH(fecha) = @month";
            return connection.Query<Evento>(query, new { avuId, year, month }).ToList();
        }
    }

    public static List<Evento> ObtenerEventosDelDia(int avuId, DateTime fecha)
    {
        using (SqlConnection connection = ObtenerConexion())
        {
            var query = @"SELECT evento_id, avu_id, nombre, fecha, hora
                           FROM Evento
                           WHERE avu_id = @avuId AND fecha = @fecha";
            return connection.Query<Evento>(query, new { avuId, fecha = fecha.Date }).ToList();
        }
    }

    public static int CrearEvento(int avuId, string nombre, DateTime fecha, TimeSpan? hora)
    {
        using (SqlConnection connection = ObtenerConexion())
        {
            var query = @"INSERT INTO Evento (avu_id, nombre, fecha, hora)
                          VALUES (@avuId, @nombre, @fecha, @hora);
                          SELECT CAST(SCOPE_IDENTITY() as int);";
            return connection.QuerySingle<int>(query, new { avuId, nombre, fecha = fecha.Date, hora });
        }
    }

    public static void ActualizarEvento(int eventoId, string nombre, TimeSpan? hora)
    {
        using (SqlConnection connection = ObtenerConexion())
        {
            var query = @"UPDATE Evento SET nombre = @nombre, hora = @hora WHERE evento_id = @eventoId";
            connection.Execute(query, new { eventoId, nombre, hora });
        }
    }

    public static void EliminarEvento(int eventoId)
    {
        using (SqlConnection connection = ObtenerConexion())
        {
            var query = @"DELETE FROM Evento WHERE evento_id = @eventoId";
            connection.Execute(query, new { eventoId });
        }
    }
}