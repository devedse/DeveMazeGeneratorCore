using System;

namespace DeveMazeGeneratorCore.InnerMaps
{
    public class CachedInnerMap : InnerMap
    {
        private InnerMap _currentMapPart = new UndefinedInnerMap(int.MinValue, int.MinValue) { StartX = int.MinValue, StartY = int.MinValue }; //-1 because no nullchecks needed etc :)
        private InnerMap[] _mapParts;
        public int GridSize { get; private set; }

        private int _currentMapCycleFactor = 0;

        private Func<int, int, int, int, InnerMap> _generatePartAction;
        private Action<InnerMap> _savePartAction;

        public CachedInnerMap(int width, int height, int caches, int gridSize, Func<int, int, int, int, InnerMap> generatePartAction, Action<InnerMap> savePartAction) : base(width, height)
        {
            GridSize = gridSize;

            _mapParts = new InnerMap[caches];

            _generatePartAction = generatePartAction;
            _savePartAction = savePartAction;

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

            return _currentMapPart.PathData[xthing, ything];
        }

        public override bool this[int x, int y]
        {
            get
            {
                EnsureMapPartLoaded(x, y);

                int ything = y % GridSize;
                int xthing = x % GridSize;

                return _currentMapPart[xthing, ything];
            }
            set
            {
                EnsureMapPartLoaded(x, y);

                int ything = y % GridSize;
                int xthing = x % GridSize;

                _currentMapPart[xthing, ything] = value;
            }
        }

        private void EnsureMapPartLoaded(int x, int y)
        {
            if (x < _currentMapPart.StartX || x >= _currentMapPart.StartX + GridSize || y < _currentMapPart.StartY || y >= _currentMapPart.StartY + GridSize)
            {
                LoadNewMapPart(x / GridSize, y / GridSize);
            }
        }

        public void LoadNewMapPart(int x, int y)
        {
            for (int i = 0; i < _mapParts.Length; i++)
            {
                var thisListMap = _mapParts[i];
                if (thisListMap != null && x == thisListMap.StartX / GridSize && y == thisListMap.StartY / GridSize)
                {
                    _currentMapPart = _mapParts[i];
                    return;
                }
            }

            _currentMapPart = _generatePartAction(x * GridSize, y * GridSize, GridSize, GridSize);

            if (_mapParts[_currentMapCycleFactor] != null)
            {
                _savePartAction(_mapParts[_currentMapCycleFactor]);
            }
            //place it at the place of the old one
            _mapParts[_currentMapCycleFactor] = _currentMapPart;

            //Turn the cyclething
            _currentMapCycleFactor++;
            if (_currentMapCycleFactor >= _mapParts.Length)
            {
                _currentMapCycleFactor = 0;
            }
        }
    }
}
