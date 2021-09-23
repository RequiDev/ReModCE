using System;
using System.Linq;
using MelonLoader;

namespace ReModCE.Core
{
    internal class ConfigValue<T>
    {
        public event Action OnValueChanged;

        private readonly MelonPreferences_Entry<T> _entry;
        private static MelonPreferences_Category _category;

        public T Value => _entry.Value;

        public ConfigValue(string name, T defaultValue, string displayName = null, string description = null, bool isHidden = false)
        {
            _category ??= MelonPreferences.CreateCategory("ReModCE");

            var entryName = string.Concat(name.Where(c => char.IsLetter(c) || char.IsNumber(c)));
            _entry = _category.GetEntry<T>(entryName) ?? _category.CreateEntry(entryName, defaultValue, displayName, description, isHidden);
            _entry.OnValueChangedUntyped += () => OnValueChanged?.Invoke();
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
