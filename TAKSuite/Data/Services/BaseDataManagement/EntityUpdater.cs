using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

namespace TAKSuite.Data.Services.BaseDataManagement
{
    public static class EntityUpdater
    {
        private static readonly HashSet<string> BlacklistedProperties = new()
        {
            "Id"  // Evitiamo di aggiornare gli ID
        };

        public static void UpdateEntity<T>(T target, T source) where T : class
        {
            if (target == null || source == null)
                throw new ArgumentNullException("Target e Source non possono essere null");

            // Otteniamo tutte le proprietà pubbliche della classe
            PropertyInfo[] properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var property in properties)
            {
                // Se la proprietà è nella blacklist, la saltiamo
                if (BlacklistedProperties.Contains(property.Name))
                    continue;

                // Se la proprietà ha l'attributo [NotUpdateField], la saltiamo
                if (Attribute.IsDefined(property, typeof(NotUpdateFieldAttribute)))
                    continue;

                // Escludiamo le proprietà di navigazione (classi e liste)
                if (!property.PropertyType.IsPrimitive &&
                    property.PropertyType != typeof(string) &&
                    property.PropertyType != typeof(decimal) &&
                    property.PropertyType != typeof(DateTime) &&
                    !property.PropertyType.IsValueType)
                {
                    continue;
                }

                // Verifica che la proprietà sia scrivibile (alcune potrebbero essere readonly)
                if (!property.CanWrite)
                    continue;

                // Aggiorniamo la proprietà solo se diversa (incluso il caso null → valore cancellato)
                var sourceValue = property.GetValue(source);
                var targetValue = property.GetValue(target);

                if (!Equals(sourceValue, targetValue))
                {
                    property.SetValue(target, sourceValue);
                }
            }
        }
    }

}
