using UnityEngine;
using System.Collections;
using System;

/// <summary>
/// ヒープデータ構造 - 優先度付きキュー実装
/// パスフィンディングのオープンリスト管理に使用
/// </summary>
/// <typeparam name="T">ヒープに格納する要素の型</typeparam>
public class Heap<T> where T : IHeapItem<T> {

	private T[] _items;
	private int _currentItemCount;

	/// <summary>
	/// ヒープを初期化
	/// </summary>
	/// <param name="maxHeapSize">ヒープの最大サイズ</param>
	public Heap(int maxHeapSize) {
		_items = new T[maxHeapSize];
	}

	/// <summary>
	/// ヒープに要素を追加
	/// </summary>
	/// <param name="item">追加する要素</param>
	public void Add(T item) {
		item.HeapIndex = _currentItemCount;
		_items[_currentItemCount] = item;
		SortUp(item);
		_currentItemCount++;
	}

	/// <summary>
	/// ヒープから最優先度の要素を削除して返す
	/// </summary>
	/// <returns>最優先度の要素</returns>
	public T RemoveFirst() {
		T firstItem = _items[0];
		_currentItemCount--;
		_items[0] = _items[_currentItemCount];
		_items[0].HeapIndex = 0;
		SortDown(_items[0]);
		return firstItem;
	}

	/// <summary>
	/// ヒープ内の要素を更新
	/// </summary>
	/// <param name="item">更新する要素</param>
	public void UpdateItem(T item) {
		SortUp(item);
	}

	/// <summary>
	/// ヒープ内の要素数を取得
	/// </summary>
	public int Count {
		get {
			return _currentItemCount;
		}
	}

	/// <summary>
	/// 指定した要素がヒープに含まれているかチェック
	/// </summary>
	/// <param name="item">チェックする要素</param>
	/// <returns>含まれている場合true</returns>
	public bool Contains(T item) {
		return Equals(_items[item.HeapIndex], item);
	}

	/// <summary>
	/// ヒープの下方向へソート（親より優先度が低い場合）
	/// </summary>
	/// <param name="item">ソートする要素</param>
	void SortDown(T item) {
		while (true) {
			int childIndexLeft = item.HeapIndex * 2 + 1;
			int childIndexRight = item.HeapIndex * 2 + 2;
			int swapIndex = 0;

			if (childIndexLeft < _currentItemCount) {
				swapIndex = childIndexLeft;

				if (childIndexRight < _currentItemCount) {
					if (_items[childIndexLeft].CompareTo(_items[childIndexRight]) < 0) {
						swapIndex = childIndexRight;
					}
				}

				if (item.CompareTo(_items[swapIndex]) < 0) {
					Swap (item,_items[swapIndex]);
				}
				else {
					return;
				}

			}
			else {
				return;
			}

		}
	}

	/// <summary>
	/// ヒープの上方向へソート（親より優先度が高い場合）
	/// </summary>
	/// <param name="item">ソートする要素</param>
	void SortUp(T item) {
		int parentIndex = (item.HeapIndex-1)/2;

		while (true) {
			T parentItem = _items[parentIndex];
			if (item.CompareTo(parentItem) > 0) {
				Swap (item,parentItem);
			}
			else {
				break;
			}

			parentIndex = (item.HeapIndex-1)/2;
		}
	}

	/// <summary>
	/// ヒープ内の2つの要素を入れ替え
	/// </summary>
	/// <param name="itemA">要素A</param>
	/// <param name="itemB">要素B</param>
	void Swap(T itemA, T itemB) {
		_items[itemA.HeapIndex] = itemB;
		_items[itemB.HeapIndex] = itemA;
		int itemAIndex = itemA.HeapIndex;
		itemA.HeapIndex = itemB.HeapIndex;
		itemB.HeapIndex = itemAIndex;
	}
	
	
	
}

/// <summary>
/// ヒープ要素インターフェース
/// ヒープで管理される要素が実装する必要があるインターフェース
/// </summary>
/// <typeparam name="T">要素の型</typeparam>
public interface IHeapItem<T> : IComparable<T> {
	/// <summary>
	/// ヒープ内のインデックス位置
	/// </summary>
	int HeapIndex {
		get;
		set;
	}
}
