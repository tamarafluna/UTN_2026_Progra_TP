<?php
session_start();


if (!isset($_SESSION["documento"])) {
    header("Location: ingreso.html");
    exit();
}

include("conexion.php");
$dni_usuario = $_SESSION["documento"];


$query_tarjeta = $conexion->prepare("SELECT * FROM tarjetas WHERE dni_titular = ?");
$query_tarjeta->bind_param("s", $dni_usuario);
$query_tarjeta->execute();
$res_tarjeta = $query_tarjeta->get_result();

$tarjeta = $res_tarjeta->fetch_assoc();
$num_cuenta = $tarjeta ? $tarjeta["num_cuenta"] : null;
?>

<!DOCTYPE html>
<html lang="es">
<head>
    <meta charset="UTF-8">
    <title>Mis Tarjetas - Resumen</title>
    <script src="https://cdn.tailwindcss.com"></script>
</head>
<body class="bg-gray-100 font-sans min-h-screen flex flex-col justify-between">

    <header class="bg-[#004691] text-white text-center py-4 shadow-md flex justify-between px-8 items-center">
        <h1 class="text-xl font-semibold">Mis <span class=\"font-bold\">Tarjetas</span></h1>
        <span class="text-sm">Bienvenido/a, <?php echo $_SESSION["nombre"] . " " . $_SESSION["apellido"]; ?></span>
    </header>

    <main class="flex-grow p-6 max-w-4xl mx-auto w-full">
        <div class="bg-white rounded-lg shadow p-6 mb-6">
            <h2 class="text-xl font-bold text-[#004691] mb-4">Información de tu Tarjeta</h2>
            <?php if ($tarjeta): ?>
                <p><strong>Banco:</strong> <?php echo $tarjeta["banco_emisor"]; ?></p>
                <p><strong>Número de Tarjeta:</strong> **** **** **** <?php echo substr($tarjeta["numero_tarjeta"], -4); ?></p>
                <p><strong>Estado:</strong> <span class="text-green-600 font-bold"><?php echo $tarjeta["estado"]; ?></span></p>
                <p><strong>Saldo Actual:</strong> $<?php echo number_format($tarjeta["saldo"], 2, ',', '.'); ?></p>
            <?php else: ?>
                <p class="text-red-500">No tenés ninguna tarjeta registrada a tu nombre todavía.</p>
            <?php endif; ?>
        </div>

        <div class="bg-white rounded-lg shadow p-6">
            <h2 class="text-xl font-bold text-[#004691] mb-4">Historial de Liquidaciones</h2>
            

            <?php 

                if ($num_cuenta) {
                
                    $query_actual = $conexion->prepare("

                        SELECT * 
                        FROM liquidaciones 
                        WHERE num_cuenta = ? 
                        ORDER BY periodo DESC 
                        LIMIT 1

                    ");

                    $query_actual->bind_param("i", $num_cuenta);
                    $query_actual->execute();
                    $res_actual = $query_actual->get_result();
                
                    if ($res_actual->num_rows > 0) {
                        $actual = $res_actual->fetch_assoc();
                        echo '<div class="bg-blue-50 border border-blue-200 rounded p-4 mb-6">';
                        echo '<h3 class="text-lg font-bold text-[#004691] mb-2">Liquidación Actual</h3>';
                        echo '<p><strong>Período:</strong> ' . $actual["periodo"] . '</p>';
                        echo '<p><strong>Vencimiento:</strong> ' . date("d/m/Y", strtotime($actual["fecha_vencimiento"])) . '</p>';
                        echo '<p><strong>Total a pagar:</strong> $' . number_format($actual["total_a_pagar"], 2, ',', '.') . '</p>';
                        echo '<p><strong>Pago mínimo:</strong> $' . number_format($actual["pago_minimo"], 2, ',', '.') . '</p>'
                        echo '</div>';
                
                        $periodo_actual = $actual["periodo"];
                        $query_historial = $conexion->prepare("
                            SELECT *
                            FROM liquidaciones 
                            WHERE num_cuenta = ? 
                            AND periodo <> ?
                            ORDER BY periodo DESC
                        ");

                        $query_historial->bind_param("is", $num_cuenta, $periodo_actual);
                        $query_historial->execute();
                        $res_historial = $query_historial->get_result();
                
                        echo '<h3 class="text-lg font-bold text-[#004691] mb-3">Historial de Liquidaciones</h3>';
                
                        if ($res_historial->num_rows > 0) {

                            echo '<table class="w-full text-left border-collapse">';
                            echo '<thead><tr class="border-b text-gray-600 font-semibold text-sm">';
                            echo '<th class="py-2">Período</th>';
                            echo '<th class="py-2">Vencimiento</th>';
                            echo '<th class="py-2">Total a Pagar</th>';
                            echo '<th class="py-2">Pago Mínimo</th>';
                            echo '</tr></thead>';
                            echo '<tbody>';
                
                            while ($liq = $res_historial->fetch_assoc()) {
                                echo '<tr class="border-b text-sm">';
                                echo '<td class="py-3">' . $liq["periodo"] . '</td>';
                                echo '<td class="py-3">' . date("d/m/Y", strtotime($liq["fecha_vencimiento"])) . '</td>';
                                echo '<td class="py-3 font-semibold">$' . number_format($liq["total_a_pagar"], 2, ',', '.') . '</td>';
                                echo '<td class="py-3">$' . number_format($liq["pago_minimo"], 2, ',', '.') . '</td>';
                                echo '</tr>';
                            }
                            echo '</tbody></table>';
                        } else {
                            echo '<p class="text-gray-500">No hay liquidaciones anteriores.</p>';
                        }     
                          $query_historial->close();
                    } else {
                        echo '<p class="text-gray-500">No registrás liquidaciones disponibles.</p>';
                    }         
                    $query_actual->close();            
                } else {
                     echo '<p class="text-gray-500">Sin datos de liquidación disponibles.</p>';
                }
                ?>
            
        </div>

        <div class="mt-6 text-center">
            <a href="ingreso.html" class="text-sm text-red-600 hover:underline font-semibold">Cerrar sesión segura</a>
        </div>
    </main>

    <footer class="bg-gray-50 text-[10px] text-gray-500 text-center p-4 border-t border-gray-200">
        &copy; 2026 Sistema Mis Tarjetas - UTN FRH
    </footer>

</body>
</html>
<?php 
$query_tarjeta->close();
$conexion->close();
?>