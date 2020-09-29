namespace DeveMazeGenerator.InnerMaps
{
    internal class UndefinedInnerMap : InnerMap
    {
        public UndefinedInnerMap(int width, int height) : base(width, height)
        {

        }

        public override bool this[int x, int y]
        {
            get
            {
                return false;
            }

            set
            {

            }
        }
    }
}
