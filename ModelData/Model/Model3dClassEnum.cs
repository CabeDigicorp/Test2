
namespace ModelData.Model
{

    public enum Model3dType
    {
        Unknown = 0,
        Ifc = 1,
        Revit = 2,
    }

    public enum Model3dClassEnum
    {
        Nothing = 0,
        IfcProject = 1,
        IfcSite = 2,
        IfcBuilding = 3,
        IfcBuildingStorey = 4,
        IfcSpace = 5,
        IfcElementType = 6,

        RvtProject = 8,
        RvtLevel = 9,
        RvtRoom = 10,
        RvtElementType = 11,
        RvtGroup = 12,
        RvtAssembly = 13,
    }
}
