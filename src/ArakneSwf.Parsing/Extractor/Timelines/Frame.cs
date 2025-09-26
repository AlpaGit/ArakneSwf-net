using ArakneSwf.Parsing.Extractor.Drawer;
using ArakneSwf.Parsing.Parser.Structure.Record;
using ArakneSwf.Parsing.Parser.Structure.Record.Filters;
using ArakneSwf.Parsing.Parser.Structure.Tag;

namespace ArakneSwf.Parsing.Extractor.Timelines;

/// <summary>
/// Représente une seule frame d’une timeline.
/// </summary>
public sealed class Frame : IDrawable
{
    private readonly Rectangle _bounds;
    private readonly SortedDictionary<int, FrameObject> _objects; // ordonnés par profondeur
    private readonly List<DoActionTag> _actions;
    private readonly string? _label;

    /// <summary>
    /// Le rectangle d’affichage de la frame (identique pour toutes les frames de la timeline).
    /// </summary>
    public Rectangle BoundsRect => _bounds;

    /// <summary>
    /// Objets affichés, ordonnés par profondeur.
    /// </summary>
    public IReadOnlyDictionary<int, FrameObject> Objects => _objects;

    /// <summary>
    /// Actions de script associées à cette frame.
    /// </summary>
    public IReadOnlyList<DoActionTag> Actions => _actions;

    /// <summary>
    /// Étiquette de la frame (utilisée par "go to label").
    /// </summary>
    public string? Label => _label;

    public Frame(
        Rectangle                     bounds,
        IDictionary<int, FrameObject> objects,
        IEnumerable<DoActionTag>      actions,
        string?                       label
    )
    {
        _bounds = bounds;
        _objects = new SortedDictionary<int, FrameObject>(objects); // tri croissant par profondeur
        _actions = new List<DoActionTag>(actions);
        _label = label;
    }

    public Rectangle Bounds() => _bounds;

    public int FramesCount(bool recursive = false)
    {
        if (!recursive) return 1;

        var count = 1;
        foreach (var kv in _objects)
        {
            var obj = kv.Value;
            var objectFramesCount = obj.Object.FramesCount(true);
            if (objectFramesCount > count)
                count = objectFramesCount;
        }

        return count;
    }

    public IDrawer Draw(IDrawer drawer, int frame = 0)
    {
        drawer.Area(_bounds);

        // Map des clips actifs : key = id de clip du drawer, value = profondeur du clip
        var activeClips = new Dictionary<string, int>();

        foreach (var kv in _objects)
        {
            var obj = kv.Value;

            if (obj.ClipDepth.HasValue)
            {
                var id = drawer.StartClip(obj.Object, obj.Matrix, frame);
                activeClips[id] = obj.ClipDepth.Value;
                continue;
            }

            // Fermer les clips dont la profondeur est inférieure à l’objet courant
            var toRemove = new List<string>();
            foreach (var pair in activeClips)
            {
                if (pair.Value < obj.Depth)
                {
                    drawer.EndClip(pair.Key);
                    toRemove.Add(pair.Key);
                }
            }
            foreach (var id in toRemove)
                activeClips.Remove(id);

            drawer.Include(
                obj.TransformedObject(),
                obj.Matrix,
                frame,
                obj.Filters ?? Array.Empty<Filter>(),
                obj.BlendMode,
                obj.Name
            );
        }

        return drawer;
    }

    public IDrawable TransformColors(ColorTransform colorTransform)
    {
        var objects = new SortedDictionary<int, FrameObject>();

        foreach (var kv in _objects)
        {
            var depth = kv.Key;
            var obj = kv.Value;
            objects[depth] = obj.TransformColors(colorTransform);
        }

        return new Frame(_bounds, objects, _actions, _label);
    }

    /// <summary>
    /// Modifie les bounds de la frame (utile pour conserver des bounds identiques sur toutes les frames d’un sprite).
    /// </summary>
    public Frame WithBounds(Rectangle newBounds)
        => new Frame(newBounds, _objects, _actions, _label);
}

