using System;
using System.Linq;
using Collections;
using NUnit.Framework;

namespace Tests;

[TestFixture]
public class PriorityQueueTests
{
    private PriorityQueue<string, int> _sut;

    #region Test Case Generators

    public static System.Collections.Generic.IEnumerable<TestCaseData> CollectionsSource()
    {
        yield return new TestCaseData(new (string, int)[]{ new("one", 1) });
        yield return new TestCaseData(new System.Collections.Generic.List<(string, int)>{ new("one", 1) });
        yield return new TestCaseData(new System.Collections.Generic.HashSet<(string, int)>{ new("one", 1) });

        yield return new TestCaseData(new (string, int)[]{ new("one", 1), new("two", 2) });
        yield return new TestCaseData(new System.Collections.Generic.List<(string, int)>{ new("one", 1), new("two", 2) });
        yield return new TestCaseData(new System.Collections.Generic.HashSet<(string, int)>{ new("one", 1), new("two", 2) });

        yield return new TestCaseData(new (string, int)[]{ new(string.Empty, 0), new("one", 1), new("two", 2) });
        yield return new TestCaseData(new System.Collections.Generic.List<(string, int)>{ new(string.Empty, 0), new("one", 1), new("two", 2) });
        yield return new TestCaseData(new System.Collections.Generic.HashSet<(string, int)>{ new(string.Empty, 0), new("one", 1), new("two", 2) });
    }


    public static System.Collections.Generic.IEnumerable<TestCaseData> CollectionsWithEmptySource()
    {
        yield return new TestCaseData(System.Array.Empty<(string, int)>());
        yield return new TestCaseData(new System.Collections.Generic.List<(string, int)>());
        yield return new TestCaseData(new System.Collections.Generic.HashSet<(string, int)>());

        foreach (var testCase in CollectionsSource())
            yield return testCase;
    }

    #endregion

    [SetUp]
    public void SetUp()
    {
        _sut = new MaxPriorityQueue<string, int>();
    }

    #region Constructors

    [TestCaseSource(nameof(CollectionsWithEmptySource))]
    public void Constructor_CopiesCollection(System.Collections.Generic.IEnumerable<(string, int)> collection)
    {
        _sut = new MaxPriorityQueue<string, int>(collection);

        var sutValues = new System.Collections.Generic.HashSet<(string, int)>();
        while (!_sut.Empty)
        {
            _sut.TryDequeue(out var value, out var priority);
            sutValues.Add(new ValueTuple<string, int>(value, priority));
        }

        Assert.That(sutValues, Is.EquivalentTo(collection.ToHashSet()));
    }

    #endregion

    #region Enqueue

    [Test]
    public void Enqueue_HasCorrectOrdering()
    {
        const int size = 1000;
        var values = new (string, int)[size];
        for (var i = 0; i < values.Length; ++i)
        {
            var random = Random.Shared.Next(0, 100);
            values[i] = (random.ToString(), random);
        }

        _sut.EnqueueRange(values);
        var inOrder = new (string, int)[size];
        for (var i = 0; i < inOrder.Length; ++i)
        {
            _sut.TryDequeue(out var value, out var priority);
            inOrder[i] = (value, priority);
        }
        Assert.That(inOrder, Is.EquivalentTo(values.OrderByDescending(item => item.Item2)));
    }

    [Test]
    public void Enqueue_MinQueue_HasCorrectOrdering()
    {
        _sut = new MinPriorityQueue<string, int>();
        const int size = 1000;
        var values = new (string, int)[size];
        for (var i = 0; i < values.Length; ++i)
        {
            var random = Random.Shared.Next(0, 100);
            values[i] = (random.ToString(), random);
        }

        _sut.EnqueueRange(values);
        var inOrder = new (string, int)[size];
        for (var i = 0; i < inOrder.Length; ++i)
        {
            _sut.TryDequeue(out var value, out var priority);
            inOrder[i] = (value, priority);
        }
        Assert.That(inOrder, Is.EquivalentTo(values.OrderBy(item => item.Item2)));
    }

    #endregion

    #region Retrieving Values

    [Test]
    public void Clear_ChangesCountAndClearsTheCollection()
    {
        for (var i = 0; i < 1000; ++i)
            _sut.Enqueue(i.ToString(), i);
        
        _sut.Clear();
        Assert.That(_sut.Count, Is.EqualTo(0));
        Assert.Throws<InvalidOperationException>(() => _sut.Dequeue());
    }

    [Test]
    public void Peek_ThrowsOnEmpty()
    {
        Assert.Throws<InvalidOperationException>(() => _sut.Peek());
    }

    [Test]
    public void TryPeek_FalseIfEmpty()
    {
        _sut.Enqueue("1", 1);
        _sut.Clear();
        Assert.That(_sut.TryPeek(out _, out _), Is.False);
    }

    [Test]
    public void TryPeek_ReturnsValueIfPossible()
    {
        _sut.Enqueue("1", 1);
        _sut.Enqueue("2", 2);
        
        Assert.That(_sut.TryPeek(out var value, out var priority), Is.True);
        Assert.That(value, Is.EqualTo("2"));
        Assert.That(priority, Is.EqualTo(2));
    }

    [Test]
    public void StressTest()
    {
        const int count = 10000;
        for (var i = 0; i < count; ++i)
        {
            _sut.Enqueue(i.ToString(), i);
        }

        for (var i = 0; i < count; ++i)
        {
            Assert.That(_sut.TryDequeue(out _, out _), Is.True);
        }

        Assert.That(_sut.Count, Is.EqualTo(0));
    }

    #endregion
}