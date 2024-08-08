using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

BenchmarkRunner.Run<BenchmarkSpecs>();

public readonly record struct FakeData(int Id, int SomeProperty);

[MemoryDiagnoser]
public class BenchmarkSpecs
{
    public int[] numbers = Enumerable.Range(1, 10000).ToArray();
    public FakeData[] data;
    public int target = 19999;
    
    public BenchmarkSpecs()
    {
        data = numbers.Select(e => new FakeData(e, e)).ToArray();
    }
    
    [Benchmark]
    public FakeData[] ImprovedForSolutionWithFakeData()
    {
        var dict = new Dictionary<int, FakeData>();
        for (var i = 0; i < data.Length; i++)
        {
            var diff = target - data[i].SomeProperty;
            if (dict.TryGetValue(diff, out var value))
            {
                return [data[i], value];
            }
            dict[data[i].Id] = data[i];
        }
        
        return [];
    }

    [Benchmark]
    public FakeData[] ImprovedLinqSolutionWithFakeData()
    {
        var dict = new Dictionary<int, FakeData>();
        var result = data
            .SelectMany(e =>
            {
                var diff = target - e.SomeProperty;
                if (dict.TryGetValue(diff, out var value))
                {
                    return new[] {e, value};
                }
                dict[e.Id] = e;
                return [];
            })
            .Take(2)
            .ToArray();
        
        return result;
    }
    
    [Benchmark]
    public FakeData[] ForSolutionWithFakeData()
    {
        for (var i1 = 0; i1 < data.Length; i1++)
        {
            for (var i2 = 0; i2 < data.Length; i2++)
            {
                if (data[i2].SomeProperty + data[i1].SomeProperty == target)
                {
                    return [data[i1], data[i2]];
                }   
            }
        }
        
        return [];   
    }
    
    [Benchmark]
    public FakeData[] LinqSolutionWithFakeData() 
        => data
            .SelectMany(e => data
                .Where(a => e.SomeProperty + a.SomeProperty == target)
                .SelectMany(a => CreateFakeDataArr(e, a)))
            .Take(2)
            .ToArray();

    [Benchmark]
    public int[] ImporvedForSolutionWithDictionary()
    {
        var dict = new Dictionary<int, int>();
        for (var i = 0; i < numbers.Length; i++)
        {
            var diff = target - numbers[i];
            if (dict.TryGetValue(diff, out var value))
            {
                return [value + 1, i + 1];
            }
            dict[numbers[i]] = i;
        }
        
        return [];
    }
    
    [Benchmark]
    public int[] ImprovedLinqSolutionWithDictionary()
    {
        var dict = new Dictionary<int, int>();
        
        return numbers
            .SelectMany((e, i) =>
            {
                var diff = target - e;
                if (dict.TryGetValue(diff, out var value))
                {
                    return new[] {value + 1, i + 1};
                }
                dict[e] = i;
                return [];
            })
            .Take(2)
            .ToArray();
    }

    [Benchmark]
    public int[] ForSolution()
    {
        for (var i1 = 0; i1 < numbers.Length; i1++)
        {
            for (var i2 = 0; i2 < numbers.Length; i2++)
            {
                if (numbers[i2] + numbers[i1] == target)
                {
                    return [i1 + 1, i2 + 1];
                }   
            }
        }
        
        return [];   
    }

    [Benchmark]
    public int[] LinqSolution()
    {
        var indexed = numbers
            .Select((n, i) => (n, i))
            .ToArray();
        
        var result = numbers
            .SelectMany((e, i) => indexed
                .Where(a => e + a.n == target)
                .SelectMany(a => CreateArr(i, a.i)))
            .Take(2)
            .ToArray();
        
        return result;
    }
    
    private static IEnumerable<int> CreateArr(int i1, int i2) => [i1 + 1, i2 + 1];
    private static IEnumerable<FakeData> CreateFakeDataArr(FakeData fd1, FakeData fd2) => [fd1, fd2];
}
