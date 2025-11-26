using System.Collections;

namespace RandomTowerDefense.Utilities
{
    /// <summary>
    /// 汎用的なユーティリティメソッドを提供する静的クラス
    /// </summary>
    public static class Utility {

    /// <summary>
    /// Fisher-Yatesアルゴリズムを使用して配列をシャッフル
    /// </summary>
    /// <typeparam name="T">配列要素の型</typeparam>
    /// <param name="array">シャッフルする配列</param>
    /// <param name="seed">乱数生成用のシード値</param>
    /// <returns>シャッフルされた配列</returns>
	public static T[] ShuffleArray<T>(T[] array, int seed) {
		System.Random prng = new System.Random (seed);

		for (int i =0; i < array.Length -1; i ++) {
			int randomIndex = prng.Next(i,array.Length);
			T tempItem = array[randomIndex];
			array[randomIndex] = array[i];
			array[i] = tempItem;
		}

		return array;
	}

}
}
