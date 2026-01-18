#define _Testing

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TGL.ServiceLocator
{
	public class ServiceLocator : MonoBehaviour
	{
		private static ServiceLocator globalLocator;
		private static Dictionary<Scene, ServiceLocator> SceneContainers = new Dictionary<Scene, ServiceLocator>();
		private static List<GameObject> tmpSceneGameObjects;
		
		private ServiceManager serviceManager;
		private static readonly string k_globalServiceLocatorName = "ServiceLocator [Global]";
		private static readonly string k_sceneServiceLocatorName = "ServiceLocator [Scene]";
		
		public ServiceLocatorType serviceLocatorType = ServiceLocatorType.Undefined;
#if Testing
		public List<string> allRegisteredTypes;
		private void Update()
		{
			allRegisteredTypes = serviceManager.RegisteredTypes.Select(service => service.FullName).ToList();
		}
#endif
		
		#region ConfiguringServiceLocator

		/// <summary>
		/// Called to set the current <see cref="ServiceLocator"/> for global scope, by default this has the <see cref="MonoBehaviour.DontDestroyOnLoad"/> property set as true
		/// </summary>
		/// <param name="doNotDestroyOnLoad">Does this object get added to <see cref="MonoBehaviour.DontDestroyOnLoad"/> method list?</param>
		internal void ConfigureAsGlobal(bool doNotDestroyOnLoad = true)
		{
			if (transform.parent != null)
			{
				Debug.LogError($"ServiceLocator being configured as Global must be a root GameObject in the scene! " +
				               $"{gameObject.name} is a child of {transform.parent.name}.", this);
				return;
			}
			
			if (globalLocator == null)
			{
				globalLocator = this;
				serviceLocatorType = ServiceLocatorType.Global;
				serviceManager = new ServiceManager(this);
				if (doNotDestroyOnLoad)
				{
					DontDestroyOnLoad(gameObject);
				}
				Debug.Log($"Global Service Locator is bootstrapped!", gameObject);
			}
			else if (globalLocator == this)
			{
				Debug.LogWarning($"We are trying to register same ServiceLocator as Global allocator once more!", gameObject);
			}
			else
			{
				Debug.LogError($"We are trying to register two ServiceLocator as global, this must be an error!");
			}
		}

		/// <summary>
		/// Called to set the current <see cref="ServiceLocator"/> for the current Scene scope. The service locator has the same scope as whichever scene the current gameObject resides in 
		/// </summary>
		internal void ConfigureForScene()
		{
			if (transform.parent != null)
			{
				Debug.LogError($"ServiceLocator configured at Scene level must be a root GameObject in the scene! " +
				               $"{gameObject.name} is a child of {transform.parent.name}.", this);
				return;
			}
			
			Scene scene = gameObject.scene;
			if (SceneContainers.TryGetValue(scene, out ServiceLocator attachedLocator))
			{
				Debug.LogError($"We are trying to attach two gameobjects ({attachedLocator.name}) and ({gameObject.name}) as SceneContainers for the same scene", gameObject);
				return;
			}
			serviceLocatorType = ServiceLocatorType.Scene;
			serviceManager = new ServiceManager(this);
			SceneContainers.Add(scene, this);
			SceneManager.sceneUnloaded += OnSceneRemoved;
			Debug.Log($"Scene Service Locator is bootstrapped!");
		}

		/// <summary>
		/// Called to set the current <see cref="ServiceLocator"/> for the current GameObject scope. The service locator has the same scope as whichever GameObject the current ServiceLocator is attached to
		/// </summary>
		internal void ConfigureForGameObject()
		{
			ServiceLocator[] gobScopedSLs = gameObject.GetComponents<ServiceLocator>();
			if (gobScopedSLs.Length != 1 || gobScopedSLs[0] != this)
			{
				Debug.LogError($"One Object cannot have more than one {nameof(ServiceLocator)} attached to it.", gameObject);
				return;
			}
			serviceLocatorType = ServiceLocatorType.GameObject;
			serviceManager = new ServiceManager(this);
		}
		
		#endregion ConfiguringServiceLocator
		
		#region GettingAServiceLocator
		/// <summary>
		/// provides the Global ServiceLocator for the app 
		/// </summary>
		public static ServiceLocator GetSlGlobal
		{
			get
			{
				if (globalLocator != null)
				{
					// if globalLocator is already assigned, return it.
					return globalLocator;
				}
				
				// Find the Bootstrapper for global Service locator if available, we might have only missed bootstrapping it to our locator
				if (FindFirstObjectByType<ServiceLocatorGlobalBootstrapper>() is { } foundBootstrapper)
				{
					foundBootstrapper.BootstrapOnDemand();
					return globalLocator;
				}
				
				// Alternative way for same code, for better understanding, 'slBootstrapper' is same as 'foundBootstrapper'
				/*
				var slBootstrapper = FindFirstObjectByType<ServiceLocatorGlobalBootstrapper>();
				if (slBootstrapper != null) 
				{
					slBootstrapper.BootstrapOnDemand();
					return globalLocator;
				}
				*/

				// create a new global service locator and bootstrap it
				GameObject globalContainer = new GameObject(k_globalServiceLocatorName, typeof(ServiceLocator));
				globalContainer.AddComponent<ServiceLocatorGlobalBootstrapper>().BootstrapOnDemand();
				
				// globalLocator is set by BootstrapOnDemand() if it is at global level irrespective of who calls it. 
				return globalLocator;
			}
		}

		/// <summary>
		/// returns a Service locator which is scoped to the scene in which the passed MonoBehaviour resides<br/>
		/// if no Service locator is scoped to this scene, we can return Global Service locator
		/// </summary>
		/// <param name="contextMb">A MonoBehaviour from the scene where we are looking for the service locator</param>
		/// <returns>returns a ServiceLocator scoped to the scene in which <paramref name="contextMb"/> resides</returns>
		public static ServiceLocator GetSlForSceneOf(MonoBehaviour contextMb)
		{
			Scene scopeScene = contextMb.gameObject.scene;
			
			// looking in SceneContainers and validating the entry is not for a destroyed service locator
			if (SceneContainers.TryGetValue(scopeScene, out ServiceLocator sLocator))
			{
				if (sLocator != null)
				{
					return sLocator;
				}
				else
				{
					Debug.LogWarning($"we have the scene but do not have a {nameof(ServiceLocator)} attached to this scene in {nameof(SceneContainers)}", contextMb);
				}
			}
			else
			{
				// look for a Service locator in the scene which we might have missed to add to 'SceneContainers' list 
				tmpSceneGameObjects.Clear();
				scopeScene.GetRootGameObjects(tmpSceneGameObjects);
				
				// find all objects in this scene where a service locator is attached and the scope is ServiceLocatorType.Scene
				IEnumerable<ServiceLocatorSceneBootstrapper> sceneLocatorContainers = tmpSceneGameObjects.Where(x => x.GetComponent<ServiceLocatorSceneBootstrapper>() != null).Select(gob => gob.GetComponent<ServiceLocatorSceneBootstrapper>());
				foreach (ServiceLocatorSceneBootstrapper sceneBootstrap in sceneLocatorContainers)
				{
					// if contextMb was a scene scoped service locator, we messed up somewhere as 'SceneContainers' should have had a reference to this.
					if (sceneBootstrap.AttachedServiceLocator != contextMb)
					{
						sceneBootstrap.BootstrapOnDemand(); // This adds the newly found 'ServiceLocator' by calling 'ConfigureForScene' in 'ServiceLocatorSceneBootstrapper'
						return sceneBootstrap.AttachedServiceLocator;
					}
					else
					{
						Debug.LogWarning($"We found an attached service locator in same object which was passed, is this an error? Did you call {nameof(ConfigureForScene)} from this object", contextMb);
						return null;
					}
				}
			}
			
			// return global Service locator as we did not find a 'ServiceLocator' in the 'SceneContainers' or in the scene root objects
			return GetSlGlobal;
		}

		/// <summary>
		/// returns a ServiceLocator scoped on the passed MonoBehaviour or it's parent,<br/>
		/// if no ServiceLocator is found, we find one scoped to the scene in which this MonoBehaviour resides, (<see cref="GetSlForSceneOf"/>)<br/>
		/// If this also fails, we return the <see cref="GetSlGlobal"/> ServiceLocator 
		/// </summary>
		/// <param name="contextMb">a MonoBehaviour for the context/scope of the GameObject</param>
		/// <returns>returns a ServiceLocator scoped on the passed MonoBehaviour or it's parent</returns>
		public static ServiceLocator GetSlForGameObjectOf(MonoBehaviour contextMb)
		{
			ServiceLocator gobServiceLocator = contextMb.GetComponentInParent<ServiceLocator>();
			// if no ServiceLocator for the GameObject is found, find the ServiceLocator for the scene
			if (gobServiceLocator == null)
			{
				gobServiceLocator = GetSlForSceneOf(contextMb);
			}
			return gobServiceLocator;
		}
		#endregion GettingAServiceLocator

		#region RegisterOrGetService

		/// <summary>
		/// Registers a service to this service locator<br/>
		/// If you pass a concrete service as an interface variable, the service will be registered as the interface type
		/// </summary>
		/// <param name="service">The service to register</param>
		/// <typeparam name="T">The type this service is registered as</typeparam>
		public void Register<T>(T service)
		{
#if Testing
			serviceManager.RegisterService(service, serviceLocatorType);
#else
			serviceManager.RegisterService(service);
#endif
		}

		/// <summary>
		/// Registers a service of a specific type to this service locator<br/>
		/// The type is passed as an argument, so whatever is passed will be used for registration
		/// </summary>
		/// <param name="type">The type this service is registered as</param>
		/// <param name="service">The service to register</param>
		public void Register(Type type, object service)
		{
#if Testing
			serviceManager.RegisterService(type, service, serviceLocatorType);
#else
			serviceManager.RegisterService(type, service);
#endif
		}
		
		/// <summary>
		/// Searches a service of passed type from current ServiceLocator to the global ServiceLocator, <br/>
		/// if a ServiceLocator is found with the passed service, we return the ServiceLocator along with the service <br/>
		/// if a ServiceLocator is not found, we search till we reach global ServiceLocator and then return null. 
		/// </summary>
		/// <param name="service">the service we found is returned as a out parameter</param>
		/// <typeparam name="T">The type of service we want to find</typeparam>
		/// <exception cref="ArgumentException">Exception if we fail to find the service being requested</exception>
		public void Get<T>(out T service) where T : class
		{ 
			if (TryGetDirect<T>(out service))
			{
				// found the service in current locator, can return it
				return;
			}

			if (this == globalLocator) // global is top level for service locator, if we did not find it here, we cannot search anywhere else
			{
				service = null;
				return;
			}
			
			// Find the next higher level ServiceLocator
			int traversedLocatorCount = 0;
			ServiceLocator currScope = this;
			while (TryGetNextServiceLocatorInHierarchy(currScope, out ServiceLocator attachedLocator))
			{
				// check if this ServiceLocator has the service registered
				if (attachedLocator.TryGetDirect<T>(out service))
				{
					return;
				}

				currScope = attachedLocator;
				traversedLocatorCount++;
				
				// for Safety which avoids infinite loop
					if (traversedLocatorCount > 10)
					{
						Debug.LogWarning($"U need better design, 10 locators but no object founc?");
						break;
					}
			}
			
#if Testing
			Debug.LogWarning($"Could not find any more ServiceLocator after searching extra {traversedLocatorCount} times. For a service of type '{typeof(T).FullName}'");
#endif
			throw new ArgumentException($"Could not resolve type '{typeof(T).FullName}'.");
		}

		/// <summary>
		/// Tries to get the service in the current ServiceLocator
		/// </summary>
		/// <param name="service">The service we found</param>
		/// <typeparam name="T">The type of service we are searching for</typeparam>
		/// <returns>returns bool with status of success result</returns>
		private bool TryGetDirect<T>(out T service) where T : class
		{
			return serviceManager.TryGetService<T>(out service);
		}

		/// <summary>
		/// Wrapper for <see cref="Get{T}"/> method, helps us by returning bool to inform the state
		/// </summary>
		/// <param name="service">the service we found is returned as a out parameter</param>
		/// <typeparam name="T">The type of service we want to find</typeparam>
		/// <returns>status of <see cref="Get{T}"/> method request, success of failure</returns>
		public bool TryGet<T>(out T service) where T : class
		{
			try
			{
				Get(out service);
				return true;
			}
			catch (ArgumentException argumentException)
			{
#if Testing
				Debug.LogWarning($"Could not find a service with type {typeof(T).FullName} at {nameof(ServiceLocator)} at {serviceLocatorType} level" + argumentException.Message);
#else
				_ = argumentException;
#endif
			}

			service = null;
			return false;
		}
		
		/// <summary>
		/// Tries to get the ServiceLocator in the order of level<br/>
		/// Starting from current Object level, to scene level, then global
		/// </summary>
		/// <param name="attachedLocator">The next level ServiceLocator found by this method</param>
		/// <returns>bool status of success or failure in finding another service locator</returns>
		bool TryGetNextServiceLocatorInHierarchy(ServiceLocator currScope, out ServiceLocator attachedLocator)
		{
			if (currScope == globalLocator || currScope.serviceLocatorType == ServiceLocatorType.Global)
			{
				// global ServiceLocator 'globalLocator' is top most level, so there is no more level above this 
				attachedLocator = null;
				return false;
			}
			attachedLocator = currScope.serviceLocatorType < ServiceLocatorType.Scene ? GetSlForSceneOf(this) : GetSlGlobal;
			return attachedLocator != null;
		}

		/// <summary>
		/// confirms if this ServiceLocator irrespective of hierarchy has the service registered locally
		/// </summary>
		/// <typeparam name="T">the type of service needed</typeparam>
		/// <returns>do we have this service registered locally?</returns>
		public bool HasService<T>() where T : class
		{
			return serviceManager.TryGetService<T>(out T service); 
		}

		#endregion RegisterOrGetService

		#region UnRegisterService

		/// <summary>
		/// Unregisters the service which was registered previously, throws error on Argument type if it was not registered previously
		/// </summary>
		/// <param name="service">The service to unregister</param>
		/// <typeparam name="T">The type of service which is being unregistered</typeparam>
		public void UnRegister<T>(T service)
		{
			serviceManager.UnRegisterService(service);
		}
		
		/// <summary>
		/// Unregisters the type of service which was registered previously, throws error on Argument type if it was not registered previously
		/// </summary>
		/// <param name="type">the type of service which was registered previously</param>
		public void UnRegister(Type type)
		{
			serviceManager.UnRegisterType(type);
		}

		#endregion UnRegisterService
		
		#region RemoveServiceLocatorSetup

		private void OnDestroy()
		{
			if (this == globalLocator)
			{
				globalLocator = null;
				return;
			}
			
			if(SceneContainers.ContainsValue(this))
			{
				SceneContainers.Remove(gameObject.scene);
			}
			
			if (serviceLocatorType == ServiceLocatorType.Scene)
			{
				SceneManager.sceneUnloaded -= OnSceneRemoved;
			}
		}

		#endregion RemoveServiceLocatorSetup

		#region OnSceneReload
		
		/// <summary>
		/// Due to attribute, this method is called when Unity's runtime systems are being initialized, specifically during the subsystem registration phase.
		/// </summary>
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
		static void ResetStatics()
		{
			globalLocator = null;
			SceneContainers = new Dictionary<Scene, ServiceLocator>();
			tmpSceneGameObjects = new List<GameObject>();
		}


		/// <summary>
		/// When a scene is unloaded, we do not want to keep ghost references for it in <see cref="SceneContainers"/>
		/// </summary>
		/// <param name="unloadedScene">The scene which was unloaded</param>
		private void OnSceneRemoved(Scene unloadedScene)
		{
			SceneContainers.Remove(unloadedScene);
		}

		#endregion OnSceneReload

		#region AddingServiceLocatorInScene
#if UNITY_EDITOR
		[MenuItem("GameObject/ServiceLocator/Add Global")]
		static void AddGlobal()
		{
			var go = new GameObject(k_globalServiceLocatorName, typeof(ServiceLocatorGlobalBootstrapper));
		}
		
		[MenuItem("GameObject/ServiceLocator/Add Scene")]
		static void AddScene()
		{
			var go = new GameObject(k_sceneServiceLocatorName, typeof(ServiceLocatorSceneBootstrapper));
			//ServiceLocatorGameObjectBootstrapper
		}
		
		[MenuItem("GameObject/ServiceLocator/Add for GameObject")]
		static void AddGameObject()
		{
			GameObject selectedObject = Selection.activeGameObject;
			if (selectedObject != null)
			{
				selectedObject.AddComponent<ServiceLocatorGameObjectBootstrapper>();
				Debug.Log($"Added ServiceLocatorGameObjectBootstrapper to {selectedObject.name}");
			}
			else
			{
				Debug.LogWarning("Please select a GameObject in the hierarchy to add ServiceLocatorGameObjectBootstrapper.");
			}
		}
#endif
		#endregion AddingServiceLocatorInScene

		#region Debugging
		
		[ContextMenu("PrintAllRegisteredServices")]
		public void PrintAllRegisteredServices()
		{
			Debug.Log($"Debugging All Registered services at {serviceLocatorType} level", this);
			// plan is to print 1 log per service with first log limited to scope of service locator and count of registered services
			// each registered service log will contain the key type and the value type, which will help identify what was wrong in case of errors.
			Dictionary<Type, object> registeredServices = serviceManager.GetRegistrationCopy;
			int index = 1;
			foreach (KeyValuePair<Type, object> registeredService in registeredServices)
			{
				Debug.Log($"Entry {index:00} :: KeyType: {registeredService.Key.FullName} - ValueType: {registeredService.Value.GetType().FullName}");
			}
		}
		#endregion Debugging
	}
}