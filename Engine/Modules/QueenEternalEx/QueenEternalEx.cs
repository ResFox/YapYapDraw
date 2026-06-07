using YapYapDraw.Engine.ModuleSetup;

namespace YapYapDraw.Modules.QueenEternalEx;
public class QueenEternalEx : DrawModule
{
    public override ModuleInfo ModuleInfo => new ModuleInfo
    {
        Category = Category.Extreme,
        GroupType = GroupType.CFC,
        GroupID = 1017u
    };
}
