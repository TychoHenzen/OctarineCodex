// OctarineCodex/Domain/Magic/MagicVector8.cs

using System;
using System.Runtime.CompilerServices;

namespace OctarineCodex.Domain.Magic;

/// <summary>
///     An 8-dimensional vector representing magical properties in OctarineCodex.
///     Each dimension corresponds to one of the 8 base Elements from EleAspects.
///     Vector operations are optimized for high-frequency magical calculations.
/// </summary>
public readonly struct MagicSignature : IEquatable<MagicSignature>
{
    /// <summary>
    ///     The 8 dimensional values corresponding to:
    ///     [0] Solidum (earth-air axis)
    ///     [1] Febris (fire-water axis)
    ///     [2] Ordinem (order-chaos axis)
    ///     [3] Lumines (light-dark axis)
    ///     [4] Varias (space-time axis)
    ///     [5] Inertiae (heavy-light axis)
    ///     [6] Subsidium (helpful-harmful axis)
    ///     [7] Spatium (distant-nearby axis)
    /// </summary>
    private readonly float[] _values;

    /// <summary>
    ///     Gets the zero vector (all dimensions = 0).
    /// </summary>
    public static MagicSignature Zero => new(0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f);

    /// <summary>
    ///     Gets the unit vector (all dimensions = 1).
    /// </summary>
    public static MagicSignature One => new(1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f);

    /// <summary>
    ///     Creates an 8D magic vector with specified component values.
    /// </summary>
    /// <param name="solidum">Solidum axis value (earth-air)</param>
    /// <param name="febris">Febris axis value (fire-water)</param>
    /// <param name="ordinem">Ordinem axis value (order-chaos)</param>
    /// <param name="lumines">Lumines axis value (light-dark)</param>
    /// <param name="varias">Varias axis value (space-time)</param>
    /// <param name="inertiae">Inertiae axis value (heavy-light)</param>
    /// <param name="subsidium">Subsidium axis value (helpful-harmful)</param>
    /// <param name="spatium">Spatium axis value (distant-nearby)</param>
    public MagicSignature(float solidum, float febris, float ordinem, float lumines,
        float varias, float inertiae, float subsidium, float spatium)
    {
        _values = new[] { solidum, febris, ordinem, lumines, varias, inertiae, subsidium, spatium };
    }

// Add a parameterless constructor for NSubstitute compatibility
    public MagicSignature() : this(0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f) { }

    /// <summary>
    ///     Creates an 8D magic vector from an array of 8 values.
    /// </summary>
    /// <param name="values">Array of 8 float values</param>
    /// <exception cref="ArgumentException">Thrown if array length is not 8</exception>
    public MagicSignature(float[] values)
    {
        if (values?.Length != 8)
        {
            throw new ArgumentException("MagicSignature requires exactly 8 values", nameof(values));
        }

        _values = new float[8];
        Array.Copy(values, _values, 8);
    }

    /// <summary>
    ///     Gets a component value by Element index.
    /// </summary>
    /// <param name="element">The element to get the value for</param>
    /// <returns>The component value for the specified element</returns>
    public float this[EleAspects.Element element] => _values[(int)element];

    /// <summary>
    ///     Gets a component value by index (0-7).
    /// </summary>
    /// <param name="index">Index (0-7)</param>
    /// <returns>The component value at the specified index</returns>
    public float this[int index] => _values[index];

    /// <summary>
    ///     Gets the magnitude (length) of the vector.
    ///     Uses optimized square root calculation.
    /// </summary>
    public float Magnitude
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            var sum = 0f;
            for (var i = 0; i < 8; i++)
            {
                sum += _values[i] * _values[i];
            }

            return MathF.Sqrt(sum);
        }
    }

    /// <summary>
    ///     Gets the squared magnitude of the vector.
    ///     More efficient than Magnitude when only comparison is needed.
    /// </summary>
    public float MagnitudeSquared
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            var sum = 0f;
            for (var i = 0; i < 8; i++)
            {
                sum += _values[i] * _values[i];
            }

            return sum;
        }
    }

    /// <summary>
    ///     Normalizes the vector to unit length.
    ///     Returns Zero vector if original magnitude is zero.
    /// </summary>
    /// <returns>A normalized copy of this vector</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public MagicSignature Normalize()
    {
        var magnitude = Magnitude;
        if (magnitude < float.Epsilon)
        {
            return Zero;
        }

        var result = new float[8];
        for (var i = 0; i < 8; i++)
        {
            result[i] = _values[i] / magnitude;
        }

        return new MagicSignature(result);
    }

    /// <summary>
    ///     Clamps all vector components to the specified range.
    /// </summary>
    /// <param name="min">Minimum value for each component</param>
    /// <param name="max">Maximum value for each component</param>
    /// <returns>A new vector with clamped components</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public MagicSignature Clamp(float min, float max)
    {
        var result = new float[8];
        for (var i = 0; i < 8; i++)
        {
            result[i] = Math.Clamp(_values[i], min, max);
        }

        return new MagicSignature(result);
    }

    /// <summary>
    ///     Projects this vector onto another vector.
    /// </summary>
    /// <param name="onto">The vector to project onto</param>
    /// <returns>The projection of this vector onto the target</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public MagicSignature Project(MagicSignature onto)
    {
        var ontoMagnitudeSquared = onto.MagnitudeSquared;
        if (ontoMagnitudeSquared < float.Epsilon)
        {
            return Zero;
        }

        var dotProduct = Dot(this, onto);
        var scalar = dotProduct / ontoMagnitudeSquared;
        return onto * scalar;
    }

    /// <summary>
    ///     Calculates the dot product of two 8D vectors.
    /// </summary>
    /// <param name="left">First vector</param>
    /// <param name="right">Second vector</param>
    /// <returns>The dot product</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Dot(MagicSignature left, MagicSignature right)
    {
        var sum = 0f;
        for (var i = 0; i < 8; i++)
        {
            sum += left._values[i] * right._values[i];
        }

        return sum;
    }

    /// <summary>
    ///     Calculates the distance between two 8D vectors.
    /// </summary>
    /// <param name="left">First vector</param>
    /// <param name="right">Second vector</param>
    /// <returns>The distance between the vectors</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Distance(MagicSignature left, MagicSignature right)
    {
        return (left - right).Magnitude;
    }

    /// <summary>
    ///     Linearly interpolates between two vectors.
    /// </summary>
    /// <param name="from">Start vector</param>
    /// <param name="to">End vector</param>
    /// <param name="t">Interpolation factor (0.0 to 1.0)</param>
    /// <returns>The interpolated vector</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static MagicSignature Lerp(MagicSignature from, MagicSignature to, float t)
    {
        t = Math.Clamp(t, 0f, 1f);
        var result = new float[8];
        for (var i = 0; i < 8; i++)
        {
            result[i] = from._values[i] + ((to._values[i] - from._values[i]) * t);
        }

        return new MagicSignature(result);
    }

    /// <summary>
    ///     Gets the component value for a specific Element.
    /// </summary>
    /// <param name="element">The element to get</param>
    /// <returns>The component value</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float GetComponent(EleAspects.Element element)
    {
        return _values[(int)element];
    }

    /// <summary>
    ///     Creates a new vector with a specific component set to a value.
    /// </summary>
    /// <param name="element">The element to set</param>
    /// <param name="value">The value to set</param>
    /// <returns>A new vector with the component updated</returns>
    public MagicSignature WithComponent(EleAspects.Element element, float value)
    {
        var result = new float[8];
        Array.Copy(_values, result, 8);
        result[(int)element] = value;
        return new MagicSignature(result);
    }

    // Operator overloads
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static MagicSignature operator +(MagicSignature left, MagicSignature right)
    {
        var result = new float[8];
        for (var i = 0; i < 8; i++)
        {
            result[i] = left._values[i] + right._values[i];
        }

        return new MagicSignature(result);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static MagicSignature operator -(MagicSignature left, MagicSignature right)
    {
        var result = new float[8];
        for (var i = 0; i < 8; i++)
        {
            result[i] = left._values[i] - right._values[i];
        }

        return new MagicSignature(result);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static MagicSignature operator *(MagicSignature vector, float scalar)
    {
        var result = new float[8];
        for (var i = 0; i < 8; i++)
        {
            result[i] = vector._values[i] * scalar;
        }

        return new MagicSignature(result);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static MagicSignature operator *(float scalar, MagicSignature vector)
    {
        return vector * scalar;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static MagicSignature operator /(MagicSignature vector, float scalar)
    {
        if (MathF.Abs(scalar) < float.Epsilon)
        {
            throw new DivideByZeroException("Cannot divide MagicVector8 by zero");
        }

        var result = new float[8];
        for (var i = 0; i < 8; i++)
        {
            result[i] = vector._values[i] / scalar;
        }

        return new MagicSignature(result);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static MagicSignature operator -(MagicSignature vector)
    {
        var result = new float[8];
        for (var i = 0; i < 8; i++)
        {
            result[i] = -vector._values[i];
        }

        return new MagicSignature(result);
    }

    // Equality operations
    public static bool operator ==(MagicSignature left, MagicSignature right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(MagicSignature left, MagicSignature right)
    {
        return !left.Equals(right);
    }

    public bool Equals(MagicSignature other)
    {
        // Handle null _values arrays (can happen with NSubstitute default instances)
        if (_values == null && other._values == null)
        {
            return true;
        }

        if (_values == null || other._values == null)
        {
            return false;
        }

        for (var i = 0; i < 8; i++)
        {
            if (MathF.Abs(_values[i] - other._values[i]) > float.Epsilon)
            {
                return false;
            }
        }

        return true;
    }

    public override bool Equals(object? obj)
    {
        return obj is MagicSignature other && Equals(other);
    }

    public override int GetHashCode()
    {
        var hash = new HashCode();
        foreach (var value in _values)
        {
            hash.Add(value);
        }

        return hash.ToHashCode();
    }

    public override string ToString()
    {
        return $"MagicSignature({_values[0]:F2}, {_values[1]:F2}, {_values[2]:F2}, {_values[3]:F2}, " +
               $"{_values[4]:F2}, {_values[5]:F2}, {_values[6]:F2}, {_values[7]:F2})";
    }

    /// <summary>
    ///     Converts this vector to a readable string with element names.
    /// </summary>
    public string ToElementString()
    {
        return $"MagicSignature(Solidum:{_values[0]:F2}, Febris:{_values[1]:F2}, Ordinem:{_values[2]:F2}, " +
               $"Lumines:{_values[3]:F2}, Varias:{_values[4]:F2}, Inertiae:{_values[5]:F2}, " +
               $"Subsidium:{_values[6]:F2}, Spatium:{_values[7]:F2})";
    }
}
