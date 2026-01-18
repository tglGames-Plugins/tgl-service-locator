using UnityEngine;

namespace TGL.ServiceLocator
{
	public class ServiceLocatorSceneBootstrapper : Bootstrapper
	{
		protected override void Bootstrap()
		{
			AttachedServiceLocator.ConfigureForScene();
		}
	}
}