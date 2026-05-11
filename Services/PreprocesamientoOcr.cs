namespace FacturasApp.Models
{
    public class PreprocesamientoOcr
    {
        public bool EscalaGrises { get; set; } = true;
        public bool Binarizacion { get; set; } = false;
        public int UmbralBinarizacion { get; set; } = 128;
        public bool Redimensionar { get; set; } = false;
        public double FactorEscala { get; set; } = 2.0;
        public bool EliminarRuido { get; set; } = true;
        public bool InvertirColores { get; set; } = false;
        public bool AutoRecortar { get; set; } = false;
        public int PaddingAutoRecorte { get; set; } = 5;
    }
}