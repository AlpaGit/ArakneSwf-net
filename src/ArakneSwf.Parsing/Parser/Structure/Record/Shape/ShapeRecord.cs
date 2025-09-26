using System.Diagnostics;

namespace ArakneSwf.Parsing.Parser.Structure.Record.Shape;

/// <summary>
/// Base type for all shape records.
/// </summary>
public abstract class ShapeRecord
{
    /// <summary>
    /// Read a collection of shape records from the SWF reader until an end record is reached
    /// (i.e. a style-change record with no flags set).
    /// </summary>
    /// <param name="reader">SWF bit/byte reader positioned at the start of a shape.</param>
    /// <param name="version">Shape records version (1..4).</param>
    public static List<ShapeRecord> ReadCollection(SwfReader reader, int version)
    {
        var numFillBits = (int)reader.ReadUb(4);
        var numLineBits = (int)reader.ReadUb(4);
        var shapeRecords = new List<ShapeRecord>();

        Debug.Assert(numFillBits < 16);
        Debug.Assert(numLineBits < 16);

        while (reader.Offset < reader.End)
        {
            var edgeRecord = reader.ReadBool();

            if (edgeRecord)
            {
                var straightFlag = reader.ReadBool();
                var numBits = (int)reader.ReadUb(4);
                Debug.Assert(numBits < 16);

                if (straightFlag)
                {
                    var generalLineFlag = reader.ReadBool();
                    var vertLineFlag = !generalLineFlag && reader.ReadBool();

                    var deltaX = (generalLineFlag || !vertLineFlag) ? reader.ReadSb(numBits + 2) : 0;
                    var deltaY = (generalLineFlag || vertLineFlag) ? reader.ReadSb(numBits + 2) : 0;

                    shapeRecords.Add(new StraightEdgeRecord(generalLineFlag, vertLineFlag, deltaX, deltaY));
                }
                else
                {
                    shapeRecords.Add(new CurvedEdgeRecord(
                                         reader.ReadSb(numBits + 2),
                                         reader.ReadSb(numBits + 2),
                                         reader.ReadSb(numBits + 2),
                                         reader.ReadSb(numBits + 2)
                                     ));
                }

                continue;
            }

            // Style change record
            var stateNewStyles = reader.ReadBool();
            var stateLineStyle = reader.ReadBool();
            var stateFillStyle1 = reader.ReadBool();
            var stateFillStyle0 = reader.ReadBool();
            var stateMoveTo = reader.ReadBool();

            // End of shape ?
            if (!stateNewStyles && !stateLineStyle && !stateFillStyle1 && !stateFillStyle0 && !stateMoveTo)
            {
                shapeRecords.Add(new EndShapeRecord());
                break;
            }

            int moveDeltaX, moveDeltaY;
            if (stateMoveTo)
            {
                var moveBits = (int)reader.ReadUb(5);
                Debug.Assert(moveBits < 32);

                moveDeltaX = reader.ReadSb(moveBits);
                moveDeltaY = reader.ReadSb(moveBits);
            }
            else
            {
                moveDeltaX = 0;
                moveDeltaY = 0;
            }

            var fillStyle0 = stateFillStyle0 ? (int)reader.ReadUb(numFillBits) : 0;
            var fillStyle1 = stateFillStyle1 ? (int)reader.ReadUb(numFillBits) : 0;
            var lineStyle = stateLineStyle ? (int)reader.ReadUb(numLineBits) : 0;

            List<FillStyle> newFillStyles;
            List<LineStyle> newLineStyles;

            if (stateNewStyles && version >= 2)
            {
                reader.AlignByte();
                newFillStyles = FillStyle.ReadCollection(reader, version);
                newLineStyles = LineStyle.ReadCollection(reader, version);
                numFillBits = (int)reader.ReadUb(4);
                numLineBits = (int)reader.ReadUb(4);

                Debug.Assert(numFillBits < 16);
                Debug.Assert(numLineBits < 16);
            }
            else
            {
                newFillStyles = new List<FillStyle>(0);
                newLineStyles = new List<LineStyle>(0);
            }

            shapeRecords.Add(new StyleChangeRecord(
                                 stateNewStyles,
                                 stateLineStyle,
                                 stateFillStyle0,
                                 stateFillStyle1,
                                 stateMoveTo,
                                 moveDeltaX,
                                 moveDeltaY,
                                 fillStyle0,
                                 fillStyle1,
                                 lineStyle,
                                 newFillStyles,
                                 newLineStyles
                             ));
        }

        reader.AlignByte();

        return shapeRecords;
    }
}