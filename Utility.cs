using System.Collections;

public static class Utility
{
	public static T[] ShuffleArray<T>(T[] array, int seed)
	{
		var prng = new System.Random(seed);
		
		for (var i = 0; i < array.Length - 1; i++)
		{
			var randomIndex = prng.Next(i, array.Length);
			var tempItem = array[randomIndex];
			array[randomIndex] = array[i];
			array[i] = tempItem;
		}
		
		return array;
	}
}
