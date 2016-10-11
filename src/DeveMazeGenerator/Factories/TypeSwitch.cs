using System;
using System.Collections.Generic;

namespace DeveMazeGenerator.Factories
{
    public class TypeSwitch<T>
    {
        Dictionary<Type, Func<T>> matches = new Dictionary<Type, Func<T>>();

        public TypeSwitch<T> Case<Typ>(Func<Typ> action) where Typ : T
        {
            matches.Add(typeof(Typ), () => action());
            return this;
        }

        public T Switch(Type type)
        {
            Func<T> val;
            var foundMatch = matches.TryGetValue(type, out val);
            if (foundMatch)
            {
                return val();
            }
            else
            {
                throw new ArgumentException($"There's no Case defined for type: '{type}'.");
            }
        }
    }
}
