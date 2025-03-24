namespace TAKSuite.Helper
{
    public class ColorConverterHelper
    {
        public static int HexToArgb(string hex)
        {
            // Rimuove il carattere '#' se presente
            if (hex.StartsWith("#"))
            {
                hex = hex.Substring(1);
            }

            // Assumiamo che il colore sia nel formato "RRGGBB"
            if (hex.Length == 6)
            {
                int r = Convert.ToInt32(hex.Substring(0, 2), 16);
                int g = Convert.ToInt32(hex.Substring(2, 2), 16);
                int b = Convert.ToInt32(hex.Substring(4, 2), 16);

                return (255 << 24) | (r << 16) | (g << 8) | b; // 255 è il valore di default per l'alpha
            }
            else if (hex.Length == 8) // Se include anche l'alpha (AARRGGBB)
            {
                int a = Convert.ToInt32(hex.Substring(0, 2), 16);
                int r = Convert.ToInt32(hex.Substring(2, 2), 16);
                int g = Convert.ToInt32(hex.Substring(4, 2), 16);
                int b = Convert.ToInt32(hex.Substring(6, 2), 16);

                return (a << 24) | (r << 16) | (g << 8) | b;
            }
            else
            {
                throw new ArgumentException("Formato colore non valido");
            }
        }

        public static string ArgbToHex(int argb)
        {
            int a = (argb >> 24) & 0xFF;
            int r = (argb >> 16) & 0xFF;
            int g = (argb >> 8) & 0xFF;
            int b = argb & 0xFF;

            return $"#{r:X2}{g:X2}{b:X2}"; // Ignora il canale alpha per la compatibilità CSS
        }
    }
}
