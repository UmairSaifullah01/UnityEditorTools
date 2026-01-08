using System;

namespace THEBADDEST
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class ShowNonSerializedFieldAttribute : SpecialCaseDrawerAttribute
    {
    }
}
