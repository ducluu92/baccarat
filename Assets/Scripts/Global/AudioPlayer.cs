using System;
using UnityEngine;

namespace Assets.Scripts.Global
{
    class AudioPlayer : MonoBehaviour
    {

        public AudioSource audiosource;

        private void Start()
        {
            audiosource = GetComponent<AudioSource>();
        }

        public void PlayAudio(AudioClip audioclip)
        {
            if (audioclip != null)
                audiosource.clip = audioclip;

            if (audiosource.clip != null)
                audiosource.Play();
        }


    }
}
