using System;
using System.Runtime.CompilerServices;

namespace DeveMazeGeneratorCore.InnerMaps
{
    public class CachedInnerMap2<T> where T : class, IMapPart
    {
        private T _currentMapPart = null;
        private T[] _mapParts;
        public int GridSize { get; private set; }

        private int _currentMapCycleFactor = 0;

        private Func<int, int, int, int, T> _generatePartAction;
        private Action<T> _savePartAction;

        public CachedInnerMap2(int caches, int gridSize, Func<int, int, int, int, T> generatePartAction, Action<T> savePartAction)
        {
            GridSize = gridSize;

            _mapParts = new T[caches];

            _generatePartAction = generatePartAction;
            _savePartAction = savePartAction;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool GetMapPoint(int x, int y, Func<T, InnerMap> mapChooser)
        {
            EnsureMapPartLoaded(x, y);

            int ything = y % GridSize;
            int xthing = x % GridSize;

            return mapChooser(_currentMapPart)[xthing, ything];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool SetMapPoint(Func<T, InnerMap> mapChooser, int x, int y, bool value)
        {
            EnsureMapPartLoaded(x, y);

            int ything = y % GridSize;
            int xthing = x % GridSize;

            return mapChooser(_currentMapPart)[xthing, ything] = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void EnsureMapPartLoaded(int x, int y)
        {
            if (_currentMapPart == null || x < _currentMapPart.StartX || x >= _currentMapPart.StartX + GridSize || y < _currentMapPart.StartY || y >= _currentMapPart.StartY + GridSize)
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
