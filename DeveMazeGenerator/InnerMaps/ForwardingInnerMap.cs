using System;
using System.Collections.Generic;
using System.Text;

namespace DeveMazeGenerator.InnerMaps
{
    public class ForwardingInnerMap : InnerMap
    {
        private Func<int, int, bool> thisAction;
        public ForwardingInnerMap(int width, int height, Func<int, int, bool> thisAction) : base(width, height)
        {
            this.thisAction = thisAction;
        }

        public override bool this[int x, int y]
        {
            get
            {
                return thisAction(x, y);
            }
            set
            {
                throw new NotImplementedException();
            }
        }
    }
}
