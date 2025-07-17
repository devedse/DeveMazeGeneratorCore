namespace DeveMazeGeneratorCore.Coaster3MF.Models
{
    public class Triangle
    {
        public int V1 { get; set; }
        public int V2 { get; set; }
        public int V3 { get; set; }
        public string PaintColor { get; set; }

        public Triangle(int v1, int v2, int v3, string paintColor)
        {
            V1 = v1;
            V2 = v2;
            V3 = v3;
            PaintColor = paintColor;
        }
    }
}