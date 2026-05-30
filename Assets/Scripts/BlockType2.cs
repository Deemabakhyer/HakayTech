// Update your current block script to include an identity value
public enum BlockType2
{
    Condition,  // Oval-shaped blocks
    Number,     // Number blocks
    IfElse      // IF/ELSE container blocks
}
public enum BlockIdentity
{
    None,
    Khlas,
    Sukkari,
    _20riyals, // Ensure this matches what you see in the inspector dropdown!
    _30riyals  // (Enums can't start with numbers, so use an underscore if needed)
}