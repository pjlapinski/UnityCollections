using System;
using System.Linq;
using NUnit.Framework;
using Collections;

namespace Tests;

[TestFixture]
public class DictionaryTests
{
    private Dictionary<string, int?> _sut;

    #region Test Cases Generators

    public static System.Collections.Generic.IEnumerable<TestCaseData> OnlyKeysSource()
    {
        yield return new TestCaseData("one");
        yield return new TestCaseData("two");
        yield return new TestCaseData("");
    }

    public static System.Collections.Generic.IEnumerable<TestCaseData> SingleValuesSource()
    {
        yield return new TestCaseData(string.Empty, 0);
        yield return new TestCaseData("one", 1);
        yield return new TestCaseData("null", null);
    }

    public static System.Collections.Generic.IEnumerable<TestCaseData> KeyValuePairsSource()
    {
        yield return new TestCaseData(new[] {""}, new int?[] {0});
        yield return new TestCaseData(new[] {"one"}, new int?[] {1});
        yield return new TestCaseData(new[] {"one", "two"}, new int?[] {1, 2});
        yield return new TestCaseData(new[] {"null"}, new int?[] {null});
        yield return new TestCaseData(Array.Empty<string>(), Array.Empty<int?>());
    }

    public static System.Collections.Generic.IEnumerable<TestCaseData> CollectionsSource()
    {
        yield return new TestCaseData(new KeyValuePair<string, int?>[]{ new("null", null) });
        yield return new TestCaseData(new System.Collections.Generic.List<KeyValuePair<string, int?>>{ new("null", null) });
        yield return new TestCaseData(new System.Collections.Generic.HashSet<KeyValuePair<string, int?>>{ new("null", null) });
        yield return new TestCaseData(new Dictionary<string, int?>{{"null", null}});

        yield return new TestCaseData(new KeyValuePair<string, int?>[]{ new("one", 1) });
        yield return new TestCaseData(new System.Collections.Generic.List<KeyValuePair<string, int?>>{ new("one", 1) });
        yield return new TestCaseData(new System.Collections.Generic.HashSet<KeyValuePair<string, int?>>{ new("one", 1) });
        yield return new TestCaseData(new Dictionary<string, int?>{{"one", 1}});

        yield return new TestCaseData(new KeyValuePair<string, int?>[]{ new("one", 1), new("two", 2) });
        yield return new TestCaseData(new System.Collections.Generic.List<KeyValuePair<string, int?>>{ new("one", 1), new("two", 2) });
        yield return new TestCaseData(new System.Collections.Generic.HashSet<KeyValuePair<string, int?>>{ new("one", 1), new("two", 2) });
        yield return new TestCaseData(new Dictionary<string, int?>{{"one", 1}, {"two", 2}});

        yield return new TestCaseData(new KeyValuePair<string, int?>[]{ new(string.Empty, 0), new("one", 1), new("two", 2) });
        yield return new TestCaseData(new System.Collections.Generic.List<KeyValuePair<string, int?>>{ new(string.Empty, 0), new("one", 1), new("two", 2) });
        yield return new TestCaseData(new System.Collections.Generic.HashSet<KeyValuePair<string, int?>>{ new(string.Empty, 0), new("one", 1), new("two", 2) });
        yield return new TestCaseData(new Dictionary<string, int?>{{string.Empty, 0}, {"one", 1}, {"two", 2}});
    }

    public static System.Collections.Generic.IEnumerable<TestCaseData> CollectionsWithEmptySource()
    {
        yield return new TestCaseData(Array.Empty<KeyValuePair<string, int?>>());
        yield return new TestCaseData(new System.Collections.Generic.List<KeyValuePair<string, int?>>());
        yield return new TestCaseData(new System.Collections.Generic.HashSet<KeyValuePair<string, int?>>());
        yield return new TestCaseData(new Dictionary<string, int?>());

        foreach (var testCase in CollectionsSource())
            yield return testCase;
    }

    #endregion

    [SetUp]
    public void SetUp()
    {
        _sut = new Dictionary<string, int?>();
    }

    #region Constructors

    [TestCaseSource(nameof(CollectionsWithEmptySource))]
    public void Constructor_ValuesAreCopiedFromAnotherCollection(System.Collections.Generic.IEnumerable<KeyValuePair<string, int?>> collection)
    {
        _sut = new Dictionary<string, int?>(collection);
        var sutValues = _sut.Select(kvp => kvp).OrderBy(kvp => kvp.Value);
        var otherValues = collection.Select(kvp => kvp).OrderBy(kvp => kvp.Value);

        Assert.That(sutValues, Is.EquivalentTo(otherValues));
    }

    #endregion

    #region Adding Values

    [TestCaseSource(nameof(KeyValuePairsSource))]
    public void Add_ValueIsPresent(string[] keys, int?[] values)
    {
        for (var i = 0; i < keys.Length; ++i)
        {
            _sut.Add(keys[i], values[i]);
        }

        Assert.That(keys.OrderBy(k => k), Is.EquivalentTo(_sut.Keys.OrderBy(k => k)));
        Assert.That(values.OrderBy(v => v), Is.EquivalentTo(_sut.Values.OrderBy(v => v)));
    }

    [TestCaseSource(nameof(KeyValuePairsSource))]
    public void Add_KeyValuePairs_ValueIsPresent(string[] keys, int?[] values)
    {
        for (var i = 0; i < keys.Length; ++i)
        {
            _sut.Add(new KeyValuePair<string, int?>(keys[i], values[i]));
        }

        Assert.That(keys.OrderBy(k => k), Is.EquivalentTo(_sut.Keys.OrderBy(k => k)));
        Assert.That(values.OrderBy(v => v), Is.EquivalentTo(_sut.Values.OrderBy(v => v)));
    }

    [TestCaseSource(nameof(KeyValuePairsSource))]
    public void Add_UpdatesCount(string[] keys, int?[] values)
    {
        for (var i = 0; i < keys.Length; ++i)
        {
            _sut.Add(keys[i], values[i]);
        }

        Assert.That(_sut.Count, Is.EqualTo(keys.Length));
    }

    [TestCaseSource(nameof(SingleValuesSource))]
    public void Add_DuplicateThrowsException(string key, int? value)
    {
        _sut.Add(key, value);
        Assert.Throws<DuplicateKeyException>(() => _sut.Add(key, value));
    }

    [TestCaseSource(nameof(KeyValuePairsSource))]
    public void TryAdd_ValueIsPresent(string[] keys, int?[] values)
    {
        for (var i = 0; i < keys.Length; ++i)
        {
            _sut.TryAdd(keys[i], values[i]);
        }

        Assert.That(keys.OrderBy(k => k), Is.EquivalentTo(_sut.Keys.OrderBy(k => k)));
        Assert.That(values.OrderBy(v => v), Is.EquivalentTo(_sut.Values.OrderBy(v => v)));
    }

    [TestCaseSource(nameof(SingleValuesSource))]
    public void TryAdd_ReturnsFalseForDuplicate(string key, int? value)
    {
        _sut.Add(key, value);

        Assert.That(_sut.TryAdd(key, value), Is.False);
    }

    [Test]
    public void StressTest()
    {
        const int count = 10000;
        for (var i = 0; i < count; ++i)
        {
            _sut.Add(i.ToString(), i);
        }

        for (var i = 0; i < count; ++i)
        {
            Assert.That(_sut.TryGetValue(i.ToString(), out _), Is.True);
            Assert.That(_sut.Remove(i.ToString()), Is.True);
        }

        Assert.That(_sut.Count, Is.EqualTo(0));
    }

    #endregion

    #region Indexer

    [TestCaseSource(nameof(KeyValuePairsSource))]
    public void Indexer_NewValueGetsAdded(string[] keys, int?[] values)
    {
        for (var i = 0; i < keys.Length; ++i)
        {
            _sut[keys[i]] = values[i];
        }

        Assert.That(keys.OrderBy(k => k), Is.EquivalentTo(_sut.Keys.OrderBy(k => k)));
        Assert.That(values.OrderBy(v => v), Is.EquivalentTo(_sut.Values.OrderBy(v => v)));
    }

    [TestCaseSource(nameof(KeyValuePairsSource))]
    public void Indexer_ValuesGetUpdated(string[] keys, int?[] values)
    {
        for (var i = 0; i < keys.Length; ++i)
        {
            _sut.Add(keys[i], keys[i].GetHashCode());
        }
        for (var i = 0; i < keys.Length; ++i)
        {
            _sut[keys[i]] = values[i];
        }

        Assert.That(keys.OrderBy(k => k), Is.EquivalentTo(_sut.Keys.OrderBy(k => k)));
        Assert.That(values.OrderBy(v => v), Is.EquivalentTo(_sut.Values.OrderBy(v => v)));
    }

    [TestCaseSource(nameof(KeyValuePairsSource))]
    public void Indexer_ValueGetsRetrieved(string[] keys, int?[] values)
    {
        for (var i = 0; i < keys.Length; ++i)
        {
            _sut.Add(keys[i], values[i]);
        }

        Assert.That(values.OrderBy(v => v), Is.EquivalentTo(keys.Select(key => _sut[key]).OrderBy(v => v)));
    }

    [TestCaseSource(nameof(OnlyKeysSource))]
    public void Indexer_EmptyDictionary_InvalidKeyThrows(string key)
    {
        Assert.Throws<KeyNotFoundException>(() =>
        {
            var value = _sut[key];
        });
    }

    [TestCaseSource(nameof(OnlyKeysSource))]
    public void Indexer_NonEmptyDictionary_InvalidKeyThrows(string key)
    {
        _sut.Add("zero", 0);

        Assert.Throws<KeyNotFoundException>(() =>
        {
            var value = _sut[key];
        });
    }
    #endregion

    #region Retrieving Values

    [TestCaseSource(nameof(CollectionsWithEmptySource))]
    public void DictionaryEquals_AreActuallyEqual(System.Collections.Generic.IEnumerable<KeyValuePair<string, int?>> collection)
    {
        _sut = new Dictionary<string, int?>(collection);

        Assert.That(_sut.DictionaryEquals(collection), Is.True);
    }

    [TestCaseSource(nameof(CollectionsSource))]
    public void DictionaryEquals_EmptyDictionary_AreNotActuallyEqual(System.Collections.Generic.IEnumerable<KeyValuePair<string, int?>> collection)
    {
        Assert.That(_sut.DictionaryEquals(collection), Is.False);
    }

    [TestCaseSource(nameof(CollectionsWithEmptySource))]
    public void DictionaryEquals_NonEmptyDictionary_AreNotActuallyEqual(System.Collections.Generic.IEnumerable<KeyValuePair<string, int?>> collection)
    {
        _sut = new Dictionary<string, int?>{{"negative two", -2}};

        Assert.That(_sut.DictionaryEquals(collection), Is.False);
    }

    [TestCaseSource(nameof(SingleValuesSource))]
    public void ContainsKey_HasAddedKeys(string key, int? value)
    {
        _sut.Add(key, value);

        Assert.That(_sut.ContainsKey(key), Is.True);
    }

    [TestCaseSource(nameof(SingleValuesSource))]
    public void ContainsValue_HasAddedValues(string key, int? value)
    {
        _sut.Add(key, value);

        Assert.That(_sut.ContainsValue(value), Is.True);
    }

    [TestCaseSource(nameof(SingleValuesSource))]
    public void Contains_HasAddedPairs(string key, int? value)
    {
        _sut.Add(key, value);

        Assert.That(_sut.Contains(new KeyValuePair<string, int?>(key, value)), Is.True);
    }

    [TestCaseSource(nameof(SingleValuesSource))]
    public void ContainsKey_EmptyDictionary_DoesntHaveNonExistentKeys(string key, int? value)
    {
        Assert.That(_sut.ContainsKey(key), Is.False);
    }

    [TestCaseSource(nameof(SingleValuesSource))]
    public void ContainsValue_EmptyDictionary_DoesntHaveNonExistentValues(string key, int? value)
    {
        Assert.That(_sut.ContainsValue(value), Is.False);
    }

    [TestCaseSource(nameof(SingleValuesSource))]
    public void Contains_EmptyDictionary_DoesntHaveNonExistentPairs(string key, int? value)
    {
        Assert.That(_sut.Contains(new KeyValuePair<string, int?>(key, value)), Is.False);
    }

    [TestCaseSource(nameof(SingleValuesSource))]
    public void ContainsKey_NonEmptyDictionary_DoesntHaveNonExistentKeys(string key, int? value)
    {
        _sut.Add("negative two", -2);

        Assert.That(_sut.ContainsKey(key), Is.False);
    }

    [TestCaseSource(nameof(SingleValuesSource))]
    public void ContainsValue_NonEmptyDictionary_DoesntHaveNonExistentValues(string key, int? value)
    {
        _sut.Add("negative two", -2);

        Assert.That(_sut.ContainsValue(value), Is.False);
    }

    [TestCaseSource(nameof(SingleValuesSource))]
    public void Contains_NonEmptyDictionary_DoesntHaveNonExistentPairs(string key, int? value)
    {
        _sut.Add("negative two", -2);

        Assert.That(_sut.Contains(new KeyValuePair<string, int?>(key, value)), Is.False);
    }

    [TestCaseSource(nameof(KeyValuePairsSource))]
    public void CopyTo_ThrowsErrorIfTargetIsTooSmall(string[] keys, int?[] values)
    {
        var target = keys
            .Zip(values)
            .Select(tuple => new KeyValuePair<string, int?>(tuple.First, tuple.Second))
            .ToArray();
        _sut.Add("0", 0);
        _sut.Add("1", 1);
        _sut.Add("2", 2);
        _sut.Add("3", 3);
        _sut.Add("4", 4);
        _sut.Add("5", 5);
        _sut.Add("6", 6);
        _sut.Add("7", 7);

        Assert.Throws<ArgumentException>(() => _sut.CopyTo(target, 0));
    }

    [TestCaseSource(nameof(KeyValuePairsSource))]
    public void CopyTo_ValuesAreSuccessfullyCopied(string[] keys, int?[] values)
    {
        var target = new KeyValuePair<string, int?>[keys.Length + 1];
        target[0] = new KeyValuePair<string, int?>("some key", null);
        for (var i = 0; i < keys.Length; ++i)
            _sut.Add(keys[i], values[i]);
        _sut.CopyTo(target, 1);

        Assert.That(target[0].Key, Is.EqualTo("some key"));
        Assert.That(target[0].Value, Is.Null);
        Assert.That(target.OrderBy(kvp => kvp.Key), Is.EquivalentTo(_sut.Select(kvp => kvp).Concat(new []{target[0]}).OrderBy(kvp => kvp.Key)));
    }

    #endregion

    #region Removing Values

    [Test]
    public void Clear_EmptyDictionary_CountAndValuesAreUpdated()
    {
        _sut.Clear();

        Assert.That(_sut.Count, Is.EqualTo(0));
        Assert.That(_sut.Empty, Is.True);
        Assert.That(_sut.Keys.Length, Is.EqualTo(0));
        Assert.That(_sut.Values.Length, Is.EqualTo(0));
    }

    [TestCaseSource(nameof(CollectionsWithEmptySource))]
    public void Clear_NonEmptyDictionary_CountAndValuesAreUpdated(System.Collections.Generic.IEnumerable<KeyValuePair<string, int?>> collection)
    {
        _sut = new Dictionary<string, int?>(collection);

        _sut.Clear();

        Assert.That(_sut.Count, Is.EqualTo(0));
        Assert.That(_sut.Empty, Is.True);
        Assert.That(_sut.Keys.Length, Is.EqualTo(0));
        Assert.That(_sut.Values.Length, Is.EqualTo(0));
    }

    [TestCaseSource(nameof(SingleValuesSource))]
    public void Clear_NonEmptyDictionary_KeyNoLongerPresent(string key, int? value)
    {
        _sut.Add(key, value);

        _sut.Clear();

        Assert.That(_sut.ContainsKey(key), Is.False);
    }

    [TestCaseSource(nameof(SingleValuesSource))]
    public void Clear_NonEmptyDictionary_ValueNoLongerPresent(string key, int? value)
    {
        _sut.Add(key, value);

        _sut.Clear();

        Assert.That(_sut.ContainsValue(value), Is.False);
    }

    [TestCaseSource(nameof(SingleValuesSource))]
    public void Clear_NonEmptyDictionary_PairNoLongerPresent(string key, int? value)
    {
        _sut.Add(key, value);

        _sut.Clear();

        Assert.That(_sut.Contains(new KeyValuePair<string, int?>(key, value)), Is.False);
    }

    [TestCaseSource(nameof(OnlyKeysSource))]
    public void Remove_EmptyDictionary_CouldNotRemove(string key)
    {
        Assert.That(_sut.Remove(key), Is.False);
        Assert.That(_sut.Count, Is.EqualTo(0));
    }

    [TestCaseSource(nameof(OnlyKeysSource))]
    public void Remove_NonEmptyDictionary_CouldNotRemoveNonExistingKey(string key)
    {
        _sut.Add("negative two", -1);

        Assert.That(_sut.Remove(key), Is.False);
        Assert.That(_sut.Count, Is.EqualTo(1));
    }

    [TestCaseSource(nameof(OnlyKeysSource))]
    public void Remove_KeyIsRemoved(string key)
    {
        _sut.Add(key, 0);

        Assert.That(_sut.Remove(key), Is.True);
        Assert.That(_sut.Count, Is.EqualTo(0));
        Assert.That(_sut.ContainsKey(key), Is.False);
    }

    [TestCaseSource(nameof(SingleValuesSource))]
    public void Remove_ValueIsRemoved(string key, int? value)
    {
        _sut.Add(key, value);

        Assert.That(_sut.Remove(key), Is.True);
        Assert.That(_sut.Count, Is.EqualTo(0));
        Assert.That(_sut.ContainsValue(value), Is.False);
    }

    [TestCaseSource(nameof(SingleValuesSource))]
    public void Remove_ValueDuplicate_ValueStillExists(string key, int? value)
    {
        _sut.Add(key + ".", value);
        _sut.Add(key, value);

        Assert.That(_sut.Remove(key), Is.True);
        Assert.That(_sut.Count, Is.EqualTo(1));
        Assert.That(_sut.ContainsValue(value), Is.True);
    }

    [TestCaseSource(nameof(SingleValuesSource))]
    public void Remove_KeyValuePair_EmptyDictionary_CouldNotRemove(string key, int? value)
    {
        Assert.That(_sut.Remove(new KeyValuePair<string, int?>(key, value)), Is.False);
        Assert.That(_sut.Count, Is.EqualTo(0));
    }

    [TestCaseSource(nameof(SingleValuesSource))]
    public void Remove_KeyValuePair_NonEmptyDictionary_CouldNotRemoveNonExistingKey(string key, int? value)
    {
        _sut.Add("negative two", -1);

        Assert.That(_sut.Remove(new KeyValuePair<string, int?>(key, value)), Is.False);
        Assert.That(_sut.Count, Is.EqualTo(1));
    }

    [TestCaseSource(nameof(SingleValuesSource))]
    public void Remove_KeyValuePair_ValueIsRemoved(string key, int? value)
    {
        _sut.Add(key, value);

        Assert.That(_sut.Remove(new KeyValuePair<string, int?>(key, value)), Is.True);
        Assert.That(_sut.Count, Is.EqualTo(0));
        Assert.That(_sut.ContainsValue(value), Is.False);
    }

    [TestCaseSource(nameof(SingleValuesSource))]
    public void Remove_KeyValuePair_ValueDuplicate_ValueStillExists(string key, int? value)
    {
        _sut.Add(key + ".", value);
        _sut.Add(key, value);

        Assert.That(_sut.Remove(new KeyValuePair<string, int?>(key, value)), Is.True);
        Assert.That(_sut.Count, Is.EqualTo(1));
        Assert.That(_sut.ContainsValue(value), Is.True);
    }

    [TestCaseSource(nameof(SingleValuesSource))]
    public void RemoveWhere_KeyBased_NoMatches(string key, int? value)
    {
        _sut.Add(key, value);

        _sut.RemoveWhere(kvp => kvp.Key == "non-existing key");
        Assert.That(_sut.ContainsKey(key), Is.True);
        Assert.That(_sut.ContainsValue(value), Is.True);
        Assert.That(_sut.Count, Is.EqualTo(1));
    }

    [TestCaseSource(nameof(SingleValuesSource))]
    public void RemoveWhere_ValueBased_NoMatches(string key, int? value)
    {
        _sut.Add(key, value);

        _sut.RemoveWhere(kvp => kvp.Value == -2);
        Assert.That(_sut.ContainsKey(key), Is.True);
        Assert.That(_sut.ContainsValue(value), Is.True);
        Assert.That(_sut.Count, Is.EqualTo(1));
    }

    [TestCaseSource(nameof(SingleValuesSource))]
    public void RemoveWhere_KeyBased_OneMatch(string key, int? value)
    {
        _sut.Add(key, value);
        _sut.Add(key + ".", -2);

        _sut.RemoveWhere(kvp => kvp.Key == key);
        Assert.That(_sut.ContainsKey(key), Is.False);
        Assert.That(_sut.ContainsValue(value), Is.False);
        Assert.That(_sut.Count, Is.EqualTo(1));
    }

    [TestCaseSource(nameof(SingleValuesSource))]
    public void RemoveWhere_ValueBased_OneMatch(string key, int? value)
    {
        _sut.Add(key, value);
        _sut.Add(key + ".", -2);

        _sut.RemoveWhere(kvp => kvp.Value == value);
        Assert.That(_sut.ContainsKey(key), Is.False);
        Assert.That(_sut.ContainsValue(value), Is.False);
        Assert.That(_sut.Count, Is.EqualTo(1));
    }

    [TestCaseSource(nameof(SingleValuesSource))]
    public void RemoveWhere_KeyBased_ManyMatches(string key, int? value)
    {
        _sut.Add(key, value);
        _sut.Add(key + ".", -2);

        _sut.RemoveWhere(kvp => kvp.Key.StartsWith(key));
        Assert.That(_sut.ContainsKey(key), Is.False);
        Assert.That(_sut.ContainsValue(value), Is.False);
        Assert.That(_sut.Count, Is.EqualTo(0));
    }

    [TestCaseSource(nameof(SingleValuesSource))]
    public void RemoveWhere_ValueBased_ManyMatches(string key, int? value)
    {
        _sut.Add(key, value);
        _sut.Add(key + ".", value);

        _sut.RemoveWhere(kvp => kvp.Value == value);
        Assert.That(_sut.ContainsKey(key), Is.False);
        Assert.That(_sut.ContainsValue(value), Is.False);
        Assert.That(_sut.Count, Is.EqualTo(0));
    }

    #endregion
}