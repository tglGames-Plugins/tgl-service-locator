using System;
using UnityEngine;

namespace TGL.ServiceLocator
{
	/// <summary>
	/// Automates the initial setup, configuration, and loading of components
	/// </summary>
	[DisallowMultipleComponent, RequireComponent(typeof(ServiceLocator))]
	public abstract class Bootstrapper : MonoBehaviour
	{
		private ServiceLocator attachedLocator;
		internal ServiceLocator AttachedServiceLocator
		{
			get
			{
				if (attachedLocator != null)
				{
					return attachedLocator;
				}
				else if(GetComponent<ServiceLocator>() != null)
				{
					attachedLocator = GetComponent<ServiceLocator>();
				}
				
				return attachedLocator;
			}
		}

		private bool hasBeenBootstrapped;

		private void Awake()
		{
			BootstrapOnDemand();
		}

		/// <summary>
		/// Attaches and configures a <see cref="ServiceLocator"/> by calling it's configure methods<br/>
		/// <see cref="ServiceLocator.ConfigureAsGlobal"/>, <see cref="ServiceLocator.ConfigureForScene"/> and <see cref="ServiceLocator.ConfigureForGameObject"/> methods
		/// </summary>
		public void BootstrapOnDemand()
		{
			if(hasBeenBootstrapped) return;
			hasBeenBootstrapped = true;
			Bootstrap();
		}
		
		protected abstract void Bootstrap();
	}
}