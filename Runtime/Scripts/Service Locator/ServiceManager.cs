#define _Testing
using System;
using System.Collections.Generic;
using UnityEngine;

namespace TGL.ServiceLocator
{
	public class ServiceManager
	{
		#region Variables
		/// <summary>
		/// Hashset for all services registered
		/// </summary>
		private readonly Dictionary<Type, object> servicesDict = new Dictionary<Type, object>();

		private readonly ServiceLocator _serviceLocator;
		#endregion Variables

		#region Properties

		/// <summary>
		/// all services currently registered
		/// </summary>
		public Dictionary<Type, object> GetRegistrationCopy => new Dictionary<Type, object>(servicesDict);

#if Testing
		/// <summary>
		/// scope of this ServiceManager
		/// </summary>
		public ServiceLocatorType _serviceLocatorType = ServiceLocatorType.Undefined;
		
		/// <summary>
		/// all types of services currently registered
		/// </summary>
		public IEnumerable<Type> RegisteredTypes => servicesDict.Keys;
#endif

		#endregion Properties

		#region Constructor

		public ServiceManager(ServiceLocator serviceLocator)
		{
			_serviceLocator = serviceLocator;
		}

		#endregion Constructor

		#region PublicMethods

		#region Register
		
		/// <summary>
		/// Used to register a service where type is auto-defined by the variable type used to register it.
		/// </summary>
		/// <param name="service">The service to register</param>
		/// <typeparam name="T">auto-assigned type of the service</typeparam>
		/// <returns>Reference of the service manager that registered the service</returns>
		public ServiceManager RegisterService<T>(T service)
		{
			Type type = typeof(T);
			
			if (!servicesDict.TryAdd(type, service))
			{
				Debug.LogError($"{nameof(ServiceManager)}.{nameof(RegisterService)}: Service of type {type.FullName} already registered");
				return null; // failed to add so cannot return the ServiceManager which registered the service
			}
#if Testing
			Debug.Log($"Registered the service of type {type.FullName} at {_serviceLocatorType} level");
#endif
			return this;
		}
		
#if Testing
		public ServiceManager RegisterService<T>(T service, ServiceLocatorType serviceLocatorType)
		{
			_serviceLocatorType = serviceLocatorType;
			return RegisterService<T>(service);
		}
		
		public ServiceManager RegisterService(Type type, object service, ServiceLocatorType serviceLocatorType)
		{
			_serviceLocatorType = serviceLocatorType;
			return RegisterService(type, service);
		}
#endif

		/// <summary>
		/// Used to register a service where type is user-defined <br/>
		/// This is used when you have many parent child services and you want to ensure you use the correct type for registration
		/// </summary>
		/// <param name="type">The explicitly defined type you want to use</param>
		/// <param name="service">The actual service to register</param>
		/// <returns>Reference of the service manager that registered the service</returns>
		public ServiceManager RegisterService(Type type, object service)
		{
			if (!type.IsInstanceOfType(service))
			{
				throw new ArgumentException($"Type of service({type.FullName}) does not match type of service variable({service.GetType().FullName})", nameof(service));
			}
			
			if (!servicesDict.TryAdd(type, service))
			{
				Debug.LogError($"{nameof(ServiceManager)}.{nameof(RegisterService)}: Service of type {type.FullName} already registered");
				return null;
			}
#if Testing
			Debug.Log($"Registered the service of type {type.FullName} at {_serviceLocatorType} level");
#endif
			return this;
		}
		
		/// <summary>
		/// unregisters the passed service if present in the dictionary
		/// </summary>
		/// <param name="service">The service to unregister</param>
		/// <typeparam name="T">The type of service we are trying to unregister</typeparam>
		/// <returns>Reference of the service manager that unregistered the service</returns>
		/// <exception cref="ArgumentException">Exception if the passed service type was not present in the dictionary</exception>
		public ServiceManager UnRegisterService<T>(T service)
		{
			Type type = typeof(T);
			if (!servicesDict.ContainsKey(type))
			{
				Debug.LogError($"We do not have the service type ({type.FullName}) that you are trying to unregister", _serviceLocator);
				if (servicesDict.ContainsValue(service))
				{
					Debug.LogWarning($"We have a service of type {service.GetType().FullName} registered which might be what you wanted", _serviceLocator);
					throw new ArgumentException($"We have a regsitered type {service.GetType().FullName}, but you are trying to unregister {type.FullName} which is wrong");
				}
				else
				{
					throw new ArgumentException($"you are trying to unregister {type.FullName} which is not available in the service locator");
				}
			}
			
			servicesDict.Remove(type);
			return this;
		}

		/// <summary>
		/// unregisters the passed type if present in the dictionary
		/// </summary>
		/// <param name="type">The type of service we want to unregister</param>
		/// <returns>Reference of the service manager that unregistered the service</returns>
		/// <exception cref="ArgumentException">Exception if the passed type was not present in the dictionary</exception>
		public ServiceManager UnRegisterType(Type type)
		{
			if (!servicesDict.ContainsKey(type))
			{
				Debug.LogError($"We do not have the service type ({type.FullName}) that you are trying to unregister", _serviceLocator);
				throw new ArgumentException($"you are trying to unregister {type.FullName} which is not available in the service locator");
			}
			
			servicesDict.Remove(type);
			return this;
		}
		
		#endregion Register

		#region Get

		/// <summary>
		/// Returns a registered service or throws an error
		/// </summary>
		/// <typeparam name="T">Type of service being fetched</typeparam>
		/// <returns>the service which was requested</returns>
		/// <exception cref="ArgumentException">exception if the service was not registered or had some registration issue</exception>
		public T GetService<T>() where T : class
		{
			Type type = typeof(T);
			if (servicesDict.TryGetValue(type, out object obj))
			{
				if (type.IsInstanceOfType(obj))
				{
					return obj as T;
				}
				throw new ArgumentException($"Type of service searched({type.FullName}) does not match type of service found({obj.GetType().FullName})");
			}
			throw new ArgumentException($"{nameof(ServiceManager)}.{nameof(GetService)}: Service of type {type.FullName} not registered");
		}
		
		/// <summary>
		/// Returns status if we successfully found a service being requested by type
		/// </summary>
		/// <param name="serviceFound">The service requested from the method</param>
		/// <typeparam name="T">The type of service being requested</typeparam>
		/// <returns>status of successfully finding the service</returns>
		public bool TryGetService<T>(out T serviceFound) where T : class
		{
			Type type = typeof(T);
			if (servicesDict.TryGetValue(type, out object obj))
			{
				if (type.IsInstanceOfType(obj))
				{
					serviceFound = obj as T;
					return true;
				}
			}

			serviceFound = null;
			return false;
		}

		#endregion Get

		#region HasServiceOrType
		/// <summary>
		/// Does the dictionary have the passed service as a key in Dictionary
		/// </summary>
		/// <param name="service">the passed service we want to test</param>
		/// <typeparam name="T">the type of the passed service we want to test</typeparam>
		/// <returns>bool stating whether the service exists</returns>
		public bool HasService<T>(T service)
		{
			Type type = typeof(T);
			if (servicesDict.ContainsKey(type))
			{
				return true;
			}

			if (servicesDict.ContainsValue(service))
			{
				Debug.LogWarning($"The service passed is present of type ({service.GetType().FullName}) but you are checking for type ({type.FullName})", _serviceLocator);
			}

			return false;
		}

		/// <summary>
		/// Does the dictionary have the passed type as a key in Dictionary
		/// </summary>
		/// <param name="type">The type of service we want to test</param>
		/// <returns>bool stating whether the key exists</returns>
		public bool HasType(Type type)
		{
			return servicesDict.ContainsKey(type);
		}
		#endregion HasServiceOrType
		#endregion PublicMethods
	}
}