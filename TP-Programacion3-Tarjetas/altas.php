<?php
include("conexion.php");
 
$tipo_doc = $_POST["tipo_doc"];
$documento = $_POST["documento"];
$usuario = $_POST["usuario"];
$passwordA = $_POST["passwordA"];
$passwordB = $_POST["passwordB"];
 
if ($passwordA != $passwordB) {
    die("<h3>Las contraseñas no coinciden.</h3>");
}
 
if ($tipo_doc != "DNI" && $tipo_doc != "PASAPORTE") {
    die("<h3>Tipo de documento inválido.</h3>");
}

$buscar = $conexion->prepare("
    SELECT * 
    FROM usuarios 
    WHERE documento = ? AND tipo_doc = ?
");

$buscar->bind_param("ss", $documento, $tipo_doc);
$buscar->execute();
$resultado = $buscar->get_result();
 
if ($resultado->num_rows == 0) {
    die("<h3>Error: el cliente no existe en el sistema administrativo.</h3>");
}
 
$actualizar = $conexion->prepare("
    UPDATE usuarios 
    SET usuario = ?, password = ? 
    WHERE documento = ? AND tipo_doc = ?
");
$actualizar->bind_param("ssss", $usuario, $passwordA, $documento, $tipo_doc);
 
if ($actualizar->execute()) {
    echo "<h3>Activación exitosa.</h3>";
    echo "<a href='ingreso.html'>Ir al ingreso</a>";
} else {
    echo "<h3>Error al activar la cuenta.</h3>";
}
 
$buscar->close();
$actualizar->close();
$conexion->close();
?>