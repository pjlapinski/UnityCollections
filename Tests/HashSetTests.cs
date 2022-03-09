using System;
using System.Linq;
using NUnit.Framework;
using Collections;

namespace Tests;

[TestFixture]
public class HashSetTests
{
    private HashSet<string> _sut;

    #region Test Case Generators

    public static System.Collections.Generic.IEnumerable<TestCaseData> MultipleValuesSource()
    {
        yield return new TestCaseData(new System.Collections.Generic.List<string> {""});
        yield return new TestCaseData(new System.Collections.Generic.List<string> {"one"});
        yield return new TestCaseData(new System.Collections.Generic.List<string> {"one", "two"});
        yield return new TestCaseData(new System.Collections.Generic.List<string> {"null"});
        yield return new TestCaseData(new System.Collections.Generic.List<string>());
    }

    public static System.Collections.Generic.IEnumerable<TestCaseData> SingleValuesSource()
    {
        yield return new TestCaseData(string.Empty);
        yield return new TestCaseData("one");
        yield return new TestCaseData("null");
        yield return new TestCaseData(null);
    }

    public static System.Collections.Generic.IEnumerable<TestCaseData> CollectionsSource()
    {
        yield return new TestCaseData(new System.Collections.Generic.List<string>{ null });
        yield return new TestCaseData(new System.Collections.Generic.HashSet<string>{ null });

        yield return new TestCaseData(new System.Collections.Generic.List<string>{ "one" });
        yield return new TestCaseData(new System.Collections.Generic.HashSet<string>{ "one" });

        yield return new TestCaseData(new System.Collections.Generic.List<string>{ "one", "two" });
        yield return new TestCaseData(new System.Collections.Generic.HashSet<string>{ "one", "two" });

        yield return new TestCaseData(new System.Collections.Generic.List<string>{ string.Empty, "one", "two" });
        yield return new TestCaseData(new System.Collections.Generic.HashSet<string>{ string.Empty, "one", "two" });
    }

    public static System.Collections.Generic.IEnumerable<TestCaseData> CollectionsWithEmptySource()
    {
        yield return new TestCaseData(new System.Collections.Generic.List<string>());
        yield return new TestCaseData(new System.Collections.Generic.HashSet<string>());

        foreach (var testCase in CollectionsSource())
            yield return testCase;
    }

    #endregion

    [SetUp]
    public void SetUp()
    {
        _sut = new HashSet<string>();
    }

    #region Constructors

    [TestCaseSource(nameof(CollectionsWithEmptySource))]
    public void Constructor_ValuesAreCopiedFromAnotherCollection(System.Collections.Generic.IEnumerable<string> collection)
    {
        _sut = new HashSet<string>(collection);
        var sutValues = _sut.Select(str => str).OrderBy(str => str);
        var otherValues = collection.Select(str => str).OrderBy(str => str);

        Assert.That(sutValues, Is.EquivalentTo(otherValues));
    }

    #endregion

    #region Adding Values

    [TestCaseSource(nameof(MultipleValuesSource))]
    public void Add_ValueIsPresent(System.Collections.Generic.List<string> values)
    {
        for (var i = 0; i < values.Count; ++i)
        {
            _sut.Add(values[i]);
        }

        Assert.That(values.OrderBy(v => v), Is.EquivalentTo(_sut.OrderBy(v => v)));
    }

    [TestCaseSource(nameof(MultipleValuesSource))]
    public void Add_UpdatesCount(System.Collections.Generic.List<string> values)
    {
        for (var i = 0; i < values.Count; ++i)
        {
            _sut.Add(values[i]);
        }

        Assert.That(_sut.Count, Is.EqualTo(values.Count));
    }

    [TestCaseSource(nameof(SingleValuesSource))]
    public void Add_ReturnsFalseForDuplicate(string value)
    {
        _sut.Add(value);

        Assert.That(_sut.Add(value), Is.False);
    }

    [TestCaseSource(nameof(SingleValuesSource))]
    public void Add_ReturnsTrueForNoDuplicate(string value)
    {
        Assert.That(_sut.Add(value), Is.True);
    }

    #endregion

    #region Retrieving Values

    [TestCaseSource(nameof(CollectionsWithEmptySource))]
    public void SetEquals_AreActuallyEqual(System.Collections.Generic.IEnumerable<string> collection)
    {
        _sut = new HashSet<string>(collection);

        Assert.That(_sut.SetEquals(collection), Is.True);
    }

    [TestCaseSource(nameof(CollectionsSource))]
    public void SetEquals_EmptySet_AreNotActuallyEqual(System.Collections.Generic.IEnumerable<string> collection)
    {
        Assert.That(_sut.SetEquals(collection), Is.False);
    }

    [TestCaseSource(nameof(CollectionsWithEmptySource))]
    public void SetEquals_NonEmptySet_AreNotActuallyEqual(System.Collections.Generic.IEnumerable<string> collection)
    {
        _sut = new HashSet<string>{"negative two"};

        Assert.That(_sut.SetEquals(collection), Is.False);
    }

    [TestCaseSource(nameof(SingleValuesSource))]
    public void TryGetValue_PresentValueIsRetrieved(string value)
    {
        _sut.Add(value);
        var result = _sut.TryGetValue(value, out var actual);

        Assert.That(result, Is.True);
        Assert.That(actual, Is.EqualTo(value));
    }

    [TestCaseSource(nameof(SingleValuesSource))]
    public void TryGetValue_EmptySet_NonPresentValueReturnsFalse(string value)
    {
        var result = _sut.TryGetValue(value, out _);

        Assert.That(result, Is.False);
    }

    [TestCaseSource(nameof(SingleValuesSource))]
    public void TryGetValue_NonEmptySet_NonPresentValueReturnsFalse(string value)
    {
        _sut.Add("negative two");
        var result = _sut.TryGetValue(value, out _);

        Assert.That(result, Is.False);
    }

    [TestCaseSource(nameof(SingleValuesSource))]
    public void Contains_HasAddedValues(string value)
    {
        _sut.Add(value);

        Assert.That(_sut.Contains(value), Is.True);
    }

    [TestCaseSource(nameof(SingleValuesSource))]
    public void Contains_EmptySet_DoesntHaveNonExistentValues(string value)
    {
        Assert.That(_sut.Contains(value), Is.False);
    }

    [TestCaseSource(nameof(SingleValuesSource))]
    public void Contains_NonEmptySet_DoesntHaveNonExistentValues(string value)
    {
        _sut.Add("negative two");

        Assert.That(_sut.Contains(value), Is.False);
    }

    [TestCaseSource(nameof(MultipleValuesSource))]
    public void CopyTo_ThrowsErrorIfTargetIsTooSmall(System.Collections.Generic.List<string> values)
    {
        var target = values.ToArray();
        _sut.Add("0");
        _sut.Add("1");
        _sut.Add("2");
        _sut.Add("3");
        _sut.Add("4");
        _sut.Add("5");
        _sut.Add("6");
        _sut.Add("7");

        Assert.Throws<ArgumentException>(() => _sut.CopyTo(target, 0));
    }

    [TestCaseSource(nameof(MultipleValuesSource))]
    public void CopyTo_ValuesAreSuccessfullyCopied(System.Collections.Generic.List<string> values)
    {
        var target = new string[values.Count + 1];
        target[0] = "some key";
        for (var i = 0; i < values.Count; ++i)
            _sut.Add(values[i]);
        _sut.CopyTo(target, 1);

        Assert.That(target[0], Is.EqualTo("some key"));
        Assert.That(target.OrderBy(str => str), Is.EquivalentTo(_sut.Concat(new []{target[0]}).OrderBy(str => str)));
    }

    #endregion

    #region Removing Values

    [Test]
    public void Clear_EmptySet_CountAndValuesAreUpdated()
    {
        _sut.Clear();

        Assert.That(_sut.Count, Is.EqualTo(0));
        Assert.That(_sut.Empty, Is.True);
    }

    [TestCaseSource(nameof(CollectionsWithEmptySource))]
    public void Clear_NonEmptySet_CountAndValuesAreUpdated(System.Collections.Generic.IEnumerable<string> collection)
    {
        _sut = new HashSet<string>(collection);

        _sut.Clear();

        Assert.That(_sut.Count, Is.EqualTo(0));
        Assert.That(_sut.Empty, Is.True);
    }

    [TestCaseSource(nameof(SingleValuesSource))]
    public void Clear_NonEmptySet_ValueNoLongerPresent(string value)
    {
        _sut.Add(value);

        _sut.Clear();

        Assert.That(_sut.Contains(value), Is.False);
    }

    [TestCaseSource(nameof(SingleValuesSource))]
    public void Remove_EmptySet_CouldNotRemove(string value)
    {
        Assert.That(_sut.Remove(value), Is.False);
        Assert.That(_sut.Count, Is.EqualTo(0));
    }

    [TestCaseSource(nameof(SingleValuesSource))]
    public void Remove_NonEmptySet_CouldNotRemoveNonExistingValue(string value)
    {
        _sut.Add("negative two");

        Assert.That(_sut.Remove(value), Is.False);
        Assert.That(_sut.Count, Is.EqualTo(1));
    }

    [TestCaseSource(nameof(SingleValuesSource))]
    public void Remove_ValueIsRemoved(string value)
    {
        _sut.Add(value);

        Assert.That(_sut.Remove(value), Is.True);
        Assert.That(_sut.Count, Is.EqualTo(0));
        Assert.That(_sut.Contains(value), Is.False);
    }

    [TestCaseSource(nameof(SingleValuesSource))]
    public void RemoveWhere_NoMatches(string value)
    {
        _sut.Add(value);

        _sut.RemoveWhere(str => str == "negative two");
        Assert.That(_sut.Contains(value), Is.True);
        Assert.That(_sut.Count, Is.EqualTo(1));
    }

    [TestCaseSource(nameof(SingleValuesSource))]
    public void RemoveWhere_OneMatch(string value)
    {
        _sut.Add(value);
        _sut.Add(value + ".");

        _sut.RemoveWhere(str => str == value);
        Assert.That(_sut.Contains(value), Is.False);
        Assert.That(_sut.Count, Is.EqualTo(1));
    }

    [TestCaseSource(nameof(SingleValuesSource))]
    public void RemoveWhere_ManyMatches(string value)
    {
        _sut.Add(value);
        if (value != null) _sut.Add(value + ".");

        _sut.RemoveWhere(str => str == null || str.StartsWith(value));
        Assert.That(_sut.Contains(value), Is.False);
        Assert.That(_sut.Count, Is.EqualTo(0));
    }

    #endregion
}
