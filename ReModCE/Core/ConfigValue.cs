using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MelonLoader;

namespace ReModCE.Core
{
    internal class ConfigValue<T>
    {
        private readonly MelonPreferences_Entry<T> _entry;

        public ConfigValue(string name, T defaultValue, string displayName = null, string description = null, bool isHidden = false)
        {
            var category = MelonPreferences.CreateCategory("ReModCE");
            _entry = category.GetEntry<T>(name) ?? category.CreateEntry(name, defaultValue, displayName, description, isHidden);
        }

        public static implicit operator T(ConfigValue<T> conf)
        {
            return conf._entry.Value;
        }

        public void SetValue(T value)
        {
            _entry.Value = value;
        }

        public override string ToString()
        {
            return _entry.Value.ToString();
        }
    }
}
