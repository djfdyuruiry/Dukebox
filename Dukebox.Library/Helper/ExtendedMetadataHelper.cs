using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TagLib;

namespace Dukebox.Library.Helper
{
    public static class ExtendedMetadataHelper
    {
        private static readonly Type audioTagType = typeof(Tag);
        private static readonly List<string> protectedTagProperties = new List<string>
        {
            "Title",
            "TitleSort",
            "Artists",
            "AlbumArtists",
            "AlbumArtistsSort",
            "Performers",
            "PerformersSort",
            "Album",
            "Pictures"
        };

        private static readonly List<PropertyInfo> readableProperties = audioTagType.GetProperties()
            .Where(p => !protectedTagProperties.Contains(p.Name) && p.CanRead)
            .ToList();
        private static readonly List<PropertyInfo> writableProperties = audioTagType.GetProperties()
            .Where(p => !protectedTagProperties.Contains(p.Name) && p.CanWrite)
            .ToList();

        public static readonly List<string> MetadataPropertyNames = readableProperties.Select(p => p.Name).ToList();

        public static Dictionary<string, List<string>> ReadExtendedMetadata(Tag tag)
        {
            var extendedMetadata = new Dictionary<string, List<string>>();

            foreach (var property in readableProperties)
            {
                var values = property.PropertyType.IsArray ?
                        ((object[])property.GetValue(tag))?.Select(o => o.ToString())?.ToList() ?? new List<string>()
                    : new List<string> { property.GetValue(tag)?.ToString() ?? string.Empty };

                var validValues = values.Where(s => !string.IsNullOrEmpty(s)).ToList();

                if (validValues.Any())
                {
                    extendedMetadata[property.Name] = validValues;
                }
            }

            return extendedMetadata;
        }

        public static void WriteExtendedMetadata(Tag tag, Dictionary<string, List<string>> extendedMetadata)
        {
            foreach (var property in writableProperties)
            {
                var propertyName = property.Name;

                if (!extendedMetadata.ContainsKey(propertyName) ||
                    !extendedMetadata[propertyName].Any())
                {
                    continue;
                }

                var extendedMetadataValue = extendedMetadata[propertyName];
                var propertyType = property.PropertyType;

                if (property.PropertyType.IsArray)
                {
                    var elementType = propertyType.GetElementType();
                    var values = extendedMetadataValue.Select(v => Convert.ChangeType(v, elementType)).ToArray();
                    var arrayValue = Array.CreateInstance(elementType, values.Length);

                    Array.Copy(values, arrayValue, values.Length);

                    property.SetValue(tag, arrayValue);
                }
                else
                {
                    var value = Convert.ChangeType(extendedMetadataValue.First(), propertyType);
                    property.SetValue(tag, value);
                }
            }
        }
    }
}
