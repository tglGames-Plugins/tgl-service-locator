using UnityEngine;

namespace TGL.ServiceLocator
{
	public class ServiceLocatorGameObjectBootstrapper : Bootstrapper
	{
		protected override void Bootstrap()
		{
			AttachedServiceLocator.ConfigureForGameObject();
		}
	}
}