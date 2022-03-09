using System;
using System.Linq;
using NUnit.Framework;
using Violet.Utilities.Collections;

namespace Tests;

[TestFixture]
public class ListTests
{
    private List<string> _sut;

    #region Test Case Generators

    public static System.Collections.Generic.IEnumerable<TestCaseData> MultipleValuesSourceWithEmpty()
    {
        foreach (var test in MultipleValuesSource())
        {
            yield return test;
        }
        yield return new TestCaseData(new System.Collections.Generic.List<string>());
    }

    public static System.Collections.Generic.IEnumerable<TestCaseData> MultipleValuesSource()
    {
        yield return new TestCaseData(new System.Collections.Generic.List<string> {""});
        yield return new TestCaseData(new System.Collections.Generic.List<string> {"one"});
        yield return new TestCaseData(new System.Collections.Generic.List<string> {"one", "two"});
        yield return new TestCaseData(new System.Collections.Generic.List<string> {"null"});
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
        _sut = new List<string>();
    }

    #region Constructors

    [TestCaseSource(nameof(CollectionsWithEmptySource))]
    public void Constructor_ValuesAreCopiedFromAnotherCollection(System.Collections.Generic.IEnumerable<string> collection)
    {
        _sut = new List<string>(collection);
        var sutValues = _sut.Select(str => str).OrderBy(str => str);
        var otherValues = collection.Select(str => str).OrderBy(str => str);

        Assert.That(sutValues, Is.EquivalentTo(otherValues));
    }

    #endregion

    #region Adding Values

    [TestCaseSource(nameof(MultipleValuesSourceWithEmpty))]
    public void Add_ValueIsPresent(System.Collections.Generic.List<string> values)
    {
        for (var i = 0; i < values.Count; ++i)
        {
            _sut.Add(values[i]);
        }

        Assert.That(values.OrderBy(v => v), Is.EquivalentTo(_sut.OrderBy(v => v)));
    }

    [TestCaseSource(nameof(MultipleValuesSourceWithEmpty))]
    public void Add_UpdatesCount(System.Collections.Generic.List<string> values)
    {
        for (var i = 0; i < values.Count; ++i)
        {
            _sut.Add(values[i]);
        }

        Assert.That(_sut.Count, Is.EqualTo(values.Count));
    }

    [TestCaseSource(nameof(SingleValuesSource))]
    public void Insert_ValuesMoveCorrectly(string value)
    {
        _sut.Add("value 0");
        _sut.Add("value 1");
        _sut.Insert(1, value);
        Assert.That(_sut, Is.EquivalentTo(new []{"value 0", value, "value 1"}));
    }

    [TestCaseSource(nameof(SingleValuesSource))]
    public void Insert_IndexEqualToCountExtends(string value)
    {
        _sut.Add("value 0");
        _sut.Add("value 1");
        _sut.Insert(2, value);
        Assert.That(_sut, Is.EquivalentTo(new []{"value 0", "value 1", value}));
        Assert.That(_sut.Count, Is.EqualTo(3));
    }

    [TestCaseSource(nameof(SingleValuesSource))]
    public void Insert_HighIndexThrows(string value)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => _sut.Insert(2, value));
    }

    #endregion

    #region Indexer

    [TestCaseSource(nameof(SingleValuesSource))]
    public void Indexer_ValuesGetUpdated(string value)
    {
        _sut.Add("Some random string");
        _sut[0] = value;
        Assert.That(_sut[0], Is.EqualTo(value));
    }

    [TestCaseSource(nameof(SingleValuesSource))]
    public void Indexer_Set_IndexOutOfRangeThrows(string value)
    {
        _sut.Add(value);
        Assert.Throws<IndexOutOfRangeException>(() => _sut[1] = value);
        Assert.Throws<IndexOutOfRangeException>(() => _sut[-1] = value);
    }

    [TestCaseSource(nameof(SingleValuesSource))]
    public void Indexer_Get_IndexOutOfRangeThrows(string value)
    {
        _sut.Add(value);
        Assert.Throws<IndexOutOfRangeException>(() => { var test = _sut[1];}); 
        Assert.Throws<IndexOutOfRangeException>(() => { var test = _sut[-1];}); 
    }

    #endregion

    #region Retrieving Values

    [TestCaseSource(nameof(SingleValuesSource))]
    public void Contains_HasAddedValues(string value)
    {
        _sut.Add(value);

        Assert.That(_sut.Contains(value), Is.True);
    }

    [TestCaseSource(nameof(SingleValuesSource))]
    public void Contains_EmptyList_DoesntHaveNonExistentKeys(string value)
    {
        Assert.That(_sut.Contains(value), Is.False);
    }

    [TestCaseSource(nameof(SingleValuesSource))]
    public void Contains_NonEmptyList_DoesntHaveNonExistentValues(string value)
    {
        _sut.Add("negative two");

        Assert.That(_sut.Contains(value), Is.False);
    }

    [TestCaseSource(nameof(MultipleValuesSourceWithEmpty))]
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

    [TestCaseSource(nameof(MultipleValuesSourceWithEmpty))]
    public void CopyTo_ValuesAreSuccessfullyCopied(System.Collections.Generic.List<string> values)
    {
        var target = new string[values.Count + 1];
        target[0] = "some key";
        for (var i = 0; i < values.Count; ++i)
            _sut.Add(values[i]);
        _sut.CopyTo(target, 1);

        Assert.That(target[0], Is.EqualTo("some key"));
        Assert.That(target, Is.EquivalentTo(_sut.Concat(new []{target[0]})));
    }

    [TestCaseSource(nameof(MultipleValuesSource))]
    public void Exists_HasTarget_ReturnsTrue(System.Collections.Generic.List<string> values)
    {
        _sut.AddRange(values);

        Assert.That(_sut.Exists(element => true), Is.True);
    }

    [TestCaseSource(nameof(MultipleValuesSource))]
    public void Exists_HasNoTarget_ReturnsFalse(System.Collections.Generic.List<string> values)
    {
        _sut.AddRange(values);

        Assert.That(_sut.Exists(element => false), Is.False);
    }
    
    [Test]
    public void Exists_EmptyList_ReturnsFalse()
    {
        Assert.That(_sut.Exists(element => false), Is.False);
    }

    [TestCaseSource(nameof(MultipleValuesSource))]
    public void Find_GetsValueIfPresent(System.Collections.Generic.List<string> values)
    {
        _sut.AddRange(values);

        Assert.That(_sut.Find(element => element != "some string"), Is.Not.EqualTo(default(string)));
    }

    [TestCaseSource(nameof(MultipleValuesSource))]
    public void Find_GetsFirstOccurence(System.Collections.Generic.List<string> values)
    {
        _sut.AddRange(values);

        Assert.That(_sut.Find(element => element != "some string"), Is.EqualTo(values[0]));
    }

    [TestCaseSource(nameof(MultipleValuesSource))]
    public void Find_SetsDefaultIfNotPresent(System.Collections.Generic.List<string> values)
    {
        _sut.AddRange(values);

        Assert.That(_sut.Find(element => element == "some string"), Is.EqualTo(default(string)));
    }

    [TestCaseSource(nameof(MultipleValuesSource))]
    public void FindLast_GetsValueIfPresent(System.Collections.Generic.List<string> values)
    {
        _sut.AddRange(values);

        Assert.That(_sut.FindLast(element => element != "some string"), Is.Not.EqualTo(default(string)));
    }

    [TestCaseSource(nameof(MultipleValuesSource))]
    public void FindLast_GetsLastOccurence(System.Collections.Generic.List<string> values)
    {
        _sut.AddRange(values);

        Assert.That(_sut.FindLast(element => element != "some string"), Is.EqualTo(values.Last()));
    }

    [TestCaseSource(nameof(MultipleValuesSource))]
    public void FindLast_SetsDefaultIfNotPresent(System.Collections.Generic.List<string> values)
    {
        _sut.AddRange(values);

        Assert.That(_sut.FindLast(element => element == "some string"), Is.EqualTo(default(string)));
    }

    [TestCaseSource(nameof(MultipleValuesSource))]
    public void FindAll_GetsValueIfPresent(System.Collections.Generic.List<string> values)
    {
        _sut.AddRange(values);

        Assert.That(_sut.FindAll(element => element != "some string").Count, Is.EqualTo(values.Count));
    }

    [TestCaseSource(nameof(MultipleValuesSource))]
    public void FindLast_GetsAllOccurrences(System.Collections.Generic.List<string> values)
    {
        const string someString = "some string";
        _sut.AddRange(values);
        _sut.Add(someString);

        var result = _sut.FindAll(element => element != someString);
        Assert.That(result.Count, Is.EqualTo(values.Count));
        Assert.That(result.Contains(someString), Is.False);
    }

    [TestCaseSource(nameof(MultipleValuesSource))]
    public void FindAll_EmptyListIfNotPresent(System.Collections.Generic.List<string> values)
    {
        _sut.AddRange(values);

        Assert.That(_sut.FindAll(element => element == "some string").Count, Is.EqualTo(0));
    }

    [TestCaseSource(nameof(SingleValuesSource))]
    public void LastIndexOf_MinusOneIfNoTarget(string value)
    {
        Assert.That(_sut.LastIndexOf(value), Is.EqualTo(-1));
    }

    [TestCaseSource(nameof(SingleValuesSource))]
    public void LastIndexOf_FindsLastIndex(string value)
    {
        for (var i = 0; i < 10; ++i)
            _sut.Add(value);
        _sut.Add(value + " excess");
        Assert.That(_sut.LastIndexOf(value), Is.EqualTo(9));
    }

    [TestCaseSource(nameof(CollectionsSource))]
    public void ForEach_ActsOnAllElements(System.Collections.Generic.IEnumerable<string> values)
    {
        _sut.AddRange(values);
        var result = new List<string>();
        _sut.ForEach(value => result.Add(value));
        Assert.That(result, Is.EquivalentTo(values));
    }

    [TestCaseSource(nameof(CollectionsWithEmptySource))]
    public void Reverse_ValuesAreReversed(System.Collections.Generic.IEnumerable<string> values)
    {
        _sut.AddRange(values);
        _sut.Reverse();
        values.Reverse(); // System list
        Assert.That(_sut, Is.EquivalentTo(values));
    }

    [TestCaseSource(nameof(CollectionsSource))]
    public void TrueForAll_TrueIfAllAreTrue(System.Collections.Generic.IEnumerable<string> values)
    {
        _sut.AddRange(values);
        Assert.That(_sut.TrueForAll(value => value != "random value"), Is.True);
    }

    [TestCaseSource(nameof(CollectionsSource))]
    public void TrueForAll_FalseIfAnyAreFalse(System.Collections.Generic.IEnumerable<string> values)
    {
        _sut.Add("some random value");
        _sut.AddRange(values);
        Assert.That(_sut.TrueForAll(value => value == values.First()), Is.False);
    }

    #endregion

    #region Removing Values

    [Test]
    public void Clear_EmptyList_CountAndValuesAreUpdated()
    {
        _sut.Clear();

        Assert.That(_sut.Count, Is.EqualTo(0));
        Assert.That(_sut.Empty, Is.True);
    }

    [TestCaseSource(nameof(CollectionsWithEmptySource))]
    public void Clear_NonEmptyList_CountAndValuesAreUpdated(System.Collections.Generic.IEnumerable<string> collection)
    {
        _sut = new List<string>(collection);

        _sut.Clear();

        Assert.That(_sut.Count, Is.EqualTo(0));
        Assert.That(_sut.Empty, Is.True);
    }

    [TestCaseSource(nameof(SingleValuesSource))]
    public void Clear_NonEmptyList_ValueNoLongerPresent(string value)
    {
        _sut.Add(value);

        _sut.Clear();

        Assert.That(_sut.Contains(value), Is.False);
    }

    [TestCaseSource(nameof(SingleValuesSource))]
    public void Remove_EmptyList_CouldNotRemove(string value)
    {
        Assert.That(_sut.Remove(value), Is.False);
        Assert.That(_sut.Count, Is.EqualTo(0));
    }

    [TestCaseSource(nameof(SingleValuesSource))]
    public void Remove_NonEmptyList_CouldNotRemoveNonExistingValue(string value)
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
    public void RemoveAll_NoMatches(string value)
    {
        _sut.Add(value);

        _sut.RemoveAll(str => str == "negative two");
        Assert.That(_sut.Contains(value), Is.True);
        Assert.That(_sut.Count, Is.EqualTo(1));
    }

    [TestCaseSource(nameof(SingleValuesSource))]
    public void RemoveAll_OneMatch(string value)
    {
        _sut.Add(value);
        _sut.Add(value + ".");

        _sut.RemoveAll(str => str == value);
        Assert.That(_sut.Contains(value), Is.False);
        Assert.That(_sut.Count, Is.EqualTo(1));
    }

    [TestCaseSource(nameof(SingleValuesSource))]
    public void RemoveAll_ManyMatches(string value)
    {
        _sut.Add(value);
        if (value != null) _sut.Add(value + ".");

        _sut.RemoveAll(str => str == null || str.StartsWith(value));
        Assert.That(_sut.Contains(value), Is.False);
        Assert.That(_sut.Count, Is.EqualTo(0));
    }

    [Test]
    public void TrimTest()
    {
        for (var i = 0; i < 12; ++i)
            _sut.Add(i.ToString());
        for (var i = 0; i < 10; ++i)
            _sut.RemoveAt(0);
        Assert.That(_sut, Is.EquivalentTo(new []{"10", "11"}));
    }

    #endregion
}