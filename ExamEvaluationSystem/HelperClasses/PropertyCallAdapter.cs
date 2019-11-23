/* Property Call Adapter
 * Faster way to get and set property values in runtime
 * Slightly edited version of https://stackoverflow.com/questions/26731159/fastest-way-for-get-value-of-a-property-reflection-in-c-sharp/26733318
 *  Added setter functions then deleted them, they broke it totally
 */

using System;
using System.Collections.Generic;
using System.Reflection;

namespace ExamEvaluationSystem
{
    public interface IPropertyCallAdapter<TThis>
    {
        object InvokeGet(TThis @this);
        // void InvokeSet(TThis @this, object value);
    }

    public class PropertyCallAdapter<TThis, TResult> : IPropertyCallAdapter<TThis>
    {
        private readonly Func<TThis, TResult> _getterInvocation;
        // private readonly Action<TThis, object> _setterInvocation;

        public PropertyCallAdapter(Func<TThis, TResult> getterInvocation /*, Action<TThis, object> setterInvocation */)
        {
            _getterInvocation = getterInvocation;
            // _setterInvocation = setterInvocation;
        }

        public object InvokeGet(TThis @this)
        {
            return _getterInvocation.Invoke(@this);
        }

        /*
        public void InvokeSet(TThis @this, object value)
        {
            _setterInvocation(@this, value);
        }
        */
    }

    public class PropertyCallAdapterProvider<TThis>
    {
        private static readonly Dictionary<string, IPropertyCallAdapter<TThis>> _instances =
            new Dictionary<string, IPropertyCallAdapter<TThis>>();

        public static IPropertyCallAdapter<TThis> GetInstance(string forPropertyName)
        {
            IPropertyCallAdapter<TThis> instance;
            if (!_instances.TryGetValue(forPropertyName, out instance))
            {
                var property = typeof(TThis).GetProperty(
                    forPropertyName,
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

                MethodInfo getMethod;
                // MethodInfo setMethod;
                Delegate getterInvocation;
                // Delegate setterInvocation;

                if (property == null)
                    throw new NullReferenceException("Non-null property is expected while creating dynamic getters/setters");

                if ((getMethod = property.GetGetMethod(true)) != null)
                {
                    var openGetterType = typeof(Func<,>);
                    var concreteGetterType = openGetterType
                        .MakeGenericType(typeof(TThis), property.PropertyType);

                    getterInvocation =
                        Delegate.CreateDelegate(concreteGetterType, null, getMethod);
                }
                else
                    throw new NullReferenceException($"Unable to get getter functions for { forPropertyName }");

                /*
                if ((setMethod = property.GetSetMethod(true)) != null)
                {
                    var openSetterType = typeof(Action<,>);
                    var concreteSetterType = openSetterType
                        .MakeGenericType(typeof(TThis), typeof(object));

                    setterInvocation =
                        Delegate.CreateDelegate(concreteSetterType, null, setMethod);
                }
                else
                    throw new NullReferenceException($"Unable to get setter functions for { forPropertyName }");
                */

                var openAdapterType = typeof(PropertyCallAdapter<,>);
                var concreteAdapterType = openAdapterType
                    .MakeGenericType(typeof(TThis), property.PropertyType);
                instance = Activator
                    .CreateInstance(concreteAdapterType, getterInvocation /*, setterInvocation */)
                        as IPropertyCallAdapter<TThis>;

                _instances.Add(forPropertyName, instance);
            }

            return instance;
        }
    }
}
