namespace DeveMazeGeneratorCore.Coaster3MF.Models
{
    public class Quad
    {
        public Vertex V1 { get; set; }
        public Vertex V2 { get; set; }
        public Vertex V3 { get; set; }
        public Vertex V4 { get; set; }
        public string PaintColor { get; set; }

        public Quad(Vertex v1, Vertex v2, Vertex v3, Vertex v4, string paintColor)
        {
            V1 = v1;
            V2 = v2;
            V3 = v3;
            V4 = v4;
            PaintColor = paintColor;
        }
    }
}