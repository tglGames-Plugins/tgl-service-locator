using System;
using System.Collections;
using UnityEngine;

namespace TGL.ServiceLocator.Samples
{
	public class ServiceTester : MonoBehaviour
	{
		private IAudioService audioService;
		private IGameService gameService;
		private ILocalization localizationService;
		private ISerializer serializerService;
		private MockMapService mapService;
		


		#region MonoBehaviourMethods

		private void Awake()
		{
			// InitializeTheServicesDirectly(); // Alternative way for ease of understanding
			RegisterServicesDirectly();
		}

		private IEnumerator StartDemo()
		{
			yield return null;
			Debug.Log($"Start of Testing");
			yield return null;
			
			// ServiceLocator.Global.Get(out localizationService); // Need to re-check
			// ServiceLocator.ForSceneOf(this).Get(out localizationService); // Need to re-check
			// ServiceLocator.For(this).Get(out localizationService); // Need to re-check
			ServiceLocator.GetSlForGameObjectOf(this)
				// .Get(out serializerService); // GameObject level registration attempted // registered at Scene Level
				// .Get(out localizationService); // Global level registration attempted // registered at Global Level
				.Get(out gameService); // 
			yield return null;
			
			if (serializerService != null)
			{
				Debug.Log($"Found serializerService and running method");
				serializerService.Serialize();
			}
			else
			{
				Debug.LogWarning($"serializerService is null");
			}

			if (localizationService != null)
			{
				Debug.Log($"Found localizationService and running method " +  localizationService.GetLocalizedWord("RandomKey"));
			}
			else
			{
				Debug.LogWarning($"localizationService is null");
			}

			if (gameService != null)
			{
				Debug.Log($"Found gameService and running method");
				gameService.StartGame();
			}
			else
			{
				Debug.LogWarning($"gameService is null");
			}
			
			Debug.Log($"Complete!");
		}

		private IEnumerator Start()
		{
			yield return null;
			Debug.Log($"Start of Testing");
			yield return null;
			ServiceLocator gobServiceLocator = ServiceLocator.GetSlForGameObjectOf(this);
			if (gobServiceLocator == null)
			{
				Debug.LogWarning($"Could not find any ServiceLocator for use");
				yield break;
			}
			else
			{
				Debug.Log($"Found a 'sl' at GameObject level, it's actual level: "+gobServiceLocator.serviceLocatorType);
			}
			yield return null;
			yield return TestAudioService(gobServiceLocator);
			yield return null;
			yield return TestLocalization(gobServiceLocator);
			yield return null;
			yield return TestSerializer(gobServiceLocator);
			yield return null;
			yield return TestGameService(gobServiceLocator);
			yield return null;
			Debug.Log($"Complete!");
		}

		private IEnumerator TestAudioService(ServiceLocator gobServiceLocator)
		{
			yield return null;
			gobServiceLocator.Get(out audioService);
			if (audioService != null)
			{
				Debug.Log($"gobServiceLocator has a service that returned us {audioService.GetType().FullName} type");
				audioService.Play();
				yield return null;
			}
			yield return null;
		}

		private IEnumerator TestLocalization(ServiceLocator gobServiceLocator)
		{
			yield return null;
			gobServiceLocator.Get(out localizationService);
			if (localizationService != null)
			{
				Debug.Log($"gobServiceLocator has a service that returned us {audioService.GetType().FullName} type : " + localizationService.GetLocalizedWord(string.Empty) );
				yield return null;
			}
			yield return null;
		}

		private IEnumerator TestSerializer(ServiceLocator gobServiceLocator)
		{
			yield return null;
			if (gobServiceLocator.TryGet(out serializerService))
			{
				Debug.Log($"gobServiceLocator has a service of {serializerService.GetType().FullName} type");
				serializerService.Serialize();
				yield return null;
			}
			yield return null;
		}

		private IEnumerator TestGameService(ServiceLocator gobServiceLocator)
		{
			yield return null;
			if (gobServiceLocator.HasService<IGameService>())
			{
				Debug.Log($"gobServiceLocator has a service of {nameof(IGameService)} type");
			}
			else
			{
				Debug.Log($"gobServiceLocator does not have a service of type {nameof(IGameService)}");
			}
			yield return null;
			
			if (gobServiceLocator.HasService<MockMapService>())
			{
				Debug.Log($"gobServiceLocator has a service of {nameof(MockMapService)} type");
			}
			else
			{
				Debug.Log($"gobServiceLocator does not have a service of type {nameof(MockMapService)}");
			}
			yield return null;
			
			if (gobServiceLocator.HasService<MockGameService>())
			{
				Debug.Log($"gobServiceLocator has a service of {nameof(MockGameService)} type");
			}
			else
			{
				Debug.Log($"gobServiceLocator does not have a service of type {nameof(MockGameService)}");
			}
			yield return null;

			if (!gobServiceLocator.TryGet(out gameService))
			{
				Debug.LogWarning($"Could not find a gameService even though it was registered because of type mismatch (registered as {nameof(MockMapService)}, Getting as {nameof(IGameService)})");
			}

			if (!gobServiceLocator.TryGet<MockMapService>(out mapService))
			{
				Debug.LogError($"Someone has changed Something, in the code originally provided, gameService was instance of {nameof(MockMapService)}");
			}
			else
			{
				mapService.StartGame();
			}
			
			yield return null;
		}
		
		#endregion MonoBehaviourMethods

		/// <summary>
		/// Registers a service while initializing it.
		/// </summary>
		private void RegisterServicesDirectly()
		{
			// Global Level service
			ServiceLocator.GetSlGlobal?.Register(localizationService = new MockLocalizer()); // this way we can register our service directly at the Global level. It is registered as 'ILocalization' type of service
			// scene level service
			ServiceLocator.GetSlForSceneOf(this)?.Register(typeof(MockMapService), gameService = new MockMapService()); // this way we can register our service directly in the scene level ServiceLocator. It is registered as 'MockMapService' type of service
			// GameObject level service
			ServiceLocator.GetSlForGameObjectOf(this)?.Register(audioService = new MockAudioService()); // this way we can register our service directly in the same GameObject level ServiceLocator. It is registered as 'IAudioService' type of service
			ServiceLocator.GetSlForGameObjectOf(this)?.Register(serializerService = new MockSerializer()); // It is registered as 'ISerializer' type of service
		}

		/// <summary>
		/// Initialized a service then registers it
		/// </summary>
		private void InitializeTheServices()
		{
			// initialize the services
			localizationService = new MockLocalizer();
			audioService = new MockAudioService();
			serializerService = new MockSerializer();
			
			// We can use same interface type with different implementations
			gameService = new MockGameService();
			// gameService = new MockMapService();
			
			
			// Register a service globally
			ServiceLocator.GetSlGlobal?.Register(localizationService); // registers this service as a global service. Here the type will be the final concrete type (MockLocalizer)
			// Register a service at scene level
			ServiceLocator.GetSlForSceneOf(this)?.Register(typeof(IGameService), gameService); // registers a service at scene level with a specific passed type (IGameService)
			// Register a service at GameObject level 
			ServiceLocator.GetSlForGameObjectOf(this)?.Register(audioService);
			ServiceLocator.GetSlForGameObjectOf(this).Register(serializerService);
		}
	}
}