
namespace JBSnorro;

static class TupleExtensions
{

    public static KeyValuePair<TKey, TValue> ToKeyValuePair<TKey, TValue>(this (TKey, TValue) tuple) => new KeyValuePair<TKey, TValue>(tuple.Item1, tuple.Item2);
}
