namespace ZoneRV.Analyzer.Samples;

public class SalesOrderRequestOptions { }

public class Class1
{
    public int Int { get; set; }

    private SalesOrderRequestOptions Filter { get; set; }
    
    public Func<SalesOrderRequestOptions, int> Func = filter => filter.ToString()!.First();
    public Func<SalesOrderRequestOptions, int, int> Func2 = (filter, i2) => filter.ToString()!.First() + i2;

    public void Test()
    {
        IEnumerable<SalesOrderRequestOptions> options = [];

        foreach (var filter in options)
        {
            
        }
    }
}

public class Class2<T> where T : SalesOrderRequestOptions
{
    protected T Filter { get; set; } = null!;

    public void Test()
    {
        Console.WriteLine(Filter);
    }
}