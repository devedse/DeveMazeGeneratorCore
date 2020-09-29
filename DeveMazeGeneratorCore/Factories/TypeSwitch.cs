using System;
using System.Collections.Generic;

namespace DeveMazeGeneratorCore.Factories
{
    public class TypeSwitch<T>
    {
        Dictionary<Type, Func<T>> _matches = new Dictionary<Type, Func<T>>();

        public TypeSwitch<T> Case<Typ>(Func<Typ> action) where Typ : T
        {
            _matches.Add(typeof(Typ), () => action());
            return this;
        }

        public T Switch(Type type)
        {
            Func<T> val;
            var foundMatch = _matches.TryGetValue(type, out val);
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
