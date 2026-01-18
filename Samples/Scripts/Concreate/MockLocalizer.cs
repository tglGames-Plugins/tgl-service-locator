using System;
using System.Collections.Generic;

namespace TGL.ServiceLocator.Samples
{
	public class MockLocalizer : ILocalization
	{
		private readonly List<string> words = new List<string>() { "hound", "Gun", "Chest", "coins" };
		private readonly Random random = new Random();

		public string GetLocalizedWord(string key)
		{
			return words[random.Next(words.Count)];
		}
	}
}