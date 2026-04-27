using System.Text.RegularExpressions;

namespace FacturasApp.Services
{
    public class NifValidator
    {
        // Patrones regex para cada tipo de documento
        private const string DniPattern = @"^\d{8}[A-HJ-NP-TV-Z]$";
        private const string NiePattern = @"^[XYZ]\d{7}[A-HJ-NP-TV-Z]$";
        private const string CifPattern = @"^[ABCDEFGHJKLMNPQRSUVW]\d{7}[0-9A-J]$";

        // Letras válidas para DNI
        private const string LetrasValidacionDni = "TRWAGMYFPDXBNJZSQVHLCKE";

        /// <summary>
        /// Valida un NIF (DNI, NIE o CIF) según las normas españolas.
        /// </summary>
        public static bool ValidarNif(string nif)
        {
            if (string.IsNullOrEmpty(nif))
                return false;

            nif = nif.Trim().ToUpper();

            if (Regex.IsMatch(nif, DniPattern))
                return ValidarDni(nif);
            else if (Regex.IsMatch(nif, NiePattern))
                return ValidarNie(nif);
            else if (Regex.IsMatch(nif, CifPattern))
                return ValidarCif(nif);
            else
                return false;
        }

        /// <summary>
        /// Valida un DNI español.
        /// </summary>
        private static bool ValidarDni(string dni)
        {
            if (!Regex.IsMatch(dni, DniPattern))
                return false;

            try
            {
                int numero = int.Parse(dni[..8]); // Primeros 8 dígitos
                char letraCalculada = LetrasValidacionDni[numero % 23];
                return dni[^1] == letraCalculada; // Último carácter
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Valida un NIE español.
        /// </summary>
        private static bool ValidarNie(string nie)
        {
            if (!Regex.IsMatch(nie, NiePattern))
                return false;

            // Convertir la primera letra a número
            string nieNormalizado = nie[0] switch
            {
                'X' => "0" + nie[1..],
                'Y' => "1" + nie[1..],
                'Z' => "2" + nie[1..],
                _ => nie
            };

            return ValidarDni(nieNormalizado);
        }

        /// <summary>
        /// Valida un CIF español.
        /// </summary>
        private static bool ValidarCif(string cif)
        {
            if (!Regex.IsMatch(cif, CifPattern))
                return false;

            try
            {
                char letra = cif[0];
                string numero = cif[1..^1]; // Números centrales (sin letra inicial ni dígito final)
                char digitoControl = cif[^1]; // Último carácter

                // Calcular suma de dígitos en posiciones pares
                int sumaPares = 0;
                for (int i = 1; i < numero.Length; i += 2)
                    sumaPares += int.Parse(numero[i].ToString());

                // Calcular suma de dígitos en posiciones impares (multiplicados por 2)
                int sumaImpares = 0;
                for (int i = 0; i < numero.Length; i += 2)
                {
                    int digito = int.Parse(numero[i].ToString()) * 2;
                    sumaImpares += (digito / 10) + (digito % 10);
                }

                int total = sumaPares + sumaImpares;
                int digitoCalculado = (10 - (total % 10)) % 10;

                // Validación según el tipo de letra
                if (letra is 'A' or 'B' or 'E' or 'H')
                    return digitoControl == digitoCalculado.ToString()[0];
                else if (letra is 'K' or 'P' or 'Q' or 'S')
                    return digitoControl == "JABCDEFGHI"[digitoCalculado];
                else
                    return digitoControl == digitoCalculado.ToString()[0] || 
                           digitoControl == "JABCDEFGHI"[digitoCalculado];
            }
            catch
            {
                return false;
            }
        }
    }
}