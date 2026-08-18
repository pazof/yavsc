namespace Yavsc.Blogspot;

/// <summary>
/// Post visibility.
///
/// <list type="bullet">
///   <item><description>
///     <see cref="Public"/>: the post is read via its ACL. If the
///     ACL is empty, every caller sees the post (including
///     unauthenticated ones, on endpoints that allow it). If the
///     ACL is non-empty, only the author, the members of the
///     circles in the ACL, and administrators can read. Public +
///     non-empty ACL is therefore the "restrict by exception"
///     shape: open by default, narrowed by the ACL.
///   </description></item>
///   <item><description>
///     <see cref="Private"/>: the ACL is ignored at read time.
///     Only the author and administrators can read. The ACL list
///     is preserved in the database so that flipping the post
///     back to <see cref="Public"/> restores the previous
///     restriction without re-entry.
///   </description></item>
/// </list>
///
/// <para>The two values together form a two-axis model: the ACL
/// is the exception list (it can narrow Public), and Visibility
/// is the master switch (it can disable the ACL entirely when
/// set to Private).</para>
///
/// <para>Stored as <c>int</c> (not the enum name) — see the
/// <c>.HasConversion&lt;int&gt;()</c> on <c>BlogPost.Visibility</c>
/// in <c>Yavsc.Server.Models.ApplicationDbContext</c>. Keeping the
/// int mapping means queries stay cheap and the wire JSON is a
/// plain number; the trade-off is that reading the column by hand
/// requires knowing the enum ordering.</para>
/// </summary>
public enum Visibility
{
    Private = 0,
    Public = 1,
}
