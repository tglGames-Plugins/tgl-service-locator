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
		#endregion Variables

		#region Properties

		/// <summary>
		/// all services currently registered
		/// </summary>
		public IEnumerable<object> RegisteredServices => servicesDict.Values;

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
			else
			{
#if Testing
				Debug.Log($"Registered the service of type {type.FullName} at {_serviceLocatorType} level");
#endif
			}
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
			else
			{
#if Testing
				Debug.Log($"Registered the service of type {type.FullName} at {_serviceLocatorType} level");
#endif
			}
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
		
		#endregion PublicMethods
	}
}