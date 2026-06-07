using System.Collections.Generic;
using System.Linq;
using YapYapDraw.Engine.Element;
using YapYapDraw.Engine.Helper;
using YapYapDraw.Engine.ModuleSetup;
using YapYapDraw.Engine.Struct;
using YapYapDraw.Engine.Util;
using YapYapDraw.Engine.Vfx;

namespace YapYapDraw.Modules.JeunoArc1;

public class Gigastrike2 : ISpecialAction
{
    public override string Name => "Gigastrike";

    public override HashSet<uint> ActionID
    {
        get
        {
            HashSet<uint> ids = new HashSet<uint>();
            ids.Add(40766u);
            ids.Add(40767u);
            ids.Add(42020u);
            ids.Add(42021u);
            ids.Add(42022u);
            ids.Add(42023u);
            foreach (uint id in CastEnd)
            {
                ids.Add(id);
            }
            return ids;
        }
    }

    private static HashSet<uint> CastEnd => new HashSet<uint> { 40768u, 40769u, 40770u, 40771u, 42024u, 42025u, 42027u, 42028u, 42029u, 42030u };

    public override uint Phase => 4u;

    public override IEnumerable<StaticVfx> ActiveAOEs => aoes.Take(1);

    public override void OnActionCast(ActorCastInfo info)
    {
        switch (info.ActionId)
        {
        case 40766:
            AddAoe(info, 225, 67.5f.Degrees());
            AddAoe(info, 270, -90.Degrees());
            break;
        case 40767:
            AddAoe(info, 225, -67.5f.Degrees());
            AddAoe(info, 270, 90.Degrees());
            break;
        case 42020:
            AddAoe(info, 225, 67.5f.Degrees());
            AddAoe(info, 270, -90.Degrees());
            AddAoe(info, 210, 0.Degrees());
            break;
        case 42021:
            AddAoe(info, 225, 67.5f.Degrees());
            AddAoe(info, 270, -90.Degrees());
            AddAoe(info, 210, 180.Degrees());
            break;
        case 42022:
            AddAoe(info, 225, -67.5f.Degrees());
            AddAoe(info, 270, 90.Degrees());
            AddAoe(info, 210, 0.Degrees());
            break;
        case 42023:
            AddAoe(info, 225, -67.5f.Degrees());
            AddAoe(info, 270, 90.Degrees());
            AddAoe(info, 210, 180.Degrees());
            break;
        }
        void AddAoe(ActorCastInfo actorCastInfo, int degree, Angle rotation)
        {
            aoes.Add(SimpleElement.Fan(actorCastInfo.SourceId.GameObject(), 60f, degree, actorCastInfo.Facing + rotation, 3000f, 0f, new HitCounter
            {
                ActionID = CastEnd,
                TargetHitCount = 3
            }));
        }
    }

    public override void OnAbilityCast(ActorAbilityInfo info)
    {
        if (CastEnd.Contains(info.ActionId) && aoes.Count > 0)
        {
            aoes[0].Remove();
            aoes.RemoveAt(0);
        }
    }
}
