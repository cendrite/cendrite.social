namespace Social.Domain.Constants;

public static class ReservedNameConstants
{
    public static readonly IReadOnlyList<string> Words = ["cendrite", "admin"];

    public static readonly IReadOnlyDictionary<char, char[]> LookAlikes = new Dictionary<char, char[]>
    {
        ['0'] = ['o'],
        ['1'] = ['i', 'l'],
        ['3'] = ['e'],
        ['4'] = ['a'],
        ['5'] = ['s'],
        ['7'] = ['t'],
    };
}
