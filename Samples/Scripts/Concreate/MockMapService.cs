using UnityEngine;

namespace TGL.ServiceLocator.Samples
{
	public class MockMapService : IGameService
	{
		public void StartGame()
		{
			Debug.Log($"This is a dummy method({nameof(StartGame)}) in a mock class({this.GetType().Name})");
		}
	}
}