using System;
using System.Linq;
using NUnit.Framework;
using Collections;

namespace Tests;

[TestFixture]
public class LinkedListTests
{
    private LinkedList<string> _sut;

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
        yield return new TestCaseData(new System.Collections.Generic.List<string> { "" });
        yield return new TestCaseData(new System.Collections.Generic.List<string> { "one" });
        yield return new TestCaseData(new System.Collections.Generic.List<string> { "one", "two" });
        yield return new TestCaseData(new System.Collections.Generic.List<string> { "null" });
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
        yield return new TestCaseData(new System.Collections.Generic.List<string> { null });
        yield return new TestCaseData(new System.Collections.Generic.HashSet<string> { null });

        yield return new TestCaseData(new System.Collections.Generic.List<string> { "one" });
        yield return new TestCaseData(new System.Collections.Generic.HashSet<string> { "one" });

        yield return new TestCaseData(new System.Collections.Generic.List<string> { "one", "two" });
        yield return new TestCaseData(new System.Collections.Generic.HashSet<string> { "one", "two" });

        yield return new TestCaseData(new System.Collections.Generic.List<string> { string.Empty, "one", "two" });
        yield return new TestCaseData(new System.Collections.Generic.HashSet<string> { string.Empty, "one", "two" });
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
        _sut = new LinkedList<string>();
    }

    #region Constructors

    [TestCaseSource(nameof(CollectionsWithEmptySource))]
    public void Constructor_ValuesAreCopiedFromAnotherCollection(System.Collections.Generic.IEnumerable<string> collection)
    {
        _sut = new LinkedList<string>(collection);
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
    public void AddAfter_ValueIsInsertedCorrectly(string value)
    {
        for (var i = 0; i < 5; ++i)
        {
            _sut.Add(i.ToString());
        }

        var node = _sut.Find("3");
        _sut.AddAfter(node, value);
        Assert.That(node.Next.Value, Is.EqualTo(value));
    }

    [TestCaseSource(nameof(SingleValuesSource))]
    public void AddBefore_ValueIsInsertedCorrectly(string value)
    {
        for (var i = 0; i < 5; ++i)
        {
            _sut.Add(i.ToString());
        }

        var node = _sut.Find("3");
        _sut.AddBefore(node, value);
        Assert.That(node.Previous.Value, Is.EqualTo(value));
    }

    [Test]
    public void StressTest()
    {
        const int count = 10000;
        for (var i = 0; i < count; ++i)
        {
            _sut.Add(i.ToString());
        }

        for (var i = 0; i < count; ++i)
        {
            Assert.That(_sut.Remove(i.ToString()), Is.True);
        }

        Assert.That(_sut.Count, Is.EqualTo(0));
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
        Assert.That(target, Is.EquivalentTo(_sut.Concat(new[] { target[0] })));
    }

    [TestCaseSource(nameof(MultipleValuesSource))]
    public void Find_GetsValueIfPresent(System.Collections.Generic.List<string> values)
    {
        _sut = new LinkedList<string>(values);

        Assert.That(_sut.Find(values[0]), Is.Not.EqualTo(default(string)));
    }

    [TestCaseSource(nameof(MultipleValuesSource))]
    public void Find_GetsFirstOccurence(System.Collections.Generic.List<string> values)
    {
        _sut = new LinkedList<string>(values);
        _sut.AddLast(values[0]);

        Assert.That(_sut.Find(values[0]).Next, Is.Not.Null);
    }

    [TestCaseSource(nameof(MultipleValuesSource))]
    public void Find_ReturnsNullIfNotPresent(System.Collections.Generic.List<string> values)
    {
        _sut = new LinkedList<string>(values);

        Assert.That(_sut.Find("Some random string"), Is.Null);
    }

    [TestCaseSource(nameof(MultipleValuesSource))]
    public void FindLast_GetsValueIfPresent(System.Collections.Generic.List<string> values)
    {
        _sut = new LinkedList<string>(values);

        Assert.That(_sut.FindLast(values[0]), Is.Not.EqualTo(default(string)));
    }

    [TestCaseSource(nameof(MultipleValuesSource))]
    public void FindLast_GetsFirstOccurence(System.Collections.Generic.List<string> values)
    {
        _sut = new LinkedList<string>(values);
        _sut.AddLast(values[0]);

        Assert.That(_sut.FindLast(values[0]).Next, Is.Null);
    }

    [TestCaseSource(nameof(MultipleValuesSource))]
    public void FindLast_ReturnsNullIfNotPresent(System.Collections.Generic.List<string> values)
    {
        _sut = new LinkedList<string>(values);

        Assert.That(_sut.FindLast("Some random string"), Is.Null);
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
        _sut = new LinkedList<string>(collection);

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
    public void RemoveFirst_CorrectValueIsRemoved(string value)
    {
        const string randomValue = "some random value";
        _sut.AddFirst(value);
        _sut.AddLast(randomValue);
        _sut.RemoveFirst();

        Assert.That(_sut.Count, Is.EqualTo(1));
        Assert.That(_sut.First.Value, Is.EqualTo(randomValue));
    }

    [TestCaseSource(nameof(SingleValuesSource))]
    public void RemoveLast_CorrectValueIsRemoved(string value)
    {
        const string randomValue = "some random value";
        _sut.AddFirst(randomValue);
        _sut.AddLast(value);
        _sut.RemoveLast();

        Assert.That(_sut.Count, Is.EqualTo(1));
        Assert.That(_sut.Last.Value, Is.EqualTo(randomValue));
    }

    #endregion

    #region Deriving Types

    [TestCaseSource(nameof(SingleValuesSource))]
    public void Queue_HasCorrectOrdering(string value)
    {
        var queue = new Queue<string>();
        queue.Enqueue(value);
        queue.Enqueue("some random value");

        Assert.That(queue.Dequeue(), Is.EqualTo(value));
        Assert.That(queue.Count, Is.EqualTo(1));
        Assert.That(queue.Dequeue(), Is.Not.EqualTo(value));
    }

    [TestCaseSource(nameof(SingleValuesSource))]
    public void Stack_HasCorrectOrdering(string value)
    {
        var stack = new Stack<string>();
        stack.Push(value);
        stack.Push("some random value");

        Assert.That(stack.Pop(), Is.Not.EqualTo(value));
        Assert.That(stack.Count, Is.EqualTo(1));
        Assert.That(stack.Pop(), Is.EqualTo(value));
    }

    #endregion
}
