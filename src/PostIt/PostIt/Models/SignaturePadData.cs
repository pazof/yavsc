namespace PostIt.Models;

/// <summary>
/// Serialized form of a signature captured by
/// <see cref="PostIt.Controls.SignaturePadControl"/>.
///
/// Wire format (length-prefixed, normalised):
/// <code>
/// int[] = [k0, x00, y00, x01, y01, ..., x0_{k0-1}, y0_{k0-1},
///          k1, x10, y10, x11, y11, ..., x1_{k1-1}, y1_{k1-1},
///          ...]
/// </code>
/// <list type="bullet">
///   <item><c>k_i</c> — number of (x, y) pairs in stroke <c>i</c>.</item>
///   <item><c>x, y</c> — coordinates normalised to <c>[0, CoordinateMax]</c>
///   (inclusive) on the control's client area. <see cref="CoordinateMax"/>
///   is <c>10_000</c> by default — a 4-decimal fixed-point fraction of
///   the surface, which is enough to discriminate 0.01% of the diagonal
///   on any reasonable screen and stays well inside <c>int</c>.</item>
///   <item>Total array length is even: each stroke contributes
///   <c>1 + 2 * k_i</c> integers, and <c>1 + 2k</c> is always odd.
///   Sum of <c>1 + 2k_i</c> over strokes is therefore odd * N, which
///   is odd when N is odd and even when N is even — so the overall
///   "size pair" property is not enforced, only the per-stroke shape
///   is. If the consumer needs a strictly even total, pad the last
///   stroke with a duplicate terminal point (or use
///   <see cref="IsEmpty"/> to drop the array entirely).</item>
/// </list>
///
/// Empty signature (no strokes) is represented by an empty array
/// (length 0). A single dot — pen down + pen up at the same point —
/// is a single stroke with <c>k = 1</c>: <c>[1, x, y]</c>.
/// </summary>
public sealed class SignaturePadData
{
    /// <summary>
    /// Upper bound of normalised coordinates. <c>10_000</c> means a
    /// surface unit is represented as 0.0001 of the whole.
    /// </summary>
    public const int CoordinateMax = 10_000;

    /// <summary>
    /// Raw payload. See <see cref="SignaturePadData"/> for the layout.
    /// Never <c>null</c>; an empty array means "no strokes".
    /// </summary>
    public int[] Strokes { get; }

    public SignaturePadData(int[] strokes)
    {
        if (strokes is null) throw new System.ArgumentNullException(nameof(strokes));
        Strokes = strokes;
    }

    /// <summary>True if no stroke has been captured.</summary>
    public bool IsEmpty => Strokes.Length == 0;

    /// <summary>
    /// Number of distinct strokes (pen-down / pen-up cycles).
    /// Returns 0 when <see cref="IsEmpty"/> is true.
    /// </summary>
    public int StrokeCount
    {
        get
        {
            if (Strokes.Length == 0) return 0;
            int n = 0;
            int i = 0;
            while (i < Strokes.Length)
            {
                int k = Strokes[i];
                // Defensive: a malformed entry is treated as 0 so we
                // never throw on read. The capture side never produces
                // these, this is only for robustness on the wire.
                if (k <= 0) return n;
                i += 1 + 2 * k;
                n++;
            }
            return n;
        }
    }
}
