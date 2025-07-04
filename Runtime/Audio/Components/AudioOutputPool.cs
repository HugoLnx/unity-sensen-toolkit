using System.Collections.Generic;
using SensenToolkit;
using UnityEngine;

namespace SensenToolkit
{
    public class AudioOutputPool : APermanentPrefabPoolBase<AudioOutputPool, AudioOutput, AudioSource>
    {
        protected override AudioOutput InstantiateNew(SimpleExpandablePool<AudioOutput> _)
        {
            AudioSource source = Instantiate(_prefab, this.transform);
            source.name = $"[{Creations.Count + 1}] {_prefab.name}";
            AudioOutput output = new(source, this);
            output.OnFinishedPlaying += Release;
            return output;
        }
    }
}
