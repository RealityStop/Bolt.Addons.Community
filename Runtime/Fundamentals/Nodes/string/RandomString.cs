using System;
using System.Linq;
using Unity.VisualScripting;

[UnitTitle("Random String")]
[UnitCategory("Community\\Utility\\string")]
[TypeIcon(typeof(string))]
public class RandomStringNode : Unit
{
    [DoNotSerialize]
    public ValueOutput Output;

    protected override void Definition()
    {
        Output = ValueOutput<string>("Output", GetRandomString);
    }

    public string GetRandomString(Flow flow)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        Random random = new Random();
        return new string(Enumerable.Repeat(chars, 10).Select(s => s[random.Next(s.Length)]).ToArray());
    }
}
