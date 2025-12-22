using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.InputSystem;

namespace SensenToolkit
{
    public class BindingPlusCollection : IReadOnlyList<BindingPlus>
    {
        private List<BindingPlus> _bindings;
        public BindingPlus this[int index] => _bindings[index];
        public int Count => _bindings.Count;

        private HashSet<string> _keys;
        public HashSet<string> Keys => _keys ??= _bindings.Select(b => b.Key).ToHashSet();

        private HashSet<string> _equivalenceKeys;

        public HashSet<string> EquivalenceKeys => _equivalenceKeys ??= _bindings
            .Select(b => b.EquivalenceKey)
            .ToHashSet();

        private BindingPlusCollection(List<BindingPlus> bindings)
        {
            _bindings = bindings;
        }

        public static BindingPlusCollection Build(IEnumerable<BindingPlus> bindings)
        {
            var bindingsList = bindings
                .Where(b => !b.Binding.isPartOfComposite)
                .ToList();
            return new BindingPlusCollection(bindingsList);
        }

        public static BindingPlusCollection BuildNormalized(IEnumerable<BindingPlus> originalBindings)
        {
            List<BindingPlus> normalizedList = new();
            HashSet<string> equivalenceKeys = new();
            foreach (BindingPlus binding in originalBindings)
            {
                if (binding.IsPartOfComposite
                || equivalenceKeys.Contains(binding.EquivalenceKey)) continue;
                normalizedList.Add(binding);
                equivalenceKeys.Add(binding.EquivalenceKey);
            }
            return new BindingPlusCollection(normalizedList);
        }

        public IEnumerator<BindingPlus> GetEnumerator() => _bindings.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public IEnumerable<BindingPlus> Flattened()
        {
            foreach (BindingPlus plus in _bindings)
            {
                Assertx.IsFalse(plus.IsPartOfComposite,
                    "Top-level BindingPlus in BindingPlusCollection should not be part of a composite.");
                yield return plus;

                bool shouldIterateOverParts = plus.IsComposite && plus.CompositeChildren != null;
                if (!shouldIterateOverParts) continue;

                foreach (BindingPlus child in plus.CompositeChildren)
                {
                    Assertx.IsTrue(child.IsPartOfComposite,
                        "Child BindingPlus in CompositeChildren should be part of a composite.");
                    yield return child;
                }
            }
        }

        public BindingPlusCollection WithPrependedDefaultBindings(
            BindingPlusCollection defaultBindings)
        {
            List<BindingPlus> newBindings = new();
            HashSet<string> equivalenceKeys = new();

            BindingPlusCollection customBindings = this.WithOnlyCustom();

            foreach (BindingPlus defaultBinding in defaultBindings)
            {
                bool hasEquivalentInCustomBindings = customBindings.EquivalenceKeys
                    .Contains(defaultBinding.EquivalenceKey);
                if (hasEquivalentInCustomBindings) continue;
                newBindings.Add(defaultBinding);
                equivalenceKeys.Add(defaultBinding.EquivalenceKey);
            }

            foreach (BindingPlus plus in customBindings)
            {
                bool hasAddedEquivalentAlready = equivalenceKeys.Contains(plus.EquivalenceKey);
                if (hasAddedEquivalentAlready) continue;
                newBindings.Add(plus);
                equivalenceKeys.Add(plus.EquivalenceKey);
            }

            return BindingPlusCollection.Build(newBindings);
        }

        public BindingPlusCollection WithOnlyCustom()
        {
            var customBindings = _bindings
                .Where(b => b.IsCustomBinding)
                .ToList();
            return BindingPlusCollection.Build(customBindings);
        }

        public BindingPlusCollection WithOnlyDefaults()
        {
            var defaultBindings = _bindings
                .Where(b => !b.IsDefaultBinding)
                .ToList();
            return BindingPlusCollection.Build(defaultBindings);
        }

        public void ReplaceActionBindings(InputAction action)
        {
            List<InputBinding> bindings = new();
            foreach (BindingPlus plus in Flattened())
            {
                bindings.Add(plus.Binding);
            }

            InputUtils.EnsureActionDisabled(action, () =>
            {
                InputUtils.RemoveAllBindings(action);

                for (int i = 0; i < bindings.Count; i++)
                {
                    InputUtils.AddNextBindingsToAction(action, bindings, ref i);
                }
            });
        }

        public bool ContainsEquivalent(BindingPlus binding)
            => EquivalenceKeys.Contains(binding.EquivalenceKey);

        public BindingPlusCollection WithAppended(BindingPlus binding)
        {
            List<BindingPlus> newBindings = new(_bindings);
            newBindings.Add(binding);
            return BindingPlusCollection.BuildNormalized(newBindings);
        }
    }
}
