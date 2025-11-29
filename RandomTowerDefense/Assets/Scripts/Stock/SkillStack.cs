using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RandomTowerDefense.Units;

namespace RandomTowerDefense.Systems
{
    static public class SkillStack
    {
        #region Constants and Fields
        /// <summary>
        /// スキルストックの最大数
        /// </summary>
        static public readonly int maxStackNum = 4;

        static private int currStackNum;
        static private int[] stackDetail = { 0, 0, 0, 0 };
        #endregion

        #region Public Methods
        static public void Init()
        {
            currStackNum = 0;
            for (int i = 0; i < stackDetail.Length; ++i)
                stackDetail[i] = 0;
        }

        /// <summary>
        /// 指定したスキルアイテムをストックに追加
        /// </summary>
        /// <param name="itemID">追加するスキルアイテムID</param>
        /// <returns>追加に成功した場合true</returns>
        static public bool AddStock(Upgrades.StoreItems itemID)
        {
            if (CheckFullStocks())
            {
                return false;
            }
            int emptySlot = -1;
            for (int i = 0; i < maxStackNum; ++i)
            {
                if (RetrieveStock(i) == 0)
                {
                    emptySlot = i;
                    break;
                }
            }

            stackDetail[emptySlot] = (int)itemID;
            currStackNum++;
            return true;
        }

        /// <summary>
        /// 指定したスロットのスキルを使用し、ストックから削除
        /// </summary>
        /// <param name="StockID">使用するスロットID</param>
        /// <returns>使用したスキルのID</returns>
        static public int UseStock(int StockID)
        {
            if (stackDetail[StockID] == 0)
            {
                return -1;
            }

            int selectedItem = stackDetail[StockID];
            stackDetail[StockID] = 0;
            currStackNum--;
            return selectedItem;
        }

        /// <summary>
        /// 指定したスロットIDのスキルストックを取得
        /// </summary>
        /// <param name="StockID">取得するスロットのID</param>
        /// <returns>ストックされたスキルのID</returns>
        static public int RetrieveStock(int StockID)
        {
            return stackDetail[StockID];
        }

        /// <summary>
        /// スキルストックが満杯かどうかをチェック
        /// </summary>
        /// <returns>ストックが満杯の場合true</returns>
        static public bool CheckFullStocks()
        {
            return currStackNum >= maxStackNum;
        }
        #endregion
    }
}
