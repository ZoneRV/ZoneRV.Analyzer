using System.Linq.Expressions;

namespace ZoneRV.Analyzer.Samples;

public class SalesOrderRequestOptions { }

public class Class1
{
    public int Int { get; set; }

    private SalesOrderRequestOptions Filter { get; set; }
    
    public Func<SalesOrderRequestOptions, int> Func = filter => filter.ToString()!.First();
    public Func<SalesOrderRequestOptions, int, int> Func2 = (filter, i2) => filter.ToString()!.First() + i2;

    private Expression<Func<object?, object, bool>> isNull => (o, o2) => o != null;
    private Expression<Func<object?, bool>> isNull2 => o => o != null;

    public void Test()
    {
        IEnumerable<SalesOrderRequestOptions?> options = [];

        foreach (var filter in options.Where(x => x != null));
    }
}

public class Class2
{
    public void Test()
    {
        Class1? class1 = null;

        if (class1 is null)
            Console.WriteLine("a");

    }
}