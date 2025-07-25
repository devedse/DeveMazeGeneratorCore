namespace DeveMazeGeneratorCore.Coaster3MF.Models
{
    /// <summary>
    /// </summary>
    /// <param name="PartId">1, 3, 5, 7</param>
    /// <param name="ObjectId">2, 4, 6, 8</param>
    /// <param name="ModelId">1, 2, 3, 4</param>
    /// <param name="MeshData">The mesh data for the model</param>
    public record ThreeMFModel(int PartId, int ObjectId, int ModelId, MeshData MeshData);
}
