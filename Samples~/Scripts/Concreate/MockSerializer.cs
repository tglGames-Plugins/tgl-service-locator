using Unity.VisualScripting;
using UnityEngine;

namespace TGL.ServiceLocator.Samples
{
	public class MockSerializer : ISerializer
	{
		public void Serialize()
		{
			Debug.Log($"This is a dummy method({nameof(Serialize)}) in a mock class({this.GetType().Name})");
		}
	}
}