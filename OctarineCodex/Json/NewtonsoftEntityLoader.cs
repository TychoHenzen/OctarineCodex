using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using LDtk;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;

public static class NewtonsoftEntityLoader
{
    public static T[] GetEntitiesWithNewtonsoft<T>(this LDtkLevel level)
        where T : new()
    {
        var entities = new List<T>();
        var targetTypeName = typeof(T).Name;

        // Access public LayerInstances property
        if (level.LayerInstances == null) return entities.ToArray();

        foreach (var layer in level.LayerInstances)
        {
            // Check if this is an entities layer
            if (layer._Identifier != "GameEntities" && layer._Identifier != "Triggerables") continue;

            // Access public EntityInstances property
            foreach (var entityInstance in layer.EntityInstances)
            {
                // Check if this entity matches our target type
                if (entityInstance._Identifier != targetTypeName) continue;

                try
                {
                    var entity = CreateEntityFromPublicApi<T>(entityInstance);
                    entities.Add(entity);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to create {targetTypeName}: {ex.Message}");
                }
            }
        }

        return entities.ToArray();
    }

    private static T CreateEntityFromPublicApi<T>(EntityInstance entityInstance)
        where T : new()
    {
        T entity = new();

        // Set base ILDtkEntity properties using public API
        SetProperty(entity, nameof(ILDtkEntity.Uid), entityInstance.DefUid);
        SetProperty(entity, nameof(ILDtkEntity.Iid), entityInstance.Iid);
        SetProperty(entity, nameof(ILDtkEntity.Identifier), entityInstance._Identifier);
        SetProperty(entity, nameof(ILDtkEntity.Position), new Vector2(entityInstance.Px.X, entityInstance.Px.Y));
        SetProperty(entity, nameof(ILDtkEntity.Pivot), entityInstance._Pivot);
        SetProperty(entity, nameof(ILDtkEntity.Size), new Vector2(entityInstance.Width, entityInstance.Height));
        SetProperty(entity, nameof(ILDtkEntity.SmartColor), entityInstance._SmartColor);

        // Handle tile if present
        if (entityInstance._Tile != null)
        {
            var tile = entityInstance._Tile;
            var rect = new Rectangle(tile.X, tile.Y, tile.W, tile.H);
            SetProperty(entity, nameof(ILDtkEntity.Tile), rect);
        }

        // Parse custom fields using Newtonsoft.Json
        ParseCustomFields(entity, entityInstance.FieldInstances);

        return entity;
    }

    private static void ParseCustomFields<T>(T entity, FieldInstance[] fieldInstances)
    {
        foreach (var field in fieldInstances)
        {
            var propertyInfo = typeof(T).GetProperty(field._Identifier);
            if (propertyInfo == null) continue;

            try
            {
                var fieldValue = ParseFieldValue(field, propertyInfo.PropertyType, entity.GetType().Namespace);
                if (fieldValue != null) propertyInfo.SetValue(entity, fieldValue);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to parse field {field._Identifier}: {ex.Message}");
            }
        }
    }

    private static object ParseFieldValue(FieldInstance field, Type targetType, string entityNamespace)
    {
        // Handle null values
        if (field._Value.ValueKind == JsonValueKind.Null) return null;

        // Convert JsonElement to string for Newtonsoft.Json
        var jsonString = field._Value.GetRawText();

        // Handle different field types
        if (field._Type.Contains("Array<") && field._Type.Contains("LocalEnum"))
        {
            // Handle enum arrays like ItemType[]
            var enumTypeName = ExtractEnumTypeName(field._Type);
            var enumType = FindEnumType(enumTypeName, entityNamespace);

            if (enumType != null)
            {
                var stringArray = JsonConvert.DeserializeObject<string[]>(jsonString);
                var enumArray = Array.CreateInstance(enumType, stringArray.Length);

                for (var i = 0; i < stringArray.Length; i++)
                {
                    var enumValue = Enum.Parse(enumType, stringArray[i]);
                    enumArray.SetValue(enumValue, i);
                }

                return enumArray;
            }
        }
        else if (field._Type.Contains("LocalEnum"))
        {
            // Handle single enums
            var enumTypeName = ExtractEnumTypeName(field._Type);
            var enumType = FindEnumType(enumTypeName, entityNamespace);

            if (enumType != null)
            {
                var enumString = JsonConvert.DeserializeObject<string>(jsonString);
                return Enum.Parse(enumType, enumString);
            }
        }
        else
        {
            // Handle primitive types and other arrays
            var underlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;
            return JsonConvert.DeserializeObject(jsonString, underlyingType);
        }

        return null;
    }

    private static string ExtractEnumTypeName(string fieldType)
    {
        // Extract "ItemType" from "LocalEnum.ItemType" or "Array<LocalEnum.ItemType>"
        var start = fieldType.IndexOf("LocalEnum.") + "LocalEnum.".Length;
        var end = fieldType.IndexOf('>', start);
        if (end == -1) end = fieldType.Length;
        return fieldType.Substring(start, end - start);
    }

    private static Type FindEnumType(string enumName, string entityNamespace)
    {
        return Assembly.GetExecutingAssembly().GetTypes()
            .FirstOrDefault(t => t.Name == enumName &&
                                 t.Namespace == entityNamespace &&
                                 t.IsEnum);
    }

    private static void SetProperty<T>(T entity, string propertyName, object value)
    {
        var propertyInfo = typeof(T).GetProperty(propertyName);
        propertyInfo?.SetValue(entity, value);
    }
}