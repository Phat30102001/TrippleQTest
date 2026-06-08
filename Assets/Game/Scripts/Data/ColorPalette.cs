using System;
using System.Collections.Generic;
using UnityEngine;

namespace PeopleFlow.Data
{
    /// <summary>
    /// Shared lookup mapping a logical <see cref="MinionColor"/> to a display color and material.
    /// One asset is shared by the whole game so adding/removing colors never touches code.
    /// </summary>
    [CreateAssetMenu(menuName = "PeopleFlow/Color Palette", fileName = "ColorPalette")]
    public class ColorPalette : ScriptableObject
    {
        [Serializable]
        public struct Entry
        {
            public MinionColor id;
            public Color color;
            public Material material;
        }

        [SerializeField] private List<Entry> entries = new List<Entry>();

        private Dictionary<MinionColor, Entry> _lookup;

        private void BuildLookup()
        {
            _lookup = new Dictionary<MinionColor, Entry>(entries.Count);
            foreach (var e in entries)
                _lookup[e.id] = e;
        }

        public Color GetColor(MinionColor id)
        {
            if (_lookup == null) BuildLookup();
            return _lookup.TryGetValue(id, out var e) ? e.color : Color.magenta;
        }

        public Material GetMaterial(MinionColor id)
        {
            if (_lookup == null) BuildLookup();
            return _lookup.TryGetValue(id, out var e) ? e.material : null;
        }

        public bool Has(MinionColor id)
        {
            if (_lookup == null) BuildLookup();
            return _lookup.ContainsKey(id);
        }

        /// <summary>Call after editing entries at runtime/editor to refresh the cache.</summary>
        public void Invalidate() => _lookup = null;
    }
}
