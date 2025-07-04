using EasyButtons;
using MyBox;
using UnityEngine;

namespace SensenToolkit
{
    public class SfxDebugPlayer : MonoBehaviour
    {
        [Header("Btn1")]
        [SerializeField] private AudioProfile _sfxBtn01_1;
        [SerializeField] private AudioProfile _sfxBtn01_2;
        [SerializeField] private AudioProfile _sfxBtn01_3;
        [Header("Btn2")]
        [SerializeField] private AudioProfile _sfxBtn02_1;
        [SerializeField] private AudioProfile _sfxBtn02_2;
        [SerializeField] private AudioProfile _sfxBtn02_3;
        [Header("Btn3")]
        [SerializeField] private AudioProfile _sfxBtn03_1;
        [SerializeField] private AudioProfile _sfxBtn03_2;
        [SerializeField] private AudioProfile _sfxBtn03_3;

        private SfxService SfxPlayer => SfxService.Instance;

        [Button]
        private void PlayBtn1Sfx()
        {
            if (_sfxBtn01_1 != null) SfxPlayer.Play(_sfxBtn01_1);
            if (_sfxBtn01_2 != null) SfxPlayer.Play(_sfxBtn01_2);
            if (_sfxBtn01_3 != null) SfxPlayer.Play(_sfxBtn01_3);
        }

        [Button]
        private void PlayBtn2Sfx()
        {
            if (_sfxBtn02_1 != null) SfxPlayer.Play(_sfxBtn02_1);
            if (_sfxBtn02_2 != null) SfxPlayer.Play(_sfxBtn02_2);
            if (_sfxBtn02_3 != null) SfxPlayer.Play(_sfxBtn02_3);
        }

        [Button]
        private void PlayBtn3Sfx()
        {
            if (_sfxBtn03_1 != null) SfxPlayer.Play(_sfxBtn03_1);
            if (_sfxBtn03_2 != null) SfxPlayer.Play(_sfxBtn03_2);
            if (_sfxBtn03_3 != null) SfxPlayer.Play(_sfxBtn03_3);
        }
    }
}
