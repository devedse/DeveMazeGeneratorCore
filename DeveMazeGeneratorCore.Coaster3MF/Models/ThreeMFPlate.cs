namespace DeveMazeGeneratorCore.Coaster3MF.Models
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="PlateId">The id of the plate</param>
    /// <param name="Models"></param>
    public record ThreeMFPlate(int PlateId, List<ThreeMFModel> Models);
}
