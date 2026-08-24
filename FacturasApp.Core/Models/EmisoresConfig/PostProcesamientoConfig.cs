using System.Xml.Serialization;

namespace FacturasApp.Core.Models.EmisoresConfig;

public class PostProcesamientoConfig
{
    public static string NormalizarTipo(string tipo) =>
        (tipo ?? string.Empty).Replace(" ", "").Replace("_", "").ToLowerInvariant();

    public string? CondicionTextoContiene { get; set; }

    public CondicionCampoPostProcesamiento? CondicionCampo { get; set; }

    public AccionPostProcesamiento? Accion { get; set; }

    public override string ToString()
    {
        string cond = string.Empty;
        if (!string.IsNullOrEmpty(CondicionTextoContiene))
            cond = $"Texto \"{CondicionTextoContiene}\" → ";
        else if (CondicionCampo is { } cc && !string.IsNullOrEmpty(cc.Campo))
            cond = $"{cc.Campo} == \"{cc.Valor}\" → ";

        var accion = Accion;
        if (accion == null || string.IsNullOrEmpty(accion.Tipo))
            return cond + "(sin acción)";

        return NormalizarTipo(accion.Tipo) switch
        {
            "invertirsigno" => $"{cond}Invertir signo de los importes",
            "establecervalor" => $"{cond}{accion.CampoDestino} = {accion.Valor}",
            "calcular" => $"{cond}{accion.CampoDestino} = {accion.CampoOrigen1} {accion.Operador} {accion.CampoOrigen2}",
            _ => $"{cond}{accion.Tipo}"
        };
    }
}

public class CondicionCampoPostProcesamiento
{
    public string Campo { get; set; } = string.Empty;

    public string Valor { get; set; } = string.Empty;
}

public class AccionPostProcesamiento
{
    [XmlAttribute("Tipo")]
    public string Tipo { get; set; } = string.Empty;

    [XmlAttribute("CampoDestino")]
    public string CampoDestino { get; set; } = string.Empty;

    [XmlAttribute("Valor")]
    public string Valor { get; set; } = string.Empty;

    [XmlAttribute("CampoOrigen1")]
    public string CampoOrigen1 { get; set; } = string.Empty;

    [XmlAttribute("Operador")]
    public string Operador { get; set; } = "+";

    [XmlAttribute("CampoOrigen2")]
    public string CampoOrigen2 { get; set; } = string.Empty;
}
