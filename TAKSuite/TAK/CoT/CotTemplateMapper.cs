namespace TAKSuite.TAK.CoT
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;

    public class CotTemplateMapper
    {
        private readonly Dictionary<string, string> _data;

        // ✅ Costruttore con supporto a EXTRA multipli
        public CotTemplateMapper(
            Dictionary<string, object> baseData,
            params Dictionary<string, object>[] extraDictionaries)
        {
            _data = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            // 1. carico base
            AddDictionary(baseData);

            // 2. carico extra (override)
            if (extraDictionaries != null)
            {
                foreach (var extra in extraDictionaries)
                {
                    AddDictionary(extra);
                }
            }
        }

        // ✅ aggiunta + normalizzazione
        private void AddDictionary(Dictionary<string, object> source)
        {
            if (source == null)
                return;

            foreach (var kvp in source)
            {
                var key = kvp.Key.ToUpperInvariant();
                _data[key] = ConvertToString(kvp.Value); // override automatico
            }
        }

        // ✅ conversione robusta
        private string ConvertToString(object value)
        {
            if (value == null)
                return string.Empty;

            return value switch
            {
                DateTime dt => dt.ToString("yyyy-MM-ddTHH:mm:ss.00Z", CultureInfo.InvariantCulture),
                DateTimeOffset dto => dto.ToString("yyyy-MM-ddTHH:mm:ss.00Z", CultureInfo.InvariantCulture),
                bool b => b ? "true" : "false",
                IFormattable f => f.ToString(null, CultureInfo.InvariantCulture),
                _ => value.ToString()
            };
        }

        // ✅ apply unico
        public string ApplyTemplate(string template)
        {
            string result = template;

            foreach (var kvp in _data)
            {
                result = result.Replace($"{{{{{kvp.Key}}}}}", kvp.Value);
            }

            return result;
        }

        // ✅ getter safe opzionale
        public string Get(string key, string defaultValue = "")
        {
            return _data.TryGetValue(key.ToUpperInvariant(), out var value)
                ? value
                : defaultValue;
        }
    }
}
