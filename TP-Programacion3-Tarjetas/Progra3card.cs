using System;
using MySql.Data.MySqlClient; 

namespace Progra3Card.Administrativo
{
    class Program
    {
        private static string connectionString = "Server=localhost;Database=mi_banco_db;Uid=root;Pwd=;";

        static void Main(string[] args)
        {
            bool salir = false;
            while (!salir)
            {
                Console.Clear();
                Console.WriteLine("========================================");
                Console.WriteLine("    SISTEMA ADMINISTRATIVO PROGRA3CARD   ");
                Console.WriteLine("========================================");
                Console.WriteLine("1. Emitir Nueva Tarjeta (Alta de Cliente)");
                Console.WriteLine("2. Listar Tarjetas");
                Console.WriteLine("3. Ver Detalle de una Tarjeta / Cliente");
                Console.WriteLine("4. Eliminar Tarjeta (Baja de Sistema)");
                Console.WriteLine("5. Emitir Nueva Liquidación Mensual");
                Console.WriteLine("6. Salir");
                Console.WriteLine("========================================");
                Console.Write("Seleccione una opción: ");

                switch (Console.ReadLine())
                {
                    case "1": MenuEmitirTarjeta(); break;
                    case "2": MenuListarTarjetas(); break;
                    case "3": MenuVerDetalleTarjeta(); break;
                    case "4": MenuEliminarTarjeta(); break;
                    case "5": MenuEmitirLiquidacion(); break;
                    case "6": salir = true; break;
                    default:
                        Console.WriteLine("Opción no válida. Presione una tecla para continuar...");
                        Console.ReadKey();
                        break;
                }
            }
        }

        // Funciones a completar:
        static void MenuListarTarjetas()
        {
            Console.Clear();
            Console.WriteLine("--- LISTADO GENERAL DE TARJETAS ---");
            Console.WriteLine("{0,-12} {1,-18} {2,-20} {3,-15}", "Nro Cuenta", "Nro Tarjeta", "Banco Emisor", "DNI Titular");
            Console.WriteLine("----------------------------------------------------------------------");

            // === A realizar ===
            // Aquí deben implementar un SELECT sobre la tabla 'tarjetas'
            // para recorrer las filas e imprimirlas en la consola.
            
            ObtenerYMostrarTarjetas();

            Console.WriteLine("\nPresione una tecla para volver al menú...");
            Console.ReadKey();
        }

        static void MenuVerDetalleTarjeta()
        {
            Console.Clear();
            Console.WriteLine("--- DETALLE DE TARJETA Y CLIENTE ---");
            Console.Write("Ingrese el Número de Cuenta a consultar: ");
            int numCuenta = Convert.ToInt32(Console.ReadLine());

            // === A realizar ===
            // Aquí deben realizar un SELECT con un JOIN entre 'tarjetas' y 'usuarios' 
            // filtrando por el numCuenta para traer todos los campos (Nombre, Apellido, Email, Saldo, etc.)
            
            MostrarDetalleCompleto(numCuenta);

            Console.WriteLine("\nPresione una tecla para volver al menú...");
            Console.ReadKey();
        }

        static void MenuEliminarTarjeta()
        {
            Console.Clear();
            Console.WriteLine("--- ELIMINAR TARJETA DEL SISTEMA ---");
            Console.Write("Ingrese el Número de Cuenta de la tarjeta a dar de baja: ");
            int numCuenta = Convert.ToInt32(Console.ReadLine());

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("\n⚠️ ADVERTENCIA: Se eliminará la tarjeta, sus liquidaciones y los datos de acceso web vinculados.");
            Console.ResetColor();
            Console.Write("¿Está seguro de continuar? (S/N): ");
            
            if (Console.ReadLine().ToUpper() == "S")
            {
                // === A realizar ===
                // Aquí deben ejecutar un DELETE sobre la tabla 'tarjetas' donde num_cuenta = numCuenta.
                // Como definimos ON DELETE CASCADE en la base de datos, las liquidaciones se borrarán solas.
                // Opcional: Evaluar si también eliminan al usuario de la tabla 'usuarios' o si lo mantienen.
                
                bool exito = DarDeBajaTarjeta(numCuenta);

                if (exito)
                    Console.WriteLine("\nTarjeta eliminada correctamente del sistema.");
                else
                    Console.WriteLine("\nError al intentar eliminar la tarjeta. Verifique el número de cuenta.");
            }
            else
            {
                Console.WriteLine("\nOperación cancelada.");
            }

            Console.WriteLine("\nPresione una tecla para volver al menú...");
            Console.ReadKey();
        }


        // =========================================================================
        // MÉTODOS COMPLETOS CON LA LÓGICA DE BASE DE DATOS
        // =========================================================================

        static void MenuEmitirTarjeta()
        {
            Console.Clear();
            Console.WriteLine("--- EMITIR NUEVA TARJETA (ALTA DE CLIENTE) ---");
            
            // Pedimos los datos del Usuario
            Console.Write("Ingrese el DNI/Documento del cliente: ");
            string documento = Console.ReadLine();
            
            Console.Write("Seleccione Tipo de Doc (1. DNI / 2. PASAPORTE): ");
            string tipoDoc = Console.ReadLine() == "2" ? "PASAPORTE" : "DNI";

            Console.Write("Nombre: ");
            string nombre = Console.ReadLine();

            Console.Write("Apellido: ");
            string apellido = Console.ReadLine();

            Console.Write("Fecha de Nacimiento (YYYY-MM-DD): ");
            string fechaNac = Console.ReadLine();

            Console.Write("Email: ");
            string email = Console.ReadLine();

            // Pedimos los datos de la Tarjeta
            Console.Write("Número de Tarjeta (16 dígitos): ");
            string nroTarjeta = Console.ReadLine();

            if (nroTarjeta.Length != 16)
            {
                Console.WriteLine("\n⚠️  Error: El número de tarjeta debe tener exactamente 16 dígitos.");
                Console.WriteLine("Operación cancelada. Presione una tecla para volver al menú...");
                Console.ReadKey();
                return; 
            }

            Console.WriteLine("Seleccione el Banco Emisor:");
            Console.WriteLine("1. Banco Nación\n2. Banco Provincia\n3. Banco Galicia\n4. Banco Santander\n5. Banco BBVA\n6. Banco Macro");
            Console.Write("Opción: ");
            string[] bancos = { "Banco Nación", "Banco Provincia", "Banco Galicia", "Banco Santander", "Banco BBVA", "Banco Macro" };
            int opcionBanco = Convert.ToInt32(Console.ReadLine()) - 1;
            
            if (opcionBanco < 0 || opcionBanco >= bancos.Length)
            {
                Console.WriteLine("\nOpción de banco inválida. Operación cancelada.");
                Console.WriteLine("Presione una tecla para volver al menú...");
                Console.ReadKey();
                return;
            }
 
string bancoEmisor = bancos[opcionBanco];

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    // Usamos una transacción porque insertamos en dos tablas unidas
                    using (MySqlTransaction trans = conn.BeginTransaction())
                    {
                        try
                        {
                            // 1. Insertamos el usuario base (usuario y password arrancan en NULL para la web)
                            string queryUser = "INSERT INTO usuarios (documento, tipo_doc, nombre, apellido, fecha_nacimiento, email, usuario, password) VALUES (@doc, @tipo, @nom, @ape, @fecha, @email, NULL, NULL)";
                            MySqlCommand cmdUser = new MySqlCommand(queryUser, conn, trans);
                            cmdUser.Parameters.AddWithValue("@doc", documento);
                            cmdUser.Parameters.AddWithValue("@tipo", tipoDoc);
                            cmdUser.Parameters.AddWithValue("@nom", nombre);
                            cmdUser.Parameters.AddWithValue("@ape", apellido);
                            cmdUser.Parameters.AddWithValue("@fecha", fechaNac);
                            cmdUser.Parameters.AddWithValue("@email", email);
                            cmdUser.ExecuteNonQuery();

                            // 2. Insertamos el plástico de la tarjeta asociado a ese DNI
                            string queryCard = "INSERT INTO tarjetas (numero_tarjeta, banco_emisor, estado, saldo, dni_titular) VALUES (@nro, @banco, 'Activa', 0.00, @doc)";
                            MySqlCommand cmdCard = new MySqlCommand(queryCard, conn, trans);
                            cmdCard.Parameters.AddWithValue("@nro", nroTarjeta);
                            cmdCard.Parameters.AddWithValue("@banco", bancoEmisor);
                            cmdCard.Parameters.AddWithValue("@doc", documento);
                            cmdCard.ExecuteNonQuery();

                            trans.Commit();
                            Console.WriteLine("\n¡Cliente y Tarjeta creados con éxito en el sistema!");
                        }
                        catch (Exception ex)
                        {
                            trans.Rollback();
                            throw;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("\nError al emitir la tarjeta: " + ex.Message);
                }
            }

            Console.WriteLine("\nPresione una tecla para volver al menú...");
            Console.ReadKey();
        }

        static void MenuEmitirLiquidacion()
{
    Console.Clear();
    Console.WriteLine("--- EMITIR NUEVA LIQUIDACIÓN MENSUAL ---");

    Console.Write("Ingrese el Número de Cuenta del cliente: ");
    int numCuenta = Convert.ToInt32(Console.ReadLine());

    using (MySqlConnection conn = new MySqlConnection(connectionString))
    {
        try
        {
            conn.Open();

            // Primero verificamos que la cuenta exista
            string verificar = "SELECT COUNT(*) FROM tarjetas WHERE num_cuenta=@num";

            MySqlCommand cmdVerificar = new MySqlCommand(verificar, conn);
            cmdVerificar.Parameters.AddWithValue("@num", numCuenta);

            int existe = Convert.ToInt32(cmdVerificar.ExecuteScalar());

            if (existe == 0)
            {
                Console.WriteLine("\nNo existe una tarjeta con ese número de cuenta.");
                Console.WriteLine("\nPresione una tecla para volver...");
                Console.ReadKey();
                return;
            }

            Console.Write("Ingrese el Período (YYYY-MM): ");
            string periodo = Console.ReadLine();

            Console.Write("Fecha de Vencimiento (YYYY-MM-DD): ");
            string vencimiento = Console.ReadLine();

            Console.Write("Monto Total a Pagar: ");
            decimal total = Convert.ToDecimal(Console.ReadLine());

            Console.Write("Monto Pago Mínimo: ");
            decimal minimo = Convert.ToDecimal(Console.ReadLine());

            string insertar =
                "INSERT INTO liquidaciones " +
                "(num_cuenta, periodo, fecha_vencimiento, total_a_pagar, pago_minimo) " +
                "VALUES (@num,@per,@ven,@tot,@min)";

            MySqlCommand cmd = new MySqlCommand(insertar, conn);

            cmd.Parameters.AddWithValue("@num", numCuenta);
            cmd.Parameters.AddWithValue("@per", periodo);
            cmd.Parameters.AddWithValue("@ven", vencimiento);
            cmd.Parameters.AddWithValue("@tot", total);
            cmd.Parameters.AddWithValue("@min", minimo);

            cmd.ExecuteNonQuery();

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\nLiquidación creada correctamente.");
            Console.ResetColor();
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("\nError: " + ex.Message);
            Console.ResetColor();
        }
    }

    Console.WriteLine("\nPresione una tecla para volver...");
    Console.ReadKey();
}

        static void ObtenerYMostrarTarjetas()
        {
            string query = "SELECT num_cuenta, numero_tarjeta, banco_emisor, dni_titular FROM tarjetas";

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        Console.WriteLine("\n{0,-12} {1,-18} {2,-20} {3,-15}", "Nro Cuenta", "Nro Tarjeta", "Banco Emisor", "DNI Titular");
                        Console.WriteLine("-------------------------------------------------------------------------");
                        while (reader.Read())
                        {
                            Console.WriteLine("{0,-12} {1,-18} {2,-20} {3,-15}", 
                                reader["num_cuenta"], 
                                reader["numero_tarjeta"], 
                                reader["banco_emisor"], 
                                reader["dni_titular"]);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error al leer la base de datos: " + ex.Message);
                }
            }
        }

        static void MostrarDetalleCompleto(int cuenta)
        {
            string query = "SELECT u.nombre, u.apellido, u.email, t.numero_tarjeta, t.banco_emisor, t.estado, t.saldo " +
                        "FROM tarjetas t INNER JOIN usuarios u ON t.dni_titular = u.documento WHERE t.num_cuenta = @cuenta";

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@cuenta", cuenta);

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            Console.WriteLine($"\nTitular: {reader["nombre"]} {reader["apellido"]}");
                            Console.WriteLine($"Email: {reader["email"]}");
                            Console.WriteLine($"Tarjeta: {reader["numero_tarjeta"]} ({reader["banco_emisor"]})");
                            Console.WriteLine($"Estado: {reader["estado"]} | Saldo: ${reader["saldo"]}");
                        }
                        else
                        {
                            Console.WriteLine("No se encontró la cuenta especificada.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error al obtener detalles: " + ex.Message);
                }
            }
        }

        static bool DarDeBajaTarjeta(int cuenta)
        {
            string query = "DELETE FROM tarjetas WHERE num_cuenta = @cuenta";

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@cuenta", cuenta);

                    int filas = cmd.ExecuteNonQuery();
                    return filas > 0;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error al eliminar: " + ex.Message);
                    return false;
                }
            }
        }
    }
}