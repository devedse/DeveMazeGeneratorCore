using System;

namespace DeveMazeGeneratorCore.InnerMaps
{
    public class ForwardingInnerMap : InnerMap
    {
        private Func<int, int, bool> _thisAction;
        public ForwardingInnerMap(int width, int height, Func<int, int, bool> thisAction) : base(width, height)
        {
            _thisAction = thisAction;
        }

        public override bool this[int x, int y]
        {
            get
            {
                return _thisAction(x, y);
            }
            set
            {
                throw new NotImplementedException();
            }
        }
    }
}
