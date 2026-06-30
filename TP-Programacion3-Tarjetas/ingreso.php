<?php
session_start();
include("conexion.php");

$usuario = $_POST["usuario"];
$password = $_POST["password"];

$consulta = $conexion->prepare("SELECT * FROM usuarios WHERE usuario = ? AND password = ?");
$consulta->bind_param("ss", $usuario, $password);
$consulta->execute();
$resultado = $consulta->get_result();

if ($resultado->num_rows == 1) {

    $datosUsuario = $resultado->fetch_assoc();
    $_SESSION["documento"] = $datosUsuario["documento"];
    $_SESSION["nombre"] = $datosUsuario["nombre"];
    $_SESSION["apellido"] = $datosUsuario["apellido"];

 
    header("Location: resumen.php");
    exit();
} else {

    die("<h3>Usuario o contraseña incorrectos. Volvé a intentarlo.</h3>");
}

$consulta->close();
$conexion->close();
?>