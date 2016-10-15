using System;

namespace DeveMazeGenerator.InnerMaps
{
    public class UndefinedInnerMap : InnerMap
    {
        public UndefinedInnerMap(int width, int height) : base(width, height)
        {

        }

        public override bool this[int x, int y]
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }
    }
}
