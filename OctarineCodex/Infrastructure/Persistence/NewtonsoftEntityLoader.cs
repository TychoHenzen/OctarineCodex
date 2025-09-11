using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using LDtk;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using OctarineCodex.Infrastructure.Logging;

namespace OctarineCodex.Infrastructure.Persistence;

public static class NewtonsoftEntityLoader
{
    public static T[] GetEntitiesWithNewtonsoft<T>(this LDtkLevel level, ILoggingService logger)
        where T : new()
    {
        var entities = new List<T>();
        var targetTypeName = typeof(T).Name;

        if (level.LayerInstances == null)
        {
            return [.. entities];
        }

        foreach (var layer in level.LayerInstances)
        {
            foreach (var entityInstance in layer.EntityInstances)
            {
                if (entityInstance._Identifier != targetTypeName)
                {
                    continue;
                }

                try
                {
                    // Pass level context for world coordinate conversion
                    var entity = CreateEntityFromPublicApi<T>(entityInstance, level, logger);
                    entities.Add(entity);
                }
                catch (Exception ex)
                {
                    logger.Exception(ex, $"Failed to create {targetTypeName}");
                }
            }
        }

        return entities.ToArray();
    }

    private static T CreateEntityFromPublicApi<T>(EntityInstance entityInstance, LDtkLevel level,
        ILoggingService logger)
        where T : new()
    {
        T entity = new();

        // Convert to world coordinates by adding level offset
        var worldPosition = new Vector2(
            level.WorldX + entityInstance.Px.X,
            level.WorldY + entityInstance.Px.Y
        );

        SetProperty(entity, nameof(ILDtkEntity.Uid), entityInstance.DefUid);
        SetProperty(entity, nameof(ILDtkEntity.Iid), entityInstance.Iid);
        SetProperty(entity, nameof(ILDtkEntity.Identifier), entityInstance._Identifier);
        SetProperty(entity, nameof(ILDtkEntity.Position), worldPosition); // Now uses world coordinates
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

        ParseCustomFields(entity, entityInstance.FieldInstances, logger);
        return entity;
    }

    private static void ParseCustomFields<T>(T entity, FieldInstance[] fieldInstances, ILoggingService logger)
    {
        foreach (var field in fieldInstances)
        {
            var propertyInfo = typeof(T).GetProperty(field._Identifier);
            if (propertyInfo == null)
            {
                continue;
            }

            try
            {
                var fieldValue = ParseFieldValue(field, propertyInfo.PropertyType, entity.GetType().Namespace);
                if (fieldValue != null)
                {
                    propertyInfo.SetValue(entity, fieldValue);
                }
            }
            catch (Exception ex)
            {
                logger.Exception(ex, $"Failed to parse field {field._Identifier}");
            }
        }
    }

    private static object ParseFieldValue(FieldInstance field, Type targetType, string entityNamespace)
    {
        // Handle null values
        if (field._Value.ValueKind == JsonValueKind.Null)
        {
            return null;
        }

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
        if (end == -1)
        {
            end = fieldType.Length;
        }

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
