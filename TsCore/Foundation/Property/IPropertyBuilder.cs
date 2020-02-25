using System;
using System.Collections.Generic;
using TsGui.Operation;

namespace TsGui.Foundation.Property
{
    public interface IPropertyBuilder
    {
        IPropertyBuilder Register<T>(string propertyName);
        IPropertyBuilder Register<T>(Func<T> getter,Action<T> setter);
        IPropertyBuilder RegisterReadOnly<T>(Func<T> getter);
        IPropertyBuilder OperationController(IOperationController controller);
        IEnumerable<IProperty> Build();
    }
}
