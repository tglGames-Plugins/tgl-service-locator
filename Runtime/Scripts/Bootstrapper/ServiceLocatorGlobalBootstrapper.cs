
using UnityEngine;

namespace TGL.ServiceLocator
{
	public class ServiceLocatorGlobalBootstrapper : Bootstrapper
	{
		[SerializeField] private bool doNotDestroyOnLoad = true;
		protected override void Bootstrap()
		{
			AttachedServiceLocator.ConfigureAsGlobal(doNotDestroyOnLoad);
		}
	}
}