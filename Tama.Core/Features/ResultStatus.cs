// SPDX-License-Identifier: MPL-2.0

namespace Tama.Core.Features;

/// <summary>
/// <see cref="Result{TValue}"/> status
/// </summary>
public enum ResultStatus
{
    /// <summary>
    /// Task completed successfully
    /// </summary>
    Success,

    /// <summary>
    /// Generic error condition
    /// </summary>
    Error,

    /// <summary>
    /// User is not authorized to perform an action
    /// </summary>
    Unauthorized,

    /// <summary>
    /// User provided invalid inputs
    /// </summary>
    BadRequest,

    /// <summary>
    /// Action can't be completed due to a bracket naming conflict
    /// </summary>
    BracketConflict,

    /// <summary>
    /// Action can't be completed due to an entry conflict
    /// </summary>
    EntryConflict,
}
