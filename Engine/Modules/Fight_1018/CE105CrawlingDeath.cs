using YapYapDraw.Engine.ModuleSetup;

namespace YapYapDraw.Modules.Fight_1018;

public class CE105CrawlingDeath : DrawModule
{
    public override ModuleInfo ModuleInfo => new ModuleInfo
    {
        Category = Category.Foray,
        GroupType = GroupType.CriticalEngagement,
        GroupID = 1018u,
        NameID = 36u
    };

    public override string Author => "Res";
}
