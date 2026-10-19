using System.Collections.Generic;
using UnityEngine;

namespace Air.UI
{
    public sealed class UISpawned
    {
        readonly List<GameObject> items = new List<GameObject>();

        public GameObject Clone(GameObject template, Transform parent = null)
        {
            var copy = Object.Instantiate(template, parent != null ? parent : template.transform.parent);
            copy.SetActive(true);
            items.Add(copy);
            return copy;
        }

        public void Clear()
        {
            for (int i = items.Count - 1; i >= 0; i--)
            {
                if (items[i] != null) Object.DestroyImmediate(items[i]);
            }
            items.Clear();
        }
    }
}
