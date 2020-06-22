
using System.Reflection;
using Bunit.Rendering;

/// <summary> Exposes the internal class `Htmlizer` from `Bunit.Web`.  </summary>
namespace Bunit
{
    static class Htmlizer
    {
        private static readonly MethodInfo _mi;
        static Htmlizer()
        {
            var bunitAssembly = System.Reflection.Assembly.GetAssembly(typeof(Bunit.SnapshotTest));
            _mi = bunitAssembly!.GetType("Bunit.Htmlizer")!.GetMethod("GetHtml")!;
        }

        public static string GetHtml(ITestRenderer renderer, int componentId)
        {
            return (string)_mi.Invoke(null, new object[] { renderer, componentId })!;
        }
    }
}