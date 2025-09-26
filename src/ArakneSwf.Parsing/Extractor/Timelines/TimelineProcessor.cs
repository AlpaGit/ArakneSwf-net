using ArakneSwf.Parsing.Error;
using ArakneSwf.Parsing.Extractor.Error;
using ArakneSwf.Parsing.Parser.Structure.Record;
using ArakneSwf.Parsing.Parser.Structure.Tag;

namespace ArakneSwf.Parsing.Extractor.Timelines
{
    /// <summary>
    /// Processor to render a timeline from SWF display-list tags.
    /// </summary>
    public sealed class TimelineProcessor
    {
        /// <summary>
        /// Maximum bounds size for a sprite (in twips). 8192 px = 8192 * 20 = 163_840 twips.
        /// Any object that would grow bounds larger than this is ignored for the sprite bounds
        /// (it is still placed on the display list).
        /// </summary>
        private const int MAX_BOUNDS = 163_840;

        /// <summary>
        /// List of supported tag type ids.
        /// </summary>
        public static readonly int[] TAG_TYPES = new[]
        {
            EndTag.TYPE,
            ShowFrameTag.TYPE,
            PlaceObjectTag.TYPE,
            RemoveObjectTag.TYPE,
            DoActionTag.TYPE,
            PlaceObject2Tag.TYPE,
            RemoveObject2Tag.TYPE,
            FrameLabelTag.TYPE,
            PlaceObject3Tag.TYPE,
        };

        private readonly SwfExtractor _extractor;

        public TimelineProcessor(SwfExtractor extractor)
        {
            _extractor = extractor ?? throw new ArgumentNullException(nameof(extractor));
        }

        /// <summary>
        /// Check if a given error is enabled.
        /// </summary>
        public bool ErrorEnabled(Errors error) => _extractor.ErrorEnabled(error);

        /// <summary>
        /// Process display tags to build the timeline frames.
        /// </summary>
        /// <param name="tags">Sequence of display-list tags (already filtered/ordered as they appear).</param>
        /// <exception cref="SwfException">On invalid data if corresponding error flag is enabled.</exception>
        public Timeline Process(IEnumerable<object> tags)
        {
            var objectsByDepth = new Dictionary<int, FrameObject>(); // depth -> FrameObject
            var actions = new List<DoActionTag>();
            string? frameLabel = null;
            var frames = new List<Frame>();
            var empty = true;

            // Sprite bounds (in twips)
            int xmin = int.MaxValue, ymin = int.MaxValue, xmax = int.MinValue, ymax = int.MinValue;

            foreach (var t in tags)
            {
                switch (t)
                {
                    case EndTag:
                        goto Done; // end of substream/sprite

                    case ShowFrameTag:
                    {
                        // Ensure depths order is respected inside a frame
                        var sorted = new SortedDictionary<int, FrameObject>(objectsByDepth);

                        var frameBounds = sorted.Count > 0
                            ? new Rectangle(xmin, xmax, ymin, ymax)
                            : new Rectangle(0, 0, 0, 0);

                        // Copy dictionaries/lists to snapshot the frame state
                        frames.Add(new Frame(
                                       frameBounds,
                                       new Dictionary<int, FrameObject>(sorted),
                                       new List<DoActionTag>(actions),
                                       frameLabel
                                   ));

                        actions.Clear();
                        frameLabel = null;
                        continue;
                    }

                    case DoActionTag a:
                        actions.Add(a);
                        continue;

                    case FrameLabelTag fl:
                        frameLabel = fl.Label;
                        continue;

                    case RemoveObject2Tag r2:
                        objectsByDepth.Remove(r2.Depth);
                        continue;

                    case RemoveObjectTag r1:
                        objectsByDepth.Remove(r1.Depth);
                        continue;

                    // Ignore stream-sound setup (not a visual object)
                    case SoundStreamHeadTag:
                        continue;
                }

                // From here we only accept PlaceObjectX tags
                if (t is not PlaceObjectTag && t is not PlaceObject2Tag && t is not PlaceObject3Tag)
                {
                    if (ErrorEnabled(Errors.UnprocessableData))
                        throw new ProcessingInvalidDataException(
                            $"Invalid tag type {t.GetType().FullName} in timeline");

                    continue;
                }

                // New placement vs modification
                var isNewObject = t switch
                {
                    PlaceObject2Tag p2 => !p2.Move,
                    PlaceObject3Tag p3 => !p3.Move,
                    PlaceObjectTag     => true, // v1 "place" is always a new object
                    _                  => true
                };

                var depth = t switch
                {
                    PlaceObjectTag p1  => p1.Depth,
                    PlaceObject2Tag p2 => p2.Depth,
                    PlaceObject3Tag p3 => p3.Depth,
                    _                  => 0
                };

                var characterId = t switch
                {
                    PlaceObjectTag p1  => p1.CharacterId,
                    PlaceObject2Tag p2 => p2.CharacterId,
                    PlaceObject3Tag p3 => p3.CharacterId,
                    _                  => null
                };

                if (isNewObject && characterId is null)
                {
                    if (ErrorEnabled(Errors.UnprocessableData))
                        throw new ProcessingInvalidDataException($"New object at depth {depth} without characterId");
                    continue;
                }

                FrameObject fo;

                if (isNewObject)
                {
                    fo = PlaceNewObject(t);
                }
                else
                {
                    if (!objectsByDepth.TryGetValue(depth, out var current))
                    {
                        if (ErrorEnabled(Errors.UnprocessableData))
                            throw new ProcessingInvalidDataException(
                                $"Cannot modify object at depth {depth}: it was not found");
                        continue;
                    }

                    // Modify is not possible with PlaceObject v1 tag
                    if (t is PlaceObjectTag)
                    {
                        if (ErrorEnabled(Errors.UnprocessableData))
                            throw new ProcessingInvalidDataException(
                                "Modify is not possible with PlaceObject (v1) tag");
                        continue;
                    }

                    fo = ModifyObject(t, current);
                }

                objectsByDepth[depth] = fo;

                // Compute sprite bounds (but ignore too large objects)
                var b = fo.Bounds;
                if (b.Width() > MAX_BOUNDS || b.Height() > MAX_BOUNDS)
                    continue;

                if (!empty &&
                    ((b.XMax - xmin) > MAX_BOUNDS ||
                     (b.YMax - ymin) > MAX_BOUNDS ||
                     (xmax - b.XMin) > MAX_BOUNDS ||
                     (ymax - b.YMin) > MAX_BOUNDS))
                {
                    // Including this object would make the sprite bounds too large
                    continue;
                }

                empty = false;
                if (b.XMax > xmax) xmax = b.XMax;
                if (b.XMin < xmin) xmin = b.XMin;
                if (b.YMax > ymax) ymax = b.YMax;
                if (b.YMin < ymin) ymin = b.YMin;
            }

            Done:

            if (frames.Count == 0)
            {
                if (ErrorEnabled(Errors.UnprocessableData))
                    throw new ProcessingInvalidDataException(
                        "No frames found in the timeline: ShowFrame tag is missing");
                return Timeline.Empty();
            }

            var spriteBounds = !empty ? new Rectangle(xmin, xmax, ymin, ymax) : new Rectangle(0, 0, 0, 0);

            // Use the same bounds for all frames
            for (var i = 0; i < frames.Count; i++)
            {
                if (!frames[i].Bounds().Equals(spriteBounds))
                {
                    frames[i] = frames[i].WithBounds(spriteBounds);
                }
            }

            return new Timelines.Timeline(spriteBounds, frames.ToArray());
        }

        /// <summary>
        /// Place a brand-new object at a given depth.
        /// </summary>
        private FrameObject PlaceNewObject(object tag)
        {
            var depth = tag switch
            {
                PlaceObjectTag p1  => p1.Depth,
                PlaceObject2Tag p2 => p2.Depth,
                PlaceObject3Tag p3 => p3.Depth,
                _                  => throw new ArgumentOutOfRangeException(nameof(tag))
            };

            var characterId = tag switch
            {
                PlaceObjectTag p1  => p1.CharacterId,
                PlaceObject2Tag p2 => p2.CharacterId ?? throw new InvalidOperationException(),
                PlaceObject3Tag p3 => p3.CharacterId ?? throw new InvalidOperationException(),
                _                  => throw new InvalidOperationException()
            };

            var drawable = _extractor.Character(characterId) as IDrawable ??
                           throw new ProcessingInvalidDataException($"Character {characterId} is not drawable");

            var ob = drawable.Bounds();

            var matrix = tag switch
            {
                PlaceObjectTag p1  => p1.Matrix,
                PlaceObject2Tag p2 => p2.Matrix,
                PlaceObject3Tag p3 => p3.Matrix,
                _                  => null
            };

            Matrix newMatrix;
            Rectangle newBounds;

            if (matrix is not null)
            {
                // Apply placement matrix to the object's offset, and transform bounds with the raw matrix
                newMatrix = matrix.Translate(ob.XMin, ob.YMin);
                newBounds = ob.Transform(matrix);
            }
            else
            {
                newMatrix = new Matrix(translateX: ob.XMin, translateY: ob.YMin);
                newBounds = ob;
            }

            var colorTransform = tag switch
            {
                PlaceObjectTag p1  => p1.ColorTransform,
                PlaceObject2Tag p2 => p2.ColorTransform,
                PlaceObject3Tag p3 => p3.ColorTransform,
                _                  => null
            };

            var clipDepth = tag switch
            {
                PlaceObject2Tag p2 => p2.ClipDepth,
                PlaceObject3Tag p3 => p3.ClipDepth,
                _                  => null
            };

            var name = tag switch
            {
                PlaceObject2Tag p2 => p2.Name,
                PlaceObject3Tag p3 => p3.Name,
                _                  => null
            };

            var filters = tag is PlaceObject3Tag p3T && p3T.SurfaceFilterList is { } fl ? fl : [];

            var blendMode = tag is PlaceObject3Tag p3B && p3B.BlendMode is not null
                ? TryBlendMode(p3B.BlendMode.Value)
                : BlendMode.Normal;

            return new FrameObject(
                characterId,
                depth,
                drawable,
                newBounds,
                newMatrix,
                colorTransform,
                clipDepth,
                name,
                filters,
                blendMode
            );
        }

        /// <summary>
        /// Modify an already placed object at a given depth (PlaceObject2 / PlaceObject3).
        /// </summary>
        private FrameObject ModifyObject(object tag, FrameObject obj)
        {
            // Change character (and rebase matrix/bounds) if CharacterId is present
            if (GetCharacterId(tag) is { } newCharId)
            {
                var oldBounds = obj.Object.Bounds();
                var newObject = _extractor.Character(newCharId) as IDrawable ??
                                throw new ProcessingInvalidDataException($"Character {newCharId} is not drawable");

                var matrix = GetMatrix(tag) ?? obj.Matrix.Translate(-oldBounds.XMin, -oldBounds.YMin);
                var newBounds = newObject.Bounds();

                obj = obj.With(
                    characterId: newCharId,
                    @object: newObject,
                    bounds: newBounds.Transform(matrix),
                    matrix: matrix.Translate(newBounds.XMin, newBounds.YMin)
                );
            }
            else if (GetMatrix(tag) is Matrix m)
            {
                var current = obj.Object.Bounds();
                obj = obj.With(
                    bounds: current.Transform(m),
                    matrix: m.Translate(current.XMin, current.YMin)
                );
            }

            // PlaceObject3: filters & blend mode
            if (tag is PlaceObject3Tag p3 && (p3.BlendMode is not null || p3.SurfaceFilterList is not null))
            {
                obj = obj.With(
                    filters: p3.SurfaceFilterList,
                    blendMode: p3.BlendMode is not null ? TryBlendMode(p3.BlendMode.Value) : null
                );
            }

            // Common: color transform, clip depth, name
            var ct = GetColorTransform(tag);
            var cd = GetClipDepth(tag);
            var name = GetName(tag);

            if (ct is not null || cd is not null || name is not null)
            {
                obj = obj.With(
                    colorTransform: ct,
                    clipDepth: cd,
                    name: name
                );
            }

            return obj;
        }

        // --- Small helpers to read optional properties without dynamic ---

        private static int? GetCharacterId(object tag) => tag switch
        {
            PlaceObject2Tag p2 => p2.CharacterId,
            PlaceObject3Tag p3 => p3.CharacterId,
            _                  => null
        };

        private static Matrix? GetMatrix(object tag) => tag switch
        {
            PlaceObjectTag p1  => p1.Matrix,
            PlaceObject2Tag p2 => p2.Matrix,
            PlaceObject3Tag p3 => p3.Matrix,
            _                  => null
        };

        private static ColorTransform? GetColorTransform(object tag) => tag switch
        {
            PlaceObjectTag p1  => p1.ColorTransform,
            PlaceObject2Tag p2 => p2.ColorTransform,
            PlaceObject3Tag p3 => p3.ColorTransform,
            _                  => null
        };

        private static int? GetClipDepth(object tag) => tag switch
        {
            PlaceObject2Tag p2 => p2.ClipDepth,
            PlaceObject3Tag p3 => p3.ClipDepth,
            _                  => null
        };

        private static string? GetName(object tag) => tag switch
        {
            PlaceObject2Tag p2 => p2.Name,
            PlaceObject3Tag p3 => p3.Name,
            _                  => null
        };

        private static BlendMode TryBlendMode(int raw) =>
            Enum.IsDefined(typeof(BlendMode), raw) ? (BlendMode)raw : BlendMode.Normal;
    }
}