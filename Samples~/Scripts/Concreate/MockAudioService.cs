using UnityEngine;

namespace TGL.ServiceLocator.Samples
{
	public class MockAudioService : IAudioService
	{
		public void Play()
		{
			Debug.Log($"This is a dummy method({nameof(Play)}) in a mock class({this.GetType().Name})");
		}
	}
}