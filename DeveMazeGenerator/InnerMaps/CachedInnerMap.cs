using System;

namespace DeveMazeGenerator.InnerMaps
{
    public class CachedInnerMap : InnerMap
    {
        private InnerMap currentMapPart = new UndefinedInnerMap(int.MinValue, int.MinValue) { StartX = int.MinValue, StartY = int.MinValue }; //-1 because no nullchecks needed etc :)
        private InnerMap[] mapParts;
        public int GridSize { get; private set; }

        private int currentMapCycleFactor = 0;

        private Func<int, int, int, int, InnerMap> GeneratePartAction;
        private Action<InnerMap> SavePartAction;

        public CachedInnerMap(int width, int height, int caches, int gridSize, Func<int, int, int, int, InnerMap> generatePartAction, Action<InnerMap> savePartAction) : base(width, height)
        {
            this.GridSize = gridSize;

            mapParts = new InnerMap[caches];

            this.GeneratePartAction = generatePartAction;
            this.SavePartAction = savePartAction;

            PathData = new ForwardingInnerMap(width, height, (x, y) =>
            {
                return GetPathData(x, y);
            });
        }

        private bool GetPathData(int x, int y)
        {
            EnsureMapPartLoaded(x, y);

            int ything = y % GridSize;
            int xthing = x % GridSize;

            return currentMapPart.PathData[xthing, ything];
        }

        public override bool this[int x, int y]
        {
            get
            {
                EnsureMapPartLoaded(x, y);

                int ything = y % GridSize;
                int xthing = x % GridSize;

                return currentMapPart[xthing, ything];
            }
            set
            {
                EnsureMapPartLoaded(x, y);

                int ything = y % GridSize;
                int xthing = x % GridSize;

                currentMapPart[xthing, ything] = value;
            }
        }

        private void EnsureMapPartLoaded(int x, int y)
        {
            if (x < currentMapPart.StartX || x >= currentMapPart.StartX + GridSize || y < currentMapPart.StartY || y >= currentMapPart.StartY + GridSize)
            {
                LoadNewMapPart(x / GridSize, y / GridSize);
            }
        }

        public void LoadNewMapPart(int x, int y)
        {
            for (int i = 0; i < mapParts.Length; i++)
            {
                var thisListMap = mapParts[i];
                if (thisListMap != null && x == thisListMap.StartX / GridSize && y == thisListMap.StartY / GridSize)
                {
                    currentMapPart = mapParts[i];
                    return;
                }
            }

            currentMapPart = GeneratePartAction(x * GridSize, y * GridSize, GridSize, GridSize);

            if (mapParts[currentMapCycleFactor] != null)
            {
                SavePartAction(mapParts[currentMapCycleFactor]);
            }
            //place it at the place of the old one
            mapParts[currentMapCycleFactor] = currentMapPart;

            //Turn the cyclething
            currentMapCycleFactor++;
            if (currentMapCycleFactor >= mapParts.Length)
            {
                currentMapCycleFactor = 0;
            }
        }
    }
}
