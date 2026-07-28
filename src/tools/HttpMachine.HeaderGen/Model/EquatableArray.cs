using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace HttpMachine.HeaderGen.Model;

/// <summary>
/// An <see cref="ImmutableArray{T}"/> that compares by value.
/// </summary>
/// <remarks>
/// <para>
/// This exists because incremental generator caching is driven entirely by
/// <see cref="IEquatable{T}"/> on the values flowing through the pipeline, and
/// <see cref="ImmutableArray{T}"/> compares by <em>reference</em>. A model holding a raw
/// <see cref="ImmutableArray{T}"/> therefore reports "changed" on every single keystroke in
/// the IDE, and every downstream stage re-runs. The generator still produces correct output,
/// so nothing fails - it just quietly becomes expensive, which is why the cacheability test
/// in HeaderGen.Tests exists to catch a regression here.
/// </para>
/// <para>
/// See Andrew Lock, "Creating a source generator - Part 9: Avoiding performance pitfalls in
/// incremental generators".
/// </para>
/// </remarks>
internal readonly struct EquatableArray<T> : IEquatable<EquatableArray<T>>, IEnumerable<T>
    where T : IEquatable<T>
{
    public static readonly EquatableArray<T> Empty = new(ImmutableArray<T>.Empty);

    private readonly ImmutableArray<T> _array;

    public EquatableArray(ImmutableArray<T> array) => _array = array;

    public EquatableArray(IEnumerable<T> items) => _array = ImmutableArray.CreateRange(items);

    public int Count => _array.IsDefault ? 0 : _array.Length;

    public T this[int index] => _array[index];

    public ImmutableArray<T> AsImmutableArray() => _array.IsDefault ? ImmutableArray<T>.Empty : _array;

    public bool Equals(EquatableArray<T> other)
    {
        // A default (uninitialised) array and an empty one are treated as equal: both mean
        // "no items", and the distinction would otherwise show up as a spurious cache miss.
        if (_array.IsDefaultOrEmpty)
        {
            return other._array.IsDefaultOrEmpty;
        }

        if (other._array.IsDefaultOrEmpty || _array.Length != other._array.Length)
        {
            return false;
        }

        for (var i = 0; i < _array.Length; i++)
        {
            if (!_array[i].Equals(other._array[i]))
            {
                return false;
            }
        }

        return true;
    }

    public override bool Equals(object? obj) => obj is EquatableArray<T> other && Equals(other);

    public override int GetHashCode()
    {
        if (_array.IsDefaultOrEmpty)
        {
            return 0;
        }

        var hash = new HashCode();

        foreach (var item in _array)
        {
            hash.Add(item);
        }

        return hash.ToHashCode();
    }

    public IEnumerator<T> GetEnumerator() => ((IEnumerable<T>)AsImmutableArray()).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)AsImmutableArray()).GetEnumerator();

    public static bool operator ==(EquatableArray<T> left, EquatableArray<T> right) => left.Equals(right);

    public static bool operator !=(EquatableArray<T> left, EquatableArray<T> right) => !left.Equals(right);
}
