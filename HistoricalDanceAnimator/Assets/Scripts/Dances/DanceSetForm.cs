/// <summary>
/// A bitmask type selector for determining both dance set form and it's pattern
/// Form is defined with bits starting from 4th (value 8).
/// Pattern is defined with the bits 0-3.
/// </summary>
public enum DanceSetForm
{
    Error           = 0,

    Circles         = 8,
    CircleInward    = 9,
    CircleCCW       = 10,

    Lines           = 16,
    LineLongways    = 17,

    Squares         = 32,
    SquareOpposing  = 33,
    SquareAB        = 34
}
