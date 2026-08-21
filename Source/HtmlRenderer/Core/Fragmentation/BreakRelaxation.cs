namespace TheArtOfDev.HtmlRenderer.Core.Fragmentation
{
    /// <summary>
    /// How much of a break decision's ideal shape survived - the staged relaxation
    /// https://www.w3.org/TR/css-break-3/#possible-breaks (CSS Fragmentation Level 3 §4.3) asks for,
    /// stated once rather than implied by which arm of layout happened to run first. Ported from
    /// PeachPDF's <c>BreakRelaxation</c>.
    /// </summary>
    /// <remarks>
    /// §4.3's rule is that a constraint which cannot be satisfied is given up progressively, never all at
    /// once and never at the cost of losing content:
    /// <list type="number">
    /// <item><description><b>Everything holds</b> - the box moves to its target and the whole keep-with-next run chained to it moves with it. <see cref="None"/>.</description></item>
    /// <item><description><b>Part of the run is left behind</b> (<see cref="RunTrimmed"/>) - trimmed from its front until what remains fits the destination.</description></item>
    /// <item><description><b>The whole run is left behind</b> (<see cref="RunDropped"/>) - no part of it can travel, so the box moves alone.</description></item>
    /// <item><description><b>The container is left behind</b> (<see cref="ContainerLeftBehind"/>) - the break is taken on the box alone and the container spans the boundary.</description></item>
    /// <item><description><b>The constraint itself is given up</b> - the box is not moved at all and the boundary cuts it (a monolithic box that fits in no fragmentainer).</description></item>
    /// <item><description><b>Break anywhere</b>, so content is never lost - the driver's own no-progress backstop lays the remainder out monolithically.</description></item>
    /// </list>
    /// Relaxation must keep the decision terminating: every tier either moves the box once or declines to
    /// move it, never re-asking the question.
    /// </remarks>
    internal enum BreakRelaxation
    {
        /// <summary>Nothing was given up.</summary>
        None,

        /// <summary>The earliest members of the keep-with-next run were left behind so the rest could travel.</summary>
        RunTrimmed,

        /// <summary>No part of the keep-with-next run could travel, so the box moves alone.</summary>
        RunDropped,

        /// <summary>
        /// The container whose break point this really is could not travel, so the box moves out of it and
        /// the container spans the boundary.
        /// </summary>
        ContainerLeftBehind
    }
}
