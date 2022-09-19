using System.Collections.Generic;
using System.Linq;
using DeconTools.Backend;

namespace DeconTools.Workflows.Backend.Data
{
    public class TopdownChromData
    {
        private List<TopdownChromDataItem> _chromDataItems;

        public TopdownChromData()
        {
            _chromDataItems = new List<TopdownChromDataItem>();
        }

        #region Properties

        #endregion

        #region Public Methods

        public List<XYData> GetChromData()
        {
            var allData = new List<XYData>();

            var listOfScanLists = new List<List<int>>();

            //get min and max scan value
            foreach (var topdownChromDataItem in _chromDataItems)
            {
                listOfScanLists.Add(topdownChromDataItem.ChromData.Keys.ToList());
            }

            var combinedScanList = UnionAll(listOfScanLists);
            combinedScanList.Sort();

            if (combinedScanList.Count==0)
            {
                return null;
            }

            foreach (var topdownChromDataItem in _chromDataItems)
            {
                foreach (var scan in combinedScanList)
                {
                    if (!topdownChromDataItem.ChromData.ContainsKey(scan))
                    {
                        topdownChromDataItem.ChromData.Add(scan, 0);
                    }
                }

                var data = new XYData();
                data.Xvalues = topdownChromDataItem.ChromData.Keys.Select(key => (double) key).ToArray();
                data.Yvalues = topdownChromDataItem.ChromData.Values.ToArray();

                allData.Add(data);
            }

            return allData;
        }

        public List<T> UnionAll<T>(IEnumerable<IEnumerable<T>> lists)
        {
            HashSet<T> hashSet = null;
            foreach (var list in lists)
            {
                if (hashSet == null)
                {
                    hashSet = new HashSet<T>(list);
                }
                else
                {
                    hashSet.UnionWith(list);
                }
            }
            return hashSet == null ? new List<T>() : hashSet.ToList();
        }

        #endregion

        #region Private Methods

        public void AddChromDataItem (XYData chromData)
        {
            if (chromData == null || chromData.Xvalues == null || chromData.Xvalues.Length < 1)
            {
                return;
            }

            var chromDataItem = new TopdownChromDataItem();
            chromDataItem.ChromData = new SortedDictionary<int, double>();

            for (var i = 0; i < chromData.Xvalues.Length; i++)
            {
                chromDataItem.ChromData.Add((int)chromData.Xvalues[i], chromData.Yvalues[i]);
            }

            _chromDataItems.Add(chromDataItem);
        }

        #endregion

    }
}
