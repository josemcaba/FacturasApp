namespace FacturasApp.Core.Models
{
    /// <summary>
    /// Resultado de una consulta al servicio VIES (VAT Information Exchange
    /// System) de la Comisión Europea para un NIF-IVA comunitario.
    /// </summary>
    public class ViesResultado
    {
        /// <summary>
        /// true = VIES contestó y <see cref="Valido"/> es fiable. false = no hubo
        /// respuesta del servicio (estado miembro caído, timeout, sin conexión):
        /// en ese caso no se puede concluir nada sobre el NIF.
        /// </summary>
        public bool Disponible { get; set; }

        /// <summary>true = el NIF-IVA está registrado como válido para operaciones
        /// intracomunitarias. Sólo es significativo con <see cref="Disponible"/>.</summary>
        public bool Valido { get; set; }

        /// <summary>Nombre del titular según el registro nacional. Puede venir
        /// vacío: algunos estados miembros no lo publican por protección de datos.</summary>
        public string? Nombre { get; set; }

        /// <summary>Dirección del titular; igual que el nombre, puede venir vacía.</summary>
        public string? Direccion { get; set; }

        /// <summary>Momento en que se realizó la consulta (hora local del cliente).</summary>
        public DateTime FechaConsulta { get; set; }

        /// <summary>Código del fallo cuando hubo problema (SOAP Fault de VIES como
        /// MS_UNAVAILABLE o TIMEOUT, o códigos propios como SIN_CONEXION).
        /// null cuando la consulta se completó.</summary>
        public string? CodigoError { get; set; }

        /// <summary>Estado persistible en el XML del emisor:
        /// "Valido", "Invalido" o "NoDisponible".</summary>
        public string Estado => Disponible ? (Valido ? "Valido" : "Invalido") : "NoDisponible";

        /// <summary>Explicación breve en español de <see cref="CodigoError"/>
        /// para mostrar en la GUI.</summary>
        public string? MensajeError => CodigoError switch
        {
            null => null,
            "INVALID_INPUT" => "VIES rechazó el formato del NIF-IVA enviado.",
            "MS_UNAVAILABLE" => "El registro del estado miembro no responde.",
            "MS_MAX_CONCURRENT_REQ" => "El registro del estado miembro está saturado.",
            "GLOBAL_MAX_CONCURRENT_REQ" => "VIES está saturado por demasiadas consultas.",
            "SERVICE_UNAVAILABLE" => "El servicio VIES no está disponible en este momento.",
            "TIMEOUT" => "El servicio no respondió dentro del tiempo de espera.",
            "SIN_CONEXION" => "No hay conexión a Internet.",
            "RESPUESTA_INVALIDA" => "VIES devolvió una respuesta que no se pudo interpretar.",
            "FORMATO_NO_VALIDO" => "El NIF no tiene formato de NIF-IVA (letras y dígitos, 2-12 caracteres).",
            _ => $"Error del servicio VIES ({CodigoError})."
        };
    }
}
