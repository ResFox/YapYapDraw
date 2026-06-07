using System.Collections.Generic;

namespace YapYapDraw.Engine.ModuleSetup;

public abstract class BaseModule
{
    public bool Enabled { get; private set; }

    public abstract ModuleInfo ModuleInfo { get; }

    public virtual string Name { get; protected set; } = "Unknown";

    public virtual string Author => "Res";

    public virtual string Description => string.Empty;

    public virtual HashSet<(uint Old, uint New)> NoResetPairs => new();

    public virtual bool DisableWeatherReset => false;

    public virtual HashSet<uint> NoLogActionID => new();

    public List<ISpecialAction> SpecialActions { get; init; } = new();

    public virtual bool UseAutoDraw => false;

    public virtual Dictionary<uint, HashSet<uint>> BlockOmenMap => new();

    public virtual Dictionary<uint, HashSet<string>> BlockOmenPathMap => new();

    public virtual void DrawConfig()
    {
    }

    public virtual void Reset()
    {
    }

    public virtual void Setup()
    {
        foreach (ISpecialAction specialAction in SpecialActions)
        {
            specialAction.Setup();
        }
    }

    public virtual void Enable()
    {
        Enabled = true;
    }

    public virtual void Disable()
    {
        Enabled = false;
    }
}
