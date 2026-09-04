using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Audio/AudioDatabase", fileName = "AudioDatabase")]
public class AudioDatabase : ScriptableObject
{
	[System.Serializable]
	public class ClipEntry
	{
		public AudioClip clip;
		[Range(0, 1)] public float volume = 1f;
	}

	public string sceneName;
	public List<ClipEntry> entries = new List<ClipEntry>();

	public int ValidCount => entries == null ? 0 : entries.FindAll(e => e != null && e.clip != null).Count;
}
