using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Reflection;
using System.Diagnostics.Contracts;

namespace a7DocumentDbStudio.Utils
{
    /// <summary>
    /// Encapsulates methods for dealing with dependency objects and properties.
    /// </summary>
    internal static class a7DependencyHelper
    {
        /// <summary>
        /// Gets the dependency property according to its name.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns></returns>
        internal static DependencyProperty GetDependencyProperty(Type type, string propertyName)
        {
            DependencyProperty prop = null;

            if (type != null)
            {
                FieldInfo fieldInfo = type.GetField(propertyName + "Property",
                System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);

                if (fieldInfo != null)
                {
                    prop = fieldInfo.GetValue(null) as DependencyProperty;
                }
            }

            return prop;
        }

        /// <summary>
        /// Retrieves a <see cref="DependencyProperty"/> using reflection.
        /// </summary>
        /// <param name="o"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        internal static DependencyProperty GetDependencyProperty(DependencyObject o, string propertyName)
        {
            DependencyProperty prop = null;

            if (o != null)
            {
                prop = GetDependencyProperty(o.GetType(), propertyName);
            }

            return prop;
        }

        internal static bool SetIfNonLocal<T>(this DependencyObject o, DependencyProperty property, T value)
        {
            Contract.Requires(o != null);
            Contract.Requires(property != null);
            
            if (!property.PropertyType.IsAssignableFrom(typeof(T)))
            {
                throw new ArgumentException("Type of dependency property is incompatible with value.");
            }

            BaseValueSource source = DependencyPropertyHelper.GetValueSource(o, property).BaseValueSource;
            if (source != BaseValueSource.Local)
            {
                o.SetValue(property, value);

                return true;
            }

            return false;
        }
    }
}
